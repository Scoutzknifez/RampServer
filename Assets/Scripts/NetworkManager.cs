using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

    [Header("Spawn Settings")]
    public Vector3 spawnCenter;
    public Vector2 spawnSize;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(Constants.MAX_PLAYERS, Constants.PORT);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnCenter, new Vector3(spawnSize.x, 2, spawnSize.y));
    }

    public Vector3 getSpawnLocation()
    {
        // TODO Math here for random
        return spawnCenter;
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, getSpawnLocation(), Quaternion.identity).GetComponent<Player>();
    }
}
