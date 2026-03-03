using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxArmor = 50f;

    private float currentHealth;
    public float currentArmor;

    void Start()
    {
        currentHealth = maxHealth;
        currentArmor = 0;
        RefreshUI();
    }

    public void ResetState()
    {
        currentHealth = maxHealth;
        // 如果你想每回合连护甲也清零，可以加上下面这句：
        // currentArmor = 0; 
        RefreshUI();
    }

    public void SetArmor(float amount)
    {
        currentArmor = amount;
        RefreshUI();
    }

    public void TakeDamage(float damage)
    {
        float damageToHealth = damage;
        float damageToArmor = 0;

        if (currentArmor > 0)
        {
            float absorption = damage * 0.66f;

            if (currentArmor >= absorption)
            {
                damageToArmor = absorption;
                damageToHealth = damage - absorption;
            }
            else
            {
                damageToArmor = currentArmor;
                damageToHealth = damage - currentArmor;
            }
        }

        currentArmor -= damageToArmor;
        currentHealth -= damageToHealth;

        if (currentHealth <= 0) currentHealth = 0;
        if (currentArmor <= 0) currentArmor = 0;

        RefreshUI();

        if (currentHealth == 0)
        {
            Die(); // 👈 这里修复了！去掉了导致报错的中文，只保留了纯净的呼叫
        }
    }

    void RefreshUI()
    {
        if (PlayerHUD.instance != null)
        {
            PlayerHUD.instance.UpdateHealth(currentHealth, currentArmor);
        }
    }

    void Die()
    {
        Debug.Log("玩家阵亡！");

        // 玩家死了，告诉裁判游戏失败
        if (RoundManager.instance != null)
        {
            RoundManager.instance.EnemyWonRound();
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        RefreshUI();
    }
}