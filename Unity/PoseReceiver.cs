using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// MediaPipe's fixed landmark order. Left/Right refer to the person's body.
public enum Joint
{
    Nose = 0, LeftEyeInner = 1, LeftEye = 2, LeftEyeOuter = 3,
    RightEyeInner = 4, RightEye = 5, RightEyeOuter = 6,
    LeftEar = 7, RightEar = 8, MouthLeft = 9, MouthRight = 10,
    LeftShoulder = 11, RightShoulder = 12, LeftElbow = 13, RightElbow = 14,
    LeftWrist = 15, RightWrist = 16, LeftPinky = 17, RightPinky = 18,
    LeftIndex = 19, RightIndex = 20, LeftThumb = 21, RightThumb = 22,
    LeftHip = 23, RightHip = 24, LeftKnee = 25, RightKnee = 26,
    LeftAnkle = 27, RightAnkle = 28, LeftHeel = 29, RightHeel = 30,
    LeftFootIndex = 31, RightFootIndex = 32
}

public class PoseReceiver : MonoBehaviour
{
    [Serializable] public class Landmark
    {
        public float x, y, z, visibility, presence;
    }
    [Serializable] public class PosePacket
    {
        public bool tracked;
        public double timestamp;
        public Landmark[] joints;
    }

    [Header("Connection")]
    public int port = 5052;
    public float timeoutSeconds = 0.5f;
    [Header("Coordinates")]
    public bool mirrorX = true;
    [Range(0, 1)] public float minimumVisibility = 0.5f;
    [Header("Live status (Play mode)")]
    [SerializeField] private bool receiving;
    [SerializeField] private bool tracking;
    [SerializeField] private Vector2 rightWrist;

    private UdpClient client;
    private Thread receiveThread;
    private volatile bool listening;
    private readonly object gate = new object();
    private string pendingJson;
    private string pendingError;
    private PosePacket latest;
    private float lastReceived = float.NegativeInfinity;

    public bool IsReceiving => Time.realtimeSinceStartup - lastReceived <= timeoutSeconds;
    public bool IsTracking => IsReceiving && latest != null && latest.tracked
        && latest.joints != null && latest.joints.Length == 33;

    private void OnEnable()
    {
        latest = null;
        lastReceived = float.NegativeInfinity;
        lock (gate) { pendingJson = null; pendingError = null; }
        try
        {
            client = new UdpClient(new IPEndPoint(IPAddress.Loopback, port));
            client.Client.ReceiveTimeout = 200;
            listening = true;
            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();
        }
        catch (Exception error)
        {
            listening = false;
            client?.Close(); client = null;
            Debug.LogError("PoseReceiver could not open port " + port + ": " + error.Message, this);
        }
    }

    private void ReceiveLoop()
    {
        // No Unity API calls on this thread. Keep only the newest packet.
        var sender = new IPEndPoint(IPAddress.Any, 0);
        while (listening)
        {
            try
            {
                byte[] bytes = client.Receive(ref sender);
                string json = Encoding.UTF8.GetString(bytes);
                lock (gate) { pendingJson = json; }
            }
            catch (SocketException error)
            {
                if (!listening) break;
                if (error.SocketErrorCode == SocketError.TimedOut) continue;
                lock (gate) { pendingError = error.Message; }
                break;
            }
            catch (ObjectDisposedException) { break; }
        }
    }

    private void Update()
    {
        string json, error;
        lock (gate)
        {
            json = pendingJson; pendingJson = null;
            error = pendingError; pendingError = null;
        }
        if (error != null) Debug.LogError("PoseReceiver: " + error, this);
        if (json != null)
        {
            try
            {
                var packet = JsonUtility.FromJson<PosePacket>(json);
                if (packet != null && (!packet.tracked || (packet.joints != null && packet.joints.Length == 33)))
                {
                    latest = packet;
                    lastReceived = Time.realtimeSinceStartup;
                }
            }
            catch (ArgumentException) { /* Ignore malformed packets. */ }
        }
        receiving = IsReceiving;
        tracking = IsTracking;
        if (TryGetJoint(Joint.RightWrist, out Vector2 point)) rightWrist = point;
    }

    public bool TryGetJoint(Joint joint, out Vector2 point)
    {
        point = Vector2.zero;
        if (!TryGetRawJoint(joint, out Landmark p)) return false;
        point = new Vector2(mirrorX ? 1f - p.x : p.x, 1f - p.y);
        return true;
    }

    // Raw image coordinates and estimated relative depth; z is not meters.
    public bool TryGetRawJoint(Joint joint, out Landmark landmark)
    {
        landmark = null;
        if (!IsTracking) return false;
        int index = (int)joint;
        if (index < 0 || index >= latest.joints.Length) return false;
        var p = latest.joints[index];
        // Web landmarks may omit presence; filter using visibility only.
        if (p == null || p.visibility < minimumVisibility) return false;
        landmark = p;
        return true;
    }

    private void OnDisable()
    {
        listening = false;
        client?.Close();
        receiveThread?.Join(1000);
        receiveThread = null; client = null;
        latest = null; lastReceived = float.NegativeInfinity;
        receiving = false; tracking = false;
    }
}
