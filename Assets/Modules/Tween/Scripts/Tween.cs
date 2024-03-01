using System;
using UnityEngine;

namespace Modules.Tween.Scripts {
    public class Tween {
        /// 用于检索的 TweenId
        public string tweenId;

        /// 存储当前的值
        private float currentValue;
        /// 起始值和结束值
        private float startValue;
        private float endValue;
        /// 结束值 - 起始值 的 差值
        private float delta;

        /// 单位为秒的持续时间
        private float duration;
        /// Tween 被第一次调用的时间
        private long startTime = 0;

        private Action<float> setter;
        private Action onComplete;

        /// 值的变化曲线
        private Func<float, float> easing;

        private TweenOperator tweenOp;

        public void setOperator(TweenOperator tweenOp) {
            this.tweenOp = tweenOp;
        }

        public Tween() {}

        public Tween setId(string id) {
            this.tweenId = id;
            return this;
        }

        public Tween setStartValue(float startValue) {
            this.startValue = startValue;
            return this;
        }

        public Tween setEndValue(float endValue) {
            this.endValue = endValue;
            return this;
        }

        public Tween setDuration(float duration) {
            this.duration = duration;
            return this;
        }
        public float getDuration() {
            return this.duration;
        }

        public Tween setOnComplete(Action onComplete) {
            this.onComplete = onComplete;
            return this;
        }

        public Tween setSetter(Action<float> setter) {
            this.setter = setter;
            return this;
        }

        public Tween setEasing(Func<float, float> easing) {
            this.easing = easing;
            return this;
        }

        private bool isFinished = false;
        public bool getIsFinished() {
            return isFinished;
        }

        private TweenManager tweenManager = null;
        public void update() {
            if (tweenManager == null)
                tweenManager = GameObject.Find("Tween Manager").GetComponent<TweenManager>();
            tweenManager.tweenUpdateTime += 1;
            if (delta == 0) delta = this.endValue - this.startValue;
            if (startTime == 0) startTime = currentTimeMillis();
            long deltaTime = currentTimeMillis() - startTime;
            float timeInSecond = timeMillisToSecond(deltaTime);
            float timeInOne = timeInSecond / duration;
            if (timeInOne > 1) {
                if (!isFinished) {
                    timeInOne = 1;
                    isFinished = true;
                    tweenOp.noticeTweenFinished();
                } else {
                    return;
                }
            }

            float diffInValue = easing(timeInOne) * delta;
            currentValue = startValue + diffInValue;
            setter?.Invoke(currentValue);

            if (isFinished) onComplete?.Invoke();
        }

        private static long currentTimeMillis() {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

        private static float timeMillisToSecond(long timeMillisDelta) {
            return timeMillisDelta / 1000.0f;
        }
    }

    public class UnsupportedTweenException : Exception {
        public static int DURATION_TOO_LONG = 0;
        public static int DURATION_IS_ZERO = 1;
        public static int WRONG_PARAM_TYPE = 2;
        public static int MISSING_PARAM = 3;
        public static int EASE_TYPE_NOT_FOUND = 4;
        private int type;

        public UnsupportedTweenException(int type) {
            this.type = type;
        }
    }
}