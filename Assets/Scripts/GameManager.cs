using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace {
    public class GameManager : MonoBehaviour {
        public System.Action OnMapChanged;

        public static GameManager I;

        private void Awake() {
            I = this;
        }

        public string         GameEndSceneName;
        public float          FixedTime = 10;
        public UIManager      UIManagerRef;
        public MapCtrl        MapCtrlRef;
        public ItemGenerator  ItemGeneratorRef;
        public List<GameItem> AllItems;

        [ReadOnly]
        public GameItem CurMouseTouchedGameItem;
        [ReadOnly]
        public GameItem CurSelectedGameItem;
        [ReadOnly]
        public MapElement CurMouseTouchedMapElement;
        [ReadOnly]
        public MapElement CurSelectedMapElement;

        public List<MapElement> AllMapElements => MapCtrlRef.AllMapElements;

        private float _RemainingTime;

        private float RemainingTime {
            get => _RemainingTime;
            set {
                _RemainingTime                          = value;
                UIManagerRef.MainPanelRef.RemainingTime = _RemainingTime;
            }
        }

        private GameState _CurGameState;

        [ShowInInspector]
        public GameState CurGameState {
            get => _CurGameState;
            set {
                _CurGameState = value;
                switch (_CurGameState) {
                    case GameState.PlayerOperating:
                        RemainingTime = FixedTime;
                        break;
                    case GameState.ItemCheck:
                        CheckItemArea();
                        ItemGeneratorRef.ReduceIntervalTime();
                        CurGameState = GameState.PlayerOperating;
                        break;
                    case GameState.ReadyStart:
                        break;
                    case GameState.GameEnd:
                        SceneManager.LoadScene(GameEndSceneName);
                        break;
                }
            }
        }

        private void Start() {
            GameData.Points = 0;
            UIManagerRef.Init();
            AllItems.Clear();
            CurGameState = GameState.ReadyStart;
        }

        private void Update() {
            if (_CurGameState == GameState.PlayerOperating) {
                if (Input.GetKey(KeyCode.Mouse0)) {
                    if (CurMouseTouchedGameItem != null && CurMouseTouchedGameItem.CurGameItemState == GameItem.GameItemState.Normal) {
                        CurSelectedGameItem = CurMouseTouchedGameItem;
                    }

                    if (CurMouseTouchedMapElement != null && CurMouseTouchedMapElement.CurMapElementState == MapElement.MapElementState.Normal) {
                        CurSelectedMapElement = CurMouseTouchedMapElement;
                        if (CurSelectedGameItem != null && CurSelectedGameItem.CanPlaceToNeedCheckMapElement(CurSelectedMapElement)) {
                            CurSelectedGameItem.PlaceToNeedCheckMapElement(CurSelectedMapElement);
                            CurSelectedGameItem   = null;
                            CurSelectedMapElement = null;
                        }
                    }
                }

                RemainingTime -= Time.deltaTime;
                if (RemainingTime <= 0) {
                    CurGameState = GameState.ItemCheck;
                }
            }
        }

        public bool IsContainItemElement(Transform target) {
            List<Transform> allItemElements = new List<Transform>();
            foreach (GameItem gameItem in AllItems) {
                allItemElements.AddRange(gameItem.AllItemElements.Select(data => data.transform));
            }

            return allItemElements.Find(data => data == target) != null;
        }

        public bool IsContainMapElement(Transform target) {
            return AllMapElements.Select(data => data.transform).ToList().Find(data => data == target) != null;
        }

        private void CheckItemArea() {
            // 检查产品区域中满足条件的产品并进行销毁
            foreach (MapCtrl.NeedCheckAreaInfo needCheckAreaInfo in MapCtrlRef.AllNeedCheckAreaInfos) {
                needCheckAreaInfo.CheckItems();
            }
        }

        public enum GameState {
            ReadyStart,
            PlayerOperating, // 玩家操作阶段
            ItemCheck,       // 物品检查阶段
            GameEnd,
        }
    }
}