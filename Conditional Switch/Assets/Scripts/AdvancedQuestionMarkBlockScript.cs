using UnityEngine;

public class AdvancedQuestionMarkBlockScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;

    public LogicSystemScript logic;

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
    }

    private void Update()
    {
        if (logic.isDead == false & logic.isPaused == false & logic.gameHasStarted)
        {
            transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
            transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, (1 * rotationSpeed) * Time.deltaTime));

            if (transform.position.x < -20)
            {
                Destroy(gameObject);
            }
        }
    }
}
