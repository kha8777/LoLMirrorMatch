using System.Collections;
using UnityEngine;

public class SkillUIManager : MonoBehaviour
{
    public SkillUI[] skillSlots;

    void Start()
    {
        // Thay vì gọi trực tiếp, ta dùng StartCoroutine để đợi tướng
        StartCoroutine(WaitAndSetupUI());
    }

    IEnumerator WaitAndSetupUI()
    {
        // Đợi 0.1 giây để đảm bảo mọi script trên tướng đã chạy Awake/Start xong
        yield return new WaitForSeconds(0.1f);

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            UpdateSkillUI(player);
        }
        else
        {
            Debug.LogError("Vẫn không tìm thấy PlayerController!");
        }
    }

    public void UpdateSkillUI(PlayerController hero)
    {
        SkillBase[] skills = hero.GetComponents<SkillBase>();
        Debug.Log("So luong skill tim thay sau khi doi: " + skills.Length);

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (i < skills.Length)
            {
                skillSlots[i].Setup(skills[i]);
                skillSlots[i].gameObject.SetActive(true);
            }
            else
            {
                // skillSlots[i].gameObject.SetActive(false);
            }
        }
    }
}