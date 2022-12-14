using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    Text levelText;
    Image healthSlider;
    Image expSlider;

    private void Awake()
    {
        levelText = transform.GetChild(2).GetComponent<Text>();
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }

    private void Update()
    {
        levelText.text = "Level " + GameManager.Instance.playerStats.characterData.currLevel.ToString("00");
        UpdateHealth();
        UpdateExp();
    }

    private void UpdateHealth()
    {
        float sliderPercent = (float)GameManager.Instance.playerStats.CurrHealth / GameManager.Instance.playerStats.MaxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    private void UpdateExp()
    {
        float sliderPercent = (float)GameManager.Instance.playerStats.characterData.currExp / GameManager.Instance.playerStats.characterData.baseExp;
        expSlider.fillAmount = sliderPercent;
    }

}
