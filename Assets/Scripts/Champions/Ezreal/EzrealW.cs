using UnityEngine;

// tạo prefab
public class EzrealW : SkillBase
{
    public override void OnSkillAnimationEvent()
    {
        GameObject projectileObj = Instantiate(skillData.skillPrefab, indicator.transform.position, skillRotation);

        string shooterTag = championState.gameObject.tag;
        projectileObj.tag = (shooterTag == "Player") ? "PlayerBullet" : "EnemyBullet";

        Projectile s = projectileObj.GetComponent<Projectile>();

        if (s != null)
        {
            s.OnHitTarget = (target) => {
                if (championState.TryGetComponent<EzrealPassive>(out var passive))
                {
                    passive.AddStack();
                }

            };
            s.ownerTag = shooterTag;
            s.speed = skillData.projectileSpeed / 100f;
            s.damage = 0f;
            s.damageType = DamageType.Magical;
        }

        if (projectileObj.TryGetComponent<EzrealWMarkTrigger>(out EzrealWMarkTrigger trigger))
        {
            trigger.damageToApply = CalculateSkillDamage(championState.GetSkillLevel(1));
        }

        if (championState.TryGetComponent<PlayerController>(out PlayerController pc)) pc.hasCastSkill = true;
        if (championState.TryGetComponent<EnemyController>(out EnemyController ec)) ec.hasCastSkill = true;
    }
}