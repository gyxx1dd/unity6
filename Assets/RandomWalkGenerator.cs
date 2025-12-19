using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomWalkGenerator : MonoBehaviour
{

    public GameObject enemyPrefab;
    public int enemyCount = 3;


    [Header("Tilemaps & Tiles")]
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Random Walk Settings")]
    public Vector2Int startPosition = Vector2Int.zero;
    public int walkLength = 200;        // кількість кроків walker
    public int walkerCount = 1;         // кількість одночасних walker
    public int maxStepsPerWalker = 200; // ліміт для кожного walker

    [Header("Wall Padding")]
    public int wallPadding = 1; // товщина стін навколо підлоги

    private HashSet<Vector2Int> floorPositions;

    void Start()
    {
        Generate();
        floorPositions = RunRandomWalkMultiple(startPosition, walkerCount, walkLength);
        PaintFloor(floorPositions);

        GameObject enemiesContainer = new GameObject("Enemies"); // контейнер для ворогів
        SpawnEnemies(enemiesContainer);
    }

    public void Generate()
    {
        floorPositions = RunRandomWalkMultiple(startPosition, walkerCount, walkLength);
        PaintFloor(floorPositions);
        PaintWalls(floorPositions);
    }

    HashSet<Vector2Int> RunRandomWalkMultiple(Vector2Int startPos, int walkers, int stepsPerWalker)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        List<Vector2Int> walkersPositions = new List<Vector2Int> { startPos };

        for (int w = 0; w < walkers; w++)
            walkersPositions.Add(startPos);

        System.Random rnd = new System.Random();

        foreach (var wp in walkersPositions)
        {
            Vector2Int current = wp;
            floor.Add(current);

            int steps = Mathf.Min(stepsPerWalker, maxStepsPerWalker);
            for (int i = 0; i < steps; i++)
            {
                current += GetRandomDirection(rnd);
                floor.Add(current);
            }
        }

        return floor;
    }

    Vector2Int GetRandomDirection(System.Random rnd)
    {
        int r = rnd.Next(4);
        switch (r)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            default: return Vector2Int.right;
        }
    }


    public void SpawnEnemies(GameObject container)
    {
        var floorList = new List<Vector2Int>(floorPositions);
        System.Random rnd = new System.Random();
        for (int i = 0; i < enemyCount; i++)
        {
            var pos = floorList[rnd.Next(floorList.Count)];
            Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity, container.transform);
        }
    }



    void PaintFloor(HashSet<Vector2Int> floors)
    {
        groundTilemap.ClearAllTiles();
        foreach (var pos in floors)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
            groundTilemap.SetTile(tilePos, floorTile);
        }
    }

    void PaintWalls(HashSet<Vector2Int> floors)
    {
        wallTilemap.ClearAllTiles();
        // Для кожної підлогової клітинки дивимося 4 сусідів — якщо сусіда немає у floor, ставимо wall
        foreach (var pos in floors)
        {
            var neighbors = new Vector2Int[] {
                pos + Vector2Int.up,
                pos + Vector2Int.down,
                pos + Vector2Int.left,
                pos + Vector2Int.right
            };

            foreach (var n in neighbors)
            {
                if (!floors.Contains(n))
                {
                    // додатково можна врахувати wallPadding
                    for (int dx = -wallPadding; dx <= wallPadding; dx++)
                        for (int dy = -wallPadding; dy <= wallPadding; dy++)
                        {
                            var wp = new Vector2Int(n.x + dx, n.y + dy);
                            Vector3Int tilePos = new Vector3Int(wp.x, wp.y, 0);
                            if (wallTilemap.GetTile(tilePos) == null && !floors.Contains(wp))
                                wallTilemap.SetTile(tilePos, wallTile);
                        }
                }
            }
        }
    }

    // Опціонально — метод щоб повернути grid дані (walkable)
    public bool[,] GetWalkableMap(out Vector2Int offset, int width, int height)
    {
        // визначити bounding box підлог
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (var p in floorPositions)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        // створимо карту заданого розміру, центруючи на старті
        offset = new Vector2Int(minX, minY);
        bool[,] map = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = false;

        foreach (var p in floorPositions)
        {
            int mx = p.x - offset.x;
            int my = p.y - offset.y;
            if (mx >= 0 && mx < width && my >= 0 && my < height)
                map[mx, my] = true;
        }
        return map;
    }
}
