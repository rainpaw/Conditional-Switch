using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawMovementScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 480f;

    private void Update()
    {
        transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
        transform.Rotate(new Vector3(transform.rotation.x, transform.rotation.y, (1 * rotationSpeed) * Time.deltaTime));
    }

}
