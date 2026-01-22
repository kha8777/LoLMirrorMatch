using UnityEngine;
using System;

public class FindingProjectile : Projectile
{
    private Transform target;
    // Hành động sẽ thực hiện khi chạm mục tiêu
    public Action<GameObject> OnHitAction;

    public void SetTarget(Transform _target, Action<GameObject> onHitCallback)
    {
        target = _target;
        OnHitAction = onHitCallback;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Logic bay đuổi (Homing)
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Xoay đầu đạn
        Vector2 dir = (Vector2)target.position - (Vector2)transform.position;
        if (dir != Vector2.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Kiểm tra va chạm
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            // Thực hiện hành động đã đăng ký (Ezreal E nổ W, Caitlyn R gây dmg...)
            OnHitAction?.Invoke(target.gameObject);

            // Tự hủy sau khi hoàn thành nhiệm vụ
            Destroy(gameObject);
        }
    }
}