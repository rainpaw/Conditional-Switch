using UnityEngine;

public class SawMovementScript : MonoBehaviour
{
    private float moveSpeed;
    public float rotationSpeed = 480f;

    public LogicSystemScript logic;

    private void Start()
    {
        moveSpeed = Random.Range(2, 6);
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
    }

    private void Update()
    {
        if (logic.isDead == false & logic.isPaused == false) {
            transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
            transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, (1 * rotationSpeed) * Time.deltaTime));

            if (transform.position.x < -20)
            {
                Destroy(gameObject);
            }
        }
    }
}
