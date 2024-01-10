using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawMovementScript : MonoBehaviour
{
    public float moveSpeed = -0.05f;

    private void Start()
    {
        transform.position = new Vector3(13, transform.position.y, transform.position.z);
    }

    private void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x + moveSpeed, transform.position.y, transform.position.z);
    }

}
