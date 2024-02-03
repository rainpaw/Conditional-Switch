using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Random = UnityEngine.Random;
using System.Globalization;

public class LogicSystemScript : MonoBehaviour
{
    public GameObject player;
    public Rigidbody2D playerRigidbody;

    public bool isDead = false;
    public bool isPaused = false;
    public int playerScore;
    public TextMeshProUGUI scoreText;

    private double timer;
    private double timerRounded;
    private string stringTimerRounded;
    public TextMeshProUGUI timerText;

    public DeathParticlesScript deathParticles;

    public GameObject gameOverScreen;

    public TextMeshProUGUI statementTextInGame;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI statementTextQuestionView;
    public TextAsset statementsList;
    public TextAsset questionsList;
    public GameObject questionUI;

    private string originalStatement = "[placeholder]";
    Dictionary<int, Dictionary<string, Dictionary<string, string>>> statements = new Dictionary<int, Dictionary<string, Dictionary<string, string>>>();
    Dictionary<string, Dictionary<string, string>> questions = new Dictionary<string, Dictionary<string, string>>();
    int statementIndex = 0;
    private List<int> usedQuestions = new List<int>();
    List<string> questionsAsList = new List<string>();

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        (Dictionary<int, Dictionary<string, Dictionary<string, string>>>, Dictionary<string, Dictionary<string, string>>, List<string>) informationFiles = loadDataFromText();

        statements = informationFiles.Item1;
        questions = informationFiles.Item2;
        questionsAsList = informationFiles.Item3;

        // Get original statement and DO NOT set to in-game text
        statementIndex = Random.Range(1, statements.Keys.Count+1); // Indexes by 1 instead of 0, the way it is in the dict
        originalStatement = statements[statementIndex]["statements"]["original"];
        //statementTextInGame.text = originalStatement;
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
    public void pauseGame()
    {
        isPaused = true;
        playerRigidbody.gravityScale = 0;
        playerRigidbody.velocity = Vector3.zero;
    }
    public void resumeGame(bool afterQuestion)
    {
        if (afterQuestion == true)
        {
            player.transform.position = Vector3.zero;
        } else
        {
            player.transform.position = new Vector3(0, transform.position.y, 0);
        }
        isPaused = false;
        Debug.Log("RESUME");
    }
    public void quitGame()
    {
        Application.Quit();
    }

    public void askQuestion()
    {
        int questionNumber = Random.Range(0, questionsAsList.Count);

        if (!(usedQuestions.Count >= questionsAsList.Count))
        {
            while (usedQuestions.Contains(questionNumber))
            {
                questionNumber = Random.Range(0, questionsAsList.Count);
            }
            usedQuestions.Add(questionNumber);

            questionText.text = questionsAsList[questionNumber];


            string questionType = ""; // isTrue or whatIs
            string questionStatementType = ""; // original, inverse, etc (the statement that the question uses/needs
            foreach (KeyValuePair<string, Dictionary<string, string>> questionItem in questions) // Loops through isTrue and whatIs in dictionary
            {
                foreach (KeyValuePair<string, string> subQuestionItem in questionItem.Value) // Loops through questions in that subthing
                {
                    if (questionsAsList[questionNumber] == subQuestionItem.Value)
                    {
                        questionType = questionItem.Key;
                        questionStatementType = subQuestionItem.Key;
                    }
                }
            }

            if (questionType == "isTrue")
            {
                TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
                statementTextQuestionView.text = cultInfo.ToTitleCase(questionStatementType) + ": " + statements[statementIndex]["statements"][questionStatementType];
            } else
            {
                statementTextQuestionView.text = "Original: " + statements[statementIndex]["statements"]["original"];
            }

            questionUI.SetActive(true);
            pauseGame();
        } else
        {
            throw new Exception("Hey! You hit all of the questions!");
        }
    }

