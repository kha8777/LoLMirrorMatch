using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
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
    private float lastMoveDirection = 0f;

    public List<SkillBase> currentSkills;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
        HandleAnimation();
        HandleSkills();
    }

    private void HandleMovement()
    {
        float moveInput = 0f;

        if ((isAttackingState && !hasFired) || (anySkillCasting() && !hasCastSkill))
        {
            moveInput = 0f;
        }
        else
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                lastMoveDirection = -1f;
            }
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                lastMoveDirection = 1f;
            }

            bool leftIsPressed = Keyboard.current.leftArrowKey.isPressed;
            bool rightIsPressed = Keyboard.current.rightArrowKey.isPressed;

            if (leftIsPressed && rightIsPressed)
            {
                moveInput = lastMoveDirection;
            }
            else if (leftIsPressed)
            {
                moveInput = -1f;
            }
            else if (rightIsPressed)
            {
                moveInput = 1f;
                lastMoveDirection = 1f;
            }
            else
            {
                moveInput = 0f;
            }

            if (moveInput != 0)
            {
                if (isAttackingState && hasFired) StopAttack();
                if (anySkillCasting() && hasCastSkill) StopAllSkillCasts();
            }
        }

        rb.linearVelocity = new Vector2(moveInput * (heroData.MS / 100f), rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = Vector3.one;
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -xRange, xRange), transform.position.y, transform.position.z);
    }

    public void OnBasicAttackButtonClick()
    {
        if (attackingTime <= 0f && hasCastSkill)
        {
            if (isAttackingState && hasFired) StopAttack();
            if (anySkillCasting() && hasCastSkill) StopAllSkillCasts();

            attackingTime = 1f / heroData.AS;
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

    void HandleAttack()
    {
        attackingTime -= Time.deltaTime;
        if (attackingTime < 0f) attackingTime = 0f;

        if ((Keyboard.current.spaceKey.isPressed || Keyboard.current.aKey.isPressed) && attackingTime <= 0f && hasCastSkill)
        {
            if (isAttackingState && hasFired) StopAttack();
            if (anySkillCasting() && hasCastSkill) StopAllSkillCasts();

            attackingTime = 1f / heroData.AS;
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
        animator.speed = heroData.AS;
        yield return new WaitUntil(() => hasFired == true);
        animator.speed = 1f;
        attackRoutine = null;
    }

    private void HandleSkills()
    {
        currentSkillCastTime -= Time.deltaTime;
        if (currentSkillCastTime < 0f) currentSkillCastTime = 0f;

        if (Keyboard.current.qKey.wasPressedThisFrame && hasCastSkill && hasFired)
        {
            if (!currentSkills[0].IsReady) return;
            currentSkills[0].ActivateSkill();
        }
    }

    public bool anySkillCasting()
    {
        return currentCastingIndex > -1;
    }

    public void StopAttack()
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
        for (int i = 0; i < currentSkills.Count; i++)
        {
            if (currentSkills[i].castingSkillCoroutine != null)
            {
                StopCoroutine(currentSkills[i].castingSkillCoroutine);
                currentSkills[i].castingSkillCoroutine = null;
            }
        }

        currentCastingIndex = -1;
        hasCastSkill = true;
        currentSkillCastTime = 0f;
        animator.speed = 1f;
        animator.SetBool("isSpellQ", false);
        // animator.SetBool("isSpellW", false);
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
        newBullet.tag = "PlayerBullet";
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

        // kỹ năng
        if (anySkillCasting())
        {
            if (currentCastingIndex == 0) animator.SetBool("isSpellQ", true);
            if (currentSkillCastTime <= 0)
            {
                StopAllSkillCasts();
            }

        }
        else
        {
            animator.SetBool("isSpellQ", false);
            // animator.SetBool("isSpellW", false);
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