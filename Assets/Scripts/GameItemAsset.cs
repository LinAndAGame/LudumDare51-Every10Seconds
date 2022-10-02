using UnityEngine;

namespace DefaultNamespace {
    [CreateAssetMenu(fileName = "GameItemAsset", menuName = "纯数据资源/GameItemAsset", order = 0)]
    public class GameItemAsset : ScriptableObject {
        private static GameItemAsset _I;

        public static GameItemAsset I {
            get {
                if (_I == null) {
                    _I = Resources.Load<GameItemAsset>("DataAsset/GameItemAsset");
                }

                return _I;
            }
        }
        
        public float MaxSpeed;
        public float SpeedDown;
        public float MinSpeed;
        public Vector3 PreviewScale;
    }
}