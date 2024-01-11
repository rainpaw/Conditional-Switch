using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PipeSpawnScript : MonoBehaviour
{
    public GameObject saw;
    public float spawnRate = 2f;
    private float timer = 0f;

    public float heightOffset = 5f;

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, Random.Range(heightOffset, 0 - heightOffset), transform.position.z);

        spawnRate = Random.Range(1.0f, 8.0f);

        if (timer < spawnRate)
        {
            timer += Time.deltaTime;
        }
        else
        {
            Instantiate(saw, transform.position, transform.rotation);
            timer = 0;
        }
    }
}
