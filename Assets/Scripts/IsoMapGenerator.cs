using UnityEngine;
using System.Collections.Generic;

public class IsoMapGenerator : MonoBehaviour
{
    public TextAsset mapFile;
    public GameObject TilePrefab;
    public GameObject VinePrefab;

    public Sprite grassSprite;
    public Sprite pathSprite;
    public Sprite pathVerticalSprite;
    public Sprite pathHorizontalSprite;
    public Sprite pathCornerUpRightSprite;
    public Sprite pathCornerUpLeftSprite;
    public Sprite pathCornerDownRightSprite;
    public Sprite pathCornerDownLeftSprite;
    public Sprite spawnSprite;
    public Sprite exitSprite;

    public float tileWidth = 1.32f;
    public float tileHeight = 0.66f;

    private char[,] map;
    private Vector3 mapCenter;
    private List<GameObject> vines;
    private List<GameObject> waypoints;
    private Dictionary<Vector2Int, GameObject> tileLookup = new Dictionary<Vector2Int, GameObject>();

    void DestroyExisting()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    public void Generate()
    {
        DestroyExisting();
        tileLookup.Clear();
        vines = new List<GameObject>();

        string[] rows =
            mapFile.text
                .Replace("\r", "")
                .Trim()
                .Split('\n');

        int height = rows.Length;
        int width = rows[0].Length;

        map = new char[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = rows[y][x];
            }
        }

        // Find left-most map position
        float mapLeft = float.MaxValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = IsoToWorld(x, y);

