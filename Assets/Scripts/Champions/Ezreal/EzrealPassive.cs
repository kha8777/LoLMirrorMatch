using UnityEngine;
using System.Collections;

public class EzrealPassive : MonoBehaviour
{
    [Header("Settings")]
    public Sprite passiveIcon;
    private Health health;
    public int maxStacks = 5;
    public float stackDuration = 6f;
    public float attackSpeedPerStack = 10f;

    private int currentStacks = 0;
    private Coroutine timerCoroutine;
    private ChampionState championState;

    void Awake()
    {
        health = GetComponent<Health>();
        championState = GetComponent<ChampionState>();
    }

    public void AddStack()
    {
        // Thêm 1 stack, tối đa 5 stacks
        currentStacks = Mathf.Min(currentStacks + 1, maxStacks);
        ApplyToChampion();

        // Cập nhật lại thời gian 6s (mỗi lần trúng là reset lại thời gian)
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(StackTimer());
    }

    private IEnumerator StackTimer()
    {
        yield return new WaitForSeconds(stackDuration);
        currentStacks = 0;
        ApplyToChampion();
        timerCoroutine = null;
    }

    private void ApplyToChampion()
    {
        if (championState != null)
        {
            championState.bonusAttackSpeedFromPassive = currentStacks * (attackSpeedPerStack/100f);

            // Cập nhật Animator
            var anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = championState.CurrentAttackSpeed;
            }

            // Gọi UI cập nhật
            if (health != null)
            {
                var hb = health.GetHealthBar();
                if (hb != null)
                {
                    if (currentStacks > 0)
                        hb.UpdateBuff("EzrealPassive", passiveIcon, currentStacks);
                    else
                        hb.RemoveBuff("EzrealPassive");
                }
            }
        }
    }

    public void ReduceAllCooldowns(float amount)
    {
        SkillBase[] skills = GetComponents<SkillBase>();
        foreach (var s in skills)
        {
            s.currentCooldown = Mathf.Max(0, s.currentCooldown - amount);
        }
    }
}