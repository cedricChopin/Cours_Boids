using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBoids : MonoBehaviour
{
    public GameObject prefabBoids;
    [SerializeField]
    private int nbBoids = 10;
    public List<GameObject> allBoids;
    private void Start()
    {

        allBoids = new List<GameObject>();

        for(int i = 0; i < nbBoids; ++i)
        {
            GameObject boid = Instantiate(prefabBoids, Random.insideUnitCircle * i * 0.08f, Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),transform);
            boid.name = "Boid " + i;
            allBoids.Add(boid);
        }
    }
}
