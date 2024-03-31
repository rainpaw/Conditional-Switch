using UnityEngine;

public class SawMovementScript : MonoBehaviour
{
    private float moveSpeed;
    public float rotationSpeed = 480f;

    public LogicSystemScript logic;

    public Collider2D col;

    private void Start()
    {
        moveSpeed = Random.Range(2, 6);
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();

        if (logic.isPresentationMode)
        {
            col.enabled = false;
        }
    }

    private void Update()
    {
        if (logic.isDead == false & logic.isPaused == false & logic.gameHasStarted) {
            transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
            transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, (1 * rotationSpeed) * Time.deltaTime));

            if (transform.position.x < -20)
            {
                Destroy(gameObject);
            }
        }
    }
}
