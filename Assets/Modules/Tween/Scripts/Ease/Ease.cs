using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ease {
    private static Dictionary<string, Ease> eases = new();
    public static Ease getEase(string easeName) {
        if (eases.ContainsKey(easeName)) return eases[easeName];
        return null;
    }
    
    public static Ease InOutCubic = new InOutCubic();
    public static Ease InOutSine = new InOutSine();
    public static Ease ProcessedSine = new ProcessedSine();
    public static Ease Sine = new Sine();
    public static Ease Cosine = new Cosine();
    public static Ease Linear = new Linear();

    public Ease() {
        Ease.eases[name()] = this;
    }
    public abstract float func(float x);

    public abstract string name();
}