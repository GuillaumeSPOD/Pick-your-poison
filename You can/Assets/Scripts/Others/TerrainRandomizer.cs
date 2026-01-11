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


    /// <summary>
    /// DEMARRAGE
    /// </summary>

    void Start()
    {
        rnd = new Random(GlobalVariables.GlobalSeed);
        SetTerrainHeight();
        PokeHolesInTerrain();

        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }




    /// <summary>
    /// UPDATE THE HEIGHTMAP
    /// </summary>






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




    /// <summary>
    /// CAVES GENERATION
    /// </summary>






    public class Mountain
    {
        public int x_min;
        public int x_max;
        public int z_min;
        public int z_max;

        public Vector2 center;

        public int[,] shape;

        public int size;

        public int numberOfHoles;

        public Mountain(int xMin, int xMax, int zMin, int zMax, int[,] shapeData)
        {
            x_min = xMin;
            x_max = xMax;
            z_min = zMin;
            z_max = zMax;
            shape = shapeData;

            

            CalculateCenter();
            CalculateSize();
            SetNumberOfHoles();
        }

        public void CalculateCenter()
        {
            this.center = new Vector2((x_max + x_min) / 2, (z_max + z_min) / 2);
        }

        private void CalculateSize()
        {
            int count = 0;

            int height = shape.GetLength(0);
            int width = shape.GetLength(1);

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (shape[z, x] == 1)
                    {
                        count++;
                    }
                }
            }

            size = count;
        }

        private void SetNumberOfHoles()
        {
            switch (size)
            {
                case > 300:
                    this.numberOfHoles = 4; break;

                case > 150:
                    this.numberOfHoles = 3; break;

                case > 50:
                    this.numberOfHoles = 2; break;

                case > 8:
                    this.numberOfHoles = 1; break;

                default:
                    this.numberOfHoles = 0;
            }
        }
    }

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int p)
    {
        yield return new Vector2Int(p.x + 1, p.y);
        yield return new Vector2Int(p.x - 1, p.y);
        yield return new Vector2Int(p.x, p.y + 1);
        yield return new Vector2Int(p.x, p.y - 1);
    }

    private Mountain FloodFillMountain(int[,] map, bool[,] visited, int startX, int startZ)
    {
        int zSize = map.GetLength(0);
        int xSize = map.GetLength(1);

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> cells = new List<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startZ));
        visited[startZ, startX] = true;

        int x_min = startX;
        int x_max = startX;
        int z_min = startZ;
        int z_max = startZ;

        while (queue.Count > 0)
        {
            Vector2Int p = queue.Dequeue();
            cells.Add(p);

            x_min = Mathf.Min(x_min, p.x);
            x_max = Mathf.Max(x_max, p.x);
            z_min = Mathf.Min(z_min, p.y);
            z_max = Mathf.Max(z_max, p.y);

            foreach (Vector2Int n in GetNeighbors(p))
            {
                if (n.x < 0 || n.x >= xSize || n.y < 0 || n.y >= zSize)
                    continue;

                if (!visited[n.y, n.x] && map[n.y, n.x] == 1)
                {
                    visited[n.y, n.x] = true;
                    queue.Enqueue(n);
                }
            }
        }

        // Création de la shape locale
        int width = x_max - x_min + 1;
        int height = z_max - z_min + 1;
        int[,] shape = new int[height, width];

        foreach (Vector2Int c in cells)
        {
            shape[c.y - z_min, c.x - x_min] = 1;
        }

        return new Mountain(x_min, x_max, z_min, z_max, shape);
    }


    public List<Mountain> DetectMountains(int[,] map)
    {
        int zSize = map.GetLength(0);
        int xSize = map.GetLength(1);

        bool[,] visited = new bool[zSize, xSize];
        List<Mountain> mountains = new List<Mountain>();

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (map[z, x] == 1 && !visited[z, x])
                {
                    Mountain m = FloodFillMountain(map, visited, x, z);
                    mountains.Add(m);
                }
            }
        }

        return mountains;
    }

    private void PokeHolesInTerrain()
    {
        int numberOfHoles = 0;

        bool[,] holes = new bool[res - 1, res - 1];

        int[,] simplifiedHeightMap = new int[(res - 1) / blocksize, (res - 1) / blocksize];  //heightMap comportant les hauteurs de chaque "bloc"


        InitHoles(holes);

        InitHeightMap(simplifiedHeightMap);

        List<Mountain> mountains = new List<Mountain>();

        mountains = DetectMountains(simplifiedHeightMap);

        //DEBUG

        Texture2D tex = MapToTexture(simplifiedHeightMap);
        ShowTexture(tex);

        DebugMountains(mountains);

        //Seems to work just fine

        Debug.Log($"Nombre de trous: {numberOfHoles}");
        terrainData.SetHoles(0, 0, holes);
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

    private void InitHeightMap(int[,] heightInit)
    {
        int zSize = heightInit.GetLength(0);
        int xSize = heightInit.GetLength(1);

        for (int z = 0; z < zSize; z++)
            for (int x = 0; x < xSize; x++)
                if (heights[1 + z * blocksize, 1 + x * blocksize] > HoleMinHeight)
                    heightInit[z, x] = 1;
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




    private void DebugMountains(List<Mountain> mountains)
    {
        Debug.Log($"Nombre de montagnes détectées : {mountains.Count}");

        for (int i = 0; i < mountains.Count; i++)
        {
            Mountain m = mountains[i];

            Debug.Log(
                $"Montagne {i} | " +
                $"Size: {m.size} | " +
                $"x_min: {m.x_min}, x_max: {m.x_max} | " +
                $"z_min: {m.z_min}, z_max: {m.z_max} | " +
                $"Center: ({m.center.x:F2}, {m.center.y:F2})"
            );
        }
    }


    public Texture2D MapToTexture(int[,] map)
    {
        int height = map.GetLength(0);
        int width = map.GetLength(1);

        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = map[z, x] == 1 ? Color.white : Color.black;
                tex.SetPixel(x, z, c);
            }
        }

        tex.Apply();
        return tex;
    }

    public void ShowTexture(Texture2D tex)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.position = new Vector3(0, 10, 0);
        quad.transform.localScale = new Vector3(tex.width / 10f, tex.height / 10f, 1);

        Material mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = tex;

        quad.GetComponent<MeshRenderer>().material = mat;
    }
}