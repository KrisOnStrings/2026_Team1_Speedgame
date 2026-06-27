using TMPro;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public TextMeshPro PreTowerHUD;
    public TextMeshPro PostTowerHUD;
    public TextMeshPro PostTowerUpgradeHUD;
    public AudioClip MisPlaceSFX;
    public AudioClip WhooshSFX;

    public int PlaceCost;
    public int UpgradeCost;

    public int Range;
    public int Damage;
    public float AttackSpeed;

    public int UpgradeRange;
    public int UpgradeDamage;
    public float UpgradeAttackSpeed;

    public float TileYOffset;

    [HideInInspector] public TowerMenuController tmc;
    [HideInInspector] public GameController gc;
    [HideInInspector] public int towerIndex;

    private bool placed;
    private bool followMouse;
    private bool canPlace;
    private float zDistance;
    private Vector3 origPos;
    private int level;

    private AudioSource m_Audio;

    // Used to not show the Tower HUD when placing a tower
    static private bool placing;

    void Start()
    {
        level = 1;
        placing = false;
        PreTowerHUD.transform.parent.gameObject.SetActive(false);
        PostTowerHUD.transform.parent.gameObject.SetActive(false);
        placed = false;
        canPlace = false;
        zDistance = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);

        m_Audio = GetComponent<AudioSource>();
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
        if (!placed)
        {
            // Let the user place the tower
            PreTowerHUD.transform.parent.gameObject.SetActive(false);
            PostTowerHUD.transform.parent.gameObject.SetActive(false);

            if (gc.GetCurrency() >= PlaceCost)
            {
                followMouse = true;
                origPos = transform.position;
                placing = true;
            }
            else
            {
                m_Audio.PlayOneShot(MisPlaceSFX);
            }
        }
        else
        {
            // Upgrade the tower
            if (gc.GetCurrency() >= (UpgradeCost + (level - 1)))
            {
                gc.SpendCurrency(UpgradeCost + (level - 1));
                level++;
                UpdateTowerStats();
            }
            else
            {
                m_Audio.PlayOneShot(MisPlaceSFX);
            }
        }
    }

    private void OnMouseUp()
    {
        if (followMouse)
        {
            placing = false;
            followMouse = false;

            SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer rend in rends)
            {
                rend.color = Color.white;
            }

            if (canPlace)
            {
                placed = true;
                gc.SpendCurrency(PlaceCost);
                tmc.TowerPlaced(towerIndex);
                Invoke("CheckAttack", GetAttackSpeed());
            }
            else
            {
                m_Audio.PlayOneShot(MisPlaceSFX);
                transform.position = origPos;
            }
        }
    }

    void CheckAttack()
    {
        AttackerController attacker = gc.waves.GetFurthestAlongPathInRange(this);

        if (attacker != null)
        {
            m_Audio.PlayOneShot(WhooshSFX);
            GameObject projectile = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
            projectile.GetComponent<ProjectileController>().Target = attacker;
            projectile.GetComponent<ProjectileController>().Damage = GetDamage();
        }

        Invoke("CheckAttack", GetAttackSpeed());
    }

    public int GetRange()
    {
        return Range + ((level - 1) * UpgradeRange);
    }

    public int GetDamage()
    {
        return Damage + ((level - 1) * UpgradeDamage);
    }

    public float GetAttackSpeed()
    {
        return AttackSpeed - ((level - 1) * UpgradeAttackSpeed);
    }

    public void OnMouseEnter()
    {
        if (!placing)
        {
            UpdateTowerStats();
        }
    }

    public void UpdateTowerStats()
    {
        if (!placed)
        {
            PreTowerHUD.transform.parent.gameObject.SetActive(true);
            PreTowerHUD.text = gameObject.name + "\n";
            PreTowerHUD.text += "Range: " + Range + "\n";
            PreTowerHUD.text += "Damage: " + Damage + "\n";
            PreTowerHUD.text += "Attack Rate: " + AttackSpeed + "\n";
        }
        else
        {
            PostTowerHUD.transform.parent.gameObject.SetActive(true);
            PostTowerHUD.transform.parent.gameObject.SetActive(true);
            PostTowerHUD.text = gameObject.name + " : " + level + "\n";
            PostTowerHUD.text += $"Range: {Range + ((level - 1) * UpgradeRange)}" + "\n";
            PostTowerHUD.text += $"Damage: {Damage + ((level - 1) * UpgradeDamage)}" + "\n";
            PostTowerHUD.text += $"Attack Rate: {AttackSpeed - ((level - 1) * UpgradeAttackSpeed)}" + "\n";

            PostTowerUpgradeHUD.transform.parent.gameObject.SetActive(true);
            PostTowerUpgradeHUD.text = $"Level : {level + 1}" + $" (cost:{UpgradeCost + (level - 1)})" + "\n";
            PostTowerUpgradeHUD.text += $"Range: {Range + (level * UpgradeRange)}" + "\n";
            PostTowerUpgradeHUD.text += $"Damage: {Damage + (level * UpgradeDamage)}" + "\n";
            PostTowerUpgradeHUD.text += $"Attack Rate: {AttackSpeed - (level * UpgradeAttackSpeed)}" + "\n";
        }
    }

    public void OnMouseExit()
    {
        PreTowerHUD.transform.parent.gameObject.SetActive(false);
        PostTowerHUD.transform.parent.gameObject.SetActive(false);
        PostTowerUpgradeHUD.transform.parent.gameObject.SetActive(false);
    }
}