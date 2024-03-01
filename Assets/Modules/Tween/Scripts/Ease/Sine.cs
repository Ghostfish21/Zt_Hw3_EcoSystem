using UnityEngine;

public class ProcessedSine : Ease {
    public override float func(float x) {
        return Mathf.Sin(x*3.1415926f)/4;
    }

    public override string name() {
        return "ProcessedSine";
    }
}

