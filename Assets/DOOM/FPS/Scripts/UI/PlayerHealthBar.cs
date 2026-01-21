using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Tooltip("Image component dispplaying current health")]
    public Image healthFillImage;

    public bool useText = true;
    [ShowIf("useText")]
    public TMP_Text healthText;
    
    Health m_PlayerHealth;

    private void Start()
    {
        PlayerCharacterController playerCharacterController = GameObject.FindAnyObjectByType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PlayerHealthBar>(playerCharacterController, this);

        m_PlayerHealth = playerCharacterController.GetComponent<Health>();
        DebugUtility.HandleErrorIfNullGetComponent<Health, PlayerHealthBar>(m_PlayerHealth, this, playerCharacterController.gameObject);

        m_PlayerHealth.onDamaged += UpdateHealthUI;
        m_PlayerHealth.onHealed += UpdateHealthUI;
    }

    void UpdateHealthUI(float heal) {UpdateHealthUI(0, null);}
    void UpdateHealthUI(float damage, GameObject damageSource)
    {
        // update health bar value
        healthFillImage.fillAmount = m_PlayerHealth.currentHealth / m_PlayerHealth.maxHealth;
        if (useText) healthText.text = Mathf.RoundToInt(m_PlayerHealth.currentHealth).ToString();
    }
}
