using UnityEngine;

namespace Code.Core
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] protected bool isDontDestroyOnLoad = true;
        
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                
                instance = FindAnyObjectByType<T>();
                
                if (instance != null)
                    return instance;
                
                var obj = new GameObject($"@{typeof(T).Name}");
                instance = obj.AddComponent<T>();

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                
                if (isDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
                Destroy(gameObject);
        }
    }
}