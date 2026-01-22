using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : MonoBehaviour
{
    private float attackingTime;
    public Rigidbody2D rb;
    public Animator animator;
    public GameObject indicator;
    public GameObject bullet;
    public HeroData heroData;
    private float xRange = 8.5f;

    [HideInInspector] public bool hasFired = true;
    private Quaternion bulletRotation;
    private Coroutine attackRoutine;
    [HideInInspector] public bool isAttackingState = false;
    private int defaultStateHash;

    [HideInInspector] public bool hasCastSkill = true;
    [HideInInspector] public int currentCastingIndex = -1;
    [HideInInspector] public float currentSkillCastTime;
    private ChampionState championState;

    private float direction = -1f;
    public List<SkillBase> currentSkills;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        championState = GetComponent<ChampionState>();
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
        HandleAnimation();
    }

    private void HandleMovement()
    {
        float speed = heroData.MS / 100f;

        if ((isAttackingState && !hasFired) || (anySkillCasting() && !hasCastSkill))
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            if (isAttackingState && hasFired) StopAttack();
            if (anySkillCasting() && hasCastSkill) StopAllSkillCasts();
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }

        if (transform.position.x > xRange) direction = -1f;
        else if (transform.position.x < -xRange) direction = 1f;

        transform.localScale = (direction > 0) ? Vector3.one : new Vector3(-1, 1, 1);
    }

    void HandleAttack()
    {
        attackingTime -= Time.deltaTime;
        if (attackingTime < 0f) attackingTime = 0f;

        if (attackingTime <= 0f)
        {
            if (isAttackingState && hasFired) StopAttack();
            if (anySkillCasting() && hasCastSkill) StopAllSkillCasts();

            attackingTime = 1f / championState.CurrentAttackSpeed;
            hasFired = false;
            isAttackingState = true;

            if (attackRoutine != null) StopCoroutine(attackRoutine);

            Vector3 aim = indicator.transform.right;
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            const float rotationOffset = 90f;
            bulletRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

            attackRoutine = StartCoroutine(FireBullet());
        }
    }

    IEnumerator FireBullet()
    {
        if (championState != null)
        {
            animator.speed = championState.CurrentAttackSpeed;
        }
        yield return new WaitUntil(() => hasFired == true);
        animator.speed = 1f;
        attackRoutine = null;
    }

    public bool anySkillCasting()
    {
        return currentCastingIndex > -1;
    }

    private void StopAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        hasFired = true;
        animator.speed = 1f;
        isAttackingState = false;
        animator.SetBool("isAttacking", false);
        animator.Play(defaultStateHash, 0, 0f);
    }

    public void StopAllSkillCasts()
    {
        if (currentSkills == null || currentSkills.Count == 0)
        {
            animator.speed = 1f;
            ResetCastStates();
            return;
        }

        for (int i = 0; i < currentSkills.Count; i++)
        {
            if (currentSkills[i] != null && currentSkills[i].castingSkillCoroutine != null)
            {
                StopCoroutine(currentSkills[i].castingSkillCoroutine);
                currentSkills[i].castingSkillCoroutine = null;
            }
        }

        ResetCastStates();
    }

    private void ResetCastStates()
    {
        currentCastingIndex = -1;
        hasCastSkill = true;
        currentSkillCastTime = 0f;
        animator.speed = 1f;
        animator.SetBool("isSpellQ", false);
        animator.SetBool("isSpellW", false);
        animator.SetBool("isSpellE", false);
        animator.SetBool("isSpellR", false);
        animator.Play(defaultStateHash, 0, 0f);
    }

    public void ExecuteShootEvent()
    {
        GameObject newBullet = Instantiate(bullet, indicator.transform.position, bulletRotation);
        Projectile p = newBullet.GetComponent<Projectile>();

        if (p != null)
        {
            p.speed = heroData.MissleSpeed / 100f;
            p.damage = heroData.AD;
            p.damageType = DamageType.Physical;
        }

        hasFired = true;
        newBullet.tag = "EnemyBullet";
    }

    public void ExecuteSkillEvent()
    {
        if (currentCastingIndex >= 0 && currentCastingIndex < currentSkills.Count)
        {
            currentSkills[currentCastingIndex].OnSkillAnimationEvent();
        }
    }

    private void HandleAnimation()
    {
        // di chuyển
        if (rb.linearVelocity.x != 0)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        // tấn công
        if (isAttackingState)
        {
            animator.SetBool("isAttacking", true);

            if (attackingTime <= 0f)
            {
                isAttackingState = false;
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }
    }

    public void SetupSkills(GameObject heroPrefab)
    {
        foreach (var s in GetComponents<SkillBase>()) Destroy(s);
        currentSkills.Clear();

        // Thêm kỹ năng mới dựa trên tướng được chọn
        // (Có thể gán thông qua Prefab tướng hoặc một Manager)
    }

    public float GetAnimationLength(string clipName)
    {
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;

        foreach (AnimationClip clip in ac.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0f;
    }
}