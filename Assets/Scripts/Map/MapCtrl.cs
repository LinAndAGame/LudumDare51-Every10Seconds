using System;
using System.Collections.Generic;
using System.Linq;
using Help;
using Items;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DefaultNamespace {
    public class MapCtrl : MonoBehaviour {
        public MapElement Prefab;
        public Transform  PrefabParent;

        public Vector2Int MapSize;
        public Vector2    IntervalDistance;

        public List<MapElement>        AllMapElements;
        public List<NeedCheckAreaInfo> AllNeedCheckAreaInfos;

        [Button]
        private void SetMapOnEditor() {
#if UNITY_EDITOR
            PrefabParent.DestroyAllChildrenImmediate();
            AllMapElements.Clear();

            for (int i = 0; i < MapSize.x; i++) {
                for (int j = 0; j < MapSize.y; j++) {
                    var ins = PrefabUtility.InstantiatePrefab(Prefab, PrefabParent) as MapElement;
                    ins.transform.localPosition = new Vector2(IntervalDistance.x * i, IntervalDistance.y * j);
                    ins.Pos                     = new Vector2Int(i, j);
                    AllMapElements.Add(ins);
                }
            }
#endif
        }
        
        [Serializable]
        public class NeedCheckAreaInfo {
            public AreaCheckEnum    AreaCheckType        = AreaCheckEnum.NxN;
            public List<MapElement> NeedCheckMapElements = new List<MapElement>();

            private MapElement _MinPosMapElement;

            private MapElement MinPosMapElement {
                get {
                    if (_MinPosMapElement == null) {
                        foreach (MapElement curItemMap in NeedCheckMapElements) {
                            if (_MinPosMapElement == null || posSamller(curItemMap.Pos,_MinPosMapElement.Pos)) {
                                _MinPosMapElement = curItemMap;
                            }
                        }
                    }

                    return _MinPosMapElement;
                }
            }
            private MapElement _MaxPosMapElement;

            private MapElement MaxPosMapElement {
                get {
                    if (_MaxPosMapElement == null) {
                        foreach (MapElement curItemMap in NeedCheckMapElements) {
                            if (_MaxPosMapElement == null || posBigger(curItemMap.Pos,_MaxPosMapElement.Pos)) {
                                _MaxPosMapElement = curItemMap;
                            }
                        }
                    }

                    return _MaxPosMapElement;
                }
            }

            public bool CanPlaceItemInArea(GameItem gameItem, MapElement moveToMapElement) {
                Vector2Int minPos = MinPosMapElement.Pos;
                Vector2Int maxPos = MaxPosMapElement.Pos;
                var allPosIfPlace = gameItem.GetAllPosIfPlace(moveToMapElement);
                Vector2Int minPosIfPlace = allPosIfPlace[0];
                Vector2Int maxPosIfPlace = allPosIfPlace[0];
                foreach (Vector2Int pos in allPosIfPlace) {
                    minPosIfPlace = new Vector2Int(Mathf.Min(pos.x,minPosIfPlace.x), Mathf.Min(pos.y,minPosIfPlace.y));
                    maxPosIfPlace = new Vector2Int(Mathf.Max(pos.x,maxPosIfPlace.x), Mathf.Max(pos.y,maxPosIfPlace.y));
                }

                if (minPosIfPlace.x < minPos.x || minPosIfPlace.y < minPos.y || maxPosIfPlace.x > maxPos.x || maxPosIfPlace.y > maxPos.y) {
                    return false;
                }

                List<MapElement> canPlaceItemMapAreas = NeedCheckMapElements.FindAll(data => data.CanPlaceItem);
                if (allPosIfPlace.Any(data=>canPlaceItemMapAreas.All(data2=>data2.Pos != data))) {
                    return false;
                }

                return true;
            }

            public void CheckItems() {
                List<MapElement> hasItemMapAreas  = NeedCheckMapElements.FindAll(data => data.CurMapElementState == MapElement.MapElementState.Placed);
                MapElement       minPosMapElement = null;
                MapElement       maxPosMapElement = null;
                foreach (MapElement curItemMap in hasItemMapAreas) {
                    if (minPosMapElement == null || posSamller(curItemMap.Pos,minPosMapElement.Pos)) {
                        minPosMapElement = curItemMap;
                    }

                    if (maxPosMapElement == null || posBigger(curItemMap.Pos,maxPosMapElement.Pos)) {
                        maxPosMapElement = curItemMap;
                    }
                }

                if (minPosMapElement != null && maxPosMapElement != null) {
                    int minMaxWidth  = maxPosMapElement.Pos.x - minPosMapElement.Pos.x + 1;
                    int minMaxHeight = maxPosMapElement.Pos.y - minPosMapElement.Pos.y + 1;
                    if (minMaxWidth <= 1 || minMaxHeight <= 1) {
                        return;
                    }

                    switch (AreaCheckType) {
                        case AreaCheckEnum.NxN:
                            if (minMaxWidth != minMaxHeight) {
                                return;
                            }
                            break;
                        case AreaCheckEnum.NxM:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    List<MapElement> curFindMapElements = new List<MapElement>();
                    for (int i = minPosMapElement.Pos.x; i <= maxPosMapElement.Pos.x; i++) {
                        for (int j = minPosMapElement.Pos.y; j <= maxPosMapElement.Pos.y; j++) {
                            var curFindMapElement = hasItemMapAreas.Find(data => data.Pos == new Vector2Int(i, j));
                            if (curFindMapElement == null) {
                                return;
                            }
                            else {
                                curFindMapElements.Add(curFindMapElement);
                            }
                        }
                    }

                    if (curFindMapElements.Count != (minMaxWidth * minMaxHeight)) {
                        return;
                    }
                    // 清空所有的物品
                    GameData.Points                                += minMaxWidth * minMaxHeight;
                    GameManager.I.UIManagerRef.MainPanelRef.Points =  GameData.Points;
                    foreach (MapElement curFindMapElement in curFindMapElements) {
                        if (curFindMapElement.PlacedGameItemElement != null) {
                            curFindMapElement.PlacedGameItemElement.ItemParent.DestroySelf();
                        }
                    }
                }
            }

            bool posSamller(Vector2Int a, Vector2Int b) {
                return a.x <= b.x && a.y <= b.y;
            }
            bool posBigger(Vector2Int a, Vector2Int b) {
                return a.x >= b.x && a.y >= b.y;
            }

            public enum AreaCheckEnum {
                NxN,    // 正方形
                NxM,    // 长方形
            }
        }
    }
}
