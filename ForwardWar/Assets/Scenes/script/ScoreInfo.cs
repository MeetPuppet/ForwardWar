using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreInfo : Monosingleton<ScoreInfo>
{
    int score = 0;

    public void InfoReset()
    {
        score = 0;
    }

    public void editScore(int num)
    {
        score += num;
    }

    public int Score
    {
        get
        {
            return score;
        }
    }
}
