using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DefaultNamespace {
    public class StartScreen : MonoBehaviour {
        public UnityEvent onGameStart;

        private Button startButton;
        private void Start() {
            startButton = GameObject.Find("Start Game Button").GetComponent<Button>();
            startButton.onClick.AddListener(() => {
                onGameStart.Invoke();
            });
        }

        public GameObject randomLayoutPrefab;
        public Transform gameScreenRoot;
        public void initGameScene() {
            GameObject randomLayout = GameObject.Find("Random Layout");
            if (randomLayout != null) {
                foreach (Transform child in randomLayout.transform) {
                    foreach (Transform childchild in child.transform)
                        Destroy(childchild.gameObject);
                    Destroy(child.gameObject);
                }

                Destroy(randomLayout);
            }
            GameObject go = Instantiate(randomLayoutPrefab, gameScreenRoot);
            go.name = "Random Layout";
            
            GameObject bouncingBalls = GameObject.Find("Bouncing Balls");
            foreach (Transform child in bouncingBalls.transform) 
                Destroy(child.gameObject);
        }
    }
}