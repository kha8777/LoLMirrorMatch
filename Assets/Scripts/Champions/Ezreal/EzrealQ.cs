using UnityEngine;

public class EzrealQ : SkillBase
{
    public override void OnSkillAnimationEvent()
    {
        GameObject projectileObj = Instantiate(skillData.skillPrefab, indicator.transform.position, skillRotation);

        string shooterTag = championState.gameObject.tag;
        projectileObj.tag = (shooterTag == "Player") ? "PlayerBullet" : "EnemyBullet";

        Projectile p = projectileObj.GetComponent<Projectile>();

        if (p != null)
        {
            // ĐĂNG KÝ: Khi viên đạn này trúng đích, thì làm cái việc trong ngoặc nhọn
            p.OnHitTarget = (target) => {
                if (championState.TryGetComponent<EzrealPassive>(out var passive)) 
                {
                    passive.AddStack();
                    passive.ReduceAllCooldowns(1.5f);
                }

            };

            p.ownerTag = shooterTag;
            p.speed = skillData.projectileSpeed / 100f;
            p.damage = CalculateSkillDamage(championState.GetSkillLevel(0));
            p.damageType = DamageType.Physical;
        }

        if (championState.TryGetComponent<PlayerController>(out PlayerController pc)) pc.hasCastSkill = true;
        if (championState.TryGetComponent<EnemyController>(out EnemyController ec)) ec.hasCastSkill = true;
        
    }

}