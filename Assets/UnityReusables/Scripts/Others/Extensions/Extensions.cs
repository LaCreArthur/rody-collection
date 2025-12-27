using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityReusables.ScriptableObjects.Events;
using Random = UnityEngine.Random;

namespace UnityReusables.Utils.Extensions
{
    public static class Extensions
    {
        public static T GetRandom<T>(this T[] array) => array[Random.Range(0, array.Length)];

        public static T GetRandom<T>(this List<T> list) => list[Random.Range(0, list.Count)];

        public static KeyValuePair<TKey, Tvalue> GetRandom<TKey, Tvalue>(this Dictionary<TKey, Tvalue> dict) 
            => dict.ElementAt(Random.Range(0, dict.Count));

        public static T Ref<T>(this T o) where T : UnityEngine.Object => o == null ? null : o;

        public static float RandomInside(this Vector2 v) => Random.Range(v.x, v.y);

        public static IEnumerator SetActive(this GameObject gameObject, bool active, float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(active);
        }

        public static List<Transform> GetChildrenByTag(this Transform transform, string tag)
        {
            List<Transform> taggedChildren = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.CompareTag(tag))
                {
                    taggedChildren.Add(child);
                }
            }

            return taggedChildren;
        }

        public static List<Transform> GetChildrenByComponent(this Transform transform, string componentTypeName)
        {
            List<Transform> childrenWithComponent = new List<Transform>();
            // get type from string
            Type type = Assembly.GetExecutingAssembly().GetType(componentTypeName);
            // get components from type
            var components = transform.GetComponentsInChildren(type);
            // get transforms from components
            components.ForEach(c => childrenWithComponent.Add(c.transform));
            return childrenWithComponent;
        }

        public static bool MatchWith(this LayerMask layerMask, int layer) => ((1 << layer) & layerMask) != 0;

        public static void MoveIndex(this ref int index, bool left, int maxIndex)
        {
            int newIndex = (index + (left ? -1 : 1)) % maxIndex;
            if (newIndex < 0) newIndex = maxIndex - 1;
            index = newIndex;
        }
        
        public static SimpleEventListenerComponent AddEventListener(this GameObject gameObject, SimpleEventSO gameEvent, UnityAction callback)
        {
            var listener = gameObject.AddComponent<SimpleEventListenerComponent>();
            listener.AddGameEvent(gameEvent);
            listener.AddCallback(callback);
            return listener;
        }
        
        public static SimpleEventListenerComponent AddEventListener(this GameObject gameObject, SimpleEventSO gameEvent, IEnumerable<UnityAction> callbacks)
        {
            var listener = gameObject.AddComponent<SimpleEventListenerComponent>();
            listener.AddGameEvent(gameEvent);
            foreach (UnityAction callback in callbacks) listener.AddCallback(callback);
            return listener;
        }
        
    }
}