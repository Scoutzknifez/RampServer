using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Dictionary<int, Ball> balls = new Dictionary<int, Ball>();
    public static int nextId = 1;

    public int id;

    private void Awake()
    {
        id = nextId++;
        balls.Add(id, this);
        ServerSend.BallSpawn(this);
    }

    private void FixedUpdate()
    {
        ServerSend.BallRoll(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            resetPlayerAndBall(collision.gameObject);
        }
    }

    public void resetPlayerAndBall(GameObject player)
    {
        player.GetComponent<Player>().Kill();

        // TODO: Make sure client gets explosion
        //Instantiate(explosionParticle, transform.position, transform.rotation);
        ServerSend.BallCollided(transform.position);
        gameObject.SetActive(false);
        ServerSend.BallActive(this);
    }

    public void toggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public bool isActive()
    {
        return gameObject.activeSelf;
    }
}
