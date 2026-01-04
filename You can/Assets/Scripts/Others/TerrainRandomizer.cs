using UnityEngine;
using Random = System.Random;
using Unity.AI.Navigation;
using System;
using System.Collections.Generic;

public class TerrainRandomizer : MonoBehaviour
{
    [Header("Réglages du relief")]
    public int blocksize = 2;

    private Random rnd;
    private Terrain terrain;
    private TerrainData terrainData;

    private int block_x_coordonates_in_terrain;
    private int block_z_coordonates_in_terrain;

    private float[,] heights;

    private int block_number_in_lane;

    private int res;

    [Header("Réglages des souterrains")]
    public float HoleMinHeight = 0.4f;
    public float MinimumHoleDistance = 3f;




    void Start()
    {
        rnd = new Random(GlobalVariables.GlobalSeed);
        SetTerrainHeight();
        PokeHolesInTerrain();

        MassPlaceTrees(10000);

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    public void SetTerrainHeight()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        res = terrainData.heightmapResolution;
        
        heights = new float[res, res];

        int big_x = 0;
        int big_z = 0;

        block_number_in_lane = res / blocksize; //Nombre de blocks dans un axe (x ou z)
 
        float h = 0.1f;


        //Permet de changer les hauteurs de façon cohérente

        for (big_x = 0; big_x < block_number_in_lane; big_x++)
        {

            for (big_z = 0; big_z < block_number_in_lane; big_z++)
            {

                h = (
                    Mathf.PerlinNoise((big_x + GlobalVariables.GlobalSeed) * 0.1f, (big_z + GlobalVariables.GlobalSeed) * 0.1f) * 0.6f +
                    Mathf.PerlinNoise(big_x + GlobalVariables.GlobalSeed , big_z + GlobalVariables.GlobalSeed) * 0.3f
                );

                FlattenZone(big_x, big_z, blocksize, h);

            }

        }

        terrainData.SetHeights(0, 0, heights);
    }


    private void FlattenZone(int chunk_x_coordonate, int chunk_z_coordonate, int localblocksize, float height)
    {
        for (int small_x = 0; small_x < localblocksize; small_x++)
        {
            for (int small_z = 0; small_z < localblocksize; small_z++)
            {
                block_x_coordonates_in_terrain = chunk_x_coordonate * localblocksize;
                block_z_coordonates_in_terrain = chunk_z_coordonate * localblocksize;

                heights[block_z_coordonates_in_terrain + small_z, block_x_coordonates_in_terrain + small_x] = height;
            }
        }
    }

    private void PokeHolesInTerrain()
    {


        bool[,] holes = new bool[res - 1, res - 1];


        InitHoles(holes);
        List<Vector3> possible_Holes = GetPossibleHoles();
        RemoveInnerHoles(possible_Holes);

        possible_Holes = RemovePercentageOfElements(possible_Holes, 20);

        possible_Holes = RemoveCloseElements(possible_Holes);




        int numberOfHoles = 0;

        int chunk_x;
        int chunk_z;
        int offset = (blocksize / 2) - 1 ;

        foreach (Vector3 hole in possible_Holes)
        {
            chunk_x = (int)hole.x;
            chunk_z = (int)hole.z;

            if (CanBeHole(chunk_x * blocksize, chunk_z * blocksize)) 
            {

                holes[chunk_z * blocksize + offset, (chunk_x * blocksize) - 1] = false;
                holes[(chunk_z * blocksize) - 1, chunk_x * blocksize + offset] = false;
                numberOfHoles++;

            }

        }

        Debug.Log($"Nombre de trous: {numberOfHoles}");

        terrainData.SetHoles(0, 0, holes);
    }

    private bool CanBeHole(int x, int z)
    {
        if (z <= blocksize + 1 || x <= blocksize + 1) return false;

        float slope = Mathf.Abs(heights[z, x] - heights[z - 1, x]) + Mathf.Abs(heights[z, x] - heights[z, x - 1]);

        if (slope < 0.02f) return false;
        return true;
    }

    private void MassPlaceTrees(int numberOfTrees)
    {
        TreeInstance[] trees = terrainData.treeInstances;

        for (int i = 0; i < numberOfTrees; i++)
        {
            TreeInstance tree = new TreeInstance();
            tree.position = new Vector3(
                (float)rnd.NextDouble(),  
                0,
                (float)rnd.NextDouble()  
            );
            tree.prototypeIndex = 0;
            tree.widthScale = 1;
            tree.heightScale = 1;
            tree.color = Color.white;
            tree.lightmapColor = Color.white;
            tree.rotation = (float)rnd.NextDouble() * 360f;

            Array.Resize(ref trees, trees.Length + 1);
            trees[trees.Length - 1] = tree;
        }

        terrainData.treeInstances = trees;
    }

    

    private void InitHoles(bool[,] holes)
    {
        for (int z = 0; z < res - 1; z++)
            for (int x = 0; x < res - 1; x++)
                holes[z, x] = true;
    }

    private List<Vector3> GetPossibleHoles()
    {
        List<Vector3> possible_Holes = new List<Vector3>();

        for (int chunk_x = 0; chunk_x < block_number_in_lane; chunk_x++)
        {
            for (int chunk_z = 0; chunk_z < block_number_in_lane; chunk_z++)
            {
                float height = heights[chunk_z * blocksize + 1, chunk_x * blocksize + 1];

                if (height > HoleMinHeight)
                    possible_Holes.Add(new Vector3(chunk_x, 0, chunk_z));
            }
        }

        return possible_Holes;
    }

    private void RemoveInnerHoles(List<Vector3> possible_Holes)
    {
        List<Vector3> copy = new List<Vector3>(possible_Holes);

        for (int i = possible_Holes.Count - 1; i >= 0; i--)
        {
            int x = (int)possible_Holes[i].x;
            int z = (int)possible_Holes[i].z;

            bool left = copy.Exists(v => (int)v.x == x - 1 && (int)v.z == z);
            bool right = copy.Exists(v => (int)v.x == x + 1 && (int)v.z == z);
            bool down = copy.Exists(v => (int)v.x == x && (int)v.z == z - 1);
            bool up = copy.Exists(v => (int)v.x == x && (int)v.z == z + 1);

            if (left && right && down && up)
                possible_Holes.RemoveAt(i);
        }
    }

    private List<Vector3> RemovePercentageOfElements(List<Vector3> list, int percentage)
    {
        int removeCount = list.Count * percentage / 100;

        for (int i = 0; i < removeCount; i++)
        {
            int index = rnd.Next(list.Count);
            list.RemoveAt(index);
        }

        return list;
    }


    private List<Vector3> RemoveCloseElements(List<Vector3> list)
    {
        List<Vector3> result = new List<Vector3>(list);

        for (int i = result.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < i; j++)
            {
                if (Distance(result[i].x, result[i].z, result[j].x, result[j].z) <= MinimumHoleDistance)
                {
                    result.RemoveAt(i);
                    break;
                }
            }
        }

        return result;
    }

    private float Distance(float  x1, float z1, float x2, float z2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
    }
}