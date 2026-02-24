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

    public float GetCurrentAngleDeg()
    {
        float rad = _ab.jointPosition[0];
        return rad * Mathf.Rad2Deg;
    }

    public IEnumerator MoveTo(float targetDeg, float speed)
    {
        var drive = GetDrive();
        float currentDeg = GetCurrentAngleDeg();
        currentDeg = Mathf.Clamp(currentDeg, drive.lowerLimit, drive.upperLimit);
        float clampedTarget = Mathf.Clamp(targetDeg, drive.lowerLimit, drive.upperLimit);
        drive.target = currentDeg;
        SetDrive(drive);
        float degPerSec = Mathf.Max(minDegPerSec, Mathf.Abs(speed) * speedScale);
        const float eps = 0.2f;
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

    public void MoveToAsync(float targetDeg, float speed)
    {
        Stop();
        _moveCo = StartCoroutine(MoveTo(targetDeg, speed));
    }

    private ArticulationDrive GetDrive()
    {
        return _ab.xDrive;
    }

    private void SetDrive(ArticulationDrive drive)
    {
        _ab.xDrive = drive;
    }
    
    public bool IsAtTarget(float targetDeg, float epsDeg = 0.5f)
    {
        float cur = GetCurrentAngleDeg();
        return Mathf.Abs(targetDeg - cur) <= epsDeg;
    }
}