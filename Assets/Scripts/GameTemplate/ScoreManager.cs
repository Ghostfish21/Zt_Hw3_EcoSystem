using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public static class Score {
    public static void setScore(string playerName, int score) {
        PlayerPrefsLinkedMap pplm = new PlayerPrefsLinkedMap("scores");
        pplm.update(playerName, score.ToString());
        TMP_Text text = GameObject.Find("Score Text").GetComponent<TMP_Text>();
        text.text = $"Score: {score}";
    }

    public static int getScore(string playerName) {
        PlayerPrefsLinkedMap pplm = new PlayerPrefsLinkedMap("scores");
        string score = pplm.getOrDefault(playerName, "0");
        return int.Parse(score);
    }

    public static bool hasScore(string playerName) {
        PlayerPrefsLinkedMap pplm = new PlayerPrefsLinkedMap("scores");
        return pplm.hasKey(playerName);
    }

    public static void addScore(string playerName, int score) {
        int currentScore = getScore(playerName);
        setScore(playerName, currentScore + score);
    }
}
