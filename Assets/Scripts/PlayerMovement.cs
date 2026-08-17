using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Autonomous wandering for the isometric character: no player input.
/// Picks a random destination inside walkableArea (a PolygonCollider2D) and
/// walks there along a path found with an 8-directional BFS over a grid
/// baked from that same polygon (each cell tested with OverlapPoint).
///
/// Containment is guaranteed by construction: every waypoint is a cell
/// center already verified to be inside the polygon, and diagonal moves are
/// rejected if they'd cut across a blocked corner. This replaced an earlier
/// approach that steered straight at the target using Dynamic-body physics
/// against a wall collider — that let the character get physically wedged
/// in concave notches of the room shape with no way to escape. A baked path
/// sidesteps notches entirely instead of reacting to them.
///
/// Drives the Animator (IsMoving / FacingUp bools) and SpriteRenderer.flipX
/// so the walk cycle reads as one of the 4 isometric diagonals.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Walkable area (sampled into a grid for pathfinding)")]
    [SerializeField] private PolygonCollider2D walkableArea;
    [SerializeField] private float gridCellSize = 0.15f;
    [SerializeField] private int maxSampleAttempts = 30;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waypointThreshold = 0.08f;

    [Header("Idle pause between destinations")]
    [SerializeField] private float idleMinDuration = 1f;
    [SerializeField] private float idleMaxDuration = 3f;

    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private static readonly int FacingUpParam = Animator.StringToHash("FacingUp");

    private static readonly Vector2Int[] Neighbors8 =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D bodyCollider;
    private Vector2 bodyHalfSize;
    private Vector2 bodyOffset;

    // Baked walkability grid (world-aligned, cell size = gridCellSize)
    private bool[,] walkableGrid;
    private int gridWidth, gridHeight;
    private Vector2 gridOrigin;

    private List<Vector2> path;
    private int pathIndex;
    private bool isMoving;
    private float idleTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<BoxCollider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        bodyHalfSize = bodyCollider != null ? bodyCollider.size * 0.5f : Vector2.zero;
        bodyOffset = bodyCollider != null ? bodyCollider.offset : Vector2.zero;
    }

    void Start()
    {
        if (walkableArea == null)
        {
            Debug.LogWarning($"{name}: PlayerMovement has no walkableArea assigned — staying idle.");
            return;
        }

        BuildWalkabilityGrid();

        if (!IsFootprintInside(rb.position))
            rb.position = GetRandomWalkablePoint();

        BeginIdle();
    }

    void FixedUpdate()
    {
        if (walkableArea == null) return;

        if (isMoving)
            MoveAlongPath();
        else
        {
            idleTimer -= Time.fixedDeltaTime;
            if (idleTimer <= 0f) PickNewDestination();
        }
    }

    void BeginIdle()
    {
        isMoving = false;
        idleTimer = Random.Range(idleMinDuration, idleMaxDuration);
        if (animator != null) animator.SetBool(IsMovingParam, false);
    }

    void PickNewDestination()
    {
        Vector2 target = GetRandomWalkablePoint();
        path = FindPath(rb.position, target);

        if (path == null || path.Count == 0)
        {
            // Shouldn't happen (grid is fully connected within one polygon),
            // but never get stuck forever if it somehow does.
            BeginIdle();
            return;
        }

        pathIndex = 0;
        isMoving = true;
        if (animator != null) animator.SetBool(IsMovingParam, true);
    }

    void MoveAlongPath()
    {
        Vector2 current = rb.position;
        Vector2 waypoint = path[pathIndex];
        Vector2 toWaypoint = waypoint - current;

        if (toWaypoint.magnitude <= waypointThreshold)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                BeginIdle();
                return;
            }
            waypoint = path[pathIndex];
            toWaypoint = waypoint - current;
        }

        Vector2 direction = toWaypoint.normalized;
        rb.MovePosition(current + direction * speed * Time.fixedDeltaTime);
        UpdateFacing(direction);
    }

    void UpdateFacing(Vector2 direction)
    {
        bool facingUp = direction.y > 0f;
        bool facingRight = direction.x >= 0f;

        spriteRenderer.flipX = !facingRight;
        if (animator != null) animator.SetBool(FacingUpParam, facingUp);
    }

    // ─── Grid ───────────────────────────────────────────────────────

    void BuildWalkabilityGrid()
    {
        Bounds b = walkableArea.bounds;
        gridOrigin = b.min;
        gridWidth = Mathf.Max(1, Mathf.CeilToInt(b.size.x / gridCellSize));
        gridHeight = Mathf.Max(1, Mathf.CeilToInt(b.size.y / gridCellSize));

        walkableGrid = new bool[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                walkableGrid[x, y] = IsFootprintInside(CellToWorld(x, y));
    }

    // Tests the character's actual box footprint (not just its pivot) against
    // walkableArea: center, 4 corners, 4 edge midpoints. This is what keeps
    // the grid — and therefore every path built from it — inset from the
    // room boundary by (roughly) the character's own half-size, the same
    // thing an agent-radius does for a baked NavMesh.
    bool IsFootprintInside(Vector2 pivot)
    {
        Vector2 c = pivot + bodyOffset;
        float hx = bodyHalfSize.x, hy = bodyHalfSize.y;

        Vector2[] samples =
        {
            c,
            new Vector2(c.x - hx, c.y - hy), new Vector2(c.x + hx, c.y - hy),
            new Vector2(c.x - hx, c.y + hy), new Vector2(c.x + hx, c.y + hy),
            new Vector2(c.x,      c.y - hy), new Vector2(c.x,      c.y + hy),
            new Vector2(c.x - hx, c.y),      new Vector2(c.x + hx, c.y),
        };

        foreach (var s in samples)
            if (!walkableArea.OverlapPoint(s)) return false;
        return true;
    }

    Vector2 CellToWorld(int x, int y) =>
        gridOrigin + new Vector2((x + 0.5f) * gridCellSize, (y + 0.5f) * gridCellSize);

    Vector2Int WorldToCell(Vector2 world)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt((world.x - gridOrigin.x) / gridCellSize), 0, gridWidth - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((world.y - gridOrigin.y) / gridCellSize), 0, gridHeight - 1);
        return new Vector2Int(x, y);
    }

    bool CellWalkable(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;
        return walkableGrid[x, y];
    }

    Vector2 GetRandomWalkablePoint()
    {
        for (int i = 0; i < maxSampleAttempts; i++)
        {
            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            if (walkableGrid[x, y]) return CellToWorld(x, y);
        }

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (walkableGrid[x, y]) return CellToWorld(x, y);

        return rb.position; // grid has no walkable cells at all — shouldn't happen
    }

    // ─── Pathfinding (unweighted 8-directional BFS) ────────────────
    // BFS naturally favors diagonal-heavy paths over cardinal ones (a
    // diagonal hop costs the same "1 step" as a cardinal one but covers
    // more ground), which happens to match this character's diagonal-only
    // walk animations nicely.

    List<Vector2> FindPath(Vector2 fromWorld, Vector2 toWorld)
    {
        Vector2Int start = WorldToCell(fromWorld);
        Vector2Int goal = WorldToCell(toWorld);
        if (!CellWalkable(start.x, start.y) || !CellWalkable(goal.x, goal.y)) return null;

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int> { start };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);

        bool found = start == goal;
        while (queue.Count > 0 && !found)
        {
            Vector2Int cur = queue.Dequeue();
            foreach (var d in Neighbors8)
            {
                Vector2Int next = cur + d;
                if (!CellWalkable(next.x, next.y) || visited.Contains(next)) continue;

                // Don't let a diagonal step cut across a blocked corner.
                if (d.x != 0 && d.y != 0 &&
                    (!CellWalkable(cur.x + d.x, cur.y) || !CellWalkable(cur.x, cur.y + d.y)))
                    continue;

                visited.Add(next);
                cameFrom[next] = cur;
                if (next == goal) { found = true; break; }
                queue.Enqueue(next);
            }
        }

        if (!found) return null;

        var cellPath = new List<Vector2Int> { goal };
        Vector2Int c = goal;
        while (c != start)
        {
            c = cameFrom[c];
            cellPath.Add(c);
        }
        cellPath.Reverse();

        var worldPath = new List<Vector2>(cellPath.Count);
        foreach (var cell in cellPath) worldPath.Add(CellToWorld(cell.x, cell.y));
        worldPath[worldPath.Count - 1] = toWorld; // land exactly on the sampled destination
        return worldPath;
    }
}
