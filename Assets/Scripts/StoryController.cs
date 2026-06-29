using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoryController : MonoBehaviour
{
    public Image Background;
    public Image[] Panels;
    public GameObject ContinuePrompt;

    public float FadeTime = 1f;

    public GameController gc;

    private int currentPanel = -1;
    private bool storyRunning = false;
    private bool isTransitioning = false;

    void Start()
    {
        ContinuePrompt.SetActive(false);
        Background.gameObject.SetActive(false);
        SetAlpha(Background, 0f);

        foreach (Image panel in Panels)
        {
            SetAlpha(panel, 0f);
        }
    }

    public void StartStory()
    {
        if (storyRunning)
            return;

        Background.gameObject.SetActive(true);
        storyRunning = true;
        StartCoroutine(StartStoryRoutine());
    }

    void Update()
    {
        if (!storyRunning || isTransitioning)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(NextPanelRoutine());
        }
    }

    IEnumerator StartStoryRoutine()
    {
        isTransitioning = true;

        yield return FadeImage(Background, 0f, 1f);

        currentPanel = 0;
        ContinuePrompt.SetActive(true);
        yield return FadeImage(Panels[currentPanel], 0f, 1f);

        isTransitioning = false;
    }

    IEnumerator NextPanelRoutine()
    {
        isTransitioning = true;

        // Fade out current panel
        yield return FadeImage(Panels[currentPanel], 1f, 0f);

        currentPanel++;

        if (currentPanel < Panels.Length)
        {
            // Fade in next panel
            yield return FadeImage(Panels[currentPanel], 0f, 1f);
            isTransitioning = false;
        }
        else
        {
            // No more panels, fade out background
            ContinuePrompt.SetActive(false);
            yield return FadeImage(Background, 1f, 0f);

            storyRunning = false;
            isTransitioning = false;

            Background.gameObject.SetActive(false);
            gc.StoryDone();
        }
    }

    IEnumerator FadeImage(Image image, float from, float to)
    {
        float t = 0f;

        while (t < FadeTime)
        {
            t += Time.deltaTime;

            float alpha = Mathf.Lerp(from, to, t / FadeTime);
            SetAlpha(image, alpha);

            yield return null;
        }

        SetAlpha(image, to);
    }

    void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}