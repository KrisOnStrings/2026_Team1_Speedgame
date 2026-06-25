using UnityEngine;
using System.Collections.Generic;

public class WaveController : MonoBehaviour
{
    public GameController gc;

    public GameObject[] AttackerPrefabs;
    public int[] WavePoints;

    public float TimeBeforeFirstWave;
    public float TimeBetweenWaves;
    public float MinWaveSpawnTime;
    public float MaxWaveSpawnTime;

    private List<AttackerController> attackers;
    private List<GameObject> path;

    private int waveIndex;
    private int curWavePoints;
    private bool waveWait;

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

    public void StartWaves(List<GameObject> mapPath)
    {
        waveIndex = 0;
        waveWait = false;
        curWavePoints = WavePoints[waveIndex];
        attackers = new List<AttackerController>();

        path = mapPath;

        Invoke("HandleSubWave", TimeBeforeFirstWave);
    }

    public void HandleWave()
    {
        waveIndex++;

        if (waveIndex < WavePoints.Length)
        {
            curWavePoints = WavePoints[waveIndex];
            HandleSubWave();
        }
        else
        {
            gc.Victory();
        }
    }

    public void HandleSubWave()
    {
        int rnd = Random.Range(0, AttackerPrefabs.Length);

        GameObject attacker = Instantiate(AttackerPrefabs[rnd], path[0].transform.position, Quaternion.identity);
        attacker.GetComponent<AttackerController>().Initialize(new List<GameObject>(path), this, gc);
        attackers.Add(attacker.GetComponent<AttackerController>());

        curWavePoints -= attacker.GetComponent<AttackerController>().WavePoints;

        if (curWavePoints <= 0)
        {
            waveWait = true;
        }
        else
        {
            float rndTime = Random.Range(MinWaveSpawnTime, MaxWaveSpawnTime);
            Invoke("HandleSubWave", rndTime);
        }
    }

    public AttackerController GetFurthestAlongPathInRange(TowerController tower)
    {
        AttackerController bestTarget = null;
        int bestProgress = -1;

        foreach (AttackerController attacker in attackers)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    attacker.transform.position);

            if (distance > tower.Range)
                continue;

            if (attacker.GetCurrentWaypointIndex() >
                bestProgress)
            {
                bestProgress =
                    attacker.GetCurrentWaypointIndex();

                bestTarget =
                    attacker;
            }
        }

        return bestTarget;
    }

    public void AttackerDie(AttackerController attacker)
    {
        attackers.Remove(attacker);
    }

    public string GetWaveStatus()
    {
        int curWave = waveIndex + 1;
        if (curWave > WavePoints.Length) curWave = WavePoints.Length;
        return $"{waveIndex + 1} / {WavePoints.Length}";
    }
}
