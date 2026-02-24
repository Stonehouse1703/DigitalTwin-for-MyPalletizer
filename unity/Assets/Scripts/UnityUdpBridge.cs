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
    public RobotLEDController ledController;

    [Header("Robot Reference")]
    public MyPalletizerArticulationAdapter adapter;

    private UdpClient _udpClient;
    private Thread _receiveThread;
    private volatile bool _running = true;

    private readonly Queue<RobotCommand> _commandQueue = new();
    private readonly object _lock = new object();

    [Serializable]
    public class RobotData
    {
        public string type;
        public float j1, j2, j3, j4;
        public float speed;
        public int r, g, b;
    }

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
        if (ledController == null) ledController = FindFirstObjectByType<RobotLEDController>();

        _receiveThread = new Thread(ReceiveData) { IsBackground = true };
        _receiveThread.Start();

        StartCoroutine(CommandRunner());

        Debug.Log($"UDP Bridge gestartet auf Port {port}");
    }

    private void ReceiveData()
    {
        _udpClient = new UdpClient(port);

        while (_running)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpClient.Receive(ref anyIP); // blockiert
                string text = Encoding.UTF8.GetString(data);

                var robotData = JsonUtility.FromJson<RobotData>(text);
                if (robotData == null || string.IsNullOrWhiteSpace(robotData.type))
                    continue;

                lock (_lock)
                {
                    _commandQueue.Enqueue(new RobotCommand
                    {
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
                    Debug.Log("Enqueued command: " + robotData.type);
                }
            }
            catch (SocketException)
            {
                // tritt beim Close() auf -> ok beim Shutdown
            }
            catch (Exception e)
            {
                Debug.LogWarning("UDP Receive Error: " + e.Message);
            }
        }
    }

    private System.Collections.IEnumerator CommandRunner()
    {
        while (_running)
        {
            RobotCommand cmd;
            bool hasCmd = false;

            lock (_lock)
            {
                if (_commandQueue.Count > 0)
                {
                    cmd = _commandQueue.Dequeue();
                    hasCmd = true;
                }
                else cmd = default;
            }

            if (!hasCmd)
            {
                yield return null;
                continue;
            }

            Debug.Log("Received command: " + cmd.type + ", j1=" + cmd.j1 + ", j2=" + cmd.j2 + ", j3=" + cmd.j3 + ", j4=" + cmd.j4 + ", speed=" + cmd.speed + ", r=" + cmd.r + ", g=" + cmd.g + ", b=" + cmd.b);
            switch (cmd.type)
            {
                case "led":
                    if (ledController != null) ledController.SetLEDColor(cmd.r, cmd.g, cmd.b);
                    else Debug.LogWarning("No RobotLEDController found in scene!");
                    break;

                case "move_joints":
                    // fire & forget
                    adapter.MoveJointsAsync(cmd.j1, cmd.j2, cmd.j3, cmd.j4, cmd.speed);
                    break;

                case "sync_move_joints":
                    // blockiert die Queue bis fertig
                    yield return adapter.MoveJointsSync(cmd.j1, cmd.j2, cmd.j3, cmd.j4, cmd.speed);
                    break;

                default:
                    Debug.LogWarning($"Unknown command type: '{cmd.type}'");
                    break;
            }
            Debug.Log("🏁 Done with command: " + cmd.type);
        }
    }

    private void OnApplicationQuit()
    {
        _running = false;
        _udpClient?.Close();       // löst Receive() auf -> Thread endet
        _receiveThread?.Join(200); // kurz warten statt Abort
    }
}