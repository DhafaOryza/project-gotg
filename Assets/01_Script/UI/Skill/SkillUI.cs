using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI txtKey;

    [SerializeField] private GameObject none;

    [SerializeField] private Image CooldownImage;
    [SerializeField] private TextMeshProUGUI txtCooldownTime;

    private int slotIndex;
    private PlayerBaseEntity playerentity;
    private SkillDataSO currentSkillData;

    public void Setup(int slot, string keyBind, SkillDataSO currentSkill = null)
    {
        slotIndex = slot;
        txtKey.text = keyBind;
        currentSkillData = currentSkill;

        if (playerentity != null)
            playerentity = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();

        if (currentSkill == null)
        {
            skillIcon.gameObject.SetActive(false);
            none.SetActive(true);
            HideCooldown();
            return;
        }

        skillIcon.sprite = currentSkill.icon;

        skillIcon.gameObject.SetActive(true);
        none.SetActive(false);

        HideCooldown();
    }

    private void Update()
    {
        if (currentSkillData == null) return;
        if (playerentity == null)
        {
            playerentity = GameManager.Instance?.levelSpawnerManager?.GetCurrentPlayer();
            if (playerentity == null) return;
        }

        float remainingCD = playerentity.GetSkillCooldownRemaining(slotIndex);

        if (remainingCD > 0f)
        {
            if (CooldownImage != null && !CooldownImage.gameObject.activeSelf)
                CooldownImage.gameObject.SetActive(true);

            if (CooldownImage != null && currentSkillData.cooldown > 0f)
                CooldownImage.fillAmount = remainingCD / currentSkillData.cooldown;

            if (txtCooldownTime != null)
                txtCooldownTime.text = remainingCD > 1f ? Mathf.CeilToInt(remainingCD).ToString(): remainingCD.ToString("F1");
        }
        else
        {
            HideCooldown();
        }
    }

    private void HideCooldown()
    {
        if (CooldownImage != null && CooldownImage.gameObject.activeSelf)
            CooldownImage.gameObject.SetActive(false);
    }
}