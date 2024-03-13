using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Random = UnityEngine.Random;
using System.Globalization;
using Unity.VisualScripting;

public class LogicSystemScript : MonoBehaviour
{
    private GameObject player;
    private Rigidbody2D playerRigidbody;
    private Vector3 previousVelocity;
    private float previousGravScale;

    public TextMeshProUGUI countdownText;
    public bool gameHasStarted = false;
    private bool displayCountdown = true;
    private double countdownTimer;

    public GameObject option1Button;
    public GameObject option2Button;
    public GameObject option3Button;
    public GameObject option4Button;
    private GameObject correctButton;

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
    public GameObject pauseMenu;

    public TextMeshProUGUI statementTextInGame;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI statementTextQuestionView;
    public TextMeshProUGUI isCorrectText;
    public TextMeshProUGUI explanationText;
    public TextAsset statementsList;
    public TextAsset advancedStatementsList;
    public TextAsset questionsList;
    public TextAsset whatIsExplanationsList;
    public TextAsset advQuestionsListText;
    public TextAsset advExplanationsText;
    public GameObject questionUI;
    public GameObject continueButton;

    private string questionType; // isTrue or whatIs
    private string questionStatementType;

    private string originalStatement = "[placeholder]";
    Dictionary<int, Dictionary<string, Dictionary<string, string>>> statements = new Dictionary<int, Dictionary<string, Dictionary<string, string>>>();
    Dictionary<string, Dictionary<string, string>> questions = new Dictionary<string, Dictionary<string, string>>();
    int statementIndex = 0;
    private List<int> usedQuestions = new List<int>();
    private List<int> usedAdvQuestions = new List<int>();
    List<string> questionsAsList = new List<string>();
    Dictionary<string, string> whatIsExplanations = new Dictionary<string, string>();
    Dictionary<int, Dictionary<string, string>> advStatements = new Dictionary<int, Dictionary<string, string>>();
    Dictionary<string,string> advQuestions = new Dictionary<string, string>();
    List<string> advQuestionsAsList = new List<string>();
    Dictionary<string, string> advExplanations = new Dictionary<string, string>();

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        (Dictionary<int, Dictionary<string, Dictionary<string, string>>>, Dictionary<string, Dictionary<string, string>>, List<string>, Dictionary<string, string>, Dictionary<int, Dictionary<string, string>>, Dictionary<string, string>, List<string>, Dictionary<string, string>) informationFiles = loadDataFromText();

        statements = informationFiles.Item1;
        questions = informationFiles.Item2;
        questionsAsList = informationFiles.Item3;
        whatIsExplanations = informationFiles.Item4;
        advStatements = informationFiles.Item5;
        advQuestions = informationFiles.Item6;
        advQuestionsAsList = informationFiles.Item7;
        advExplanations = informationFiles.Item8;

        // Get original statement and set to in-game text
        statementIndex = Random.Range(1, statements.Keys.Count+1); // Indexes by 1 instead of 0, the way it is in the dict
        originalStatement = statements[statementIndex]["statements"]["original"];
        statementTextInGame.text = originalStatement;

