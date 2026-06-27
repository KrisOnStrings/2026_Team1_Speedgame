using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float timeOfDay = 0f;

    public Light2D globalLight;
    public float dayIntensity = 1.0f;
    public float nightIntensity = 0.15f;
    public Color daySkyColor = new Color(0.45f, 0.75f, 1.0f);
    public Color nightSkyColor = Color.black;

    public Transform sun;
    public Transform moon;

    public Vector3 skyCenter = new Vector3(0, -3, 0);
    public float horizontalRadius = 14f;
    public float verticalRadius = 7f;

    [Header("Moon")]
    public float moonOffset = 0.5f;   // Opposite side of the sky

    void Update()
    {
        UpdateLighting();
        UpdateCelestialBodies();
    }

    void UpdateLighting()
    {
        // Peaks at noon, lowest at midnight
        float daylight = Mathf.Clamp01(Mathf.Sin(timeOfDay * Mathf.PI * 1.25f));

        globalLight.intensity = Mathf.Lerp(
            nightIntensity,
            dayIntensity,
            daylight);

        Camera.main.backgroundColor = Color.Lerp(nightSkyColor, daySkyColor, daylight);
    }

    void UpdateCelestialBodies()
    {
        PositionBody(sun, timeOfDay);
        PositionBody(moon, Mathf.Repeat(timeOfDay + moonOffset, 1f));
    }

    void PositionBody(Transform body, float t)
    {
        if (body == null)
            return;

        // 0 = sunrise (left horizon)
        // 0.25 = noon (top)
        // 0.5 = sunset (right horizon)
        // 0.75 = below the world
        float angle = 180f - (t * 360f);

        float radians = angle * Mathf.Deg2Rad;

        Vector3 pos = skyCenter;
        pos.x += Mathf.Cos(radians) * horizontalRadius;
        pos.y += Mathf.Sin(radians) * verticalRadius;

        body.position = pos;

        // Rotate to face along its path (optional)
        body.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }
}