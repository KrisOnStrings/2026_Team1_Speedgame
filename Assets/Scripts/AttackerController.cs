using UnityEngine;
using System.Collections.Generic;

public class AttackerController : MonoBehaviour
{
    public int WavePoints;
    public float MaxHealth = 5f;
    public float moveSpeed = 2f;

    [HideInInspector] public WaveController wc;
    [HideInInspector] public GameController gc;

    private List<GameObject> path;
    private int currentPoint = 0;
    private bool initialized = false;

    private float curHealth;
    private float origXScale;
    private float origYScale;

    public void Initialize(List<GameObject> intended_path, WaveController new_wc, GameController new_gc)
    {
        wc = new_wc;
        gc = new_gc;
        path = intended_path;

        if (path == null || path.Count == 0)
        {
            Debug.LogError("Attacker path is empty!");
            Destroy(gameObject);
            return;
        }

        // Start at the first waypoint
        transform.position = path[0].transform.position;

        currentPoint = 1;
        initialized = true;

        curHealth = MaxHealth;

        origXScale = transform.localScale.x;
        origYScale = transform.localScale.y;
    }

    void Update()
    {
        if (!initialized)
            return;

        if (currentPoint >= path.Count)
        {
            // Reached the end of the path
            wc.AttackerThrough();
            Die();
            return;
        }

        Vector3 target = path[currentPoint].transform.position;

        // Direction toward current waypoint
        Vector3 dir = (target - transform.position).normalized;

        // Isometric direction detection
        if (dir.x > 0)
        {
            if (dir.y > 0)
            {
                // Left (world down-left)
                transform.localScale = new Vector3(origXScale, origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, 25f);
            }
            else
            {
                // Up (world up-left)
                transform.localScale = new Vector3(origXScale, origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, -40f);
            }
        }
        else
        {
            if (dir.y > 0)
            {
                // Down (world down-right)
                transform.localScale = new Vector3(-origXScale, origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, -40f);
            }
            else
            {
                // Right (world up-right)
                transform.localScale = new Vector3(-origXScale, origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, 25f);
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Reached waypoint?
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            currentPoint++;
        }
    }

    public void TakeDamage(int damage)
    {
        curHealth -= damage;

        if (curHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        wc.AttackerDie(this);
        Destroy(gameObject);
    }

    public int GetCurrentWaypointIndex()
    {
        return currentPoint;
    }
}