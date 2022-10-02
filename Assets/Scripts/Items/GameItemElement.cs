using DefaultNamespace;
using UnityEngine;

namespace Items {
    public class GameItemElement : MonoBehaviour,IMouseEnterAndExit {
        public GameObject     DestroyEffect;
        public SpriteRenderer SpriteRendererSelf;
        public GameItem       ItemParent;
        public Vector2Int     PosToCenter;

        public MapElement ReadyPlacedToMapElement;
        public MapElement CurPlacedMapElement;

        public Color OriginColor;
        
        public void SetHighLight(bool highLight) {
            SpriteRendererSelf.color = highLight ? Color.blue : OriginColor;
        }

        public void DestroySelf() {
            CurPlacedMapElement.CurMapElementState    = MapElement.MapElementState.Normal;
            CurPlacedMapElement.PlacedGameItemElement = null;
            var destroyEffectIns = Instantiate(DestroyEffect);
            destroyEffectIns.transform.position = this.transform.position;
            Destroy(this.gameObject);
        }

        public void SetReadyPlacedMapElement(MapElement mapElement) {
            ReadyPlacedToMapElement                    = mapElement;
            ReadyPlacedToMapElement.CurMapElementState = MapElement.MapElementState.ReadyPlace;
        }

        public void PlaceItemToMapElement() {
            ReadyPlacedToMapElement.PlaceItem(this);
            CurPlacedMapElement = ReadyPlacedToMapElement;
        }

        public void Self_OnMouseEnter() {
            if (ItemParent.CurGameItemState != GameItem.GameItemState.Normal) {
                return;
            }
            GameManager.I.CurMouseTouchedGameItem = this.ItemParent;
            ItemParent.SetHighLight(true);
        }

        public void Self_OnMouseExit() {
            if (ItemParent.CurGameItemState != GameItem.GameItemState.Normal) {
                return;
            }
            ItemParent.SetHighLight(false);
            GameManager.I.CurMouseTouchedGameItem = null;
        }

        public void InitOnEditor(GameItem itemParent, Vector2Int posToCenter) {
            ItemParent  = itemParent;
            PosToCenter = posToCenter;
        }
    }
}