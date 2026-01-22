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
    public SkillBase linkedSkill;
    public int skillSlotIndex;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null && pc.currentSkills.Count > skillSlotIndex)
            {
                linkedSkill = pc.currentSkills[skillSlotIndex];
            }
        }
    }

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

        if (linkedSkill.CurrentCooldown > 0)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = linkedSkill.CurrentCooldown / linkedSkill.skillData.cooldown;
            }

            if (cooldownText != null)
            {
                if (linkedSkill.CurrentCooldown > 1f)
                {
                    cooldownText.text = Mathf.Ceil(linkedSkill.CurrentCooldown).ToString();
                }
                else
                {
                    cooldownText.text = linkedSkill.CurrentCooldown.ToString("F1");
                }
            }
        }
        else
        {
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
            if (cooldownText != null) cooldownText.text = "";
        }
    }

    public void ClickToActivate()
    {
        if (linkedSkill == null || !linkedSkill.IsReady) return;

        PlayerController pc = linkedSkill.championState.GetComponent<PlayerController>();
        if (pc != null && pc.hasCastSkill && pc.hasFired)
        {
            linkedSkill.ActivateSkill();
        }
    }
}