using UnityEngine;

public class CollectController : MonoBehaviour
{
    public int Amount;

    [HideInInspector] public VineController vc;

    public void OnMouseDown()
    {
        vc.HarvestGrape(this);
        Destroy(gameObject);
    }
}
