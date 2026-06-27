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
    public VineController vines;
    public DayController dayNightCycle;
    public int StartingCurrency;
    public GameObject VictoryObj;
    public AudioClip VictorySFX;
    public GameObject GameOverObj;
    public GameObject DefeatObj;
    public AudioClip DefeatSFX;
    public AudioClip MenuClickSFX;

    private int mapIndex;
    private int currency;
    private float startTime;
    private float curDayCycle;
    private float targetDayCycle;

    private AudioSource m_Audio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapIndex = 0;
        curDayCycle = 0.2f;
        VictoryObj.SetActive(false);
        DefeatObj.SetActive(false);
        startTime = -1;
        towerMenu.gameObject.SetActive(false);
        dayNightCycle.timeOfDay = curDayCycle;

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

            // Update Day Night Cycle
            // Level Start: 0.2
            // Level End: 0.8
            targetDayCycle = Mathf.Lerp(0.2f, 0.8f, waves.GetWaveProgress());

            if (targetDayCycle > curDayCycle)
            {
                curDayCycle += Time.deltaTime * .0075f;
            }

            dayNightCycle.timeOfDay = curDayCycle;
        }
    }

    public void StartButton()
    {
        m_Audio.PlayOneShot(MenuClickSFX);
        StartButtonObj.SetActive(false);

        startTime = Time.time;
        curDayCycle = 0.2f;

        mapGen.Generate(Maps[mapIndex]);
        mapGen.UpdatePathSprites();

        currency = StartingCurrency;
        waves.StartWaves(mapGen.GetPath(), Maps[mapIndex].WavePoints);

        towerMenu.gameObject.SetActive(true);
        towerMenu.StartGame();

        vines.StartVines(mapGen.GetVines());
    }

    public void ContinueButton()
    {
        mapIndex++;

        towerMenu.CleanUp();
        vines.Cleanup();

        VictoryObj.SetActive(false);

        StartButton();
    }

    public void RetryButton()
    {
        towerMenu.CleanUp();
        vines.Cleanup();

        DefeatObj.SetActive(false);

        StartButton();
    }

    public void QuitButton()
    {
        m_Audio.PlayOneShot(MenuClickSFX);
        Application.Quit();
    }

    public int GetCurrency()
    {
        return currency;
    }

    public void SpendCurrency(int cost)
    {
        currency -= cost;
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
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
