using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SkillUI : MonoBehaviour
{
    public Image iconImage;
    public Image cooldownOverlay;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI keyText;

    private SkillBase linkedSkill;

    // Hàm này để "gắn" một kỹ năng thực tế vào ô UI này
    public void Setup(SkillBase skill)
    {
        linkedSkill = skill;
        if (skill != null && skill.skillData != null)
        {
            iconImage.sprite = skill.skillData.skillIcon;
            cooldownOverlay.fillAmount = 0;
            cooldownText.text = "";
        }
    }

    void Update()
    {
        if (linkedSkill == null) return;

        // Cập nhật vòng tròn hồi chiêu
        if (linkedSkill.CurrentCooldown > 0)
        {
            cooldownOverlay.fillAmount = linkedSkill.CurrentCooldown / linkedSkill.skillData.cooldown;
            if (linkedSkill.CurrentCooldown > 1f)
            {
                cooldownText.text = Mathf.Ceil(linkedSkill.CurrentCooldown).ToString();
            }
            else
            {
                cooldownText.text = linkedSkill.CurrentCooldown.ToString("F1");
            }
        }
        else
        {
            cooldownOverlay.fillAmount = 0;
            cooldownText.text = "";
        }
    }

    public void ClickToActivateQ()
    {
        if (linkedSkill == null || !linkedSkill.IsReady) return;

        PlayerController pc = linkedSkill.championState.GetComponent<PlayerController>();

        if (pc != null)
        {
            if (pc.currentSkillCastTime <= 0 && pc.hasFired)
            {
                pc.currentCastingIndex = 0;
                linkedSkill.ActivateSkill();
            }
        }
    }
}