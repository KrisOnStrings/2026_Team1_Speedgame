using UnityEngine;

public class TowerMenuController : MonoBehaviour
{
    public GameObject[] TowerLocations;

    private void Start()
    {
        Camera cam = Camera.main;

        float leftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        transform.position = new Vector3(leftEdge, transform.position.y, transform.position.z);
    }
}
