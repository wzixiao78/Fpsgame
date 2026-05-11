using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float maxArmor = 50f;

    public float currentHealth; // ⬅️ 修复1：把 private 改成了 public，大门敞开！
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
            Die(); 
        }
    }

    // ⬅️ 修复2：加上了 public！这样发牌员给完血，可以直接呼叫 UI 刷新屏幕数字！
    public void RefreshUI() 
    {
        if (PlayerHUD.instance != null)
        {
            PlayerHUD.instance.UpdateHealth(currentHealth, currentArmor);
        }
    }

    void Die()
    {
        Debug.Log("玩家阵亡！");

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