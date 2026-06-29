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
    public AudioClip PlacementSFX;
    public GameObject UpgradeVFX;
    public Transform TowerBase;
    public Transform CharacterTrans;
    public GameObject TowerProjectile;
    public Sprite IdleSprite;
    public Sprite AttackSprite;

    public int PlaceCost;
    public int UpgradeCost;

    public float Range;
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
    private float origCharacterXScale;
    private int level;
    Collider2D collider;
    private TileController tTile;

    private AudioSource m_Audio;

    // Used to not show the Tower HUD when placing a tower
    static private bool placing;

    void Start()
    {
        level = 1;
        placing = false;
        PreTowerHUD.transform.parent.gameObject.SetActive(false);
        PostTowerHUD.transform.parent.gameObject.SetActive(false);
        origCharacterXScale = CharacterTrans.localScale.x;
        placed = false;
        canPlace = false;
        zDistance = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        if (TowerProjectile != null) TowerProjectile.SetActive(true);
        tTile = null;

        collider = GetComponent<Collider2D>();
        m_Audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (followMouse)
        {
            // Just make sure the collider is always on for towers being placed
            collider.enabled = true;

            if (Input.GetMouseButtonDown(1))
            {
                followMouse = false;
                placing = false;
                tTile = null;
                transform.position = origPos;
                m_Audio.PlayOneShot(MisPlaceSFX);
                tmc.DonePlacingTower();
                SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer rend in rends)
                {
                    rend.color = Color.white;
                }
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 20f, ~(1 << 6));

            if (hit.collider != null)
            {
                TileController tile = hit.collider.GetComponent<TileController>();

                if (tile != null)
                {
                    canPlace = true;
                    transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y + TileYOffset, transform.position.z);
                    SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();

                    if (!tile.isOccupied && tile.isPlaceable)
                    {
                        tTile = tile;
                        foreach (SpriteRenderer rend in rends)
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

            if (tTile == null)
            {
                transform.position = new Vector3(mousePos.x, mousePos.y + TileYOffset, transform.position.z);
            }
        }
    }

    private void OnMouseUp()
    {
        if (!placed)
        {
            if (!followMouse)
            {
                // Disable all other tower colliders
                tmc.StartPlacingTower();
                EnableCollider();

                // Let the user place the tower
                PreTowerHUD.transform.parent.gameObject.SetActive(false);
                PostTowerHUD.transform.parent.gameObject.SetActive(false);

                if (gc.GetCurrency() >= PlaceCost)
                {
                    followMouse = true;
                    origPos = transform.position;
                    placing = true;

                    TutorialController tc = gc as TutorialController;
                    if (tc)
                    {
                        if (tc.GetTutorialStep() == 4)
                        {
                            tc.ClickTower();
                        }
                    }
                }
                else
                {
                    m_Audio.PlayOneShot(MisPlaceSFX);
                }
            }
            else
            {
                tmc.DonePlacingTower();
                placing = false;
                followMouse = false;

                SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer rend in rends)
                {
                    rend.color = Color.white;
                }

                if (canPlace && (tTile != null))
                {
                    placed = true;
                    tTile.isOccupied = true;
                    m_Audio.PlayOneShot(PlacementSFX);
                    gc.SpendCurrency(PlaceCost);
                    tmc.TowerPlaced(towerIndex);
                    Invoke("CheckAttack", GetAttackSpeed());

                    TutorialController tc = gc as TutorialController;
                    if (tc)
                    {
                        if (tc.GetTutorialStep() == 5)
                        {
                            tc.PlaceDangerDoveTower();
                        }
                        else if (tc.GetTutorialStep() == 11)
                        {
                            tc.PlaceRamrockTower();
                        }
                    }
                }
                else
                {
                    tTile = null;
                    transform.position = origPos;
                    m_Audio.PlayOneShot(MisPlaceSFX);
                }
            }
        }
        else
        {
            // Upgrade the tower
            if (gc.GetCurrency() >= (UpgradeCost + (level - 1)))
            {
                m_Audio.PlayOneShot(PlacementSFX);
                gc.SpendCurrency(UpgradeCost + (level - 1));
                level++;
                UpdateTowerStats();
                Instantiate(UpgradeVFX, transform.position, Quaternion.identity);
            }
            else
            {
                m_Audio.PlayOneShot(MisPlaceSFX);
            }
        }
    }

    void CheckAttack()
    {
        AttackerController attacker = gc.waves.GetFurthestAlongPathInRange(this);

        if (attacker != null)
        {
            m_Audio.PlayOneShot(WhooshSFX);
            GameObject projectile = Instantiate(ProjectilePrefab, CharacterTrans.position, Quaternion.identity);
            projectile.GetComponent<ProjectileController>().Target = attacker;
            projectile.GetComponent<ProjectileController>().Damage = GetDamage();
            CharacterTrans.GetComponent<SpriteRenderer>().sprite = AttackSprite;
            Invoke("SetIdle", 0.5f);
            if (TowerProjectile != null) TowerProjectile.SetActive(false);

            // Switch Character direction to face the attacker
            if (CharacterTrans.position.x >= attacker.transform.position.x)
            {
                Vector3 scale = CharacterTrans.localScale;
                scale.x = origCharacterXScale;
                CharacterTrans.localScale = scale;
            }
            else
            {
                Vector3 scale = CharacterTrans.localScale;
                scale.x = -1 * origCharacterXScale;
                CharacterTrans.localScale = scale;
            }
        }

        Invoke("CheckAttack", GetAttackSpeed());
    }

    private void SetIdle()
    {
        CharacterTrans.GetComponent<SpriteRenderer>().sprite = IdleSprite;
        if (TowerProjectile != null) TowerProjectile.SetActive(true);
    }

    public float GetRange()
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

    public void DisableCollider()
    {
        collider.enabled = false;
    }

    public void EnableCollider()
    {
        collider.enabled = true;
    }
}