using UnityEngine;

public class Property<T> : MonoBehaviour where T : Component
{
    private T _instance;

    public T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
            }

            return _instance;
        }
    }
}
