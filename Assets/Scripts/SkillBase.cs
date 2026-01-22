using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    public SkillData skillData;
    [HideInInspector] public float currentCooldown = 0f;
    public ChampionState championState;
    public HeroData heroData;
    public Transform indicator;
    public Coroutine castingSkillCoroutine;
    protected Quaternion skillRotation;
    public int skillIndex;

    public bool IsReady => currentCooldown <= 0f;

    void Update()
    {
        currentCooldown -= Time.deltaTime;
        if (currentCooldown < 0) currentCooldown = 0f;
    }

    public virtual void ActivateSkill()
    {
        int level = championState.GetSkillLevel(skillIndex);
        if (level <= 0)
        {
            Debug.Log("Skill chưa được nâng cấp!");
            return;
        }

        PlayerController pc = championState.GetComponent<PlayerController>();
        if (pc != null)
        {
            if (pc.isAttackingState && pc.hasFired) pc.StopAttack();
            if (pc.anySkillCasting() && pc.hasCastSkill) pc.StopAllSkillCasts();

            pc.currentCastingIndex = skillIndex;
            pc.hasCastSkill = false;
            pc.currentSkillCastTime = AnimationCastTime();
        }

        currentCooldown = CalculateCooldown(level);

        if (castingSkillCoroutine != null) StopCoroutine(castingSkillCoroutine);

        Vector3 aim = indicator.transform.right;
        float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
        const float rotationOffset = 90f;
        skillRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        castingSkillCoroutine = StartCoroutine(ExecuteSkill());
    }

    IEnumerator ExecuteSkill()
    {
        PlayerController playerController = championState.GetComponent<PlayerController>();
        yield return new WaitUntil(() => playerController.hasCastSkill == true);
        playerController.animator.speed = 1f;
        castingSkillCoroutine = null;
    }

    public abstract void OnSkillAnimationEvent();

    public float CalculateSkillDamage(int skillLevel)
    {
        if (skillLevel <= 0) return 0f;

        float baseSkillDamage = skillData.damageLV1 + (skillData.damagePerLevel * (skillLevel - 1));

        // Tính tỷ lệ % theo cấp
        float currentPhysicPercent = skillData.additionalPhysicDamagePercent + (skillData.addPhysicPercentPerLevel * (skillLevel - 1));
        float currentMagicPercent = skillData.additionalMagicDamagePercent + (skillData.addMagicPercentPerLevel * (skillLevel - 1));

        float totalAD = championState.GetTotalAD(heroData);
        float totalAP = championState.GetTotalAP(heroData);

        float bonusDamage = (totalAD * (currentPhysicPercent / 100f)) +
                            (totalAP * (currentMagicPercent / 100f));

        return baseSkillDamage + bonusDamage;
    }

    public float CalculateSkillDamageBaseOnBonus(int skillLevel)
    {
        if (skillLevel <= 0) return 0f;

        float baseSkillDamage = skillData.damageLV1 + (skillData.damagePerLevel * (skillLevel - 1));

        // Tính tỷ lệ % theo cấp
        float currentPhysicPercent = skillData.additionalPhysicDamagePercent + (skillData.addPhysicPercentPerLevel * (skillLevel - 1));
        float currentMagicPercent = skillData.additionalMagicDamagePercent + (skillData.addMagicPercentPerLevel * (skillLevel - 1));

        float bonusAD = championState.GetBonusAD(heroData);
        float totalAP = championState.GetTotalAP(heroData);

        float bonusDamage = (bonusAD * (currentPhysicPercent / 100f)) +
                            (totalAP * (currentMagicPercent / 100f));

        return baseSkillDamage + bonusDamage;
    }

    public float CalculateManaCost(int skillLevel)
    {
        if (skillLevel <= 0) return skillData.manaCost; // Lv0 dùng cost của Lv1
        return skillData.manaCost + (skillData.manaCostPerLevel * (skillLevel - 1));
    }

    public float CalculateCooldown(int skillLevel)
    {
        if (skillLevel <= 0) return skillData.cooldown;
        float cd = skillData.cooldown + (skillData.cooldownPerLevel * (skillLevel - 1));
        return Mathf.Max(cd, 0.01f); // Đảm bảo cooldown không bị âm hoặc quá nhanh
    }

    public float CurrentCooldown => currentCooldown;

    public float AnimationCastTime()
    {
        PlayerController pc = championState.GetComponent<PlayerController>();
        return pc.GetAnimationLength(skillData.animationClipName);
    }

}