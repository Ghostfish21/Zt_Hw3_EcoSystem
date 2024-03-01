using UnityEngine;

public class InOutSine : Ease {
    public override float func(float x) {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }

    public override string name() {
        return "InOutSine";
    }
}
