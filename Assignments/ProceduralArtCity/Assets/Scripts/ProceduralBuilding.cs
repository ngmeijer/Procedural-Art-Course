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
    [SerializeField] private int maxAmountOfStacks = 5;
    [SerializeField] private List<GameObject> availableFloorStacks;
    [SerializeField] private List<GameObject> availableMiddleStacks;
    [SerializeField] private List<GameObject> availableRoofStacks;
    [SerializeField] private Transform middleStacksParent;

    [Header("House values")] [SerializeField]
    private float houseDistanceFactor;

    [SerializeField] private float houseWeightFactor;
    [SerializeField] private float minRandomValue_House = -10;
    [SerializeField] private float maxRandomValue_House = 70;


    [Header("Skyscraper values")] [SerializeField]
    private float skyscraperDistanceFactor;

    [SerializeField] private float skyscraperWeightFactor;
    [SerializeField] private float minRandomValue_Skyscraper = -30;
    [SerializeField] private float maxRandomValue_Skyscraper = 100;

    [SerializeField] private List<Vector3> billboardSpawnpoints = new List<Vector3>();

    [SerializeField] private List<GameObject> billboardPrefabs = new List<GameObject>();

    public BuildingType buildingType;
    public float distanceToCentroid;
    public float houseValue;
    public float skyscraperValue;
    public Vector3 size;

    public static bool EnableBillboards;
    public static float StackHeight = 1;

    private Vector3 bottomPosition;
    private float maxDistanceToCenter;
    private int currentAmountOfStacks;
    private List<GameObject> spawnedStacks = new List<GameObject>();

    private void Start()
    {
        maxDistanceToCenter = calculateDistanceCenterToCorner();
        determineBuildingType();
        initializeGeneration();
    }

    private float calculateDistancerToCenter() => Vector3.Distance(transform.position, NodeEditor.Centroid);
    private float calculateDistanceCenterToCorner() => Vector3.Distance(NodeEditor.Centroid, NodeEditor.TopLeftCorner);

    private float calculateUtilityValue(float pDistanceFactor, float pWeightFactor, float pMinRandomValue = 0,
        float pMaxRandomValue = 0)
    {
        distanceToCentroid = calculateDistancerToCenter();
        float randomValue = Random.Range(pMinRandomValue, pMaxRandomValue);
        float utilityValue = (1 - (distanceToCentroid - maxDistanceToCenter)) * pDistanceFactor + pWeightFactor +
                             randomValue;

        return utilityValue;
    }

    private void handleBillboardSpawning(Transform pCurrentStack, int pCurrentIndex)
    {
        Transform spawnpointParent = pCurrentStack.GetChild(0);
        int spawnpointCount = spawnpointParent.childCount;
        int randomAmount = Random.Range(0, spawnpointCount);
        List<Vector3> spawnpoints = new List<Vector3>();
        for (int i = 0; i < spawnpointCount; i++)
        {
            Vector3 offset = spawnpointParent.GetChild(i).localPosition;
            offset.y += StackHeight * pCurrentIndex;
            Vector3 spawnpoint = transform.position + offset;
            spawnpoints.Add(spawnpoint);
        }

        for (int i = 0; i < randomAmount; i++)
        {
            if (Random.value > 0.5)
            {
                GameObject billboard =
                    Instantiate(billboardPrefabs[0], spawnpoints[i], Quaternion.identity, pCurrentStack);
                Vector3 offset = spawnpoints[i] - transform.position;
                if (offset.x < 0 || offset.x > 0)
                {
                    billboard.transform.Rotate(new Vector3(0, 1, 0), 90f);
                }
            }
        }
    }

    private void determineBuildingType()
    {
        houseValue = calculateUtilityValue(houseDistanceFactor, houseWeightFactor, minRandomValue_House,
            maxRandomValue_House);
        skyscraperValue = calculateUtilityValue(skyscraperDistanceFactor, skyscraperWeightFactor,
            minRandomValue_Skyscraper, maxRandomValue_Skyscraper);

        buildingType = houseValue > skyscraperValue ? BuildingType.House : BuildingType.Skyscraper;
    }

    private void initializeGeneration()
    {
        currentAmountOfStacks = 0;

        int maxAmount, minAmount;
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

        size = new Vector3(spawnedStacks[0].transform.lossyScale.x + 1, StackHeight * maxAmountOfStacks + (3 * StackHeight),
            spawnedStacks[0].transform.lossyScale.z + 1);
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

        if (EnableBillboards) handleBillboardSpawning(stack.transform, currentAmountOfStacks);

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var spawnpoint in billboardSpawnpoints)
        {
            Gizmos.DrawSphere(spawnpoint, 0.5f);
        }
    }
}