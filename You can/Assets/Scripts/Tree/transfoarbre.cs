using System.Collections.Generic;
using UnityEngine;

public class transfoarbre : MonoBehaviour
{
    public GameObject[] treePrefabs; // Tableau contenant les 4 types d’arbres
    public Terrain thisTerrain;

    void ConvertTerrainTreesToPrefabs()
    {
        TerrainData terrainData = thisTerrain.terrainData;

        GameObject treeParent = new GameObject("treeParent");

        foreach (TreeInstance terrainTree in terrainData.treeInstances)
        {
            // Position monde de l’arbre
            Vector3 worldTreePos = Vector3.Scale(terrainTree.position, terrainData.size) + thisTerrain.transform.position;

            // Choisir un prefab aléatoirement
            GameObject selectedPrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // Instancier le prefab à la position de l’arbre
            GameObject prefabTree = Instantiate(selectedPrefab, worldTreePos, Quaternion.identity, treeParent.transform);

            // Rotation de l’arbre (en degrés)
            prefabTree.transform.rotation = Quaternion.AngleAxis(terrainTree.rotation * Mathf.Rad2Deg, Vector3.up);

            // Taille aléatoire entre 100% et 120%
            float scaleMultiplier = Random.Range(1.0f, 1.2f);
            prefabTree.transform.localScale = prefabTree.transform.localScale * scaleMultiplier;
        }

        // Supprimer tous les arbres du terrain
        terrainData.treeInstances = new TreeInstance[0];
    }

    void Start()
    {
        ConvertTerrainTreesToPrefabs();
    }
}
