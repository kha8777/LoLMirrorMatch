using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    public SkillData skillData;
    protected float currentCooldown = 0f;
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
        PlayerController pc = championState.GetComponent<PlayerController>();
        if (pc != null)
        {
            if (pc.isAttackingState && pc.hasFired) pc.StopAttack();
            if (pc.anySkillCasting() && pc.hasCastSkill) pc.StopAllSkillCasts();

            pc.currentCastingIndex = skillIndex;
            pc.hasCastSkill = false;
            pc.currentSkillCastTime = AnimationCastTime();
        }

        currentCooldown = skillData.cooldown;

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
        Debug.Log("dang doi skill");
        yield return new WaitUntil(() => playerController.hasCastSkill == true);
        Debug.Log("da su dung skill xong");
        playerController.animator.speed = 1f;
        castingSkillCoroutine = null;
    }

    public abstract void OnSkillAnimationEvent();

    public float CalculateSkillDamage(int skillLevel)
    {
        float baseSkillDamage = (skillLevel > 0)
            ? skillData.damageLV1 + (skillData.damagePerLevel * (skillLevel - 1))
            : 0f;

        float totalAD = championState.GetTotalAD(heroData);
        float totalAP = championState.GetTotalAP(heroData);

        float bonusDamage = (totalAD * skillData.additionalPhysicDamagePercent) +
                            (totalAP * skillData.additionalMagicDamagePercent);

        return baseSkillDamage + bonusDamage;
    }

    public float CurrentCooldown => currentCooldown;

    public float AnimationCastTime()
    {
        PlayerController pc = championState.GetComponent<PlayerController>();
        return pc.GetAnimationLength(skillData.animationClipName);
    }

}