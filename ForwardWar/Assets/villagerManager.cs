using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class villagerManager : Monosingleton<villagerManager>
{
    public Text all;
    public Text left;
    static List<VillagerComp> lv = new List<VillagerComp>();
    static VillagerComp[] gos;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Villager"))
        {
            lv.Add(go.GetComponent<VillagerComp>());
        }
        gos = lv.ToArray();

        all.text = gos.Length.ToString();
        left.text = gos.Length.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        gos = lv.ToArray();
        left.text = gos.Length.ToString();
    }

    public static void VillagerUpdate()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Villager"))
        {
            VillagerComp vc = go.GetComponent<VillagerComp>();
            if(vc.score == false)
                lv.Add(go.GetComponent<VillagerComp>());
        }
        gos = lv.ToArray();
    }
}
