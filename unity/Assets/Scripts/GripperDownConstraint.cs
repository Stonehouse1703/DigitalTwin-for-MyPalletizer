using UnityEngine;

public class KeepToolXAxisDown : MonoBehaviour
{
    public Transform toolVisual;      // nur Visual!
    public Transform toolFrame;       // Achsen-Referenz (optional)
    public Transform referenceFrame;  // z.B. RobotBase, sonst World
    [Range(0f,1f)] public float strength = 1f;

    void LateUpdate()
    {
        if (!toolVisual) return;
        if (!toolFrame) toolFrame = toolVisual;

        // Zielrichtung für Tool-X
        Vector3 down = referenceFrame ? -referenceFrame.up : Vector3.down;

        // Zweite Achse: nutze "forward" als Stabilisierung gegen beliebiges Rollen
        // Wir projizieren toolFrame.forward auf die Ebene orthogonal zu 'down'
        Vector3 fwd = toolFrame.forward;
        Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, down);
        if (fwdOnPlane.sqrMagnitude < 1e-6f)
        {
            // fallback wenn forward fast parallel zu down
            fwdOnPlane = Vector3.ProjectOnPlane(toolFrame.up, down);
        }
        fwdOnPlane.Normalize();

        // Wir bauen eine Rotation, bei der:
        // - X nach down zeigt
        // - Z so nah wie möglich an fwdOnPlane bleibt
        // Unity: LookRotation(forward, up) definiert Z=forward, Y=up.
        // Wir wollen X=down → darum konstruieren wir zuerst Basisachsen.
        Vector3 x = down.normalized;
        Vector3 z = fwdOnPlane;
        Vector3 y = Vector3.Cross(z, x).normalized;   // orthogonal
        z = Vector3.Cross(x, y).normalized;           // re-orthogonalize

        Quaternion target = Quaternion.LookRotation(z, y);

        toolVisual.rotation = (strength >= 0.999f)
            ? target
            : Quaternion.Slerp(toolVisual.rotation, target, strength);
    }
}