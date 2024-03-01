using UnityEngine;

public class Sine : Ease {
    public override float func(float x) {
        return Mathf.Sin(x);
    }

    public override string name() {
        return "Sine";
    }
}


public class Cosine : Ease {
    public override float func(float x) {
        return Mathf.Cos(x);
    }

    public override string name() {
        return "Cosine";
    }
}

