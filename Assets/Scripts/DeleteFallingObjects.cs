using UnityEngine;

public class DeleteFallingObjects : MonoBehaviour
{
    [SerializeField]
    [Tooltip("If a player touches this, should they go back to spawn.")]
    private bool sendToSpawn = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!sendToSpawn)
                return;

            other.GetComponent<Player>().Kill();
        } 
        else if (other.gameObject.CompareTag("Ball")) { 
            other.gameObject.SetActive(false);

            Ball ball = other.GetComponent<Ball>();
            ServerSend.BallActive(ball);
        }
    }
}
