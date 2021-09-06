using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class villagerManager : MonoBehaviour
{
    public Text all;
    public Text Rescue;
    public PlayableDirector Timeline;
    public GameObject End;
    public Text EndScore;
    static string Num;
    static List<VillagerComp> lv = new List<VillagerComp>();
    // Start is called before the first frame update
    void Start()
    {
        Timeline.gameObject.SetActive(false);
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Villager"))
        {
            lv.Add(go.GetComponent<VillagerComp>());
        }
        Num = "0";
        Rescue.text = Num;
        all.text = "11";
        End.SetActive(false);
    }

    // Update is called once per frame
    bool isDone = false;
    void Update()
    {
        Rescue.text = Num;

        if (Input.GetKeyDown(KeyCode.M))
        {
            MissionComplete();
        }
        Debug.Log(Timeline.state);
        if(!(Timeline.state == PlayState.Playing) && isDone == true)
        {
            End.SetActive(true);
        }
    }

    public void VillagerRemove(VillagerComp villager)
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Villager"))
        {
            VillagerComp vc = go.GetComponent<VillagerComp>();
            if (vc == villager)
            {
                lv.Remove(vc);
                int RescueNum = int.Parse(Num) + 1;
                Num = RescueNum.ToString();
                break;
            }
        }
        if (int.Parse(Num) == int.Parse(all.text))
            MissionComplete();
    }

    public void MissionComplete()
    {
        isDone = true;
        EndScore.text = GameManager.Score.Score.ToString();
        Timeline.gameObject.SetActive(true);
        Timeline.Play();
    }

    IEnumerator EndUIWait()
    {
        yield break;
    }
}
