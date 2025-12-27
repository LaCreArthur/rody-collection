using UnityEngine;

namespace UnityReusables.Managers
{
    /// <summary>
    /// SingletonMB's design intentions:
    /// - Singleton is already instantiated in the scene at game start (preferably, but it can be instantiated on-demand).
    /// - The singleton instance is not destroyable between scene changes.
    /// - The instantiated singleton should be active ("FindObjectofType" will not find it if disabled).
    /// - "OnAwake" is called once, either at the singleton MonoBehaviour's "Awake" message call or at the first get of the instance.
    /// - Don't call from a non-main thread.
    /// - "Awake" should not be defined in derived classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMB<T> : MonoBehaviour where T : SingletonMB<T>
    {
        private static T _instance;

        private bool _isAwoken;

        public static T instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Debug.LogError("Cannot call SingletonMB instance in Editor mode.");
                    return null;
                }
#endif

                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        System.Type t = typeof(T);
                        _instance = new GameObject(t.Name, t).GetComponent<T>();
                    }

                    _instance.Init();
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarningFormat(
                    _instance.gameObject,
                    "An instance of \"{0}\" already exists. Destroying duplicate one.",
                    typeof(T).Name);
                Destroy(this);
            }
            else
            {
                _instance = this as T;
                Init();
            }
        }

        private void Init()
        {
            if (_isAwoken)
                return;

            _isAwoken = true;
            DontDestroyOnLoad(transform.root.gameObject);
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }
}