using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class villagerManager : Monosingleton<villagerManager>
{
    public Text all;
    public Text Rescue;
    static string Num;
    static List<VillagerComp> lv = new List<VillagerComp>();
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Villager"))
        {
            lv.Add(go.GetComponent<VillagerComp>());
        }
        Num = "0";
        Rescue.text = Num;
        all.text = lv.Count.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        Rescue.text = Num;
    }

    public static void VillagerRemove(VillagerComp villager)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Villager"))
        {
            VillagerComp vc = go.GetComponent<VillagerComp>();
            if(vc == villager)
            {
                lv.Add(go.GetComponent<VillagerComp>());
                int RescueNum = int.Parse(Num) + 1;
                Num = RescueNum.ToString();
                break;
            }
        }
    }
}
