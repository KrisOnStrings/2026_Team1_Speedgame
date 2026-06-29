using UnityEngine;

public class CollectController : MonoBehaviour
{
    public int Amount;
    public VineController vc;

    public void OnMouseUp()
    {
        vc.HarvestGrape(this);
        Destroy(gameObject);
    }
}
