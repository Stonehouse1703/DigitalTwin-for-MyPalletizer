using UnityEngine;

public class GripperVisualXAxisDownCompensation : MonoBehaviour
{
    [Header("What to rotate (visual only, NOT the articulation link)")]
    public Transform toolVisual;     // z.B. Graber/ToolVisual

    [Header("Which transform defines tool axes? (usually same as toolVisual or a ToolFrame child)")]
    public Transform toolFrame;      // z.B. Graber/ToolVisual/ToolFrame (preferred)

    [Header("Down reference (optional)")]
    public Transform referenceFrame; // z.B. MainBody/RobotBase; if null => world down

    [Range(0f, 1f)]
    public float strength = 1f;      // 1 = hard lock, 0.2 = softer

    void LateUpdate()
    {
        if (toolVisual == null) return;
        if (toolFrame == null) toolFrame = toolVisual;

        // Desired down direction
        Vector3 down = referenceFrame ? -referenceFrame.up : Vector3.down;

        // Current tool X axis in world (local X = right)
        Vector3 xAxisWorld = toolFrame.right;

        // Minimal correction to align xAxisWorld -> down
        Quaternion correction = Quaternion.FromToRotation(xAxisWorld, down);

        // Apply correction to the VISUAL only (keeps articulation physics intact)
        Quaternion targetWorldRot = correction * toolVisual.rotation;

        toolVisual.rotation = (strength >= 0.999f)
            ? targetWorldRot
            : Quaternion.Slerp(toolVisual.rotation, targetWorldRot, strength);
    }
}