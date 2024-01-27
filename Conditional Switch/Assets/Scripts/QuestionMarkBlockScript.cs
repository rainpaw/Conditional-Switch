using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionMarkBlockScript : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 200f;

    private void Update()
    {
        transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
        transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, (1 * rotationSpeed) * Time.deltaTime));

        if (transform.position.x < -20)
        {
            Destroy(gameObject);
        }
    }
}
