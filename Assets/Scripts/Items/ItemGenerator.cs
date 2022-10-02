using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Help;
using Sirenix.Utilities;
using UnityEngine;

namespace Items {
    public class ItemGenerator : MonoBehaviour {
        public event System.Action OnGenNextAfter;
        
        public List<GameItem> AllGameItemPrefabs;
        public Transform      InsParent;
        public Transform      NextItemParent;
        public GameItem       NextItem;

        public float MinTime;
        public float DefaultTime;
        public float ReduceTime;
        
        private float _IntervalTime;

        public void Init() {
            _IntervalTime = DefaultTime;
            GenItem();
            StartCoroutine(LoopGen());
        }

        private IEnumerator LoopGen() {
            while (true) {
                yield return new WaitForSeconds(_IntervalTime);
                GenItem();
            }
        }

        public void ReduceIntervalTime() {
            _IntervalTime = Mathf.Max(MinTime, _IntervalTime - ReduceTime);
        }

        private GameItem GenItem() {
            if (NextItem == null) {
                GenNext();
            }
            
            GameItem result = NextItem;
            var allCanPlaceNormalMapElements = GameManager.I.AllMapElements.FindAll(data => result.CanPlaceToNotNeedCheckMapElement(data));
            if (allCanPlaceNormalMapElements.IsNullOrEmpty()) {
                // 游戏结束！
                GameManager.I.CurGameState = GameManager.GameState.GameEnd;
                return null;
            }
            else {
                // 选择一个随机的可放置地方进行放置

                result.SetRandomColor();
                result.transform.SetParent(InsParent);
                result.transform.localScale = Vector3.one;
                result.CurGameItemState     = GameItem.GameItemState.Normal;
                GameManager.I.AllItems.Add(result);

                result.PlaceToNotNeedCheckMapElement(allCanPlaceNormalMapElements.GetRandomElement());
                GenNext();
                return result;
            }
        }

        private void GenNext() {
            var usedGameItemPrefabs = new List<GameItem>(AllGameItemPrefabs);
            if (NextItem != null) {
                // 确保相同的形状不会连续出现
                var nextItemPrefab = usedGameItemPrefabs.Find(data => data.ShapeType == NextItem.ShapeType);
                usedGameItemPrefabs.Remove(nextItemPrefab);
            }
            NextItem                         = Instantiate(usedGameItemPrefabs.GetRandomElement(), NextItemParent);
            NextItem.CurGameItemState        = GameItem.GameItemState.PreviewAsNextItem;
            NextItem.transform.localPosition = Vector3.zero;
            NextItem.transform.localScale    = GameItemAsset.I.PreviewScale;
            OnGenNextAfter?.Invoke();
        }
    }
}