using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrarianGen : MonoBehaviour
{

    [Header("Tile Sprites")]
    public Sprite grass;
    public Sprite dirt;
    public Sprite stone;
    public Sprite log;
    public Sprite leaf;
    public Sprite leafOrange;

    [Header("Trees")]
    public int treeChance = 10;
    public int minTreeHeight = 3;
    public int maxTreeHeight = 6;

    [Header("Generation Settings")]
    public int ChunkSize = 32;
    public int worldSize = 100;   
    public bool generateCaves = true;   
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 40f;
    public int heightAddition = 25;

    [Header("Noise Settings")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public float seed;
    public Texture2D noiseTexture;

    public GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();


    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        CreateChunks();
        GenerateNoiseTexture();
        GenerateTerrain();
    }
    public void CreateChunks()
    {
        int numChunks = worldSize / ChunkSize;
        worldChunks = new GameObject[numChunks];
        for(int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++)
            {

                Sprite tileSprite;
                if (y < height - dirtLayerHeight)
                {
                    tileSprite = stone;                    
                }
                else if (y < height - 1)
                {
                    tileSprite = dirt;
                }
                else
                {
                    // top layer of terrain
                    tileSprite = grass;

                }

                // Cave generation
                if (generateCaves)
                {
                    if (noiseTexture.GetPixel(x, y).r > surfaceValue)
                    {
                        placeTile(tileSprite, x, y);
                    }
                }
                else
                {
                    placeTile(tileSprite, x, y);
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        // Generate a tree
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            GenerateTree(x, y + 1);
                        }
                        
                    }
                }
               




            }
        }
    }

    // RNG of the world
    private void GenerateNoiseTexture()
    {
        noiseTexture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
    }


    // Generate Tree
    void GenerateTree(float x, float y)
    {
        // Generate Logs
        if (worldTiles.Contains(new Vector2(x, y - 1)))
        {
            int treeHeight = Random.Range(minTreeHeight, maxTreeHeight);
            for (int i = 0; i < treeHeight; i++)
            {
                placeTile(log, x, y + i);
            }
            

            // Generate Leaves
            placeTile(leaf, x, y + treeHeight);
            placeTile(leaf, x, y + treeHeight + 1);
            placeTile(leafOrange, x, y + treeHeight + 2);

            placeTile(leaf, x - 1, y + treeHeight);
            placeTile(leaf, x - 2, y + treeHeight);
            placeTile(leaf, x - 1, y + treeHeight + 1);        

            placeTile(leaf, x + 1, y + treeHeight);
            placeTile(leaf, x + 2, y + treeHeight);
            placeTile(leaf, x + 1, y + treeHeight + 1);
        }
    }

    public void placeTile(Sprite tileSprite, float x, float y)
    {
        GameObject newTile = new GameObject();

        //float chunkCoord  = Mathf.Round(x / ChunkSize) * ChunkSize;
        //newTile.transform.parent = worldChunks[(int)chunkCoord].transform;

        // This is from Chat GPT
        int chunkIndex = Mathf.Clamp((int)(x / ChunkSize), 0, worldChunks.Length - 1);
        newTile.transform.parent = worldChunks[chunkIndex].transform;




        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name + x + y;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
    }
}
