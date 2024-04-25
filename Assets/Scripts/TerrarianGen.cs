using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrarianGen : MonoBehaviour
{
    public PlayerController player;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

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
    public Texture2D caveNoiseTexture;

    [Header("ore Settings")]
    public float coalRarity;
    public float coalSize;
    public float ironRarity, ironSize;
    public float goldRarity, goldSize;
    public float diamondRarity, diamondSize;
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;

    private GameObject[] worldChunks;

    private List<Vector2> worldTiles = new List<Vector2>();
    private List<GameObject> worldTileObjects = new List<GameObject> ();

    private void OnValidate()
    {
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        // Ore
        GenerateNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondSize, diamondSpread);
    }

    private void Start()
    {
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }


        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture(caveFreq, surfaceValue,caveNoiseTexture);
        // Ore
        GenerateNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondSize, diamondSpread);

        CreateChunks();
        GenerateTerrain();
       
        player.Spawn();

        RefreshChunks();
    }

    private void Update()
    {
        RefreshChunks();
    }



    void RefreshChunks()
    {
        for (int i = 0; i <worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2(i * ChunkSize, 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 24f)
                worldChunks[i].SetActive(false);
            else
                worldChunks[i].SetActive(true);
        }
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
            if (x == worldSize / 2)
                player.spawnPos = new Vector2(x, height + 1);

            for (int y = 0; y < height; y++)
            {

                Sprite tileSprite;
                if (y < height - dirtLayerHeight)
                {
                    if (coalSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.coal.tileSprite;                   
                   else if (ironSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.iron.tileSprite;                    
                   else if (goldSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.gold.tileSprite;                    
                   else if (diamondSpread.GetPixel(x, y).r > 0.5f) 
                        tileSprite = tileAtlas.diamond.tileSprite;
                   else
                    tileSprite = tileAtlas.stone.tileSprite;
                }
                else if (y < height - 1)
                {
                    tileSprite = tileAtlas.dirt.tileSprite;
                }
                else
                {
                    // top layer of terrain
                    tileSprite = tileAtlas.grass.tileSprite;

                }

                // Cave generation
                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        placeTile(tileSprite, x, y, false);
                    }
                }
                else
                {
                    placeTile(tileSprite, x, y, false);
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
    public void GenerateNoiseTexture(float frequency, float limit,Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (v > limit)
                noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
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
                placeTile(tileAtlas.log.tileSprite, x, y + i, true);
            }
            

            // Generate Leaves
            placeTile(tileAtlas.leaf.tileSprite, x, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprite, x, y + treeHeight + 1, true);
            placeTile(tileAtlas.leafOrange.tileSprite, x, y + treeHeight + 2, true);

            placeTile(tileAtlas.leaf.tileSprite, x - 1, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprite, x - 2, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprite, x - 1, y + treeHeight + 1, true);        

            placeTile(tileAtlas.leaf.tileSprite, x + 1, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprite, x + 2, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprite, x + 1, y + treeHeight + 1, true);
        }
    }

    public void RemoveTile(int x, int y)
    {
       if (worldTiles.Contains(new Vector2Int(x,y)) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
       {
           Destroy(worldTileObjects[worldTiles.IndexOf(new Vector2(x, y))]);
       }


    }

    public void placeTile(Sprite tileSprite, float x, float y, bool backgroundElement)
    {
      



            GameObject newTile = new GameObject();

        //float chunkCoord  = Mathf.Round(x / ChunkSize) * ChunkSize;
        //newTile.transform.parent = worldChunks[(int)chunkCoord].transform;

        // This is from Chat GPT
        int chunkIndex = Mathf.Clamp((int)(x / ChunkSize), 0, worldChunks.Length - 1);
        newTile.transform.parent = worldChunks[chunkIndex].transform;




        newTile.AddComponent<SpriteRenderer>();
        if (!backgroundElement)
        {
        newTile.AddComponent<BoxCollider2D>();
        newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
        newTile.tag = "Ground"; 
        }
        
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name + x + y;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
        worldTileObjects.Add(newTile);
    }
}
