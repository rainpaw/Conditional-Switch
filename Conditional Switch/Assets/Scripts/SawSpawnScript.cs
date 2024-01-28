using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PipeSpawnScript : MonoBehaviour
{
    public GameObject saw;
    public GameObject questionMarkBlock;
    public float spawnRate = 2f;
    private float timer = 0f;

    public float heightOffset = 5f;

    public LogicSystemScript logic;

    private void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicSystemScript>();
    }

    private void Update()
    {
        if (logic.isDead == false)
        {
            transform.position = new Vector3(transform.position.x, Random.Range(heightOffset, 0 - heightOffset), transform.position.z);

            spawnRate = Random.Range(1.0f, 8.0f);

            if (timer < spawnRate)
            {
                timer += Time.deltaTime;
            }
            else
            {
                if (Random.Range(1, 20) == 1)
                {
                    Instantiate(questionMarkBlock, transform.position, transform.rotation);
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
