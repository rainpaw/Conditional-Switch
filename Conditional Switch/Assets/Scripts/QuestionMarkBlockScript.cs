using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionMarkBlockScript : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 200f;

    public LogicSystemScript logic;

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
    }

    private void Update()
    {
        if (logic.isDead == false)
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
