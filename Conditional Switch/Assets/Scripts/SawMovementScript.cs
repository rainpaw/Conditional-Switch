using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SawMovementScript : MonoBehaviour
{
    private float moveSpeed;
    public float rotationSpeed = 480f;

    private void Start()
    {
        moveSpeed = Random.Range(1, 14);
    }

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
