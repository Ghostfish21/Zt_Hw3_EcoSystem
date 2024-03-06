using System;
using UnityEngine;

namespace DefaultNamespace {
    public static class Utility {
        public static string playerName => PlayerPrefs.GetString("playerName", "");
        
        public static long currentTimeMillis() {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public static string uuid() {
            return System.Guid.NewGuid().ToString("B").ToUpper();
        }
        
        public static string sceneName() {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }
}