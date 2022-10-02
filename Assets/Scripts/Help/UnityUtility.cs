using System.Collections.Generic;
using UnityEngine;

namespace Help {
    public static class UnityUtility {
        public static void DestroyAllChildren(this Component origin) {
            for (int i = origin.transform.childCount - 1; i >= 0; i--) {
                UnityEngine.Object.Destroy(origin.transform.GetChild(i).gameObject);
            }
        }
        public static void DestroyAllChildrenImmediate(this Component origin) {
            for (int i = origin.transform.childCount - 1; i >= 0; i--) {
                UnityEngine.Object.DestroyImmediate(origin.transform.GetChild(i).gameObject);
            }
        }

        public static List<T> RandomList<T>(this List<T> origin) {
            System.Random random  = new System.Random();
            List<T>       newList = new List<T>();
            foreach (T item in origin)
            {
                newList.Insert(random.Next(newList.Count), item);
            }
            return newList;
        }

        public static T GetRandomElement<T>(this List<T> origin) {
            return origin[new System.Random().Next(0, origin.Count)];
        }
    }
}