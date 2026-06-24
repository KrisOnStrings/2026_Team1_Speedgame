using UnityEngine;
using System.Collections.Generic;

public class AttackerController : MonoBehaviour
{
    public float MaxHealth = 5f;
    public float moveSpeed = 2f;

    [HideInInspector] public GameController gc;

    private List<GameObject> path;
    private int currentPoint = 0;
    private bool initialized = false;

    private float curHealth;

    public void Initialize(List<GameObject> intended_path, GameController new_gc)
    {
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
    }

    void Update()
    {
        if (!initialized)
            return;

        if (currentPoint >= path.Count)
        {
            // Reached the end of the path
            gc.FoxLetGo();
            Die();
            return;
        }

        Vector3 target = path[currentPoint].transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        // Check if we reached this waypoint
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
        gc.AttackerDie(this);
        Destroy(gameObject);
    }

    public int GetCurrentWaypointIndex()
    {
        return currentPoint;
    }
}