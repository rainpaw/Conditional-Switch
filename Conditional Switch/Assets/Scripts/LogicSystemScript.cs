using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogicSystemScript : MonoBehaviour
{
    bool isDead = false;
    public int playerScore;
    public TextMeshProUGUI scoreText;

    private double timer;
    private double timerRounded;
    private string stringTimerRounded;
    public TextMeshProUGUI timerText;

    [ContextMenu("Increase Score")]
    public void addScore()
    {
        playerScore += 1;
        scoreText.text = playerScore.ToString();
    }

    public void Update()
    {
        timer += Time.deltaTime;
        timerRounded = Math.Round(timer * 10.0) * 0.1;
        stringTimerRounded = timerRounded.ToString();

        if (!stringTimerRounded.Contains("."))
        {
            timerText.text = stringTimerRounded + ".0";
        }
        else
        {
            timerText.text = stringTimerRounded;
        }
    }
}
