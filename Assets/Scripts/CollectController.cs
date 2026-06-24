using UnityEngine;

public class CollectController : MonoBehaviour
{
    public int Amount;

    [HideInInspector] public GameController gc;

    public void OnMouseDown()
    {
        gc.HarvestGrape(this);
        Destroy(gameObject);
    }
}
