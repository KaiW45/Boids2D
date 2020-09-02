using System.Collections.Generic;
using UnityEngine;

public class BoidHandler : MonoBehaviour
{
    public GameObject BoidPrefab;
    List<Boid> listOfBoids;

    // Start is called before the first frame update
    void Start()
    {
        listOfBoids = new List<Boid>();
        GameObject[] currentBoidObjects = GameObject.FindGameObjectsWithTag("Boid");
        foreach(GameObject boidObject in currentBoidObjects)
        {
            listOfBoids.Add(boidObject.GetComponent<Boid>());
        }

        for (int i = 0; i < 100; i++)
        {
            //generateRandomBoid();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            generateRandomBoid();
        }
    }

    void generateRandomBoid()
    {
        Vector3 randomLocation = new Vector3(Random.Range(-30f, 30f), Random.Range(-15f, 15f));
        Vector3 randomRotation = Vector3.forward * Random.Range(0f, 360f);
        Vector3 randomVelocity = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f));

        GameObject boid = (GameObject)Instantiate(BoidPrefab, randomLocation, Quaternion.Euler(randomRotation));
        Boid boidComponent = boid.GetComponent<Boid>();
        boidComponent.velocity = randomVelocity;
        listOfBoids.Add(boidComponent);

        foreach (Boid boidCharacter in listOfBoids)
        {
            boidCharacter.updateBoidList();

        }
    }
}
