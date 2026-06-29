using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public IsoMapGenerator mapGen;
    public Map[] Maps;
    public GameObject TitleObj;
    public TextMeshProUGUI CurrencyHUD;
    public TextMeshProUGUI FoxHUD;
    public TextMeshProUGUI TimerHUD;
    public TowerMenuController towerMenu;
    public WaveController waves;
    public VineController vines;
    public DayController dayNightCycle;
    public MusicController music;
    public int StartingCurrency;
    public GameObject VictoryObj;
    public AudioClip VictorySFX;
    public GameObject DefeatObj;
    public AudioClip DefeatSFX;
    public AudioClip MenuClickSFX;
    public GameObject FireworkPrefab;
    public Transform FireworksLoc;
    public StoryController IntroStory;
    public StoryController EndingStory;

    [SerializeField] protected int mapIndex;
    [SerializeField] protected int currency;
    [SerializeField] protected float startTime;
    private float curDayCycle;
    private float targetDayCycle;
    private List<GameObject> fireworks;

    [SerializeField] protected AudioSource m_Audio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapIndex = 0;
        curDayCycle = 0.2f;
        TitleObj.SetActive(true);
        VictoryObj.SetActive(false);
        DefeatObj.SetActive(false);
        startTime = -1;
        towerMenu.gameObject.SetActive(false);
        dayNightCycle.timeOfDay = curDayCycle;
        music.SetMusic(MusicController.MusicType.Day);

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

        if (mapIndex == 0)
        {
            IntroStory.StartStory();
        }
        else
        {
            music.SetMusic(MusicController.MusicType.Day);

            startTime = Time.time;
            curDayCycle = 0.2f;

            mapGen.Generate(Maps[mapIndex]);
            mapGen.UpdatePathSprites();

            currency = StartingCurrency;
            waves.StartWaves(mapGen.GetPath(), Maps[mapIndex].WavePoints, Maps[mapIndex].MinibossWave, Maps[mapIndex].BossWave);

            towerMenu.gameObject.SetActive(true);
            towerMenu.StartGame();

            vines.StartVines(mapGen.GetVines());
        }

        Invoke("DelayDisableTitle", 2f);
    }

    private void DelayDisableTitle()
    {
        TitleObj.SetActive(false);
    }

    public virtual void ContinueButton()
    {
        CancelInvoke("PlayFireworks");
        foreach (GameObject firework in fireworks)
        {
            Destroy(firework);
        }
        fireworks.Clear();

        towerMenu.CleanUp();
        vines.Cleanup();

        VictoryObj.SetActive(false);

        mapIndex++;

        if (mapIndex < Maps.Length)
        {

            StartButton();
        }
        else
        {
            Invoke("DelayTitleEnable", 2f);
            EndingStory.StartStory();
        }
    }

    private void DelayTitleEnable()
    {
        TitleObj.SetActive(true);
        music.SetMusic(MusicController.MusicType.Day);
    }

    public void RetryButton()
    {
        towerMenu.CleanUp();
        vines.Cleanup();
        waves.CleanUp();
        mapGen.Cleanup();

        DefeatObj.SetActive(false);

        StartButton();
    }

    public void TutorialButton()
    {
        m_Audio.PlayOneShot(MenuClickSFX);
        SceneManager.LoadScene("Tutorial");
    }

    public virtual void QuitButton()
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

    public virtual void Victory()
    {
        music.SetMusic(MusicController.MusicType.None);
        vines.StopGrowing();
        m_Audio.PlayOneShot(VictorySFX);

        VictoryObj.SetActive(true);
        startTime = -1;

        fireworks = new List<GameObject>();
        Invoke("PlayFireworks", VictorySFX.length - 2);
    }

    public void PlayFireworks()
    {
        fireworks.Add(Instantiate(FireworkPrefab, FireworksLoc.position, Quaternion.identity));
        Invoke("PlayFireworks", Random.Range(0.2f, 1f));
    }

    public virtual void Defeat()
    {
        music.SetMusic(MusicController.MusicType.None);
        m_Audio.PlayOneShot(DefeatSFX);
        DefeatObj.SetActive(true);
        vines.StopGrowing();
        startTime = -1;
    }

    public void StoryDone()
    {
        if (mapIndex < Maps.Length)
        {
            music.SetMusic(MusicController.MusicType.Day);

            startTime = Time.time;
            curDayCycle = 0.2f;

            mapGen.Generate(Maps[mapIndex]);
            mapGen.UpdatePathSprites();

            currency = StartingCurrency;
            waves.StartWaves(mapGen.GetPath(), Maps[mapIndex].WavePoints, Maps[mapIndex].MinibossWave, Maps[mapIndex].BossWave);

            towerMenu.gameObject.SetActive(true);
            towerMenu.StartGame();

            vines.StartVines(mapGen.GetVines());
        }
        else
        {
            mapIndex = 0;
        }
    }
}
