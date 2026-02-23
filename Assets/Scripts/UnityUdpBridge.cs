using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class UnityUdpBridge : MonoBehaviour
{
    [Header("Networking")]
    public int port = 5005;

    [Header("Robot Reference")]
    public MyPalletizerArticulationAdapter adapter;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    
    // Da Unity-Funktionen (Coroutinen) nur im Main-Thread laufen dürfen, 
    // speichern wir Befehle zwischen.
    private readonly Queue<RobotCommand> _commandQueue = new();
    private readonly object _lock = new object();

    [Serializable]
    public class RobotData
    {
        public float j1, j2, j3, j4;
        public float speed;
    }

    private struct RobotCommand
    {
        public float[] angles;
        public float speed;
    }

    void Start()
    {
        if (adapter == null) adapter = GetComponent<MyPalletizerArticulationAdapter>();
        
        _receiveThread = new Thread(ReceiveData) { IsBackground = true };
        _receiveThread.Start();
        Debug.Log($"UDP Bridge gestartet auf Port {port}");
    }

    private void ReceiveData()
    {
        _udpClient = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpClient.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);

                // JSON parsen
                RobotData robotData = JsonUtility.FromJson<RobotData>(text);

                // In Thread-sichere Queue schieben
                lock (_lock)
                {
                    _commandQueue.Enqueue(new RobotCommand {
                        angles = new float[] { robotData.j1, robotData.j2, robotData.j3, robotData.j4 },
                        speed = robotData.speed
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("UDP Receive Error: " + e.Message);
            }
        }
    }

    void Update()
    {
        // Im Main-Thread die Befehle an den Adapter weiterreichen
        lock (_lock)
        {
            while (_commandQueue.Count > 0)
            {
                var cmd = _commandQueue.Dequeue();
                ExecuteOnAdapter(cmd);
            }
        }
    }

    private void ExecuteOnAdapter(RobotCommand cmd)
    {
        // Wir senden alle 4 Winkel an deinen Adapter
        for (int i = 0; i < 4; i++)
        {
            adapter.SendAngle(i + 1, cmd.angles[i], cmd.speed);
        }
    }

    private void OnApplicationQuit()
    {
        _udpClient?.Close();
        _receiveThread?.Abort();
    }
}