using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace {
    public class GameUI_StartPanel : MonoBehaviour {
        public Button BtnStartGame;

        public void Init() {
            BtnStartGame.onClick.AddListener(() => {
                GameManager.I.ItemGeneratorRef.Init();
                GameManager.I.CurGameState = GameManager.GameState.ItemCheck;
                this.gameObject.SetActive(false);
            });
        }
    }
}