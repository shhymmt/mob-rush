using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Transform spawnPoint;

    void Awake()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
}
