using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralBuilding : MonoBehaviour
{
    [SerializeField] [Range(0, 1.5f)] private float buildDelay = 1f;
    private int currentAmountOfStacks;
    [SerializeField] private int maxAmountOfStacks = 5;

    [SerializeField] private List<GameObject> floorStacks;
    [SerializeField] private List<GameObject> regularStacks;
    [SerializeField] private List<GameObject> roofStacks;
    [SerializeField] [Range(1, 10f)] private float stackHeight = 2f;

    private Vector3 bottomPosition;

    private void Start()
    {
        maxAmountOfStacks = Random.Range(2, 10);
        initializeGeneration();
    }

    private void initializeGeneration()
    {
        generateFloor();
        StartCoroutine(generateMiddleStacks());
    }

    private void generateFloor()
    {
        int randomIndex = Random.Range(0, 3);

        bottomPosition = transform.position + new Vector3(0, stackHeight / 2, 0);
        GameObject stack = Instantiate(floorStacks[randomIndex], bottomPosition, Quaternion.identity);
    }

    private IEnumerator generateMiddleStacks()
    {
        yield return new WaitForSeconds(buildDelay);

        int randomIndex = Random.Range(0, 3);

        GameObject stack = Instantiate(regularStacks[randomIndex],
            bottomPosition + new Vector3(0, stackHeight * currentAmountOfStacks, 0), Quaternion.identity);

        currentAmountOfStacks++;
        if (currentAmountOfStacks < maxAmountOfStacks - 1)
        {
            StartCoroutine(generateMiddleStacks());
            yield return null;
        }
        else generateRoof();
    }

    private void generateRoof()
    {
        int randomIndex = Random.Range(0, 3);

        GameObject stack = Instantiate(roofStacks[randomIndex],
            bottomPosition + new Vector3(0, stackHeight * currentAmountOfStacks, 0), Quaternion.identity);
    }
}