using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    int score;

    // Start is called before the first frame update
    void Start()
    {
        score = GameManager.Score.Score;
    }

    // Update is called once per frame
    void Update()
    {
        score = GameManager.Score.Score;
    }
}
