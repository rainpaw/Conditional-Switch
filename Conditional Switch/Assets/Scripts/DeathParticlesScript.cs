using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathParticlesScript : MonoBehaviour
{
    public Transform player;

    public void runParticles()
    {
        transform.position = player.position;
        gameObject.SetActive(true);
    }
}