        gameHasStarted = false;
        displayCountdown = true;
        countdownText.text = "3";
    }

    [ContextMenu("Increase Score")]
    public void addScore(int amount)
    {
        playerScore += amount;
        scoreText.text = "Score: " + playerScore.ToString();
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
        gameHasStarted = false;
        previousGravScale = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0;
        previousVelocity = playerRigidbody.velocity;
        playerRigidbody.velocity = Vector3.zero;

        pauseMenu.SetActive(true);
    }
    public void resumeGame(bool afterQuestion)
    {
        if (afterQuestion == true)
        {
            player.transform.position = new Vector3(-3, 2, 0);
            playerRigidbody.velocity = Vector3.zero;

            GameObject[] saws = GameObject.FindGameObjectsWithTag("Saw");
            GameObject[] questions = GameObject.FindGameObjectsWithTag("QuestionMarkBlock");
            GameObject[] advancedQuestions = GameObject.FindGameObjectsWithTag("AdvancedQuestionMarkBlock");
            GameObject[] objectsToDestroy = saws.Concat(questions).Concat(advancedQuestions).ToArray();
            foreach (GameObject obj in objectsToDestroy)
            {
                Destroy(obj);
            }

            questionUI.SetActive(false);

            gameHasStarted = false;
            displayCountdown = true;
            countdownTimer = 0;
        } else
        {
            player.transform.position = new Vector3(-3, transform.position.y, 0);
            playerRigidbody.gravityScale = previousGravScale;
            playerRigidbody.velocity = previousVelocity;

            gameHasStarted = true;

            pauseMenu.SetActive(false);
        }
        isPaused = false;
    }
    public void quitGame()
    {
        Application.Quit();
    }

    public void askQuestion(bool advanced)
    {
        int questionNumber;

        if (advanced)
        {
            questionNumber = Random.Range(0, advQuestionsAsList.Count);
            usedAdvQuestions.Add(questionNumber);

            questionText.text = advQuestionsAsList[questionNumber];

            foreach (KeyValuePair<string, string> advQuestionItem in advQuestions) // Loops through ponens and tollens in dict
            {
                if (advQuestionsAsList[questionNumber] == advQuestionItem.Value)
                {
                    questionType = null; // there is no question type in the advanced questions
                    questionStatementType = advQuestionItem.Key; // ponens or tollens (the statement that the question uses/needs
                }
            }

            statementTextQuestionView.text = "Original: " + statements[statementIndex]["statements"]["original"]; // show original statement on screen so player can create the modus
        } else
        {
            questionNumber = Random.Range(0, questionsAsList.Count);
            usedQuestions.Add(questionNumber);

            questionText.text = questionsAsList[questionNumber];

            foreach (KeyValuePair<string, Dictionary<string, string>> questionItem in questions) // Loops through isTrue and whatIs in dictionary
            {
                foreach (KeyValuePair<string, string> subQuestionItem in questionItem.Value) // Loops through questions in that subthing
                {
                    if (questionsAsList[questionNumber] == subQuestionItem.Value)
                    {
                        questionType = questionItem.Key; // isTrue or whatIs
                        questionStatementType = subQuestionItem.Key; // original, inverse, etc (the statement that the question uses/needs
                    }
                }
            }

            if (questionType == "isTrue")
            {
                TextInfo cultInfo = new CultureInfo("en-US", false).TextInfo;
                statementTextQuestionView.text = cultInfo.ToTitleCase(questionStatementType) + ": " + statements[statementIndex]["statements"][questionStatementType];
            }
            else
            {
                statementTextQuestionView.text = "Original: " + statements[statementIndex]["statements"]["original"];
            }
        }

        // Set options
        TextMeshProUGUI option1Text = option1Button.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI option2Text = option2Button.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI option3Text = option3Button.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI option4Text = option4Button.GetComponentInChildren<TextMeshProUGUI>();

        option1Button.SetActive(true);
        option2Button.SetActive(true);
        option3Button.SetActive(true);
        option4Button.SetActive(true);

        if (advanced)
        {
            option1Text.fontSize = 40;
            option2Text.fontSize = 40;

            List<string> advRandomOptions = new List<string> { "ponens", "tollens" };
            string advOptionToUse;

            advOptionToUse = advRandomOptions[Random.Range(0, advRandomOptions.Count)];
            option1Text.text = advStatements[statementIndex][advOptionToUse];
            advRandomOptions.Remove(advOptionToUse);

            advOptionToUse = advRandomOptions[Random.Range(0, advRandomOptions.Count)];
            option2Text.text = advStatements[statementIndex][advOptionToUse];
            advRandomOptions.Remove(advOptionToUse);

            option3Button.SetActive(false);
            option4Button.SetActive(false);

            List<TextMeshProUGUI> advOptionTexts = new List<TextMeshProUGUI> { option1Text, option2Text };

            for (int i = 0; i < advOptionTexts.Count; i++) // Loops through advOptionTexts, to get value, use advOptionTexts[i]
            {
                if (advOptionTexts[i].text == advStatements[statementIndex][questionStatementType])
                {
                    correctButton = advOptionTexts[i].gameObject.transform.parent.gameObject;
                }
            }
        } else
        {
            if (questionType == "isTrue")
            {
                option1Text.text = "True";
                option2Text.text = "False";
                option3Button.SetActive(false);
                option4Button.SetActive(false);

                if (statements[statementIndex]["truth"][questionStatementType] == "T")
                {
                    correctButton = option1Button;
                }
                else
                {
                    correctButton = option2Button;
                }
            }
            else
            {
                // Set buttons

                List<string> randomOptions = new List<string> { "original", "inverse", "converse", "contrapositive", "biconditional" };
                string optionToUse;

                optionToUse = randomOptions[Random.Range(0, randomOptions.Count)];
                option1Text.text = statements[statementIndex]["statements"][optionToUse];
                randomOptions.Remove(optionToUse);

                optionToUse = randomOptions[Random.Range(0, randomOptions.Count)];
                option2Text.text = statements[statementIndex]["statements"][optionToUse];
                randomOptions.Remove(optionToUse);

                optionToUse = randomOptions[Random.Range(0, randomOptions.Count)];
                option3Text.text = statements[statementIndex]["statements"][optionToUse];
                randomOptions.Remove(optionToUse);

                optionToUse = randomOptions[Random.Range(0, randomOptions.Count)];
                option4Text.text = statements[statementIndex]["statements"][optionToUse];
                randomOptions.Remove(optionToUse);

                if (randomOptions.Contains(questionStatementType)) // Checks if correct answer is the one left behind
                {
                    List<TextMeshProUGUI> optionButtons = new List<TextMeshProUGUI> { option1Text, option2Text, option3Text, option4Text };

                    optionButtons[Random.Range(0, optionButtons.Count)].text = statements[statementIndex]["statements"][questionStatementType]; // Adds correct answer to button group
                }

                // Determine which is the correct button
                List<TextMeshProUGUI> optionTexts = new List<TextMeshProUGUI> { option1Text, option2Text, option3Text, option4Text };

                for (int i = 0; i < optionTexts.Count; i++) // Loops through optionTexts, to get value, use optionTexts[i]
                {
                    if (optionTexts[i].text == statements[statementIndex]["statements"][questionStatementType])
                    {
                        correctButton = optionTexts[i].gameObject.transform.parent.gameObject;
                    }
                }
            }
        }

        if (usedQuestions.Count >= questionsAsList.Count & usedAdvQuestions.Count >= advQuestionsAsList.Count)
        {
            // Regular statements
            List<int> usableIndices = statements.Keys.ToList();
            usableIndices.Remove(statementIndex); // Remove the current statement index from the list (so you don't get the same statement twice)
            statementIndex = Random.Range(1, usableIndices.Count + 1); // Indexes by 1 instead of 0, the way it is in the dict

            originalStatement = statements[statementIndex]["statements"]["original"];
            statementTextInGame.text = originalStatement;

            usedQuestions.Clear();
            usedAdvQuestions.Clear();

            player.GetComponent<PlayerMoveScript>().regularQuestionNumber = 0;
            player.GetComponent<PlayerMoveScript>().advQuestionNumber = 0;
        }

        questionText.gameObject.SetActive(true);
        statementTextQuestionView.gameObject.SetActive(true);

        isCorrectText.gameObject.SetActive(false);
        explanationText.gameObject.SetActive(false);
        continueButton.SetActive(false);

        questionUI.SetActive(true);
        pauseGame();
    }

    public void onQuestionButtonClick(GameObject button)
    {
        option1Button.SetActive(false);
        option2Button.SetActive(false);
        option3Button.SetActive(false);
        option4Button.SetActive(false);

        option1Button.GetComponentInChildren<TextMeshProUGUI>().fontSize = 50;
        option2Button.GetComponentInChildren<TextMeshProUGUI>().fontSize = 50;

        questionText.gameObject.SetActive(false);
        statementTextQuestionView.gameObject.SetActive(false);

        isCorrectText.gameObject.SetActive(true);
        explanationText.gameObject.SetActive(true);
        continueButton.SetActive(true);

        if (button == correctButton)
        {
            isCorrectText.text = "Correct!";
            if (questionType == null) // If question is advanced
            {
                addScore(2);
            } else
            {
                addScore(1);
            }
        }
        else
        {
            isCorrectText.text = "Wrong";
        }

        if (questionType == "isTrue")
        {
            explanationText.text = "Explanation: " + statements[statementIndex]["reasons"][questionStatementType];
        }
        else if (questionType == "whatIs")
        {
            explanationText.text = "Explanation: " + whatIsExplanations[questionStatementType];
        } else if (questionType == null)
        {
            explanationText.text = "Explanation: " + advExplanations[questionStatementType];
        }
    }

    [ContextMenu("Load Data")]
    public (Dictionary<int, Dictionary<string, Dictionary<string, string>>>, Dictionary<string, Dictionary<string, string>>, List<string>, Dictionary<string, string>, Dictionary<int, Dictionary<string, string>>, Dictionary<string, string>, List<string>, Dictionary<string, string>) loadDataFromText()
    {
        string statementsTextWithComments = statementsList.text;
        string advStatementstextWithComments = advancedStatementsList.text;
        string questionsTextWithComments = questionsList.text;
        string whatIsExplanationsTextWithComments = whatIsExplanationsList.text;
        string advQuestionsTextWithComments = advQuestionsListText.text;
        string advExplanationsTextWithComments = advExplanationsText.text;

        List<string> textFiles = new List<string> { statementsTextWithComments, questionsTextWithComments, whatIsExplanationsTextWithComments, advStatementstextWithComments, advQuestionsTextWithComments, advExplanationsTextWithComments }; // Two text files as strings with \n separator
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

        // At this point you have the dictionary, statementsIndexed, with the keys of each conditional statement number.

        Dictionary<int, Dictionary<string, string>> statementsContentSorted = getContentFromIndexedLines(statementsIndexed);

        Dictionary<int, Dictionary<string, Dictionary<string, string>>> statements = splitLinesIntoStatementsAndReasons(statementsContentSorted);

        // Questions-specific
        List<string> questionsListOriginal = textFilesEdited[1];
        Dictionary<string, string> isTrueQuestions = new Dictionary<string, string>();
        Dictionary<string, string> whatIsQuestions = new Dictionary<string, string>();
        Dictionary<string, Dictionary<string, string>> questions = new Dictionary<string, Dictionary<string, string>>(); // isTrue/whatIs = first dict key, original, inverse, etc = sub dict key, question = sub dict value

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

        // What is explanations-specific
        List<string> whatIsOriginal = textFilesEdited[2];
        Dictionary<string, string> whatIsExplanations = new Dictionary<string, string>();

        for (int k = 0; k < whatIsOriginal.Count; k++) // Loops through whatIsOriginal lines, to get value, use whatIsOriginal[k]
        {
            switch (whatIsOriginal[k][0])
            {
                case '~':
                    whatIsExplanations.Add("inverse", whatIsOriginal[k].Substring(1));
                    break;
                case '+':
                    whatIsExplanations.Add("converse", whatIsOriginal[k].Substring(1));
                    break;
                case '-':
                    whatIsExplanations.Add("contrapositive", whatIsOriginal[k].Substring(1));
                    break;
                case '/':
                    whatIsExplanations.Add("biconditional", whatIsOriginal[k].Substring(1));
                    break;
            }
        }

        // Advanced statements-specific (very similar to statements)

        List<string> advNumLines = getLinesStartsWith(textFilesEdited[3], "|");
        int advNumberOfSections = Int32.Parse(advNumLines[0].Substring(1));
        if (advNumLines.Count > 1)
        {
            throw new Exception("There is more than one number line, starting with |, in the AdvConditionalStatementList.");
        }

        textFilesEdited[3] = removeLinesStartWith(textFilesEdited[3], "|");
        Dictionary<int, List<string>> advStatementsIndexed = createNumberLists(textFilesEdited[3], advNumberOfSections);

        // At this point you have the dictionary, advStatementsIndexed, with the keys of each conditional statement number.

        Dictionary<int, Dictionary<string, string>> advancedStatements = getContentFromAdvIndexedLines(advStatementsIndexed);

        // Advanced Questions List Specific

        List<string> advQuestionsListOriginal = textFilesEdited[4];
        Dictionary<string, string> advQuestionsDict = new Dictionary<string, string>(); // ponens/tollens = key, question = value

        for (int l = 0; l < advQuestionsListOriginal.Count; l++) // Loops through lines in questions list
        {
            switch (advQuestionsListOriginal[l][0])
            {
                case '<':
                    advQuestionsDict.Add("ponens", advQuestionsListOriginal[l].Substring(1));
                    break;
                case '_':
                    advQuestionsDict.Add("tollens", advQuestionsListOriginal[l].Substring(1));
                    break;
            }
        }

        List<string> advQuestionsList = new List<string>();
        for (int m = 0; m < advQuestionsListOriginal.Count; m++)
        {
            advQuestionsList.Add(advQuestionsListOriginal[m].Substring(1));
        }

        // Advanced Explanations Specific

        List<string> advExplanationsOriginal = textFilesEdited[5];
        Dictionary<string, string> advExplanationsDict = new Dictionary<string, string>();

        for (int n = 0; n < advExplanationsOriginal.Count; n++) // Loops through advExplanationsOriginal lines, to get value, use advExplanationsOriginal[n]
        {
            switch (advExplanationsOriginal[n][0])
            {
                case '<':
                    advExplanationsDict.Add("ponens", advExplanationsOriginal[n].Substring(1));
                    break;
                case '_':
                    advExplanationsDict.Add("tollens", advExplanationsOriginal[n].Substring(1));
                    break;
                default:
                    throw new Exception("Not all lines in AdvExplanations have a type value preceding them!");
            }
        }

        return (statements, questions, questionsListToReturn, whatIsExplanations, advancedStatements, advQuestionsDict, advQuestionsList, advExplanationsDict);
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
        List<string> whatIsEdited = new List<string>();
        List<string> advStatementsEdited = new List<string>();
        List<string> advQuestionsEdited = new List<string>();
        List<string> advExplanationsEdited = new List<string>();

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
                case 2:
                    whatIsEdited = linesListNoComments;
                    break;
                case 3:
                    advStatementsEdited = linesListNoComments;
                    break;
                case 4:
                    advQuestionsEdited = linesListNoComments;
                    break;
                case 5:
                    advExplanationsEdited = linesListNoComments;
                    break;
            }
        }

        List<List<string>> textFilesEdited = new List<List<string>> { statementsEdited, questionsEdited, whatIsEdited, advStatementsEdited, advQuestionsEdited, advExplanationsEdited };
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
        Dictionary<int, Dictionary<string, string>> statements = new Dictionary<int, Dictionary<string, string>>(); // Dictionary<int> = indexes (1, 2, etc), next part: Dictionary<string> = type, so "statements", "original", "converse" etc, nextpart: line without the type indicator, so just "" and the T/F value

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
    private Dictionary<int, Dictionary<string, string>> getContentFromAdvIndexedLines(Dictionary<int, List<string>> advStatementsIndexed)
    {
        Dictionary<int, Dictionary<string, string>> advStatements = new Dictionary<int, Dictionary<string, string>>(); // Dictionary<int> = indexes (1, 2, etc), next part: Dictionary<string> = type, so "statements", "original", "converse" etc, nextpart: line without the type indicator, so just "" and the T/F value

        foreach (KeyValuePair<int, List<string>> entry in advStatementsIndexed) // Loops through the keys (numbers) and their value (List of lines), to get key, use entry.Key
        {
            List<string> linesList = entry.Value; // List of lines under the index we are looping through
            Dictionary<string, string> sortedLines = new Dictionary<string, string>(); // Dict to store each line as it is sorted by ponens/tollens

            for (int i = 0; i < linesList.Count; i++) // Loops through linesList (list of lines under index), to get value, use linesList[i]
            {
                string currentLine = linesList[i];

                switch (currentLine[0]) // switch first character in the line we're working with
                {
                    case '=': // if that char is an = (statements)
                        string currentLineSubstring = linesList[i].Substring(1);

                        int typeCounter = 1;
                        int previousStartIndex = 0;
                        int length = 0;
                        for (int j = 0; j < currentLineSubstring.Length; j++) // Loops through this line, to get char, use currentLine[j]
                        {
                            if (currentLineSubstring[j] == '|')
                            {
                                switch (typeCounter)
                                {
                                    case 1:
                                        sortedLines.Add("ponens", currentLineSubstring.Substring(previousStartIndex, length));
                                        break;
                                    case 2:
                                        sortedLines.Add("tollens", currentLineSubstring.Substring(previousStartIndex + 1, length - 1));
                                        break;
                                }
                                previousStartIndex = j;
                                typeCounter++;
                                length = 0;
                            }
                            length++;
                        }

                        break;
                    default:
                        throw new Exception("Not all lines in ConditionalStatementList have a type value preceding them");
                }
            }

            advStatements.Add(entry.Key, sortedLines);
        }

        return advStatements;
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
                        reasonsDict.Add("original", lineType.Value.Substring(1).Replace("\"", ""));
                        truthDict.Add("original", "T");
                        break;
                    case "inverse":
                        reasonsDict.Add("inverse", lineType.Value.Substring(1).Replace("\"", ""));
                        if (lineType.Value[0] == 'T')
                        {
                            truthDict.Add("inverse", "T");
                        } else
                        {
                            truthDict.Add("inverse", "F");
                        }
                        break;
                    case "converse":
                        reasonsDict.Add("converse", lineType.Value.Substring(1).Replace("\"", ""));
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
                        reasonsDict.Add("contrapositive", lineType.Value.Substring(1).Replace("\"", ""));
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
                        reasonsDict.Add("biconditional", lineType.Value.Substring(1).Replace("\"", ""));
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
        if (displayCountdown == true)
        {
            countdownText.gameObject.SetActive(true);

            countdownTimer += Time.deltaTime;
            if (countdownTimer < 1)
            {
                countdownText.text = "3";
            }
            else if (countdownTimer > 1 & countdownTimer < 2)
            {
                countdownText.text = "2";
            }
            else if (countdownTimer > 2 & countdownTimer < 3)
            {
                countdownText.text = "1";
            }
            else if (countdownTimer > 3 & countdownTimer < 3.5)
            {
                countdownText.text = "Go!";
                gameHasStarted = true;
                if (playerRigidbody.gravityScale == 0)
                {
                    playerRigidbody.gravityScale = player.GetComponent<PlayerMoveScript>().gravityScale;
                }
            }
            else if (countdownTimer > 3.5)
            {
                countdownText.gameObject.SetActive(false);
                displayCountdown = false;
            }
        }
        if (gameHasStarted)
        {
            if (isDead == false & isPaused == false)
            {
                timer += Time.deltaTime;
                timerRounded = Math.Round(timer * 10.0) * 0.1;
                stringTimerRounded = timerRounded.ToString();

                if (!stringTimerRounded.Contains("."))
                {
                    timerText.text = "Time: " + stringTimerRounded + ".0";
                }
                else
                {
                    timerText.text = "Time: " + stringTimerRounded;
                }
            }
        }
    }
}
