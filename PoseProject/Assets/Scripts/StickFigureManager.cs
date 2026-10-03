using System.Collections.Generic;
using UnityEngine;

public class StickFigureManager : MonoBehaviour
{
    [Header("Head")]
    public Transform nose;

    [Header("Left arm")]
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftWrist;

    [Header("Right arm")]
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightWrist;

    [Header("Left leg")]
    public Transform leftHip;
    public Transform leftKnee;
    public Transform leftHeel;

    [Header("Right leg")]
    public Transform rightHip;
    public Transform rightKnee;
    public Transform rightHeel;

    [Header("Line appearance")]
    public Material lineMaterial;
    public Color lineColor = Color.white;
    [Min(0.001f)] public float lineWidth = 0.08f;
    public string sortingLayerName = "Default";
    public int sortingOrder = -1;
    public float lineZ = 0f;

    private Transform neck;

    private class Connection
    {
        public Transform a;
        public Transform b;
        public LineRenderer line;
    }

    private readonly List<Connection> connections = new List<Connection>();

    private void Start()
    {
        GameObject neckObject = new GameObject("Neck");
        neckObject.transform.SetParent(transform, false);
        neck = neckObject.transform;

        AddLine("Neck To Nose", neck, nose);
        AddLine("Shoulders", leftShoulder, rightShoulder);
        AddLine("Left Upper Arm", leftShoulder, leftElbow);
        AddLine("Left Forearm", leftElbow, leftWrist);
        AddLine("Right Upper Arm", rightShoulder, rightElbow);
        AddLine("Right Forearm", rightElbow, rightWrist);
        AddLine("Left Torso", leftShoulder, leftHip);
        AddLine("Right Torso", rightShoulder, rightHip);
        AddLine("Hips", leftHip, rightHip);
        AddLine("Left Thigh", leftHip, leftKnee);
        AddLine("Left Lower Leg", leftKnee, leftHeel);
        AddLine("Right Thigh", rightHip, rightKnee);
        AddLine("Right Lower Leg", rightKnee, rightHeel);
    }

    private void AddLine(string lineName, Transform a, Transform b)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.loop = false;
        line.numCapVertices = 6;
        line.enabled = false;
        connections.Add(new Connection { a = a, b = b, line = line });
    }

    private void LateUpdate()
    {
        if (neck != null)
        {
            bool shouldersAvailable =
                leftShoulder != null && rightShoulder != null &&
                leftShoulder.gameObject.activeInHierarchy &&
                rightShoulder.gameObject.activeInHierarchy;
            neck.gameObject.SetActive(shouldersAvailable);
            if (shouldersAvailable)
                neck.position = (leftShoulder.position + rightShoulder.position) * 0.5f;
        }

        foreach (Connection connection in connections)
        {
            LineRenderer line = connection.line;
            bool canDraw =
                connection.a != null && connection.b != null &&
                connection.a.gameObject.activeInHierarchy &&
                connection.b.gameObject.activeInHierarchy;
            line.enabled = canDraw;
            if (!canDraw) continue;

            line.sharedMaterial = lineMaterial;
            line.startColor = lineColor;
            line.endColor = lineColor;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.sortingLayerName = sortingLayerName;
            line.sortingOrder = sortingOrder;

            Vector3 a = connection.a.position;
            Vector3 b = connection.b.position;
            a.z = lineZ;
            b.z = lineZ;
            line.SetPosition(0, a);
            line.SetPosition(1, b);
        }
    }
}
