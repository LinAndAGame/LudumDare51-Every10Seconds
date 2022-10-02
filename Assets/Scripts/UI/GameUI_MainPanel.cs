using TMPro;
using UnityEngine;

namespace DefaultNamespace {
    public class GameUI_MainPanel : MonoBehaviour {
        public TextMeshProUGUI TMP_RemainingTime;
        public TextMeshProUGUI TMP_Points;
        public GameObject      WarringInfoObj;

        private float _RemainingTime;
        public float RemainingTime {
            get => _RemainingTime;
            set {
                _RemainingTime         = Mathf.Max(0, value);
                TMP_RemainingTime.text = _RemainingTime.ToString("F1");
            }
        }
        
        private int _Points;
        public int Points {
            get => _Points;
            set {
                _Points         = value;
                TMP_Points.text = _Points.ToString();
            }
        }

        public void ShowOrHideWarringInfo(bool show) {
            WarringInfoObj.SetActive(show);
        }
    }
}