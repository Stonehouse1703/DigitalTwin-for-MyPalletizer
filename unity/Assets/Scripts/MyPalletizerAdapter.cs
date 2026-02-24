using System.Collections;
using UnityEngine;

public class MyPalletizerArticulationAdapter : MonoBehaviour
{
    public ArticulationJointActuator j1;
    public ArticulationJointActuator j2;
    public ArticulationJointActuator j3;
    public ArticulationJointActuator j4;

    public void MoveJointsAsync(float a1, float a2, float a3, float a4, float speed)
    {
        j1.MoveToAsync(a1, speed);
        j2.MoveToAsync(a2, speed);
        j3.MoveToAsync(a3, speed);
        j4.MoveToAsync(a4, speed);
    }

    public IEnumerator MoveJointsSync(float a1, float a2, float a3, float a4, float speed, float epsDeg = 0.5f)
    {
        // 1) Start movement
        MoveJointsAsync(a1, a2, a3, a4, speed);

        // 2) Wait until all reached target (best-effort)
        while (true)
        {
            bool done =
                j1.IsAtTarget(a1, epsDeg) &&
                j2.IsAtTarget(a2, epsDeg) &&
                j3.IsAtTarget(a3, epsDeg) &&
                j4.IsAtTarget(a4, epsDeg);

            if (done) yield break;

            yield return new WaitForFixedUpdate();
        }
    }
}