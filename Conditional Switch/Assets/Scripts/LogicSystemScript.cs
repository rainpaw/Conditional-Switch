using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogicSystemScript : MonoBehaviour
{
    public bool isDead = false;
    public int playerScore;
    public TextMeshProUGUI scoreText;

    private double timer;
    private double timerRounded;
    private string stringTimerRounded;
    public TextMeshProUGUI timerText;

    public DeathParticlesScript deathParticles;

    public GameObject gameOverScreen;

    [ContextMenu("Increase Score")]
    public void addScore()
    {
        playerScore += 1;
        scoreText.text = playerScore.ToString();
    }

    public void gameOver()
    {
        deathParticles.runParticles();
        gameOverScreen.SetActive(true);
    }

    public void resetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void quitGame()
    {
        Application.Quit();
    }

    public void Update()
    {
        if (isDead == false)
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
}
