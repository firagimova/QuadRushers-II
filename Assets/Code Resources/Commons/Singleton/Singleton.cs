using UnityEngine;

namespace Resources
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new();

        public static T Instance
        {
            get
            {
                if (_instance) return _instance;

                lock (_lock)
                {
                    if (_instance) return _instance;

                    _instance = FindAnyObjectByType<T>();
                    if (_instance && Application.isPlaying)
                        DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;

            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
            
            name += " (DontDestroyOnLoad)";
        }
    }
}