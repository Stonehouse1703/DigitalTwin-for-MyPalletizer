using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ArticulationBody))]
public class ArticulationJointActuator : MonoBehaviour
{
    public enum DriveAxis { X } // Revolute in Unity ist i.d.R. X-Drive

    [Header("Axis (usually X for Revolute)")]
    public DriveAxis axis = DriveAxis.X;

    [Header("Speed mapping")]
    [Tooltip("Interpret speed like 0..100 -> deg/s via: degPerSec = max(minDegPerSec, speed * speedScale)")]
    public float speedScale = 1.5f;
    public float minDegPerSec = 8f;

    private ArticulationBody _ab;
    private Coroutine _moveCo;

    void Awake()
    {
        _ab = GetComponent<ArticulationBody>();
    }

    public void Stop()
    {
        if (_moveCo != null) StopCoroutine(_moveCo);
        _moveCo = null;
    }

    /// <summary>Reads current joint angle in degrees (best effort for 1-DOF revolute).</summary>
    public float GetCurrentAngleDeg()
    {
        // jointPosition is in radians; for a 1-DOF revolute, [0] is the position along the joint axis.
        float rad = _ab.jointPosition[0];
        return rad * Mathf.Rad2Deg;
    }

    /// <summary>Move to target angle (deg). Clamps automatically to ArticulationDrive limits.</summary>
    public IEnumerator MoveTo(float targetDeg, float speed)
{
    var drive = GetDrive();

    // 1) Aktuelle reale Gelenkposition lesen (nicht drive.target!)
    float currentDeg = GetCurrentAngleDeg();

    // Falls numerical noise / init: current in limits ziehen
    currentDeg = Mathf.Clamp(currentDeg, drive.lowerLimit, drive.upperLimit);

    // 2) Ziel clampen auf Limits aus dem ArticulationBody
    float clampedTarget = Mathf.Clamp(targetDeg, drive.lowerLimit, drive.upperLimit);

    // Optional: damit kein Sprung entsteht, initial drive.target auf aktuelle Position setzen
    drive.target = currentDeg;
    SetDrive(drive);

    // 3) Speed -> deg/s
    float degPerSec = Mathf.Max(minDegPerSec, Mathf.Abs(speed) * speedScale);

    // 4) Monoton Richtung Ziel fahren (ohne Wrap/DeltaAngle)
    const float eps = 0.2f; // Toleranz in Grad (anpassen)
    while (Mathf.Abs(clampedTarget - currentDeg) > eps)
    {
        float step = degPerSec * Time.fixedDeltaTime;
        currentDeg = Mathf.MoveTowards(currentDeg, clampedTarget, step);

        drive = GetDrive();
        drive.target = currentDeg;
        SetDrive(drive);

        yield return new WaitForFixedUpdate();
    }

    // Final snap
    drive = GetDrive();
    drive.target = clampedTarget;
    SetDrive(drive);
}

    /// <summary>Starts movement immediately (non-blocking). Adapter/Queue kann stattdessen IEnumerator nutzen.</summary>
    public void MoveToAsync(float targetDeg, float speed)
    {
        Stop();
        _moveCo = StartCoroutine(MoveTo(targetDeg, speed));
    }

    private ArticulationDrive GetDrive()
    {
        // For revolute, XDrive is typically used.
        return _ab.xDrive;
    }

    private void SetDrive(ArticulationDrive drive)
    {
        _ab.xDrive = drive;
    }
}