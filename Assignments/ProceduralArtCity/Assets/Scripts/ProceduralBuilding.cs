using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public enum BuildingType
{
    House,
    Skyscraper,
    NoPreference
}

public class ProceduralBuilding : MonoBehaviour
{
    [SerializeField] [Range(0, 1.5f)] private float buildDelay = 1f;
    private int currentAmountOfStacks;
    [SerializeField] private int maxAmountOfStacks = 5;

    private List<GameObject> spawnedStacks = new List<GameObject>();
    [SerializeField] private List<GameObject> availableFloorStacks;
    [SerializeField] private List<GameObject> availableMiddleStacks;
    [SerializeField] private List<GameObject> availableRoofStacks;
    public static float StackHeight;

    [SerializeField] private Transform middleStacksParent;

    [Header("House values")] [SerializeField]
    private float houseDistanceFactor;

    [SerializeField] private float houseWeightFactor;

    [Header("Skyscraper values")] [SerializeField]
    private float skyscraperDistanceFactor;

    [SerializeField] private float skyscraperWeightFactor;

    public BuildingType buildingType;
    private Vector3 bottomPosition;
    public float distanceToCentroid;
    public float houseValue;
    public float skyscraperValue;

    [SerializeField] private List<Vector3> billboardSpawnpoints = new List<Vector3>();
    [SerializeField] private List<GameObject> billboardPrefabs = new List<GameObject>();

    private void Start()
    {
        determineBuildingType();
        initializeGeneration();
        handleBillboardSpawning();
    }

    private float calculateDistancerToCenter() => Vector3.Distance(transform.position, NodeEditor.Centroid);

    private float calculateUtilityValue(float pDistanceFactor, float pWeightFactor, float pMinRandomValue = 0,
        float pMaxRandomValue = 0)
    {
        distanceToCentroid = calculateDistancerToCenter();
        float randomValue = Random.Range(pMinRandomValue, pMaxRandomValue);
        float utilityValue = (1 - (distanceToCentroid - 100)) * pDistanceFactor + pWeightFactor + randomValue;

        return utilityValue;
    }

    private void handleBillboardSpawning()
    {
        for (int i = 0; i < availableMiddleStacks.Count; i++)
        {
            Transform spawnpointParent = availableMiddleStacks[i].transform.GetChild(0);

            for (int j = 0; j < spawnpointParent.transform.childCount; j++)
            {
                Vector3 spawnPos = transform.position + spawnpointParent.GetChild(j).position;
                billboardSpawnpoints.Add(spawnPos);
            }
        }

        int chanceToSpawn = 50;

        for (int i = 0; i < billboardSpawnpoints.Count; i++)
        {
            if (Random.value > 0.5)
            {
                Instantiate(billboardPrefabs[0], transform.position + billboardSpawnpoints[i], Quaternion.identity);
            }
        }
    }

    private void determineBuildingType()
    {
        houseValue = calculateUtilityValue(houseDistanceFactor, houseWeightFactor, -30, 50);
        skyscraperValue = calculateUtilityValue(skyscraperDistanceFactor, -10, 100);

        buildingType = houseValue > skyscraperValue ? BuildingType.House : BuildingType.Skyscraper;
    }

    private void initializeGeneration()
    {
        currentAmountOfStacks = 0;

        int maxAmount = 0;
        int minAmount = 0;
        if (buildingType == BuildingType.House)
        {
            minAmount = 1;
            maxAmount = 5;
        }
        else
        {
            minAmount = 10;
            maxAmount = 30;
        }

        maxAmountOfStacks = Random.Range(minAmount, maxAmount);

        generateFloor();
        StartCoroutine(generateMiddleStacks());
    }

    public void RegenerateBuilding()
    {
        for (int i = 0; i < spawnedStacks.Count; i++)
        {
            Destroy(spawnedStacks[i]);
        }

        initializeGeneration();
    }

    private void generateFloor()
    {
        int randomIndex = Random.Range(0, availableFloorStacks.Count - 1);

        bottomPosition = transform.position + new Vector3(0, StackHeight / 2, 0);
        GameObject stack = Instantiate(availableFloorStacks[randomIndex], bottomPosition, Quaternion.identity,
            transform);
        stack.name = "[ FLOOR ]";
        currentAmountOfStacks++;
        spawnedStacks.Add(stack);
    }

    private IEnumerator generateMiddleStacks()
    {
        yield return new WaitForSeconds(buildDelay);

        int randomIndex = Random.Range(0, availableMiddleStacks.Count - 1);

        GameObject stack = Instantiate(availableMiddleStacks[randomIndex],
            bottomPosition + new Vector3(0, StackHeight * currentAmountOfStacks, 0), Quaternion.identity,
            middleStacksParent);
        stack.name = $"[ MIDDLE STACK {currentAmountOfStacks} ]";
        spawnedStacks.Add(stack);

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
        int randomIndex = Random.Range(0, availableRoofStacks.Count - 1);

        GameObject stack = Instantiate(availableRoofStacks[randomIndex],
            bottomPosition + new Vector3(0, StackHeight * currentAmountOfStacks, 0), Quaternion.identity, transform);
        stack.name = "[ ROOF ]";
        spawnedStacks.Add(stack);
    }
}