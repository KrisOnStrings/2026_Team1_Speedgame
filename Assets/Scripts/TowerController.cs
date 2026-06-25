using UnityEngine;

public class TowerController : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public int cost;

    public int Range;
    public int Damage;
    public float AttackSpeed;

    public float TileYOffset;

    [HideInInspector] public GameController gc;
    [HideInInspector] public int towerIndex;

    private bool placed;
    private bool followMouse;
    private bool canPlace;
    private float zDistance;
    private Vector3 origPos;

    void Start()
    {
        placed = false;
        canPlace = false;
        zDistance = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
    }

    void Update()
    {
        if (followMouse)
        {
            Vector2 mousePos =
                Camera.main.ScreenToWorldPoint(
                    Input.mousePosition);

            RaycastHit2D hit =
                Physics2D.Raycast(
                    mousePos,
                    Vector2.zero);

            if (hit.collider != null)
            {
                TileController tile = hit.collider.GetComponent<TileController>();

                if (tile != null)
                {
                    canPlace = true;
                    transform.position = tile.transform.position + new Vector3(0, TileYOffset);
                    SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();

                    if (!tile.isOccupied && tile.isPlaceable)
                    {
                        foreach(SpriteRenderer rend in rends)
                        {
                            rend.color = Color.white;
                        }
                    }
                    else
                    {
                        canPlace = false;
                        foreach (SpriteRenderer rend in rends)
                        {
                            rend.color = Color.red;
                        }
                    }
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (placed)
            return;

        if (gc.GetCurrency() >= cost)
        {
            followMouse = true;
            origPos = transform.position;
        }
    }

    private void OnMouseUp()
    {
        if (followMouse)
        {
            followMouse = false;

            SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer rend in rends)
            {
                rend.color = Color.white;
            }

            if (canPlace)
            {
                placed = true;
                gc.BuyTower(cost);
                gc.TowerPlaced(towerIndex);
                InvokeRepeating("CheckAttack", AttackSpeed, AttackSpeed);
            }
            else
            {
                transform.position = origPos;
            }
        }
    }

    void CheckAttack()
    {
        AttackerController attacker = gc.waves.GetFurthestAlongPathInRange(this);

        if (attacker != null)
        {
            GameObject projectile = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<ProjectileController>().Target = attacker;
            projectile.GetComponent<ProjectileController>().Damage = Damage;
        }
    }
}