using UnityEngine;
using System.Collections.Generic;

public class IsoMapGenerator : MonoBehaviour
{
    public TextAsset mapFile;
    public GameObject TilePrefab;

    public Sprite grassSprite;
    public Sprite pathSprite;
    public Sprite spawnSprite;
    public Sprite exitSprite;
    public Sprite vineSprite;

    public float tileWidth = 1.32f;
    public float tileHeight = 0.66f;

    private char[,] map;
    private Vector3 mapCenter;
    private List<GameObject> vines;

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

        vines = new List<GameObject>();

        string[] rows = mapFile.text.Replace("\r", "").Trim().Split('\n');

        int height = rows.Length;
        int width = rows[0].Length;

        map = new char[width, height];

        mapCenter = IsoToWorld((width - 1) / 2f, (height - 1) / 2f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, y] = rows[y][x];
            }
        }

        // Create visual tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateTile(x, y, map[x, y]);
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
            case 'S':
                sprite = spawnSprite != null
                    ? spawnSprite
                    : pathSprite;
                break;
            case 'E':
                sprite = exitSprite != null
                    ? exitSprite
                    : pathSprite;
                break;
            case 'V':
                sprite = vineSprite;
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

        if (isVine) vines.Add(tileObj);
    }

    public List<GameObject> GeneratePath()
    {
        List<GameObject> waypoints = new List<GameObject>();

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

    public List<GameObject> GetVines()
    {
        return vines;
    }
}