using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    Text text;
    int score;

    // Start is called before the first frame update
    void Start()
    {
        score = GameManager.Score.Score;
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        score = GameManager.Score.Score;
        text.text = score.ToString();
    }
}
