using System;
using Unity.VisualScripting;

namespace DefaultNamespace {
    public class Timer {
        private long lastRunTime = 0L;
        private long interval = 0L;
        
        public Timer(long interval) {
            this.interval = interval;
            lastRunTime = Utility.currentTimeMillis();
        }
        
        public void changeInterval(long interval) {
            this.interval = interval;
        }
        
        private bool isTimeUp() {
            long currentTime = Utility.currentTimeMillis();
            if (currentTime - lastRunTime >= interval) {
                lastRunTime = currentTime;
                return true;
            }
            return false;
        }
        
        public void run(Action runAction) {
            if (isTimeUp()) runAction();
        }
    }
}