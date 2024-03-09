using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class PipeSpawnScript : MonoBehaviour
{
    public GameObject saw;
    public GameObject questionMarkBlock;
    public GameObject advancedQuestionMarkBlock;
    public float spawnRate = 3f;
    private float timer = 0f;

    public float heightOffset = 4f;

    public LogicSystemScript logic;
    public PlayerMoveScript playerScript;

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMoveScript>();
    }

    private void Update()
    {
        if (logic.isDead == false & logic.isPaused == false & logic.gameHasStarted)
        {
            transform.position = new Vector3(transform.position.x, Random.Range(heightOffset, 0 - heightOffset), transform.position.z);

            spawnRate = Random.Range(2.0f, 6.5f);

            if (timer < spawnRate)
            {
                timer += Time.deltaTime;
            }
            else
            {
                if (Random.Range(1, 6) == 1)
                {
                    if (Random.Range(1, 4) == 1 & playerScript.advQuestionNumber < 2) // 2 questions: what is for modus ponens and tollens
                    {
                        Instantiate(advancedQuestionMarkBlock, transform.position, transform.rotation);
                    } else if (playerScript.regularQuestionNumber < 9) // 9 questions in questionsList
                    {
                        Instantiate(questionMarkBlock, transform.position, transform.rotation);
                    } else
                    {
                        throw new Exception("Bug in LogicSystemScript: all of the questions have been used but there isn't a changed statement!");
                    }
                }
                else
                {
                    Instantiate(saw, transform.position, transform.rotation);
                }
                timer = 0;
            }
        }
    }
}
