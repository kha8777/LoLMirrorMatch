using System.Collections.Generic;
using UnityEngine;

// utility to apply a new heroData + state to an existing GameObject instance
public static class ChampionSwitcher
{
    public static void ApplyHero(GameObject target, HeroData newHero, int level, IEnumerable<ItemData> items = null,
                                 RuntimeAnimatorController animController = null, Sprite skin = null, bool setFullHP = true)
    {
        if (target == null || newHero == null) return;

        var health = target.GetComponent<Health>();
        var state = target.GetComponent<ChampionState>();
        var animator = target.GetComponent<Animator>();
        var sprite = target.GetComponent<SpriteRenderer>();

        // 1) update HeroData reference
        if (health != null) health.heroData = newHero;

        // 2) update state (level/items)
        if (state != null)
        {
            state.SetLevel(level);
            if (items != null)
            {
                // replace items: you can provide APIs to clear/add
                // simple approach (implement ClearItems in ChampionState)
                // state.ClearItems();
                foreach (var it in items) state.AddItem(it);
            }
        }

        // 3) recompute and apply maxHP
        if (health != null)
        {
            float newMax = (state != null) ? state.ComputeMaxHP(newHero) : (newHero.HP + newHero.HPPL * (level - 1));
            if (setFullHP) health.currentHP = newMax;
            else
            {
                // keep same ratio
                float ratio = (health.maxHP > 0f) ? (health.currentHP / health.maxHP) : 1f;
                health.currentHP = Mathf.Clamp(ratio * newMax, 0f, newMax);
            }
            health.maxHP = newMax;

            // add a public method on Health to refresh visuals; if none, call UpdateBar via public API
            // assuming Health exposes public void RefreshUI() that calls UpdateBar()
            var refreshMethod = health as dynamic;
            try { refreshMethod.RefreshUI(); } catch { /* implement RefreshUI in Health */ }
            // health.RefreshUI();
        }

        // 4) visuals
        if (animator != null && animController != null) animator.runtimeAnimatorController = animController;
        if (sprite != null && skin != null) sprite.sprite = skin;

        // 5) update other systems (abilities, HUD icons...) — call their refresh APIs similarly
    }
}