using UnityEngine;

public class InOutCubic : Ease {
    public override float func(float x) {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }

    public override string name() {
        return "InOutCubic";
    }
}

public class Linear : Ease {
    public override float func(float x) {
        return x;
    }

    public override string name() {
        return "Linear";
    }
}