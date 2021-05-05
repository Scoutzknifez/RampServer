using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The time inbetween each spawn in seconds.")]
    private float spawnRateInSeconds = 0;

    [SerializeField]
    [Tooltip("The random range where we can spawn balls in this area in accordance to the x direction.")]
    private float xVariance = 0;

    [SerializeField]
    [Tooltip("The random range where we can spawn balls in this area in accordance to the z direction .")]
    private float zVariance = 0;

    public float ballScale = 1;

    private void OnDrawGizmos()
    {
        Vector3 cubeSize = new Vector3(xVariance, 0, zVariance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, cubeSize);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnRegularly());
    }

    private IEnumerator SpawnRegularly()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRateInSeconds);

            Vector3 spawnLocation = transform.position + new Vector3(xVariance == 0 ? 0 : Random.Range(-xVariance / 2, xVariance / 2), 0, zVariance == 0 ? 0 : Random.Range(-zVariance / 2, zVariance / 2));

            GameObject ball = ObjectPooler.pooler.GetPooledObject();

            // All transform properties needing to be changed
            ball.transform.position = spawnLocation;
            ball.transform.rotation = transform.rotation;
            ball.transform.localScale = new Vector3(ballScale, ballScale, ballScale);

            // Rigidbody properties
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;


            // Set the ball free
            ball.SetActive(true);
            Ball ballScript = ball.GetComponent<Ball>();
            ServerSend.BallActive(ballScript);
        }
    }
}
