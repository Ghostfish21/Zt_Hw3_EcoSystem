using UnityEngine;

namespace Modules.Tween.Scripts {
    public class TweenManager : MonoBehaviour {
        // 子Tween 寿命 在 一分钟以内的 TweenOperator
        private TweenOperator secondOperator;

        // 子Tween 寿命 在 一到五分钟的 TweenOperator
        private TweenOperator lowMinuteOperator;

        // 子Tween 寿命 在 五到十五分钟的 TweenOperator
        private TweenOperator midMinuteOperator;
    
        // 子Tween 寿命 在 十五分钟到半小时的 TweenOperator
        private TweenOperator highMinuteOperator;

        // 子Tween 寿命 在 半小时到一个小时的 TweenOperator
        private TweenOperator ultraMinuteOperator;

        // 子Tween 寿命 在 一个小时到两个小时的 TweenOperator
        private TweenOperator lowHourOperator;

        // 定义在单个 TweenOperator 中最多存在多少个 Tween
        private readonly int tweenOpCapacity = 30;

        [SerializeField] public int tweenUpdateTime = 0;

        void Awake() {
            secondOperator = TweenOperator.create(tweenOpCapacity);
            lowMinuteOperator = TweenOperator.create(tweenOpCapacity);
            midMinuteOperator = TweenOperator.create(tweenOpCapacity);
            highMinuteOperator = TweenOperator.create(tweenOpCapacity);
            ultraMinuteOperator = TweenOperator.create(tweenOpCapacity);
            lowHourOperator = TweenOperator.create(tweenOpCapacity);
        }

        public void register(Tween tween) {
            float duration = tween.getDuration();
            if (duration == 0f) {
                throw new UnsupportedTweenException(UnsupportedTweenException.DURATION_IS_ZERO);
            } else if (duration < 60f) {
                secondOperator = secondOperator.addTween(tween);
                return;
            } else if (duration < 5*60f) {
                lowMinuteOperator = lowMinuteOperator.addTween(tween);
                return;
            } else if (duration < 15*60f) {
                midMinuteOperator = midMinuteOperator.addTween(tween);
                return;
            } else if (duration < 30*60f) {
                highMinuteOperator = highMinuteOperator.addTween(tween);
                return;
            } else if (duration < 60*60f) {
                ultraMinuteOperator = ultraMinuteOperator.addTween(tween);
                return;
            } else if (duration < 120*60f) {
                lowHourOperator = lowHourOperator.addTween(tween);
                return;
            }
            throw new UnsupportedTweenException(UnsupportedTweenException.DURATION_TOO_LONG);        
        }
    }
}
