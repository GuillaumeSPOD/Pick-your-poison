using UnityEngine;
using System.Collections.Generic;

public class DynamicTreeConverter : MonoBehaviour
{
    [Header("Références")]
    public Terrain terrain;
    public Transform player;
    public TreeManager treeManager;

    [Header("Arbres")]
    public GameObject[] treePrefabs;
    public Transform treeParent;

    [Header("Distances")]
    public float convertDistance = 30f;
    public float checkInterval = 0.1f;

    private TerrainData data;
    private Vector3 terrainPos;
    private float timer;

    // ID TreeInstance -> GameObject actif
    private Dictionary<int, GameObject> activeTrees = new Dictionary<int, GameObject>();

    private void Start()
    {
        if (terrain == null) terrain = GetComponent<Terrain>();
        if (treeParent == null) treeParent = transform;
        if (treeManager == null) treeManager = FindAnyObjectByType<TreeManager>();

        data = terrain.terrainData;
        terrainPos = terrain.transform.position;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        Vector3 playerPos = player.position;
        TreeInstance[] trees = data.treeInstances;

        // Instanciation des arbres proches
        for (int i = 0; i < trees.Length; i++)
        {
            if (activeTrees.ContainsKey(i)) continue; // déjà instancié

            Vector3 worldPos = Vector3.Scale(trees[i].position, data.size) + terrainPos;
            float dist = Vector3.Distance(playerPos, worldPos);

            if (dist <= convertDistance)
            {
                GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                GameObject treeGO = Instantiate(prefab, worldPos, Quaternion.Euler(0, Mathf.Rad2Deg * trees[i].rotation, 0), treeParent);
                treeGO.transform.localScale = new Vector3(trees[i].widthScale, trees[i].heightScale, trees[i].widthScale);
                treeGO.name = prefab.name + " (Clone)";
                treeGO.tag = "Arbre";

                treeManager.trees.Add(treeGO);
                activeTrees[i] = treeGO;
            }
        }

        // Destruction des GameObjects trop loin
        List<int> toRemove = new List<int>();
        foreach (var kvp in activeTrees)
        {
            int index = kvp.Key;
            GameObject treeGO = kvp.Value;

            if (treeGO == null)
            {
                toRemove.Add(index);
                continue;
            }

            Vector3 worldPos = treeGO.transform.position;
            float dist = Vector3.Distance(playerPos, worldPos);

            if (dist > convertDistance)
            {
                treeManager.trees.Remove(treeGO);
                Destroy(treeGO);
                toRemove.Add(index);
            }
        }

        foreach (int idx in toRemove)
            activeTrees.Remove(idx);
    }
}