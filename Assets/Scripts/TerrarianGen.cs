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

    [Header("Addons")]
    public int tallGrassChance = 10;

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
    public OreClass[] ores;
   /* public float coalRarity;
    public float coalSize;
    public float ironRarity, ironSize;
    public float goldRarity, goldSize;
    public float diamondRarity, diamondSize;
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;*/

    private GameObject[] worldChunks;

    private List<Vector2> worldTiles = new List<Vector2>();
    private List<GameObject> worldTileObjects = new List<GameObject> ();

    private void OnValidate()
    {
        
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);
        

        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        // Ore
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
        GenerateNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture);
    }

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
       
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);
        


        GenerateNoiseTexture(caveFreq, surfaceValue,caveNoiseTexture);
        // Ore
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
        GenerateNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture);

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
            if (Vector2.Distance(new Vector2(i * ChunkSize, 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 16.5f)
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

                Sprite[] tileSprites;
                if (y < height - dirtLayerHeight)
                {
                    tileSprites = tileAtlas.stone.tileSprites;

                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                        tileSprites = tileAtlas.coal.tileSprites;                   
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                        tileSprites = tileAtlas.iron.tileSprites;                    
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                        tileSprites = tileAtlas.gold.tileSprites;                    
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight) 
                        tileSprites = tileAtlas.diamond.tileSprites;
                    
                        
                }
                else if (y < height - 1)
                {
                    tileSprites = tileAtlas.dirt.tileSprites;
                }
                else
                {
                    // top layer of terrain
                    tileSprites = tileAtlas.grass.tileSprites;

                }

                // Cave generation
                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        placeTile(tileSprites, x, y, false);
                    }
                }
                else
                {
                    placeTile(tileSprites, x, y, false);
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
                    else
                    {
                        int i = Random.Range(0, tallGrassChance);
                        if (i == 1)
                        {
                            // Generate Grass
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                placeTile(tileAtlas.tallGrass.tileSprites, x, y + 1, true); //backgroundElement);
                            }
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
                placeTile(tileAtlas.log.tileSprites, x, y + i, true);
            }
            

            // Generate Leaves
            placeTile(tileAtlas.leaf.tileSprites, x, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprites, x, y + treeHeight + 1, true);
            placeTile(tileAtlas.leaf.tileSprites, x, y + treeHeight + 2, true);

            placeTile(tileAtlas.leaf.tileSprites, x - 1, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprites, x - 2, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprites, x - 1, y + treeHeight + 1, true);        

            placeTile(tileAtlas.leaf.tileSprites, x + 1, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprites, x + 2, y + treeHeight, true);
            placeTile(tileAtlas.leaf.tileSprites, x + 1, y + treeHeight + 1, true);
        }
    }

    public void RemoveTile(int x, int y)
    {
       if (worldTiles.Contains(new Vector2Int(x,y)) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
       {
           Destroy(worldTileObjects[worldTiles.IndexOf(new Vector2(x, y))]);
            worldTileObjects.RemoveAt(worldTiles.IndexOf(new Vector2(x,y)));
            worldTiles.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
       }


    }

    public void placeTile(Sprite[] tileSprites, float x, float y, bool backgroundElement)
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

        int spriteIndex = Random.Range(0, tileSprites.Length);
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];

        newTile.name = tileSprites[0].name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
        worldTileObjects.Add(newTile);
    }
}
