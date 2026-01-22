using UnityEngine;
using System.Collections;
using System.Linq;

public class EzrealE : SkillBase
{
    public GameObject afterImagePrefab;
    public float searchRadius = 10f;
    public float blinkDistance = 4f;

    public override void OnSkillAnimationEvent()
    {
        // 1. Tạo tàn dư
        if (afterImagePrefab != null)
        {
            GameObject afterImage = Instantiate(afterImagePrefab, championState.transform.position, championState.transform.rotation);
            var ezSr = championState.GetComponentInChildren<SpriteRenderer>();
            var aiSr = afterImage.GetComponent<SpriteRenderer>();
            if (ezSr != null && aiSr != null)
            {
                aiSr.flipX = ezSr.flipX;
                aiSr.sortingOrder = ezSr.sortingOrder - 1;
                aiSr.color = new Color(1f, 1f, 1f, 0.6f);
            }
        }

        // 2. Dịch chuyển theo hướng nhìn
        float lookDirection = Mathf.Sign(championState.transform.localScale.x);

        // Nếu dùng flipX thì dùng dòng này thay thế:
        // var sr = championState.GetComponentInChildren<SpriteRenderer>();
        // float lookDirection = (sr.flipX) ? -1f : 1f;

        Vector3 blinkOffset = new Vector3(lookDirection * blinkDistance, 0, 0);
        championState.transform.position += blinkOffset;

        // 3. Tìm mục tiêu
        Transform target = FindPriorityTarget();

        // 4. Bắn đạn E
        if (target != null)
        {
            GameObject projectileObj = Instantiate(skillData.skillPrefab, championState.transform.position, Quaternion.identity);

            string shooterTag = championState.gameObject.tag;
            projectileObj.tag = (shooterTag == "Player") ? "PlayerBullet" : "EnemyBullet";

            FindingProjectile fp = projectileObj.GetComponent<FindingProjectile>();

            if (fp != null)
            {
                fp.damage = CalculateSkillDamage(championState.GetSkillLevel(2));
                fp.damageType = DamageType.Magical;
                fp.speed = skillData.projectileSpeed / 100f;
                fp.ownerTag = shooterTag;

                // Truyền mục tiêu VÀ logic nổ W vào
                fp.SetTarget(target, (hitObject) => {
                    Health h = hitObject.GetComponent<Health>();
                    if (h != null)
                    {
                        h.TakeDamage(fp.damage, fp.damageType); // Damage của E

                        // Gọi nội tại từ đây
                        if (championState.TryGetComponent<EzrealPassive>(out var passive))
                        {
                            passive.AddStack();
                        }

                        // Check nổ W
                        EzrealWMark mark = hitObject.GetComponentInChildren<EzrealWMark>();
                        if (mark != null) mark.TriggerMark(h);
                    }
                });
            }
        }

        if (championState.TryGetComponent<PlayerController>(out PlayerController pc)) pc.hasCastSkill = true;
        if (championState.TryGetComponent<EnemyController>(out EnemyController ec)) ec.hasCastSkill = true;
    }

    private Transform FindPriorityTarget()
    {
        // Tìm tất cả vật thể trong tầm đánh
        Collider2D[] colliders = Physics2D.OverlapCircleAll(championState.transform.position, searchRadius);

        // 1. Ưu tiên nhất: Đứa nào đang dính dấu ấn W
        var marked = colliders.FirstOrDefault(c => c.GetComponentInChildren<EzrealWMark>() != null);
        if (marked != null) return marked.transform;

        // 2. Ưu tiên nhì: Tướng địch (Enemy)
        var hero = colliders.FirstOrDefault(c => c.CompareTag("Enemy"));
        if (hero != null) return hero.transform;

        // 3. Ưu tiên ba: Bất cứ thứ gì có máu (Lính, Quái, v.v.) mà không phải là bản thân
        var anyTarget = colliders.FirstOrDefault(c =>
            c.gameObject != championState.gameObject && // Không tự bắn mình
            c.GetComponent<Health>() != null            // Có thanh máu thì bắn
        );

        if (anyTarget != null) return anyTarget.transform;

        return null;
    }
}