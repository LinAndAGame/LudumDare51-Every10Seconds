using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DefaultNamespace {
    public class GameUI_SinglePanel_GameEnd : MonoBehaviour {
        public Button          BtnReplay;
        public string          PlaySceneName;
        public TextMeshProUGUI TMP_TotalPoints;

        public void Start() {
            TMP_TotalPoints.text = GameData.Points.ToString();
            BtnReplay.onClick.AddListener(() => {
                SceneManager.LoadScene(PlaySceneName);
            });
        }
    }
}