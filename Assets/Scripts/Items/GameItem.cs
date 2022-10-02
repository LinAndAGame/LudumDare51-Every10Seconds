using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Help;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Items {
    public class GameItem : SerializedMonoBehaviour {
        public GameItemElement Prefab;
        public Transform       PrefabParent;
        
        public bool[,]    ItemArea;
        public Vector2Int CenterPos;
        public Vector2    IntervalDistance;
        public float      MoveToErrorRange = 0.2f;

        [ReadOnly]
        public List<GameItemElement> AllItemElements;

        public  ShapeEnum     ShapeType;
        public  GameItemState CurGameItemState = GameItemState.PreviewAsNextItem;
        private MapElement    _MoveToMapElement;

        private float Speed     => Mathf.Max(MinSpeed, MaxSpeed - AllItemElements.Count * SpeedDown);
        public  float MaxSpeed  => GameItemAsset.I.MaxSpeed;
        public  float SpeedDown => GameItemAsset.I.SpeedDown;
        public  float MinSpeed  => GameItemAsset.I.MinSpeed;

        private void Update() {
            if (CurGameItemState == GameItemState.OnMoving) {
                this.transform.position = Vector3.MoveTowards(this.transform.position, _MoveToMapElement.transform.position, Speed);
                if (Vector3.Distance(this.transform.position,_MoveToMapElement.transform.position) <= MoveToErrorRange) {
                    this.transform.position = _MoveToMapElement.transform.position;
                    CurGameItemState        = GameItemState.ReadyToCheck;
                    
                    foreach (GameItemElement gameItemElement in AllItemElements) {
                        gameItemElement.PlaceItemToMapElement();
                    }
                    
                    GameManager.I.OnMapChanged?.Invoke();
                }
            }
        }

        public void SetRandomColor() {
            Color color = UnityEngine.Random.ColorHSV();
            foreach (GameItemElement gameItemElement in AllItemElements) {
                gameItemElement.OriginColor = color;
                gameItemElement.SetHighLight(false);
            }
        }

        public void SetHighLight(bool highLight) {
            foreach (GameItemElement gameItemElement in AllItemElements) {
                gameItemElement.SetHighLight(highLight);
            }
        }

        public void DestroySelf() {
            GameManager.I.AllItems.Remove(this);
            foreach (GameItemElement gameItemElement in AllItemElements) {
                gameItemElement.DestroySelf();
            }
            AllItemElements.Clear();
        }

        public bool CanPlaceToNotNeedCheckMapElement(MapElement placedMapElement) {
            if (placedMapElement.MapElementAreaType != MapElement.MapElementAreaEnum.NotCheckArea || placedMapElement.CanPlaceItem == false) {
                return false;
            }
            // 如果将物品放置在地图的一个元素上，是否能够放置，判断地图上的所有相应坐标上是否已经含有物品了
            var allCanPlacedMapElements = GameManager.I.AllMapElements.FindAll(data => data.MapElementAreaType == MapElement.MapElementAreaEnum.NotCheckArea && data.CanPlaceItem);
            if (CheckAllItemElementInMapRange(placedMapElement,allCanPlacedMapElements) == false) {
                return false;
            }

            return true;
        }

        public void PlaceToNotNeedCheckMapElement(MapElement placedMapElement) {
            this.transform.position = placedMapElement.transform.position;
            var allCanPlacedMapElements = GameManager.I.AllMapElements.FindAll(data => data.MapElementAreaType == MapElement.MapElementAreaEnum.NotCheckArea && data.CanPlaceItem);
            foreach (GameItemElement gameItemElement in AllItemElements) {
                Vector2Int pos        = gameItemElement.PosToCenter + placedMapElement.Pos;
                var        mapElement = allCanPlacedMapElements.Find(data => data.Pos == pos);
                mapElement.CurMapElementState       = MapElement.MapElementState.Normal;
                mapElement.PlacedGameItemElement    = gameItemElement;
                gameItemElement.CurPlacedMapElement = mapElement;
            }
            
            GameManager.I.OnMapChanged?.Invoke();
        }

        public bool CanPlaceToNeedCheckMapElement(MapElement curSelectedMapElement) {
            if (curSelectedMapElement.MapElementAreaType != MapElement.MapElementAreaEnum.CheckArea || curSelectedMapElement.CanPlaceItem == false) {
                return false;
            }
            
            var needCheckAreaInfo = GameManager.I.MapCtrlRef.AllNeedCheckAreaInfos.Find(data => data.NeedCheckMapElements.Contains(curSelectedMapElement));
            if (needCheckAreaInfo == null) {
                Debug.LogError("数据错误，检查区域却没有将其添加到需要添加区域的数据中！");
                return false;
            }

            if (needCheckAreaInfo.CanPlaceItemInArea(this,curSelectedMapElement) == false) {
                return false;
            }

            if (CheckAllItemElementInMapRange(curSelectedMapElement,needCheckAreaInfo.NeedCheckMapElements) == false) {
                return false;
            }

            return true;
        }

        public void PlaceToNeedCheckMapElement(MapElement curSelectedMapElement) {
            _MoveToMapElement                 = curSelectedMapElement;
            CurGameItemState                  = GameItemState.OnMoving;
            // GameManager.I.CurSelectedGameItem = null;
            
            foreach (GameItemElement gameItemElement in AllItemElements) {
                gameItemElement.CurPlacedMapElement.CurMapElementState    = MapElement.MapElementState.Normal;
                gameItemElement.CurPlacedMapElement.PlacedGameItemElement = null;
                Vector2Int placePos            = gameItemElement.PosToCenter + _MoveToMapElement.Pos;
                var        needPlaceMapElement = GameManager.I.AllMapElements.Find(data => data.Pos == placePos);
                gameItemElement.SetReadyPlacedMapElement(needPlaceMapElement);
            }
            
            GameManager.I.OnMapChanged?.Invoke();
        }

        private bool CheckAllItemElementInMapRange(MapElement readyPlaceMapElement, List<MapElement> mapElementsRange) {
            // 检查如果要将物品放置在地图上，相应的坐标是否在指定范围内存在
            foreach (GameItemElement gameItemElement in AllItemElements) {
                Vector2Int pos = gameItemElement.PosToCenter + readyPlaceMapElement.Pos;
                if (mapElementsRange.All(data=>data.Pos != pos)) {
                    return false;
                }
            }

            return true;
        }

        public List<Vector2Int> GetAllPosIfPlace(MapElement mapElement) {
            // 输出所有可能占有的区域坐标
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (GameItemElement gameItemElement in AllItemElements) {
                result.Add(mapElement.Pos + gameItemElement.PosToCenter);
            }

            return result;
        }

        [Button]
        private void GenerateOnEditor() {
#if UNITY_EDITOR
            string path      = AssetDatabase.GetAssetPath(this);
            var    prefabIns = PrefabUtility.LoadPrefabContents(path).GetComponent<GameItem>();
            prefabIns.PrefabParent.DestroyAllChildrenImmediate();
            prefabIns.AllItemElements.Clear();
            
            for (int i = 0; i < prefabIns.ItemArea.GetLength(1); i++) {
                for (int j = 0; j < prefabIns.ItemArea.GetLength(0); j++) {
                    bool hasArea = prefabIns.ItemArea[j,i];
                    if (hasArea) {
                        var ins = PrefabUtility.InstantiatePrefab(prefabIns.Prefab,prefabIns.PrefabParent) as GameItemElement;
                        ins.InitOnEditor(this,new Vector2Int(j - prefabIns.CenterPos.y,prefabIns.CenterPos.x - i));
                        ins.transform.localPosition = new Vector3(ins.PosToCenter.x * prefabIns.IntervalDistance.x, ins.PosToCenter.y * prefabIns.IntervalDistance.y, 0);
                        prefabIns.AllItemElements.Add(ins);
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabIns.gameObject, path);
            PrefabUtility.UnloadPrefabContents(prefabIns.gameObject);
            
#endif
        }

        public enum GameItemState {
            PreviewAsNextItem,  // 作为下一个物品预览
            Normal,
            OnMoving,
            ReadyToCheck,
        }

        public enum ShapeEnum {
            L_左上空,
            L_右上空,
            L_左下空,
            L_右下空,
            
            一列_1个,
            一列_2个_横向,
            一列_2个_纵向,
            一列_3个_横向,
            一列_3个_纵向,
            
            十字型,
            
            正方形_左上空,
            正方形_右上空,
            正方形_左下空,
            正方形_右下空,
            
            土字型_超上,
            土字型_超下,
            土字型_超左,
            土字型_超右,
        }
    }
}