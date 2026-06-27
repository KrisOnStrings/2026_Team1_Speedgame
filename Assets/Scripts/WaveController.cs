using UnityEngine;
using System.Collections.Generic;

public class WaveController : MonoBehaviour
{
    public GameController gc;

    public GameObject[] AttackerPrefabs;
    public GameObject Miniboss;

    public int DefeatThreshold;

    public float TimeBeforeFirstWave;
    public float TimeBetweenWaves;
    public float MinWaveSpawnTime;
    public float MaxWaveSpawnTime;
    public float SpawnTimeReduction;

    private List<AttackerController> attackers;
    private List<GameObject> path;

    private int[] wavePoints;
    private int waveIndex;
    private int curWavePoints;
    private bool waveWait;
    private int attackersLetGo;
    private float calcMinSpawnTime;
    private float calcMaxSpawnTime;
    private int totalWavePoints;
    private int minibossWave;

    private bool minibossSpawned;
    private bool bossSpawned;

    private void Start()
    {
        waveWait = false;
    }

    private void Update()
    {
        if (waveWait && (attackers.Count == 0))
        {
            waveWait = false;
            Invoke("HandleWave", TimeBetweenWaves);
        }
    }

    public void StartWaves(List<GameObject> mapPath, int[] levelWavePoints, int mbWave)
    {
        waveIndex = 0;
        attackersLetGo = 0;
        minibossSpawned = false;
        bossSpawned = false;

        wavePoints = levelWavePoints;
        path = mapPath;
        minibossWave = mbWave;

        calcMinSpawnTime = MinWaveSpawnTime;
        calcMaxSpawnTime = MaxWaveSpawnTime;
        waveWait = false;
        curWavePoints = wavePoints[waveIndex];
        attackers = new List<AttackerController>();

        totalWavePoints = 0;
        foreach (int points in wavePoints)
            totalWavePoints += points;

        Invoke("HandleSubWave", TimeBeforeFirstWave);
    }

    public void HandleWave()
    {
        waveIndex++;

        if (waveIndex < wavePoints.Length)
        {
            curWavePoints = wavePoints[waveIndex];
            calcMinSpawnTime *= SpawnTimeReduction;
            calcMaxSpawnTime *= SpawnTimeReduction;
            HandleSubWave();
        }
        else
        {
            gc.Victory();
        }
    }

    public void HandleSubWave()
    {
        if (!minibossSpawned & (waveIndex == minibossWave))
        {
            minibossSpawned = true;
            GameObject attacker = Instantiate(Miniboss, path[0].transform.position, Quaternion.identity);
            attacker.GetComponent<AttackerController>().Initialize(new List<GameObject>(path), this, gc);
            attackers.Add(attacker.GetComponent<AttackerController>());
            curWavePoints -= attacker.GetComponent<AttackerController>().WavePoints;
        }
        else
        {
            List<GameObject> validAttackers = new List<GameObject>();
            foreach (GameObject attPrefab in AttackerPrefabs)
            {
                if (waveIndex >= attPrefab.GetComponent<AttackerController>().StartSpawnWave)
                {
                    validAttackers.Add(attPrefab);
                }
            }
            int rnd = Random.Range(0, validAttackers.Count);

            GameObject attacker = Instantiate(validAttackers[rnd], path[0].transform.position, Quaternion.identity);
            attacker.GetComponent<AttackerController>().Initialize(new List<GameObject>(path), this, gc);
            attackers.Add(attacker.GetComponent<AttackerController>());
            curWavePoints -= attacker.GetComponent<AttackerController>().WavePoints;
        }

        if (curWavePoints <= 0)
        {
            waveWait = true;
        }
        else
        {
            float rndTime = Random.Range(calcMinSpawnTime, calcMaxSpawnTime);
            Invoke("HandleSubWave", rndTime);
        }
    }

    public AttackerController GetFurthestAlongPathInRange(TowerController tower)
    {
        AttackerController bestTarget = null;
        int bestProgress = -1;

        foreach (AttackerController attacker in attackers)
        {
            float distance = Vector3.Distance(tower.transform.position, attacker.transform.position);

            if (distance > tower.GetRange())
                continue;

            if (attacker.GetCurrentWaypointIndex() > bestProgress)
            {
                bestProgress = attacker.GetCurrentWaypointIndex();
                bestTarget = attacker;
            }
        }

        return bestTarget;
    }

    public void AttackerDie(AttackerController attacker)
    {
        attackers.Remove(attacker);
    }

    public void AttackerThrough()
    {
        attackersLetGo++;

        if (attackersLetGo >= DefeatThreshold)
        {
            foreach(AttackerController attacker in attackers)
            {
                Destroy(attacker);
            }
            attackers.Clear();

            CancelInvoke("HandleSubWave");
            CancelInvoke("HandleWave");
            gc.Defeat();
        }
    }

    public string GetWaveStatus()
    {
        int curWave = waveIndex + 1;
        if (curWave > wavePoints.Length) curWave = wavePoints.Length;
        return $"{curWave} / {wavePoints.Length}";
    }

    public string GetAttackerStatus()
    {
        return $"{attackersLetGo} / {DefeatThreshold}";
    }

    public float GetWaveProgress()
    {
        if (totalWavePoints <= 0)
            return 1f;

        int remainingPoints = curWavePoints;

        for (int i = waveIndex + 1; i < wavePoints.Length; i++)
            remainingPoints += wavePoints[i];

        float progress = 1f - ((float)remainingPoints / totalWavePoints);

        return Mathf.Clamp01(progress);
    }
}
