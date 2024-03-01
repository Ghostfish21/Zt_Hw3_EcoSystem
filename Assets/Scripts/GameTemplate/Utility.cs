using UnityEngine;

namespace DefaultNamespace {
    public static class Utility {
        public static string playerName => PlayerPrefs.GetString("playerName", "");
    }
}