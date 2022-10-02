using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;

namespace DefaultNamespace {
    public class MouseCtrl : MonoBehaviour {
        private List<IMouseEnterAndExit> _LastMouseEnterAndExits = new List<IMouseEnterAndExit>();

        private void Update() {
            foreach (IMouseEnterAndExit lastMouseEnterAndExit in _LastMouseEnterAndExits) {
                lastMouseEnterAndExit.Self_OnMouseExit();
            }
            _LastMouseEnterAndExits.Clear();
            
            RaycastHit2D[] results = new RaycastHit2D[20];
            var            count   = Physics2D.RaycastNonAlloc(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, results);

            List<Transform> curRaycastTrans = results.ToList().GetRange(0, count).Select(data=>data.transform).ToList();
            var             gameItem        = curRaycastTrans.ToList().Find(data => GameManager.I.IsContainItemElement(data))?.GetComponent<GameItemElement>();
            var             mapElement      = curRaycastTrans.ToList().Find(data => GameManager.I.IsContainMapElement(data))?.GetComponent<MapElement>();

            if (gameItem != null && gameItem.ItemParent.CurGameItemState == GameItem.GameItemState.Normal) {
                _LastMouseEnterAndExits.Add(gameItem);
                
            }
            else if (mapElement != null && mapElement.CurMapElementState == MapElement.MapElementState.Normal) {
                _LastMouseEnterAndExits.Add(mapElement);
            }
            
            foreach (IMouseEnterAndExit lastMouseEnterAndExit in _LastMouseEnterAndExits) {
                lastMouseEnterAndExit.Self_OnMouseEnter();
            }
        }
    }
}