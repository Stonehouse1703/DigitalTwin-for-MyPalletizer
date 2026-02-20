using UnityEngine;

public class DemoProgram : MonoBehaviour
{
    public MyPalletizerArticulationAdapter mc;

    void Awake()
    {
        if (mc == null)
            mc = GetComponent<MyPalletizerArticulationAdapter>();

        if (mc == null)
            Debug.LogError("DemoProgram: No MyPalletizerArticulationAdapter found on this GameObject.");
    }

    void Start()
    {
        if (mc == null) return;

        mc.SendAngle(1, 0, 30);
        mc.SendAngle(2, 0, 30);
        mc.SendAngle(3, 0, 30);
        mc.SendAngle(4, 0, 30);
        mc.Wait(1f);
        mc.SendAngle(1, -160, 30);
        mc.SendAngle(2, 0, 30);
        mc.SendAngle(3, 0, 30);
        mc.SendAngle(4, 0, 30);
        mc.Wait(1f);
        mc.SendAngle(1, 160, 30);
        mc.SendAngle(2, 90, 30);
        mc.SendAngle(3, 60, 30);
        mc.SendAngle(4, 0, 30);
    }
}