using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;
    public float detectionRadius = 6f; // в≥дстань ви€вленн€
    public float loseSightTime = 3f; // ск≥льки ворог продовжуЇ шукати
    public Transform playerTransform;

    // Pathfinding dependencies
    public RandomWalkGenerator generator; // посиланн€, щоб отримати map
    public int mapWidth = 100;
    public int mapHeight = 100;

    private Rigidbody2D rb;
    private Vector2[] currentPathPositions;
    private int currentPathIndex = 0;
    private float loseSightTimer = 0f;
    private enum State { Idle, Patrol, Chase }
    private State state = State.Patrol;

    private bool[,] walkableMap;
    private Vector2Int mapOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // ќтримати масив прох≥дних кл≥тин
        walkableMap = generator.GetWalkableMap(out mapOffset, mapWidth, mapHeight);
        // початково Ч патруль (можна додати Waypoints)
        state = State.Patrol;
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= detectionRadius)
        {
            state = State.Chase;
            loseSightTimer = loseSightTime;
            // запитати шл€х до гравц€
            var path = AStarPathfinding.FindPath(walkableMap, mapOffset, Vector2Int.RoundToInt(transform.position), Vector2Int.RoundToInt(playerTransform.position));
            if (path != null && path.Count > 0)
            {
                ConvertPathToVector2Array(path);
                currentPathIndex = 0;
            }
            else
            {
                // €кщо шл€ху немаЇ Ч просто рухаЇмось пр€мо до гравц€ (простий fallback)
                currentPathPositions = new Vector2[] { playerTransform.position };
                currentPathIndex = 0;
            }
        }
        else if (state == State.Chase)
        {
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
                state = State.Patrol;
        }

        // “ут можна реал≥зувати патруль (наприклад, просто сто€ти або блукати)
    }

    void FixedUpdate()
    {
        if (currentPathPositions == null || currentPathPositions.Length == 0) return;
        Vector2 target = currentPathPositions[currentPathIndex];
        Vector2 pos = rb.position;
        Vector2 dir = (target - pos);
        if (dir.magnitude < 0.1f)
        {
            // д≥йшли до waypoint
            if (currentPathIndex < currentPathPositions.Length - 1)
                currentPathIndex++;
            return;
        }
        dir.Normalize();
        rb.MovePosition(pos + dir * speed * Time.fixedDeltaTime);
    }

    void ConvertPathToVector2Array(List<Vector2Int> path)
    {
        currentPathPositions = new Vector2[path.Count];
        for (int i = 0; i < path.Count; i++)
            currentPathPositions[i] = new Vector2(path[i].x + 0.5f, path[i].y + 0.5f); // центр тайла
    }

    // ƒл€ дебагу Ч намалювати рад≥ус ви€вленн€
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
