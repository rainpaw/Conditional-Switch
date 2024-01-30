using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using System.Text.RegularExpressions;
using System.Linq;

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
    public TextMeshProUGUI questionTextInGame;

    public TextAsset statementsList;
    public TextAsset questionsList;

    private void Start()
    {
        questionTextInGame.text = "Hello";
    }

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

    public void askQuestion()
    {
        Debug.Log("QUESTION");
    }

    [ContextMenu("Load Data From Text")]
    public void loadDataFromText()
    {
        string statementsTextWithComments = statementsList.text;
        string questionsTextWithComments = questionsList.text;
    
        List<string> textFiles = new List<string> { statementsTextWithComments, questionsTextWithComments }; // Two text files as strings with \n separator

        List<string> statementsEdited;
        List<string> questionsEdited;

        // Removes comments and new lines, gives an edited list of lines
        for (int i = 0; i < textFiles.Count; i++) // Loops through textFiles, to get value, use textFiles[i]
        {
            List<string> linesList = splitStringAtNewline(textFiles[i]);
            List<string> linesListNoComments = removeLinesStartWith(linesList, "#");


            switch (i) {
                case 0:
                    statementsEdited = linesListNoComments;
                    break;
                case 1:
                    questionsEdited = linesListNoComments;
                    break;
            }
        }
    }

    private List<string> splitStringAtNewline(String str) // Splits a string into its lines
    {
        string[] lines = str.Split(
            new string[] { "\r\n", "\r", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );
        return lines.ToList();
    }
    private List<string> removeLinesStartWith(List<string> lines, string startsWithCharacters) // Removes the comments from a list of lines (comment: #)
    {
        lines.RemoveAll(line => line.StartsWith(startsWithCharacters));
        return lines;
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
