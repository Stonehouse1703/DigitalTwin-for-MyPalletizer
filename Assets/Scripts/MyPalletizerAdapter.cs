using System.Collections;
using System.Collections.Generic;
using UnityEngine;  

public class MyPalletizerArticulationAdapter : MonoBehaviour
{
    [Header("Assign joints J1..J4 (each has ArticulationJointActuator)")]
    public ArticulationJointActuator j1;
    public ArticulationJointActuator j2;
    public ArticulationJointActuator j3;
    public ArticulationJointActuator j4;

    [Header("Sequencing")]
    public bool useQueue = true;

    private readonly Queue<IEnumerator> _queue = new();
    private Coroutine _runner;

    void Awake()
    {
        if (useQueue)
            _runner = StartCoroutine(RunQueue());
    }

    public void SendAngle(int joint1Based, float angleDeg, float speed)
    {
        var j = GetJoint(joint1Based);
        if (j == null) { Debug.LogWarning($"Invalid joint {joint1Based}"); return; }

        if (!useQueue)
        {
            j.MoveToAsync(angleDeg, speed);
            return;
        }

        _queue.Enqueue(j.MoveTo(angleDeg, speed));
    }

    public void Wait(float seconds)
    {
        if (!useQueue)
        {
            StartCoroutine(WaitRoutine(seconds));
            return;
        }

        _queue.Enqueue(WaitRoutine(seconds));
    }

    public void ClearQueue() => _queue.Clear();

    private IEnumerator RunQueue()
    {
        while (true)
        {
            if (_queue.Count == 0) { yield return null; continue; }
            yield return StartCoroutine(_queue.Dequeue());
        }
    }

    private IEnumerator WaitRoutine(float seconds)
    {
        float t = Mathf.Max(0f, seconds);
        while (t > 0f)
        {
            t -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private ArticulationJointActuator GetJoint(int joint1Based) => joint1Based switch
    {
        1 => j1,
        2 => j2,
        3 => j3,
        4 => j4,
        _ => null
    };
}