using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TutorialController : GameController
{
    public GameObject[] TutorialObjs;

    private int tutorialStep;
    private int numFoxesDefeated;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(GameObject tut in TutorialObjs)
        {
            tut.SetActive(false);
        }

        tutorialStep = 0;
        mapIndex = 0;
        VictoryObj.SetActive(false);
        startTime = -1;
        towerMenu.gameObject.SetActive(false);
        music.SetMusic(MusicController.MusicType.Day);
        mapGen.Generate(Maps[mapIndex]);
        mapGen.UpdatePathSprites();
        currency = StartingCurrency;

        m_Audio = GetComponent<AudioSource>();

        Invoke("TutorialStep1", 1f);
    }

    void TutorialStep1()        // Beloved Welcome
    {
        tutorialStep = 1;
        TutorialObjs[0].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        CurrencyHUD.text = currency.ToString();
        FoxHUD.text = waves.GetAttackerStatus();
        TimerHUD.text = Mathf.FloorToInt(Time.time - startTime).ToString();

        switch (tutorialStep)
        {
            case 1:
                if (Input.GetMouseButtonDown(0))        // Stop the Foxes
                {
                    TutorialObjs[0].SetActive(false);
                    TutorialObjs[1].SetActive(true);
                    tutorialStep = 2;
                }
                break;
            case 2:
                if (Input.GetMouseButtonDown(0))        // Introduce Danger Dove.
                {
                    TutorialObjs[1].SetActive(false);
                    TutorialObjs[2].SetActive(true);
                    tutorialStep = 3;
                }
                break;
            case 3:
                if (Input.GetMouseButtonDown(0))        // Click a tower
                {
                    TutorialObjs[2].SetActive(false);
                    towerMenu.gameObject.SetActive(true);
                    towerMenu.AddFirstTower();
                    TutorialObjs[3].SetActive(true);
                    tutorialStep = 4;
                }
                break;
            case 6:
                if (Input.GetMouseButtonDown(0))        // Test Danger Dove's tower with a fox
                {
                    TutorialObjs[5].SetActive(false);
                    tutorialStep = 7;
                    waves.TutorialSpawn1Fox(mapGen.GetPath());
                }
                break;
            case 8:
                if (Input.GetMouseButtonDown(0))        // Help from Ramrock
                {
                    TutorialObjs[6].SetActive(false);
                    TutorialObjs[7].SetActive(true);
                    tutorialStep = 9;
                }
                break;
            case 9:
                if (Input.GetMouseButtonDown(0))        // Pick 4 bunches of grapes
                {
                    TutorialObjs[7].SetActive(false);
                    TutorialObjs[8].SetActive(true);
                    tutorialStep = 10;
                }
                break;
            case 10:
                if (currency >= 4)                      // Place Ramrock's Tower
                {
                    TutorialObjs[8].SetActive(false);
                    TutorialObjs[9].SetActive(true);
                    towerMenu.AddSecondTower();
                    tutorialStep = 11;
                }
                break;
            case 12:
                if (Input.GetMouseButtonDown(0))        // Test Ramrock's Tower against 2 foxes
                {
                    TutorialObjs[10].SetActive(false);
                    numFoxesDefeated = 0;
                    waves.TutorialSpawn2Foxes();
                    tutorialStep = 13;
                }
                break;
            case 14:
                if (Input.GetMouseButtonDown(0))        // Done with Tutorial
                {
                    TutorialObjs[11].SetActive(false);
                    Victory();
                }
                break;
        }
    }

    public int GetTutorialStep()
    {
        return tutorialStep;
    }

    public void ClickTower()
    {
        TutorialObjs[3].SetActive(false);
        TutorialObjs[4].SetActive(true);
        tutorialStep = 5;
    }

    public void PlaceDangerDoveTower()
    {
        TutorialObjs[4].SetActive(false);
        TutorialObjs[5].SetActive(true);
        tutorialStep = 6;
    }

    public void PlaceRamrockTower()
    {
        TutorialObjs[9].SetActive(false);
        TutorialObjs[10].SetActive(true);
        tutorialStep = 12;
    }

    public void FoxDefeated()
    {
        TutorialObjs[6].SetActive(true);
        tutorialStep = 8;
    }

    public void FoxesDefeated()
    {
        numFoxesDefeated++;

        if (numFoxesDefeated >= 2)
        {
            TutorialObjs[11].SetActive(true);
            tutorialStep = 14;
        }
    }

    public override void ContinueButton()
    {
        mapIndex++;

        towerMenu.CleanUp();
        vines.Cleanup();

        VictoryObj.SetActive(false);

        SceneManager.LoadScene("Main");
    }

    public override void QuitButton()
    {
        m_Audio.PlayOneShot(MenuClickSFX);
        SceneManager.LoadScene("Main");
    }

    public override void Victory()
    {
        music.SetMusic(MusicController.MusicType.None);
        vines.StopGrowing();
        m_Audio.PlayOneShot(VictorySFX);
        VictoryObj.SetActive(true);
    }
}
