using System; // Action이 들어있음
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updater : Monosingleton<Updater>
{
    private readonly List<IEnumerator> IList;

    public Updater()
        => IList = new List<IEnumerator>();


    public static void Add(IEnumerator iEnum)
    => Get.IList.Add(iEnum);
    
    public static void Remove(IEnumerator iEnum)
    => Get.IList.Remove(iEnum);
    
    public static void RemoveAt(int index)
    => Get.IList.RemoveAt(index);
    

    private static IEnumerator UpdateFreeAction(Action action)
    {
        action.Invoke();
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        if (IList.Count == 0)
            return;

        for(int i = 0; i < IList.Count; ++i)
        {
            if(IList[i].MoveNext() == false)
            {
                IList.RemoveAt(i--);
            }
            
        }
    }
}
