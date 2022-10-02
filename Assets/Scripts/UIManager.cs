using Sirenix.Utilities;
using UnityEngine;

namespace DefaultNamespace {
    public class UIManager : MonoBehaviour {
        public GameUI_MainPanel  MainPanelRef;
        public GameUI_StartPanel StartPanelRef;

        public void Init() {
            StartPanelRef.Init();
            GameManager.I.OnMapChanged += CheckWarringInfo;
            GameManager.I.ItemGeneratorRef.OnGenNextAfter += CheckWarringInfo;
        }

        private void CheckWarringInfo() {
            var nextItem                     = GameManager.I.ItemGeneratorRef.NextItem;
            var allCanPlaceNormalMapElements = GameManager.I.AllMapElements.FindAll(data => nextItem.CanPlaceToNotNeedCheckMapElement(data));
            MainPanelRef.ShowOrHideWarringInfo(allCanPlaceNormalMapElements.IsNullOrEmpty());
        }
    }
}