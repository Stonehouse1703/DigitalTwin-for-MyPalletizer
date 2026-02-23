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
    
    [Header("LED Controller")]
    public RobotLEDController ledController;  // im Inspector zuweisen

    [Header("Robot Reference")]
    public MyPalletizerArticulationAdapter adapter;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    
    private readonly Queue<RobotCommand> _commandQueue = new();
    private readonly object _lock = new object();

    // 1. Die Datenstruktur für das JSON-Parsing
    [Serializable]
    public class RobotData
    {
        public string type;
        public float j1, j2, j3, j4;
        public float speed;
        public int r, g, b;
    }

    // 2. Die interne Struktur für die Warteschlange (HIER lagen die Fehler)
    private struct RobotCommand
    {
        public string type;
        public float j1, j2, j3, j4;
        public float speed;
        public int r, g, b;
    }

    void Start()
    {
        if (adapter == null) adapter = GetComponent<MyPalletizerArticulationAdapter>();
        
        _receiveThread = new Thread(ReceiveData) { IsBackground = true };
        _receiveThread.Start();
        Debug.Log($"UDP Bridge gestartet auf Port {port}");
        
        if (ledController == null)
            ledController = FindFirstObjectByType<RobotLEDController>();
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
                Debug.Log("UDP RAW: " + text);

                RobotData robotData = JsonUtility.FromJson<RobotData>(text);

                lock (_lock)
                {
                    // Wir übertragen alle Werte in das Command-Objekt
                    _commandQueue.Enqueue(new RobotCommand {
                        type = robotData.type,
                        j1 = robotData.j1,
                        j2 = robotData.j2,
                        j3 = robotData.j3,
                        j4 = robotData.j4,
                        speed = robotData.speed,
                        r = robotData.r,
                        g = robotData.g,
                        b = robotData.b
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
        lock (_lock)
        {
            while (_commandQueue.Count > 0)
            {
                var cmd = _commandQueue.Dequeue();
                
                // Entscheidung: LED oder Bewegung?
                if (cmd.type == "led")
                {
                    if (cmd.type == "led")
                    {
                        if (ledController != null)
                            ledController.SetLEDColor(cmd.r, cmd.g, cmd.b);
                        else
                            Debug.LogWarning("No RobotLEDController found in scene!");
                    }
                }
                else if (cmd.type == "move")
                {
                    adapter.SendAngle(1, cmd.j1, cmd.speed);
                    adapter.SendAngle(2, cmd.j2, cmd.speed);
                    adapter.SendAngle(3, cmd.j3, cmd.speed);
                    adapter.SendAngle(4, cmd.j4, cmd.speed);
                }
                else
                {
                    Debug.LogWarning($"Unknown command type: '{cmd.type}'");
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        _udpClient?.Close();
        _receiveThread?.Abort();
    }
}