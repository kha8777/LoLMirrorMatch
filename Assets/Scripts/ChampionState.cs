using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// per-instance state: level, items, runtime modifiers
[DisallowMultipleComponent]
public class ChampionState : MonoBehaviour, ILevelProvider
{
    [Tooltip("Level dành cho instance này nếu không được set từ spawner")]
    [SerializeField] private int level = 1;

    [Tooltip("Items equipped on this champion (referencing ItemData ScriptableObjects)")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    [SerializeField] private int[] skillLevels = new int[4] { 0, 0, 0, 0 };
    [HideInInspector] public float bonusAttackSpeedFromPassive = 0f;

    // event khi level/items thay đổi -> các hệ thống UI/skills có thể lắng nghe
    public event Action OnStateChanged;
    public HeroData heroData;

    public float CurrentAttackSpeed
    {
        get
        {
            if (heroData == null) return 1f;
            return heroData.AS * (1f + bonusAttackSpeedFromPassive);
        }
    }

    public int GetSkillLevel(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillLevels.Length)
            return skillLevels[skillIndex];
        return 0;
    }

    public void UpgradeSkill(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillLevels.Length)
        {
            skillLevels[skillIndex]++;
            OnStateChanged?.Invoke();
        }
    }

    public int GetLevel() => Mathf.Max(1, level);

    // API: set level (spawner hoặc save/load gọi)
    public void SetLevel(int newLevel)
    {
        newLevel = Mathf.Max(1, newLevel);
        if (level == newLevel) return;
        level = newLevel;
        OnStateChanged?.Invoke();
    }

    // Items management
    public IReadOnlyList<ItemData> Items => items;

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        items.Add(item);

        // Nếu món đồ có script nội tại, kích hoạt nó ngay
        if (item.effect != null)
        {
            item.effect.OnEquip(this);
        }

        OnStateChanged?.Invoke();
    }

    public bool RemoveItem(ItemData item)
    {
        if (item == null) return false;
        bool removed = items.Remove(item);
        if (removed) OnStateChanged?.Invoke();
        return removed;
    }

    // Example helper: compute modified HP from base heroData and level scaling
    public float ComputeMaxHP(HeroData baseHero)
    {
        if (baseHero == null) return 0f;
        // base + per-level
        float hp = baseHero.HP + baseHero.HPPL * (GetLevel() - 1);
        // apply flat item mods
        float flat = items.Sum(i => i.HP);
        hp = (hp + flat);
        return hp;
    }

    // similar helpers can be added for AD, AR, etc.

    public float GetTotalAD(HeroData baseHero)
    {
        if (baseHero == null) return 0f;
        // Sát thương gốc + tăng mỗi cấp
        float baseAD = baseHero.AD + baseHero.ADPL * (GetLevel() - 1);
        // Sát thương từ trang bị (Cộng thẳng và Phần trăm)
        float flatItemAD = items.Sum(i => i.AD); // Giả sử ItemData có biến flatAD

        return (baseAD + flatItemAD);
    }

    public float GetBonusAD(HeroData baseHero)
    {
        if (baseHero == null) return 0f;
        // Sát thương từ trang bị (Cộng thẳng và Phần trăm)
        float flatAD = items.Sum(i => i.AD);
        return flatAD;
    }

    public float GetTotalAP(HeroData baseHero)
    {
        if (baseHero == null) return 0f;
        // AP thường không có base theo tướng mà chủ yếu từ Item
        float flatAP = items.Sum(i => i.AP);
        return flatAP;
    }
}