using UnityEngine;

public class AllyHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        // 出生时满血
        currentHealth = maxHealth;
    }

    // 留给敌人攻击时调用的扣血接口
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"<color=blue>友军受到攻击！受到 {damage} 点伤害，剩余血量: {currentHealth}</color>");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("<color=grey>一名友军阵亡了...</color>");
        // TODO: 以后可以在这里加上死亡动画或者变成布娃娃系统(Ragdoll)
        Destroy(gameObject); // 直接销毁尸体
    }
}