    public (Dictionary<int, Dictionary<string, Dictionary<string, string>>>, Dictionary<string, Dictionary<string, string>>, List<string>) loadDataFromText()
    {
        string statementsTextWithComments = statementsList.text;
        string questionsTextWithComments = questionsList.text;

        List<string> textFiles = new List<string> { statementsTextWithComments, questionsTextWithComments }; // Two text files as strings with \n separator
        List<List<string>> textFilesEdited = editTextFiles(textFiles);

        // Statement-specific
        List<string> numLines = getLinesStartsWith(textFilesEdited[0], "|");
        int numberOfSections = Int32.Parse(numLines[0].Substring(1));
        if (numLines.Count > 1)
        {
            throw new Exception("There is more than one number line, starting with |, in the ConditionalStatementList.");
        }

        textFilesEdited[0] = removeLinesStartWith(textFilesEdited[0], "|");

        Dictionary<int, List<string>> statementsIndexed = createNumberLists(textFilesEdited[0], numberOfSections);

        // At this point you have the dictionary, statements, with the keys of each conditional statement number.

        Dictionary<int, Dictionary<string, string>> statementsContentSorted = getContentFromIndexedLines(statementsIndexed);

        Dictionary<int, Dictionary<string, Dictionary<string, string>>> statements = splitLinesIntoStatementsAndReasons(statementsContentSorted);

        // Questions-specific
        List<string> questionsListOriginal = textFilesEdited[1];
        Dictionary<string, string> isTrueQuestions = new Dictionary<string, string>();
        Dictionary<string, string> whatIsQuestions = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> questions = new Dictionary<string, Dictionary<string, string>>();

        for (int i = 0; i < questionsListOriginal.Count; i++) // Loops through lines in questions list
        {
            switch (questionsListOriginal[i][0])
            {
                case '>':
                    if (questionsListOriginal[i][1] == 'I')
                    {
                        isTrueQuestions.Add("original", questionsListOriginal[i].Substring(1));
                    } else if (questionsListOriginal[i][1] == 'W')
                    {
                        whatIsQuestions.Add("original", questionsListOriginal[i].Substring(1));
                    }
                    break;
                case '~':
                    if (questionsListOriginal[i][1] == 'I')
                    {
                        isTrueQuestions.Add("inverse", questionsListOriginal[i].Substring(1));
                    }
                    else if (questionsListOriginal[i][1] == 'W')
                    {
                        whatIsQuestions.Add("inverse", questionsListOriginal[i].Substring(1));
                    }
                    break;
                case '+':
                    if (questionsListOriginal[i][1] == 'I')
                    {
                        isTrueQuestions.Add("converse", questionsListOriginal[i].Substring(1));
                    }
                    else if (questionsListOriginal[i][1] == 'W')
                    {
                        whatIsQuestions.Add("converse", questionsListOriginal[i].Substring(1));
                    }
                    break;
                case '-':
                    if (questionsListOriginal[i][1] == 'I')
                    {
                        isTrueQuestions.Add("contrapositive", questionsListOriginal[i].Substring(1));
                    }
                    else if (questionsListOriginal[i][1] == 'W')
                    {
                        whatIsQuestions.Add("contrapositive", questionsListOriginal[i].Substring(1));
                    }
                    break;
                case '/':
                    if (questionsListOriginal[i][1] == 'I')
                    {
                        isTrueQuestions.Add("biconditional", questionsListOriginal[i].Substring(1));
                    }
                    else if (questionsListOriginal[i][1] == 'W')
                    {
                        whatIsQuestions.Add("biconditional", questionsListOriginal[i].Substring(1));
                    }
                    break;
            }
        }

        questions.Add("isTrue", isTrueQuestions);
        questions.Add("whatIs", whatIsQuestions);

        List<string> questionsListToReturn = new List<string>();
        for (int j = 0; j < questionsListOriginal.Count; j++)
        {
            questionsListToReturn.Add(questionsListOriginal[j].Substring(1));
        }

        return (statements, questions, questionsListToReturn);
    }