                if (pos.x < mapLeft)
                {
                    mapLeft = pos.x;
                }
            }
        }

        // Left edge of camera view
        float screenLeft =
            Camera.main.transform.position.x -
            Camera.main.orthographicSize *
            Camera.main.aspect;

        // Small margin so tiles aren't touching edge
        float padding = tileWidth * 0.5f;

        // Amount to shift map
        float shiftX =
            mapLeft -
            (screenLeft + padding);

        // Keep map vertically centered
        mapCenter = new Vector3(
            shiftX,
            IsoToWorld(
                (width - 1) / 2f,
                (height - 1) / 2f).y,
            0
        );

        // Create visual tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateTile(
                    x,
                    y,
                    map[x, y]
                );
            }
        }

        GeneratePath();
    }

    void CreateTile(int x, int y, char type)
    {
        Sprite sprite = null;
        bool placeable = false;
        bool isVine = false;

        switch (type)
        {
            case 'G':
                sprite = grassSprite;
                placeable = true;
                break;
            case 'P':
                sprite = pathSprite;
                break;
            case 'V':
                sprite = pathVerticalSprite;
                break;
            case 'H':
                sprite = pathHorizontalSprite;
                break;
            case 'Q':
                sprite = pathCornerDownRightSprite;
                break;
            case 'W':
                sprite = pathCornerDownLeftSprite;
                break;
            case 'R':
                sprite = pathCornerUpRightSprite;
                break;
            case 'T':
                sprite = pathCornerUpLeftSprite;
                break;
            case 'S':
                sprite = spawnSprite != null
                    ? spawnSprite
                    : pathVerticalSprite;
                break;
            case 'E':
                sprite = exitSprite != null
                    ? exitSprite
                    : pathVerticalSprite;
                break;
            case 'F':
                sprite = grassSprite;
                isVine = true;
                break;
        }

        if (sprite == null)
            return;

        GameObject tileObj = Instantiate(TilePrefab, IsoToWorld(x, y) - mapCenter, Quaternion.identity, transform);
        tileObj.name = $"Tile_{x}_{y}";
        tileObj.GetComponent<SpriteRenderer>().sprite = sprite;
        tileObj.GetComponent<SpriteRenderer>().sortingOrder = -(x + y);
        tileObj.GetComponent<TileController>().isPlaceable = placeable;
        tileObj.GetComponent<TileController>().isOccupied = false;
        tileLookup[new Vector2Int(x, y)] = tileObj;

        if (isVine)
        {
            Instantiate(VinePrefab, IsoToWorld(x, y) - mapCenter, Quaternion.identity, transform);
            vines.Add(tileObj);
        }
    }

    public List<GameObject> GeneratePath()
    {
        waypoints = new List<GameObject>();

        Vector2Int start = FindTile('S');
        Vector2Int end = FindTile('E');

        if (start.x < 0 || end.x < 0)
        {
            Debug.LogError("Missing Spawn or Exit");
            return null;
        }

        List<Vector2Int> gridPath = FindPath(start, end);

        foreach (Vector2Int point in gridPath)
        {
            GameObject waypoint = new GameObject(
                $"Waypoint_{point.x}_{point.y}"
            );

            waypoint.transform.parent = transform;

            waypoint.transform.position =
                IsoToWorld(
                    point.x,
                    point.y
                ) - mapCenter;

            waypoints.Add(waypoint);
        }

        return waypoints;
    }

    Vector2Int FindTile(char target)
    {
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                if (map[x, y] == target)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    // Simple BFS path finder
    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };


        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
                break;

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = current + dir;

                if (!Inside(next))
                    continue;


                if (cameFrom.ContainsKey(next))
                    continue;

                char tile = map[next.x, next.y];

                if (tile != 'P' && tile != 'S' && tile != 'E')
                    continue;

                cameFrom[next] = current;

                queue.Enqueue(next);
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();

        if (!cameFrom.ContainsKey(end))
            return path;

        Vector2Int p = end;

        while (p != start)
        {
            path.Add(p);
            p = cameFrom[p];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }

    bool Inside(Vector2Int p)
    {
        return
            p.x >= 0 &&
            p.y >= 0 &&
            p.x < map.GetLength(0) &&
            p.y < map.GetLength(1);
    }

    Vector3 IsoToWorld(float x, float y)
    {
        return new Vector3(
            (x - y) * tileWidth * 0.5f,
            (x + y) * tileHeight * 0.5f,
            0
        );
    }

    public List<GameObject> GetPath()
    {
        return waypoints;
    }

    public List<GameObject> GetVines()
    {
        return vines;
    }

    public void UpdatePathSprites()
    {
        Vector2Int start = FindTile('S');
        Vector2Int end = FindTile('E');

        List<Vector2Int> path =
            FindPath(start, end);

        if (path.Count < 3)
            return;

        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2Int prev = path[i - 1];
            Vector2Int curr = path[i];
            Vector2Int next = path[i + 1];

            Vector2Int dirIn =
                curr - prev;

            Vector2Int dirOut =
                next - curr;

            Sprite sprite = null;

            // Straight vertical
            if (
                (dirIn == Vector2Int.up &&
                 dirOut == Vector2Int.up) ||
                (dirIn == Vector2Int.down &&
                 dirOut == Vector2Int.down)
            )
            {
                sprite = pathVerticalSprite;
            }
            // Straight horizontal
            else if (
                (dirIn == Vector2Int.left &&
                 dirOut == Vector2Int.left) ||
                (dirIn == Vector2Int.right &&
                 dirOut == Vector2Int.right)
            )
            {
                sprite = pathHorizontalSprite;
            }
            // Down -> Right
            else if (
                (dirIn == Vector2Int.up &&
                 dirOut == Vector2Int.right) ||
                (dirIn == Vector2Int.left &&
                 dirOut == Vector2Int.down)
            )
            {
                sprite = pathCornerDownRightSprite;
            }
            // Down -> Left
            else if (
                (dirIn == Vector2Int.up &&
                 dirOut == Vector2Int.left) ||
                (dirIn == Vector2Int.right &&
                 dirOut == Vector2Int.down)
            )
            {
                sprite = pathCornerDownLeftSprite;
            }
            // Up -> Right
            else if (
                (dirIn == Vector2Int.down &&
                 dirOut == Vector2Int.right) ||
                (dirIn == Vector2Int.left &&
                 dirOut == Vector2Int.up)
            )
            {
                sprite = pathCornerUpRightSprite;
            }
            // Up -> Left
            else if (
                (dirIn == Vector2Int.down &&
                 dirOut == Vector2Int.left) ||
                (dirIn == Vector2Int.right &&
                 dirOut == Vector2Int.up)
            )
            {
                sprite = pathCornerUpLeftSprite;
            }

            if (sprite != null &&
                tileLookup.TryGetValue(
                    curr,
                    out GameObject tile))
            {
                tile.GetComponent<SpriteRenderer>()
                    .sprite = sprite;
            }
        }
    }
}