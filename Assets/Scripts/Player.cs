using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform cameraTransform;
    public float maxHealth = 1;
    public float health = 1;

    public float gravity = -9.81f;
    public float moveSpeed = 3;
    public float sprintMultiplier = 1.7f;
    public float jumpSpeed = 5;
    
    private bool[] inputs;
    private float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;

        inputs = new bool[6];
    }

    public void FixedUpdate()
    {
        if (playerIsDead())
        {
            return;
        }

        Vector2 inputDirection = Vector2.zero;
        if (inputs[0])
        {
            inputDirection.y += 1;
        }
        if (inputs[1])
        {
            inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            inputDirection.x += 1;
        }

        Move(inputDirection);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.gameObject.CompareTag("Ball"))
        {
            return;
        }

        // We are hitting a ball
        GameObject ball = hit.collider.gameObject;
        ball.GetComponent<Ball>().resetPlayerAndBall(gameObject);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= moveSpeed;

        if (inputs[5])
        {
            moveDirection *= sprintMultiplier;
        }

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public bool playerIsDead()
    {
        return health <= 0;
    }

    public void TakeDamage(float damage)
    {
        // Do not hurt again if dead already
        if (playerIsDead())
        {
            return;
        }

        health -= damage;
        if (playerIsDead())
        {
            Kill();
        }

        ServerSend.PlayerHealth(this);
    }

    public void SetHealth(float _health)
    {
        health = _health;
        ServerSend.PlayerHealth(this);
    }

    public void Kill()
    {
        health = 0f;
        controller.enabled = false;

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(3f);

        SetHealth(maxHealth);
        transform.position = NetworkManager.instance.getSpawnLocation();
        ServerSend.PlayerPosition(this);

        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }
}
