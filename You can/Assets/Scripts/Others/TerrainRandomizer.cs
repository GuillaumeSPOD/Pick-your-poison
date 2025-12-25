using UnityEngine;
using Random = System.Random;
using Unity.AI.Navigation;

public class TerrainRandomizer : MonoBehaviour
{
    [Header("Réglages du relief")]
    public int blocksize = 2;
    public float scale = 0.5f;
    public float height_multiplier = 0.5f;

    private Random rnd;
    private Terrain terrain;
    private TerrainData terrainData;

    private int block_x_coordonates_in_terrain;
    private int block_z_coordonates_in_terrain;

    private float[,] heights;

    private int block_number_in_lane;

    private int res;

    



    void Start()
    {
        rnd = new Random(GlobalVariables.GlobalSeed);
        SetTerrainHeight();
        PokeHolesInTerrain();

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



        block_number_in_lane = res/blocksize; //Nombre de blocks dans un axe (x ou z)
        float h = 0.1f;




        //Permet de changer les hauteurs de façon cohérente

        for (big_x = 0; big_x < block_number_in_lane; big_x++)
        {

            for (big_z = 0; big_z < block_number_in_lane; big_z++)
            {

                h = (
                    Mathf.PerlinNoise((big_x + GlobalVariables.GlobalSeed) * scale * 0.05f, (big_z + GlobalVariables.GlobalSeed) * scale * 0.05f) * 0.6f +
                    Mathf.PerlinNoise((big_x + GlobalVariables.GlobalSeed) * scale, (big_z + GlobalVariables.GlobalSeed) * scale) * 0.4f
                ) * height_multiplier;
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

        //Initialise holes à true
        for (int z = 0; z < res - 1; z++)
        {
            for (int x = 0; x < res - 1; x++)
            {
                holes[z, x] = true;
            }

        }

        for (int z = blocksize - 1 ; z < res - blocksize - 1; z+=blocksize)
        {
            for (int x = blocksize/2 -1 ; x < res - blocksize - 1; x+=blocksize)
            {
                holes[z, x] = false;
            }

        }

        for (int z = blocksize / 2 - 1; z < res - blocksize - 1; z += blocksize)
        {
            for (int x = blocksize - 1; x < res - blocksize - 1; x += blocksize)
            {
                holes[z, x] = false;
            }

        }


        terrainData.SetHoles(0, 0, holes);
    }
}