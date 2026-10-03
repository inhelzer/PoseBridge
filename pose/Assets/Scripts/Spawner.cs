using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public float dlay;
    float lastSpawnTime;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instantiate(objectToSpawn, transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad - lastSpawnTime > dlay)
        {
            Instantiate(objectToSpawn, transform.position, transform.rotation);
            lastSpawnTime = Time.timeSinceLevelLoad;
        }
    }
}
