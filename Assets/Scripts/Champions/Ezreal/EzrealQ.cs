using UnityEngine;

public class EzrealQ : SkillBase
{
    public override void OnSkillAnimationEvent()
    {
        GameObject projectileObj = Instantiate(skillData.skillPrefab, indicator.transform.position, skillRotation);

        string shooterTag = championState.gameObject.tag;
        if (shooterTag == "Player")
        {
            projectileObj.tag = "PlayerBullet";
        }
        else if (shooterTag == "Enemy")
        {
            projectileObj.tag = "EnemyBullet";
        }

        Projectile s = projectileObj.GetComponent<Projectile>();

        if (s != null)
        {
            s.ownerTag = shooterTag;
            s.speed = skillData.projectileSpeed / 100f;
            s.damage = CalculateSkillDamage(championState.GetSkillLevel(0));
            s.damageType = (skillData.damegeType == "Magical") ? DamageType.Magical : DamageType.Physical;
        }
        if (championState.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            playerController.hasCastSkill = true;
        }

        if (championState.TryGetComponent<EnemyController>(out EnemyController enemyController))
        {
            enemyController.hasCastSkill = true;
        }
        
    }

}