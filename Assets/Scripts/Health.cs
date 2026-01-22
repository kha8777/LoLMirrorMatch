using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Health : MonoBehaviour
{

    [Header("Animations")]
    public Animator animator;

    public HeroData heroData;
    public float maxHP = 100f;
    [HideInInspector] public float currentHP;

    [Header("Shields")]
    public float physicalShield = 0f;   // only absorbs Physical
    public float magicShield = 0f;      // only absorbs Magical
    public float universalShield = 0f;  // absorbs any damage

    [Header("HealthBar")]
    public GameObject healthBarPrefab;      // should be a UI prefab (RectTransform root)
    public Transform healthBarAnchor;       // optional anchor on character (local position)
    public float anchorOffsetY = 2f;      // fallback vertical offset
    public float anchorSideOffset = 0.5f;   // fallback horizontal offset (always treated positive/right)
    [HideInInspector] public TextMeshProUGUI healthText;
    [HideInInspector] public TextMeshProUGUI levelText;

    [Header("Instance Level")]
    [Tooltip("Per-instance level if no ILevelProvider is present. Can be set in inspector or by spawner.")]
    public int startingLevel = 1;

    private HealthBar healthBarInstance;
    private ChampionState championState;
    private bool isDead = false;

    void Start()
    {
        // Tự động tìm Animator nếu chưa gán trong Inspector
        if (animator == null) animator = GetComponent<Animator>();

        if (heroData != null) maxHP = heroData.HP;
        currentHP = maxHP;

        // Determine offsets (use anchor if provided)
        float side = Mathf.Abs(anchorSideOffset);
        float up = anchorOffsetY;
        if (healthBarAnchor != null)
        {
            up = healthBarAnchor.localPosition.y;
            side = Mathf.Abs(healthBarAnchor.localPosition.x);
        }

        if (healthBarPrefab != null)
        {
            // Find or create a Screen-space Canvas named "HealthCanvas"
            GameObject canvasObj = GameObject.Find("HealthCanvas");
            Canvas canvas = null;
            if (canvasObj == null)
            {
                canvasObj = new GameObject("HealthCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvas = canvasObj.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }

            // Instantiate prefab under the HealthCanvas
            GameObject hb = Instantiate(healthBarPrefab, canvasObj.transform);
            RectTransform hbRect = hb.GetComponent<RectTransform>();
            if (hbRect != null)
            {
                // parent already set to canvas; ensure rect transform is correct for screen-space canvas
                hbRect.SetParent(canvasObj.transform, worldPositionStays: false);
                hbRect.localScale = Vector3.one;

                // Optional: force desired pixel size so it matches your prefab preview
                // Adjust these values to the width/height you expect (pixels at reference resolution)
                hbRect.sizeDelta = new Vector2(300f, 50f);

                // Optional: reset anchors to center so SetTarget placement works consistently
                hbRect.anchorMin = new Vector2(0.5f, 0.5f);
                hbRect.anchorMax = new Vector2(0.5f, 0.5f);
                hbRect.pivot = new Vector2(0.5f, 0.5f);
            }
            healthBarInstance = hb.GetComponent<HealthBar>();
            if (healthBarInstance != null)
            {
                // ensure TextMeshPro references are assigned on the instantiated prefab
                // try to auto-assign by name if not assigned in prefab
                if (healthBarInstance.healthText == null || healthBarInstance.levelText == null)
                {
                    var texts = hb.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                    foreach (var t in texts)
                    {
                        var n = t.gameObject.name.ToLowerInvariant();
                        if (healthBarInstance.healthText == null && n.Contains("hp") || n.Contains("health"))
                            healthBarInstance.healthText = t;
                        else if (healthBarInstance.levelText == null && n.Contains("level"))
                            healthBarInstance.levelText = t;
                    }
                }

                // now safe to call
                healthBarInstance.SetTarget(transform, up, side, canvas);
                healthBarInstance.SetHealth(currentHP, maxHP);

                ILevelProvider lvlProv = GetComponent<ILevelProvider>() ?? GetComponentInParent<ILevelProvider>();
                int displayLevel = lvlProv != null ? lvlProv.GetLevel() : startingLevel;
                healthBarInstance.SetLevel(displayLevel);

                healthBarInstance.SetHealthText($"{currentHP} / {maxHP}");

                championState = lvlProv as ChampionState ?? GetComponent<ChampionState>() ?? GetComponentInParent<ChampionState>();
                if (championState != null)
                {
                    // đảm bảo UI cập nhật khi state thay đổi
                    championState.OnStateChanged += UpdateBar;
                }
            }
        }
    }

    // Public API: add shields
    public void AddShield(float amount, ShieldType type)
    {
        switch (type)
        {
            case ShieldType.Physical:
                physicalShield += amount;
                break;
            case ShieldType.Magical:
                magicShield += amount;
                break;
            case ShieldType.Universal:
                universalShield += amount;
                break;
        }
        UpdateBar();
    }

    // Main damage path: apply to shields first based on damageType, then HP
    public void TakeDamage(float dmg, DamageType damageType)
    {
        if (isDead) return;

        float remaining = dmg;

        if (damageType == DamageType.Physical)
        {
            if (physicalShield > 0f)
            {
                float used = Mathf.Min(physicalShield, remaining);
                physicalShield -= used;
                remaining -= used;
            }
        }
        else if (damageType == DamageType.Magical)
        {
            if (magicShield > 0f)
            {
                float used = Mathf.Min(magicShield, remaining);
                magicShield -= used;
                remaining -= used;
            }
        }

        if (remaining > 0f && universalShield > 0f)
        {
            float used = Mathf.Min(universalShield, remaining);
            universalShield -= used;
            remaining -= used;
        }

        if (damageType == DamageType.True)
        {
            remaining = dmg;
        }

        if (remaining > 0f)
        {
            currentHP -= remaining;
            currentHP = Mathf.Max(0f, currentHP);
        }

        UpdateBar();

        if (currentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (isDead || healthBarInstance == null) return;
        if (healthBarInstance != null)
        {
            healthBarInstance.SetHealth(currentHP, maxHP);
            healthBarInstance.SetHealthText($"{currentHP} / {maxHP}");
        }
        // optionally expose shield values to HealthBar for visualization
    }

    public void RefreshUI()
    {
        UpdateBar();

        // Cập nhật thêm level nếu cần
        if (healthBarInstance != null)
        {
            ILevelProvider lvlProv = GetComponent<ILevelProvider>() ?? GetComponentInParent<ILevelProvider>();
            int displayLevel = lvlProv != null ? lvlProv.GetLevel() : startingLevel;
            healthBarInstance.SetLevel(displayLevel);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Ẩn thanh máu
        if (healthBarInstance != null)
        {
            healthBarInstance.gameObject.SetActive(false);
        }

        // Vô hiệu hóa các Controller (Player/Enemy)
        if (TryGetComponent<PlayerController>(out var pc))
        {
            if (pc.indicator != null) pc.indicator.SetActive(false);
            pc.StopAllSkillCasts();
            pc.enabled = false;
        }
        if (TryGetComponent<EnemyController>(out var ec))
        {
            if (ec.indicator != null) ec.indicator.SetActive(false);
            ec.StopAllSkillCasts();
            ec.enabled = false;
        }

        // Ép vật lý đứng yên
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero; // Dừng vận tốc
            rb.bodyType = RigidbodyType2D.Static; // Chuyển sang Static để không bị đẩy
            rb.simulated = false; // Ngừng giả lập vật lý hoàn toàn
        }

        // Tắt Indicator của kỹ năng
        SkillBase[] skills = GetComponents<SkillBase>();
        foreach (var s in skills)
        {
            if (s.indicator != null) s.indicator.gameObject.SetActive(false);
        }

        // Tắt va chạm
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }

        // Chạy Animation chết
        if (animator != null) animator.SetTrigger("die");

        StopAllCoroutines();
    }

    void OnDestroy()
    {
        if (championState != null) championState.OnStateChanged -= UpdateBar;
    }

    public HealthBar GetHealthBar()
    {
        return healthBarInstance;
    }
}

public enum ShieldType
{
    Physical,
    Magical,
    Universal
}