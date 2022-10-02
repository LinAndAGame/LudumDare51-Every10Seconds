using System;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace {
    public class MapElement : MonoBehaviour,IMouseEnterAndExit {
        public SpriteRenderer     SpriteRendererSelf;
        public MapElementAreaEnum MapElementAreaType = MapElementAreaEnum.NotCheckArea;
        public MapElementState    CurMapElementState = MapElementState.Normal;
        public Vector2Int         Pos;

        public GameItemElement PlacedGameItemElement;

        public bool CanPlaceItem => CurMapElementState == MapElementState.Normal && PlacedGameItemElement == null;

        public void Self_OnMouseEnter() {
            GameManager.I.CurMouseTouchedMapElement = this;
            SpriteRendererSelf.color                = Color.red;
        }

        public void Self_OnMouseExit() {
            RefreshAreaColor();
            GameManager.I.CurMouseTouchedMapElement = null;
        }

        public void PlaceItem(GameItemElement itemElement) {
            PlacedGameItemElement = itemElement;
            CurMapElementState    = MapElementState.Placed;
        }

        [Button]
        private void RefreshAreaColor() {
            switch (MapElementAreaType) {
                case MapElementAreaEnum.NotCheckArea:
                    SpriteRendererSelf.color = Color.white;
                    break;
                case MapElementAreaEnum.CheckArea:
                    SpriteRendererSelf.color = Color.green;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum MapElementAreaEnum {
            NotCheckArea,
            CheckArea,
        }

        public enum MapElementState {
            Normal,     // 正常
            ReadyPlace, // 准备被放置
            Placed,     // 已经放置了
        }
    }
}