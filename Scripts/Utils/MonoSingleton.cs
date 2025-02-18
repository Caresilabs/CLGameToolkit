using UnityEngine;

namespace CLGameToolkit
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static bool AllowCreation = false;

        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null && AllowCreation)
                {
                    if (!Application.isPlaying)
                        throw new System.Exception("Trying to create a Singleton in editor mode!");

                    GameObject singleton = new();
                    singleton.AddComponent<T>();
                    singleton.name = $"[Singleton]_{typeof(T)}";
                    Logger.Debug($"Created dynamic singleton {singleton.name}");
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            instance = this as T;
        }
    }

    public abstract class PersistedMonoSingleton<T> : MonoSingleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                DontDestroyOnLoad(this);
        }
    }
}
