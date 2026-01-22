using System.Collections;
using UnityEngine;

public class EzrealR : SkillBase
{
    [Header("R Bow Settings")]
    public GameObject bowPrefab;
    private float fadeOutDuration = 0.1f;
    private float animationLength;

    public override void ActivateSkill()
    {
        base.ActivateSkill();

        if (bowPrefab != null)
        {
            // Tạo cây cung và gán làm con của Ezreal để nó bám theo khi di chuyển
            GameObject bowImage = Instantiate(bowPrefab, championState.transform.position, championState.transform.rotation, championState.transform);

            var ezSr = championState.GetComponentInChildren<SpriteRenderer>();
            var bowSr = bowImage.GetComponent<SpriteRenderer>();

            if (ezSr != null && bowSr != null)
            {
                bowSr.flipX = ezSr.flipX;
                bowSr.sortingOrder = ezSr.sortingOrder + 1;
            }

            // Lấy thời gian animation
            if (championState.TryGetComponent<PlayerController>(out PlayerController pc))
                animationLength = pc.GetAnimationLength("Ezreal_R");
            else if (championState.TryGetComponent<EnemyController>(out EnemyController ec))
                animationLength = ec.GetAnimationLength("Ezreal_R");

            // Chạy logic mờ ảo trong Coroutine
            StartCoroutine(HandleBowEffect(bowSr, bowImage));

        }
    }

    private IEnumerator HandleBowEffect(SpriteRenderer bowSr, GameObject bowObj)
    {
        float elapsed = 0f;
        Color color = new Color(1, 1, 1, 0);

        // Fade In: Hiện dần theo thời gian gồng
        while (elapsed < animationLength)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / animationLength);
            if (bowSr != null) bowSr.color = color;
            yield return null; // Đợi đến frame tiếp theo (Tránh treo game)
        }

        // Đợi đến khi OnSkillAnimationEvent được gọi hoặc thêm 1 khoảng chờ ngắn
        float fadeElapsed = fadeOutDuration;
        while (fadeElapsed > 0)
        {
            fadeElapsed -= Time.deltaTime;
            color.a = Mathf.Clamp01(fadeElapsed / fadeOutDuration);
            if (bowSr != null) bowSr.color = color;
            yield return null;
        }

        // Xóa object cung sau khi xong việc
        Destroy(bowObj);
    }

    public override void OnSkillAnimationEvent()
    {
        GameObject projectileObj = Instantiate(skillData.skillPrefab, indicator.transform.position, skillRotation);

        string shooterTag = championState.gameObject.tag;
        projectileObj.tag = (shooterTag == "Player") ? "PlayerBullet" : "EnemyBullet";

        EzrealRProjectile p = projectileObj.GetComponent<EzrealRProjectile>();

        if (p != null)
        {
            p.OnHitTarget = (target) => {
                if (championState.TryGetComponent<EzrealPassive>(out var passive))
                {
                    passive.AddStack();
                }

            };
            p.ownerTag = shooterTag;
            p.speed = skillData.projectileSpeed / 100f;
            p.damage = CalculateSkillDamageBaseOnBonus(championState.GetSkillLevel(3));
            p.damageType = DamageType.Magical;
        }

        if (championState.TryGetComponent<PlayerController>(out PlayerController pc)) pc.hasCastSkill = true;
        if (championState.TryGetComponent<EnemyController>(out EnemyController ec)) ec.hasCastSkill = true;

    }
}
