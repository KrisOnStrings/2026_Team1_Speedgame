using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Splash : MonoBehaviour
{
    public RawImage Border;
    public RawImage Background;
    public TextMeshProUGUI WaveText;
    public float FadeTime = 1f;
    public float HoldTime = 2f;

    private Coroutine fadeRoutine;

    void Start()
    {
        SetAlpha(0f);
    }

    public void Show(string text)
    {
        WaveText.text = text;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        // Fade In
        yield return Fade(0f, 1f);

        // Hold
        yield return new WaitForSeconds(HoldTime);

        // Fade Out
        yield return Fade(1f, 0f);

        fadeRoutine = null;
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float t = 0f;

        while (t < FadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t / FadeTime);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(endAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color c = Border.color;
        c.a = alpha;
        Border.color = c;

        c = Background.color;
        c.a = alpha;
        Background.color = c;

        c = WaveText.color;
        c.a = alpha;
        WaveText.color = c;
    }
}