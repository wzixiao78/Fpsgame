using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    public bool isInvulnerable = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (GameEconomy.instance != null) GameEconomy.instance.AddMoney(300);

        
        if (RoundManager.instance != null)
        {
            RoundManager.instance.OnEnemyDied();
        }

        Destroy(gameObject);
    }
}