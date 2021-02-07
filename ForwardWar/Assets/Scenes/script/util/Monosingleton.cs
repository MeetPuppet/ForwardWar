using UnityEngine;

public class Monosingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static bool isDestroy = false;

    protected virtual void OnDestroy() => isDestroy = true;

    public static T Get
    {
        get
        {
            if (isDestroy) return null;
            if(instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                if(FindObjectsOfType(typeof(T)).Length > 1)
                {
                    return instance;
                }

                if(instance == null)
                {
                    GameObject go = new GameObject();
                    instance = go.AddComponent<T>();
                    go.name = typeof(T).Name;

                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
}
