using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{
    public static List<Vector2Int> FindPath(bool[,] map, Vector2Int offset, Vector2Int startWorld, Vector2Int targetWorld)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        if (map == null) return null;

        Vector2Int start = startWorld - offset;
        Vector2Int target = targetWorld - offset;

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        if (start.x < 0 || start.x >= width || start.y < 0 || start.y >= height) return null;
        if (target.x < 0 || target.x >= width || target.y < 0 || target.y >= height) return null;
        if (!map[start.x, start.y] || !map[target.x, target.y]) return null;

        // Простий BFS для 2D
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == target) break;

            foreach (var dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x < 0 || neighbor.x >= width || neighbor.y < 0 || neighbor.y >= height) continue;
                if (!map[neighbor.x, neighbor.y]) continue;
                if (cameFrom.ContainsKey(neighbor)) continue;

                queue.Enqueue(neighbor);
                cameFrom[neighbor] = current;
            }
        }

        if (!cameFrom.ContainsKey(target)) return null;

        // Відновлення шляху
        Vector2Int step = target;
        while (step != start)
        {
            path.Add(step + offset);
            step = cameFrom[step];
        }
        path.Reverse();
        return path;
    }
}
