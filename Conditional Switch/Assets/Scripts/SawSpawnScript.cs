using UnityEngine;

public class PipeSpawnScript : MonoBehaviour
{
    public GameObject saw;
    public GameObject questionMarkBlock;
    public GameObject advancedQuestionMarkBlock;
    public float spawnRate = 3f;
    private float timer = 0f;

    public float heightOffset = 4f;

    public LogicSystemScript logic;

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
    }

    private void Update()
    {
        if (logic.isDead == false & logic.isPaused == false & logic.gameHasStarted)
        {
            transform.position = new Vector3(transform.position.x, Random.Range(heightOffset, 0 - heightOffset), transform.position.z);

            spawnRate = Random.Range(1.6f, 6.0f);

            if (timer < spawnRate)
            {
                timer += Time.deltaTime;
            }
            else
            {
                if (Random.Range(1, 4) == 1)
                {
                    if (Random.Range(1, 4) == 1)
                    {
                        Instantiate(advancedQuestionMarkBlock, transform.position, transform.rotation);
                    } else
                    {
                        Instantiate(questionMarkBlock, transform.position, transform.rotation);
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
