using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameObject AttackerPrefab;
    public IsoMapGenerator mapGen;
    public GameObject StartButtonObj;
    public TextMeshProUGUI CurrencyHUD;
    public TextMeshProUGUI FoxHUD;
    public TextMeshProUGUI TimerHUD;
    public TowerMenuController towerMenu;
    public GameObject[] TowerPrefabs;
    public GameObject GrapePrefab;
    public int StartingCurrency;
    public float VineGrowFrequency;

    private int currency;
    private int foxesLetGo;
    private float startTime;
    private List<GameObject> towerList;
    private List<GameObject> path;
    private List<AttackerController> attackers;
    private List<GameObject> grapes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = -1;
        currency = StartingCurrency;
        foxesLetGo = 0;
        towerList = new List<GameObject>();
        attackers = new List<AttackerController>();
        grapes = new List<GameObject>();
        towerMenu.gameObject.SetActive(false);
        mapGen.Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > -1)
        {
            CurrencyHUD.text = currency.ToString();
            FoxHUD.text = foxesLetGo.ToString();
            TimerHUD.text = Mathf.FloorToInt(Time.time - startTime).ToString();
        }
    }

    public void StartButton()
    {
        StartButtonObj.SetActive(false);

        startTime = Time.time;

        path = mapGen.GeneratePath();
        InvokeRepeating("GenerateAttacker", 10, 3);

        towerMenu.gameObject.SetActive(true);
        for (int i = 0; i < TowerPrefabs.Length; i++)
        {
            GenerateTower(i);
        }

        InvokeRepeating("VineGrow", 0.5f, VineGrowFrequency);
    }

    public void GenerateAttacker()
    {
        GameObject attacker = Instantiate(AttackerPrefab, path[0].transform.position, Quaternion.identity);
        attacker.GetComponent<AttackerController>().Initialize(new List<GameObject>(path), this);
        attackers.Add(attacker.GetComponent<AttackerController>());
    }

    public void TowerPlaced(int towerIndex)
    {
        GenerateTower(towerIndex);
    }

    public void GenerateTower(int index)
    {
        GameObject tower = Instantiate(TowerPrefabs[index]);
        tower.transform.position = towerMenu.TowerLocations[index].transform.position;
        tower.GetComponent<TowerController>().gc = this;
        tower.GetComponent<TowerController>().towerIndex = index;
        towerList.Add(tower);
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

    public int GetCurrency()
    {
        return currency;
    }

    public void BuyTower(int cost)
    {
        currency -= cost;
    }

    public void VineGrow()
    {
        List<GameObject> vines = mapGen.GetVines();

        int rnd = Random.Range(0, vines.Count);
        GameObject grape = Instantiate(GrapePrefab, vines[rnd].transform.position, Quaternion.identity);
        grape.GetComponent<CollectController>().gc = this;
        grapes.Add(grape);
    }

    public void HarvestGrape(CollectController grape)
    {
        currency += grape.Amount;
        grapes.Remove(grape.gameObject);
    }

    public void FoxLetGo()
    {
        foxesLetGo++;
    }
}
