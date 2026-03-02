using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD instance;

    [Header("UI 组件")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText; // 新增护甲显示
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI moneyText; // 为第三步预留

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateHealth(float currentHealth, float currentArmor)
    {
        if (healthText != null) healthText.text = $"{Mathf.CeilToInt(currentHealth)}";

        // 更新护甲文字
        if (armorText != null)
        {
            armorText.text = currentArmor > 0 ? $"{Mathf.CeilToInt(currentArmor)}" : "0";
            armorText.color = currentArmor > 0 ? Color.cyan : Color.gray; // 有甲显示青色
        }
    }

    public void UpdateAmmo(int current, int reserve)
    {
        if (ammoText != null) ammoText.text = $"{current} / {reserve}";
    }

    public void UpdateMoney(int money)
    {
        if (moneyText != null) moneyText.text = $"$ {money}";
    }
}