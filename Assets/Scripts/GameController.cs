using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public IsoMapGenerator mapGen;
    public GameObject StartButtonObj;
    public TextMeshProUGUI CurrencyHUD;
    public TextMeshProUGUI FoxHUD;
    public TextMeshProUGUI TimerHUD;
    public TextMeshProUGUI WaveHUD;
    public TowerMenuController towerMenu;
    public WaveController waves;
    public GameObject[] TowerPrefabs;
    public GameObject GrapePrefab;
    public int StartingCurrency;
    public float VineGrowFrequency;
    public GameObject VictoryObj;

    private int currency;
    private int foxesLetGo;
    private float startTime;
    private List<GameObject> towerList;
    private List<GameObject> grapes;
    private List<GameObject> vines;
    private List<int> availableVines;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VictoryObj.SetActive(false);
        startTime = -1;
        currency = StartingCurrency;
        foxesLetGo = 0;
        towerList = new List<GameObject>();
        grapes = new List<GameObject>();
        towerMenu.gameObject.SetActive(false);
        mapGen.Generate();
        mapGen.UpdatePathSprites();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > -1)
        {
            CurrencyHUD.text = currency.ToString();
            FoxHUD.text = foxesLetGo.ToString();
            TimerHUD.text = Mathf.FloorToInt(Time.time - startTime).ToString();
            WaveHUD.text = waves.GetWaveStatus();
        }
    }

    public void StartButton()
    {
        StartButtonObj.SetActive(false);

        startTime = Time.time;

        waves.StartWaves(mapGen.GetPath());

        towerMenu.gameObject.SetActive(true);
        for (int i = 0; i < TowerPrefabs.Length; i++)
        {
            GenerateTower(i);
        }

        vines = mapGen.GetVines();
        availableVines = new List<int>();
        for (int i = 0; i < vines.Count; i++) availableVines.Add(i);

        InvokeRepeating("VineGrow", 0.5f, VineGrowFrequency);
    }

    public void QuitButton()
    {
        Application.Quit();
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
        if (availableVines.Count > 0)
        {
            int rnd = Random.Range(0, availableVines.Count);
            int vineIndex = availableVines[rnd];

            GameObject grape = Instantiate(GrapePrefab, vines[vineIndex].transform.position, Quaternion.identity);
            grape.GetComponent<CollectController>().gc = this;
            grape.name = vineIndex.ToString();
            grapes.Add(grape);

            availableVines.RemoveAt(rnd);
        }
    }

    public void HarvestGrape(CollectController grape)
    {
        currency += grape.Amount;
        grapes.Remove(grape.gameObject);

        availableVines.Add(int.Parse(grape.name));
    }

    public void FoxLetGo()
    {
        foxesLetGo++;
    }

    public void Victory()
    {
        VictoryObj.SetActive(true);

        CancelInvoke("VineGrow");
    }
}