    // Helper methods for loadDataFromText
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
    private List<string> getLinesStartsWith(List<string> lines, string startsWithCharacters)
    {
        List<string> startsWithLines = new List<string>();

        for (int i = 0; i < lines.Count; i++) // Loops through lines, to get value, use lines[i]
        {
            if (lines[i].StartsWith(startsWithCharacters))
            {
                startsWithLines.Add(lines[i]);
            }
        }

        return startsWithLines;
    }
    private List<List<string>> editTextFiles(List<string> textFiles) // Removes comments and new lines, gives an edited list of lines
    {
        List<string> statementsEdited = new List<string>();
        List<string> questionsEdited = new List<string>();

        for (int i = 0; i < textFiles.Count; i++) // Loops through textFiles, to get value, use textFiles[i]
        {
            List<string> linesList = splitStringAtNewline(textFiles[i]);
            List<string> linesListNoComments = removeLinesStartWith(linesList, "#");

            switch (i)
            {
                case 0:
                    statementsEdited = linesListNoComments;
                    break;
                case 1:
                    questionsEdited = linesListNoComments;
                    break;
            }
        }

        List<List<string>> textFilesEdited = new List<List<string>> { statementsEdited, questionsEdited };
        return textFilesEdited;
    }
    private Dictionary<int, List<string>> createNumberLists(List<string> textFile, int numberOfNums)
    {
        Dictionary<int, List<string>> numberedTextFile = new Dictionary<int, List<string>>();

        for (int k = 1; k <= numberOfNums; k++) // loops through possible numbers. from 1 through numberOfNums
        {
            List<string> tempLines = new List<string>();

            for (int j = 0; j < textFile.Count; j++) // Loops through the lines of textFile, to get value, use textFile[j]
            {
                if (textFile[j].StartsWith(k.ToString())) // Checks if the line starts with the number
                {
                    tempLines.Add(textFile[j].Substring(1));
                }
            }
            numberedTextFile.Add(k, tempLines);
        }

        return numberedTextFile;
    }
    private Dictionary<int, Dictionary<string, string>> getContentFromIndexedLines(Dictionary<int, List<string>> statementsIndexed)
    {
        Dictionary<int, Dictionary<string, string>> statements = new Dictionary<int, Dictionary<string, string>>(); // Dictionary<int> = indexes (1, 2, etc), next part: Dictionary<string> = type, so "original", "converse" etc, nextpart: 

        foreach (KeyValuePair<int, List<string>> entry in statementsIndexed) // Loops through the keys (numbers) and their value (List of lines), to get key, use entry.Key
        {
            List<string> linesList = entry.Value;
            Dictionary<string, string> linesByType = new Dictionary<string, string>();

            for (int i = 0; i < linesList.Count; i++) // Loops through linesList, to get value, use linesList[i]
            {
                switch (linesList[i][0])
                {
                    case '=' :
                        linesByType.Add("statements", linesList[i].Substring(1));
                        break;
                    case '>':
                        linesByType.Add("original", linesList[i].Substring(1));
                        break;
                    case '~':
                        linesByType.Add("inverse", linesList[i].Substring(1));
                        break;
                    case '+':
                        linesByType.Add("converse", linesList[i].Substring(1));
                        break;
                    case '-':
                        linesByType.Add("contrapositive", linesList[i].Substring(1));
                        break;
                    case '/':
                        linesByType.Add("biconditional", linesList[i].Substring(1));
                        break;
                    default:
                        throw new Exception("Not all lines in ConditionalStatementList have a type value preceding them");
                }
            }

            statements.Add(entry.Key, linesByType);
        }

        return statements;
    }
    private Dictionary<int, Dictionary<string, Dictionary<string, string>>> splitLinesIntoStatementsAndReasons(Dictionary<int, Dictionary<string, string>> statementsContentSorted) // In type, int is index, first string is statements/reasons, second string is type
    {
        Dictionary<int, Dictionary<string, Dictionary<string, string>>> statements = new Dictionary<int, Dictionary<string, Dictionary<string, string>>>();

        foreach (KeyValuePair<int, Dictionary<string, string>> entry in statementsContentSorted) // Loops through the keys (numbers) and their value (Dictionary of lines with type), to get key, use entry.Key
        {
            Dictionary<string, string> statementsType = entry.Value;
            Dictionary<string, Dictionary<string, string>> linesWithType = new Dictionary<string, Dictionary<string, string>>();

            Dictionary<string, string> statementsDict = new Dictionary<string, string>();
            Dictionary<string, string> reasonsDict = new Dictionary<string, string>();
            Dictionary<string, string> truthDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> lineType in statementsType)
            {
                switch (lineType.Key)
                {
                    case "statements":
                        string lineTypeValue = lineType.Value;

                        int typeCounter = 1;
                        int previousStartIndex = 0;
                        int length = 0;
                        for (int i = 0; i < lineTypeValue.Length; i++) // Loops through lineTypeValue, to get char, use lineTypeValue[i]
                        {
                            if (lineTypeValue[i] == '|')
                            {
                                switch (typeCounter)
                                {
                                    case 1:
                                        statementsDict.Add("original", lineTypeValue.Substring(previousStartIndex, length));
                                        break;
                                    case 2:
                                        statementsDict.Add("inverse", lineTypeValue.Substring(previousStartIndex+1, length-1));
                                        break;
                                    case 3:
                                        statementsDict.Add("converse", lineTypeValue.Substring(previousStartIndex+1, length-1));
                                        break;
                                    case 4:
                                        statementsDict.Add("contrapositive", lineTypeValue.Substring(previousStartIndex+1, length-1));
                                        break;
                                    case 5:
                                        statementsDict.Add("biconditional", lineTypeValue.Substring(previousStartIndex+1, length-1));
                                        break;
                                }
                                previousStartIndex = i;
                                typeCounter++;
                                length = 0;
                            }
                            length++;
                        }

                        break;
                    case "original":
                        reasonsDict.Add("original", lineType.Value.Substring(1));
                        truthDict.Add("original", "T");
                        break;
                    case "inverse":
                        reasonsDict.Add("inverse", lineType.Value.Substring(1));
                        if (lineType.Value[0] == 'T')
                        {
                            truthDict.Add("inverse", "T");
                        } else
                        {
                            truthDict.Add("inverse", "F");
                        }
                        break;
                    case "converse":
                        reasonsDict.Add("converse", lineType.Value.Substring(1));
                        if (lineType.Value[0] == 'T')
                        {
                            truthDict.Add("converse", "T");
                        }
                        else
                        {
                            truthDict.Add("converse", "F");
                        }
                        break;
                    case "contrapositive":
                        if (lineType.Value[0] == 'T')
                        {
                            truthDict.Add("contrapositive", "T");
                        }
                        else
                        {
                            truthDict.Add("contrapositive", "F");
                        }
                        reasonsDict.Add("contrapositive", lineType.Value.Substring(1));
                        break;
                    case "biconditional":
                        if (lineType.Value[0] == 'T')
                        {
                            truthDict.Add("biconditional", "T");
                        }
                        else
                        {
                            truthDict.Add("biconditional", "F");
                        }
                        reasonsDict.Add("biconditional", lineType.Value.Substring(1));
                        break;
                }
            }

            linesWithType.Add("reasons", reasonsDict);
            linesWithType.Add("statements", statementsDict);
            linesWithType.Add("truth", truthDict);

            statements.Add(entry.Key, linesWithType);
        }

        return statements;
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
