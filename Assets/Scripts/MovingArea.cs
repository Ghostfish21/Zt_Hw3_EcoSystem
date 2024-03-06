using System;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace {
    public class MovingArea : MonoBehaviour {
        private Timer moveTimer;
        public SpriteRenderer possibleArea;

        private void Start() {
            moveTimer = new Timer(30000);
        }

        private void Update() {
            moveTimer.run(() => {
                transform.position = new Vector3(
                    UnityEngine.Random.Range(possibleArea.bounds.min.x, possibleArea.bounds.max.x),
                    UnityEngine.Random.Range(possibleArea.bounds.min.y, possibleArea.bounds.max.y),
                    0
                );
                moveTimer.changeInterval(UnityEngine.Random.Range(20000, 50000));
            });
        }
    }
}