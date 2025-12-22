using UnityEngine;
using Random = System.Random;
public class TerrainRandomizer : MonoBehaviour
{
    [Header("Réglages du relief")]
    public int blocksize = 2;
    public float noiseScale = 1f;
    public float offsetX = 0f;
    public float offsetY = -1f;
    private Random rnd;
    private Terrain terrain;
    private TerrainData terrainData;

    private int block_x_coordonates_in_terrain;
    private int block_z_coordonates_in_terrain;

    private float[,] heights;
    private float[,] current_heights;

    private int block_number_in_lane;

    void Start()
    {
        rnd = new Random(GlobalVariables.GlobalSeed);
        ModifyEntireTerrain();
    }

    public void ModifyEntireTerrain()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        int res = terrainData.heightmapResolution;
        
        heights = new float[res, res];

        int big_x = 0;
        int big_z = 0;

 

        block_number_in_lane = res/blocksize; //Nombre de blocks dans un axe (x ou z)
        current_heights = new float[block_number_in_lane, block_number_in_lane];
        float h = 10/1000;

        

        for (big_x = 0; big_x < block_number_in_lane; big_x++)
        {

            for (big_z = 0; big_z < block_number_in_lane; big_z++)
            {
                FlattenZone(big_x, big_z, blocksize, h);

                if (big_x != 0 && big_z != 0)
                    h = AverageOfPreviousZones(big_x, big_z) + (RandomInt(0, 20) - 10) / 1000;
                else if (big_z !=0)
                    h = current_heights[(big_x), (big_z - 1)] + (RandomInt(0, 20) - 10) / 1000;

                current_heights[big_x, big_z] = h;
            }
            
        }

        terrainData.SetHeights(0, 0, heights);
    }

    public float RandomInt(int min, int max)
    {
        if (min>max) return rnd.Next(max , min);
        return rnd.Next(min, max);
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

    private float AverageOfPreviousZones(int big_x, int big_z)
    {
        if (big_z == 0) big_z = block_number_in_lane - 1;
        if (big_x == 0) big_x = block_number_in_lane - 1;

        return (current_heights[big_x , big_z - 1] + current_heights[ big_x-1, big_z] )/ 2;
    }
}