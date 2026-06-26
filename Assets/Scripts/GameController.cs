using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public IsoMapGenerator mapGen;
    public Map[] Maps;
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
    public AudioClip VictorySFX;
    public GameObject GameOverObj;
    public GameObject DefeatObj;
    public AudioClip DefeatSFX;
    public AudioClip MenuClickSFX;

    private int mapIndex;
    private int currency;
    private float startTime;
    private List<GameObject> towerList;
    private List<GameObject> grapes;
    private List<GameObject> vines;
    private List<int> availableVines;

    private AudioSource m_Audio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapIndex = 0;
        VictoryObj.SetActive(false);
        DefeatObj.SetActive(false);
        startTime = -1;
        towerList = new List<GameObject>();
        grapes = new List<GameObject>();
        towerMenu.gameObject.SetActive(false);

        m_Audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > -1)
        {
            CurrencyHUD.text = currency.ToString();
            FoxHUD.text = waves.GetAttackerStatus();
            TimerHUD.text = Mathf.FloorToInt(Time.time - startTime).ToString();
            WaveHUD.text = waves.GetWaveStatus();
        }
    }

    public void StartButton()
    {
        m_Audio.PlayOneShot(MenuClickSFX);
        StartButtonObj.SetActive(false);

        startTime = Time.time;

        mapGen.Generate(Maps[mapIndex]);
        mapGen.UpdatePathSprites();

        currency = StartingCurrency;
        waves.StartWaves(mapGen.GetPath(), Maps[mapIndex].WavePoints);

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

    public void ContinueButton()
    {
        mapIndex++;
        foreach(GameObject tower in towerList)
        {
            Destroy(tower);
        }
        towerList.Clear();

        foreach (GameObject grape in grapes)
        {
            Destroy(grape);
        }
        grapes.Clear();

        VictoryObj.SetActive(false);

        StartButton();
    }

    public void QuitButton()
    {
        m_Audio.PlayOneShot(MenuClickSFX);
        Application.Quit();
    }

    public void TowerPlaced(int towerIndex)
    {
        GenerateTower(towerIndex);
    }

    public void GenerateTower(int index)
    {
        GameObject tower = Instantiate(TowerPrefabs[index], towerMenu.transform);
        tower.transform.position = towerMenu.TowerLocations[index].transform.position;
        tower.name = TowerPrefabs[index].name;
        tower.GetComponent<TowerController>().gc = this;
        tower.GetComponent<TowerController>().towerIndex = index;
        towerList.Add(tower);
    }

    public int GetCurrency()
    {
        return currency;
    }

    public void SpendCurrency(int cost)
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

    public void Victory()
    {
        CancelInvoke("VineGrow");
        m_Audio.PlayOneShot(VictorySFX);

        if (mapIndex < (Maps.Length - 1))
        {
            VictoryObj.SetActive(true);
        }
        else
        {
            GameOverObj.SetActive(true);
        }
        startTime = -1;
    }

    public void Defeat()
    {
        m_Audio.PlayOneShot(DefeatSFX);
        DefeatObj.SetActive(true);
        CancelInvoke("VineGrow");
        startTime = -1;
    }
}
