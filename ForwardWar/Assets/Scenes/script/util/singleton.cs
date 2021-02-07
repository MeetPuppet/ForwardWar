using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T instance = null;
    public static T Get
    {
        get
        {
            if (instance == null)
                instance = new T();

            return instance;
        }
    }
}
