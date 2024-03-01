using TMPro;
using UnityEngine;

namespace DefaultNamespace {
    public class DefaultClickMethods : MonoBehaviour {
        public void onQuitButtonClick() {
# if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
# else
            Application.Quit();
#endif
        }
        
        public TMP_InputField playerNameInputField;
        public GameObject content;
        public void onStartButtonClick() {
            if (playerNameInputField.text == "") playerNameInputField.text = "Empty";
            if (playerNameInputField != null) {
                PlayerPrefs.SetString("playerName", playerNameInputField.text);
                PlayerPrefs.Save();
            }
            content.SetActive(false);
        }
    }
}