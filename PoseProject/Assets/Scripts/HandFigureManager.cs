using System.Collections.Generic;
using UnityEngine;

public class HandFigureManager : MonoBehaviour
{
    [Header("Assign the 21 hand joints once")]
    public Transform wrist;
    public Transform thumbCMC;
    public Transform thumbMCP;
    public Transform thumbIP;
    public Transform thumbTip;
    public Transform indexMCP;
    public Transform indexPIP;
    public Transform indexDIP;
    public Transform indexTip;
    public Transform middleMCP;
    public Transform middlePIP;
    public Transform middleDIP;
    public Transform middleTip;
    public Transform ringMCP;
    public Transform ringPIP;
    public Transform ringDIP;
    public Transform ringTip;
    public Transform pinkyMCP;
    public Transform pinkyPIP;
    public Transform pinkyDIP;
    public Transform pinkyTip;

    [Header("Line appearance")]
    public Material lineMaterial;
    public Color lineColor = Color.white;
    [Min(0.001f)] public float lineWidth = 0.08f;
    public string sortingLayerName = "Default";
    public int sortingOrder = -1;
    public float lineZ = 0f;
    [Header("Optional: hide lines when tracking is lost")]
    public HandReceiver hand;
    public bool hideWhenNotTracking = true;

    private class Connection
    {
        public Transform a, b;
        public LineRenderer line;
    }
    private readonly List<Connection> connections = new List<Connection>();

    private void Start()
    {
        AddLine("wrist To thumbCMC", wrist, thumbCMC);
        AddLine("thumbCMC To thumbMCP", thumbCMC, thumbMCP);
        AddLine("thumbMCP To thumbIP", thumbMCP, thumbIP);
        AddLine("thumbIP To thumbTip", thumbIP, thumbTip);
        AddLine("wrist To indexMCP", wrist, indexMCP);
        AddLine("indexMCP To indexPIP", indexMCP, indexPIP);
        AddLine("indexPIP To indexDIP", indexPIP, indexDIP);
        AddLine("indexDIP To indexTip", indexDIP, indexTip);
        AddLine("indexMCP To middleMCP", indexMCP, middleMCP);
        AddLine("middleMCP To middlePIP", middleMCP, middlePIP);
        AddLine("middlePIP To middleDIP", middlePIP, middleDIP);
        AddLine("middleDIP To middleTip", middleDIP, middleTip);
        AddLine("middleMCP To ringMCP", middleMCP, ringMCP);
        AddLine("ringMCP To ringPIP", ringMCP, ringPIP);
        AddLine("ringPIP To ringDIP", ringPIP, ringDIP);
        AddLine("ringDIP To ringTip", ringDIP, ringTip);
        AddLine("ringMCP To pinkyMCP", ringMCP, pinkyMCP);
        AddLine("wrist To pinkyMCP", wrist, pinkyMCP);
        AddLine("pinkyMCP To pinkyPIP", pinkyMCP, pinkyPIP);
        AddLine("pinkyPIP To pinkyDIP", pinkyPIP, pinkyDIP);
        AddLine("pinkyDIP To pinkyTip", pinkyDIP, pinkyTip);
    }

    private void AddLine(string lineName, Transform a, Transform b)
    {
        var obj = new GameObject(lineName);
        obj.transform.SetParent(transform, false);
        var line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.numCapVertices = 6;
        line.enabled = false;
        connections.Add(new Connection { a = a, b = b, line = line });
    }

    private void LateUpdate()
    {
        foreach (var c in connections)
        {
            bool visible = c.a != null && c.b != null &&
                c.a.gameObject.activeInHierarchy && c.b.gameObject.activeInHierarchy &&
                (!hideWhenNotTracking || hand == null || hand.IsTracking);
            c.line.enabled = visible;
            if (!visible) continue;
            c.line.sharedMaterial = lineMaterial;
            c.line.startColor = c.line.endColor = lineColor;
            c.line.startWidth = c.line.endWidth = lineWidth;
            c.line.sortingLayerName = sortingLayerName;
            c.line.sortingOrder = sortingOrder;
            var a = c.a.position;
            var b = c.b.position;
            a.z = b.z = lineZ;
            c.line.SetPosition(0, a);
            c.line.SetPosition(1, b);
        }
    }

    private void OnDisable()
    {
        foreach (var c in connections)
            if (c.line != null) c.line.enabled = false;
    }
}
