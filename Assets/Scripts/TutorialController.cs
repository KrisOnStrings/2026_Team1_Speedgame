using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TutorialController : GameController
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapIndex = 0;
        VictoryObj.SetActive(false);
        DefeatObj.SetActive(false);
        startTime = -1;
        towerMenu.gameObject.SetActive(false);
        music.SetMusic(MusicController.MusicType.Day);
        mapGen.Generate(Maps[mapIndex]);
        mapGen.UpdatePathSprites();
        currency = StartingCurrency;

        towerMenu.gameObject.SetActive(true);
        towerMenu.StartGame();

        m_Audio = GetComponent<AudioSource>();

        Invoke("TutorialStep1", 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime > -1)
        {
            CurrencyHUD.text = currency.ToString();
            FoxHUD.text = waves.GetAttackerStatus();
            TimerHUD.text = Mathf.FloorToInt(Time.time - startTime).ToString();
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

    public override void Defeat()
    {
        music.SetMusic(MusicController.MusicType.None);
        m_Audio.PlayOneShot(DefeatSFX);
        DefeatObj.SetActive(true);
        vines.StopGrowing();
        startTime = -1;
    }

    void TutorialStep1()
    {

    }
}
