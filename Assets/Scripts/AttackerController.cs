using UnityEngine;
using System.Collections.Generic;

public class AttackerController : MonoBehaviour
{
    public int WavePoints;
    public float MaxHealth = 5f;
    public float moveSpeed = 2f;
    public int StartSpawnWave;

    public float LeftXScale = 1;
    public float LeftYScale = 1;
    public float LeftZRot = 25f;
    public float LeftAnimDir = 1;
    public float UpXScale = 1;
    public float UpYScale = 1;
    public float UpZRot = -15f;
    public float UpAnimDir = 1;
    public float DownXScale = -1;
    public float DownYScale = 1;
    public float DownZRot = -40f;
    public float DownAnimDir = 1;
    public float RightXScale = -1;
    public float RightYScale = 1;
    public float RightZRot = 25f;
    public float RightAnimDir = 1;

    [HideInInspector] public WaveController wc;
    [HideInInspector] public GameController gc;

    private List<GameObject> path;
    private int currentPoint = 0;
    private bool initialized = false;

    private float curHealth;
    private float origXScale;
    private float origYScale;

    private Animator m_Anim;

    public void Initialize(List<GameObject> intended_path, WaveController new_wc, GameController new_gc)
    {
        m_Anim = GetComponent<Animator>();

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
                transform.localScale = new Vector3(LeftXScale * origXScale, LeftXScale * origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, LeftZRot);
                m_Anim.SetFloat("Dir", LeftAnimDir);
            }
            else
            {
                // Up (world up-left)
                transform.localScale = new Vector3(UpXScale * origXScale, UpYScale * origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, UpZRot);
                m_Anim.SetFloat("Dir", UpAnimDir);
            }
        }
        else
        {
            if (dir.y > 0)
            {
                // Down (world down-right)
                transform.localScale = new Vector3(DownXScale * origXScale, DownYScale * origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, DownZRot);
                m_Anim.SetFloat("Dir", DownAnimDir);
            }
            else
            {
                // Right (world up-right)
                transform.localScale = new Vector3(RightXScale * origXScale, RightYScale * origYScale, 1);
                transform.rotation = Quaternion.Euler(0, 0, RightZRot);
                m_Anim.SetFloat("Dir", RightAnimDir);
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