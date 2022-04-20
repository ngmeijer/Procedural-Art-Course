using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
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
    public static float BuildDelay;
    public static float StackHeight = 1;
    [SerializeField] private List<GameObject> availableFloorStacks;
    [SerializeField] private List<GameObject> availableMiddleStacks;
    [SerializeField] private List<GameObject> availableRoofStacks;
    [SerializeField] private Transform middleStacksParent;
    [SerializeField] private Transform temporaryStacksParent;
    [SerializeField] private Transform editedStacksParent;
    [SerializeField] private List<GameObject> billboardPrefabs = new List<GameObject>();
    [SerializeField] private int minAmountOfStacks;
    [SerializeField] private int maxAmountOfStacks;
    [SerializeField] private bool enableBillboards;

    public BuildingType buildingType;
    public float utilityValueHouse;
    public float utilityValueSkyscraper;
    public Vector3 size;


    private List<GameObject> tempFloorStacks = new();
    private List<GameObject> tempMiddleStacks = new();
    private List<GameObject> tempRoofStacks = new();
    private int finalMaxStackCount;
    private bool isGenerating;
    private Vector3 bottomPosition;
    private GameObject tempStack;
    private int lastTempStackIndex;
    private int currentAmountOfStacks;
    private List<Vector3> billboardSpawnpoints = new List<Vector3>();
    public List<GameObject> spawnedStacks = new List<GameObject>();
    public int floorStacksCount;
    public int middleStacksCount;
    public int roofStacksCount;

    private void Start()
    {
        floorStacksCount = availableFloorStacks.Count;
        middleStacksCount = availableMiddleStacks.Count;
        roofStacksCount = availableRoofStacks.Count;

        createTemporaryStacks(availableFloorStacks, tempFloorStacks);
        createTemporaryStacks(availableMiddleStacks, tempMiddleStacks);
        createTemporaryStacks(availableRoofStacks, tempRoofStacks);

        initializeGeneration();
    }

    private void createTemporaryStacks(List<GameObject> pStacksToClone, List<GameObject> pTempStacks)
    {
        foreach (var stack in pStacksToClone)
        {
            GameObject tempStack = Instantiate(stack, transform.position, Quaternion.identity, temporaryStacksParent);
            pTempStacks.Add(tempStack);
            tempStack.SetActive(false);
            tempStack.transform.position = transform.position;
        }
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
            offset.y += (StackHeight * pCurrentIndex) + (StackHeight / 2);
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
                if (offset.x is < 0 or > 0)
                {
                    billboard.transform.Rotate(new Vector3(0, 1, 0), 90f);
                }
            }
        }
    }

    private void initializeGeneration()
    {
        if (isGenerating) return;
        isGenerating = true;
        spawnedStacks.Clear();
        currentAmountOfStacks = 0;
        finalMaxStackCount = Random.Range(minAmountOfStacks, maxAmountOfStacks);

        generateFloor();
        StartCoroutine(generateMiddleStacks());

        size = new Vector3(spawnedStacks[0].transform.lossyScale.x + 1,
            StackHeight * finalMaxStackCount + (3 * StackHeight),
            spawnedStacks[0].transform.lossyScale.z + 1);
    }

    public void RegenerateBuilding()
    {
        if (isGenerating) return;
        for (int i = 0; i < spawnedStacks.Count; i++)
        {
            Destroy(spawnedStacks[i]);
        }

        initializeGeneration();
    }

    public void ShowPotentialStack(int pStackIndex, int pPrefabIndex)
    {
        if (isGenerating) return;
        if (spawnedStacks.Count == 0) return;
        if (pPrefabIndex < 0)
        {
            spawnedStacks[pStackIndex].SetActive(true);
            if(tempStack != null) tempStack.SetActive(false);
            return;
        }
        if(tempStack != null) tempStack.SetActive(false);
        spawnedStacks[lastTempStackIndex].SetActive(true);
        lastTempStackIndex = pStackIndex;
        spawnedStacks[lastTempStackIndex].SetActive(false);
        Vector3 stackPosition = spawnedStacks[pStackIndex].transform.position;
        if (pStackIndex == 0)
        {
            tempStack = tempFloorStacks[pPrefabIndex];
        }
        else if (pStackIndex == finalMaxStackCount - 1)
        {
            tempStack = tempRoofStacks[pPrefabIndex];
        }
        else
        {
            tempStack = tempMiddleStacks[pPrefabIndex];
        }

        tempStack.SetActive(true);
        tempStack.transform.position = stackPosition;
    }

    public void ReplaceStack(int pStackIndex, int pPrefabIndex)
    {
        if (isGenerating) return;
        
        Vector3 stackPosition = spawnedStacks[pStackIndex].transform.position;
        GameObject newPrefab;
        if (pStackIndex == 0) newPrefab = availableFloorStacks[pPrefabIndex];
        else if (pStackIndex > 0 && pStackIndex < finalMaxStackCount - 1)
            newPrefab = availableMiddleStacks[pPrefabIndex];
        else newPrefab = availableRoofStacks[pPrefabIndex];

        GameObject oldPrefab = spawnedStacks[pStackIndex];
        spawnedStacks.Remove(oldPrefab);
        Destroy(oldPrefab);

        GameObject stack = Instantiate(newPrefab,
            stackPosition, Quaternion.identity,
            editedStacksParent);
        stack.name = $"[ MIDDLE STACK {pStackIndex} ]";
        spawnedStacks.Insert(pStackIndex, newPrefab);
    }

    private void generateFloor()
    {
        int randomIndex = Random.Range(0, availableFloorStacks.Count);

        bottomPosition = transform.position + new Vector3(0, StackHeight / 2, 0);
        GameObject stack = Instantiate(availableFloorStacks[randomIndex], bottomPosition, Quaternion.identity,
            transform);
        stack.name = "[ FLOOR ]";
        currentAmountOfStacks++;
        spawnedStacks.Add(stack);
    }

    private IEnumerator generateMiddleStacks()
    {
        yield return new WaitForSeconds(BuildDelay);

        int randomIndex = Random.Range(0, availableMiddleStacks.Count);

        GameObject stack = Instantiate(availableMiddleStacks[randomIndex],
            bottomPosition + new Vector3(0, StackHeight * currentAmountOfStacks, 0), Quaternion.identity,
            middleStacksParent);
        stack.name = $"[ MIDDLE STACK {currentAmountOfStacks} ]";
        spawnedStacks.Add(stack);

        if (enableBillboards) handleBillboardSpawning(stack.transform, currentAmountOfStacks);

        currentAmountOfStacks++;
        if (currentAmountOfStacks < finalMaxStackCount - 1)
        {
            StartCoroutine(generateMiddleStacks());
            yield return null;
        }
        else generateRoof();
    }

    private void generateRoof()
    {
        int randomIndex = Random.Range(0, availableRoofStacks.Count);

        GameObject stack = Instantiate(availableRoofStacks[randomIndex],
            bottomPosition + new Vector3(0, StackHeight * currentAmountOfStacks, 0), Quaternion.identity, transform);
        stack.name = "[ ROOF ]";
        spawnedStacks.Add(stack);

        isGenerating = false;
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