using UnityEngine;

public class MoveWithJoint : MonoBehaviour
{
    public PoseReceiver body;
    public Joint joint = Joint.RightWrist;
    [Tooltip("Width and height of the movement area in Unity units")]
    public Vector2 movementSize = new Vector2(10f, 6f);
    public Vector2 center = Vector2.zero;
    [Tooltip("Higher = faster response. 0 = no smoothing.")]
    public float smoothing = 15f;

    private void Update()
    {
        if (body == null || !body.TryGetJoint(joint, out Vector2 point)) return;
        Vector3 target = new Vector3(
            center.x + (point.x - 0.5f) * movementSize.x,
            center.y + (point.y - 0.5f) * movementSize.y,
            transform.position.z
        );
        float amount = smoothing <= 0 ? 1f : 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target, amount);
    }
}
