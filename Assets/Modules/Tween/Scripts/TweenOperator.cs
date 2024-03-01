using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Modules.Tween.Scripts {
    public class TweenOperator : MonoBehaviour {

        // Update is called once per frame
        void Update() {
            foreach (Tween value in tweens.Values) {
                value.update();
            }
        }

        /// TweenId 对 Tween
        private readonly Dictionary<string, Tween> tweens = new();

        /// 定义该 TweenOperator 最高接受多少个 子Tween 在里面运行
        [SerializeField] private int capacity = -1;
        private void init(int capacity) {
            this.capacity = capacity;
        }

        [SerializeField] int sizeCount = 0;

        /// 如果当前 tweens.Count 没有超过 capacity 则直接添加到本实例中，并返回本实例
        /// 否则的话，新建一个 TweenOperator，并将Tween添加至它里面，并且将它返回
        public TweenOperator addTween(Tween tween) {
            if (this.capacity == -1) return null;
            if (tweens.Count >= capacity) {
                TweenOperator to = create(capacity);
                to.addTween(tween);
                return to;
            }
            tweens[tween.tweenId] = tween;
            sizeCount += 1;
            tween.setOperator(this);
            return this;
        }

        [SerializeField] int finishCount = 0;
        public void noticeTweenFinished() {
            Interlocked.Increment(ref finishCount);
            if (finishCount >= capacity) {
                Destroy(this.gameObject);
            }
        }

        public static TweenOperator create(int capacity) {
            UnityEngine.Object prefabObj = Resources.Load("Tween/Tween Operator");
            if (prefabObj != null) {
                GameObject prefab = (GameObject) prefabObj;
                GameObject parentObject = GameObject.Find("Tween Manager");
                if (parentObject != null) {
                    GameObject childObject = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, parentObject.transform);
                    childObject.name = "Tween Operator";
                    TweenOperator to = childObject.GetComponent<TweenOperator>();
                    to.init(capacity);
                    return to;
                }
                throw new TweenException(TweenException.TWEEN_MANAGER_NOT_FOUND);
            }
            throw new TweenException(TweenException.TWEEN_OP_PREFAB_NOT_FOUND);
        }
    }

    public class TweenException : Exception {
        public static int TWEEN_OP_PREFAB_NOT_FOUND = 0;
        public static int TWEEN_MANAGER_NOT_FOUND = 0;
        private int type;

        public TweenException(int type) {
            this.type = type;
        }
    }
}