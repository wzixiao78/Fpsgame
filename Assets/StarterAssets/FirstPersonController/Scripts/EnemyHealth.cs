using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("引用")]
    public Animator animator;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // ================== 新增核心联动区 ==================

        // 1. 通知回合管理器（修复卡回合Bug）
        if (RoundManager.instance != null)
        {
            // ⚠️ 注意：如果这行代码报错，说明你之前 RoundManager 里用来处理敌人死亡的方法不叫这个名字。
            // 请把 .EnemyKilled() 替换成你真实的减少敌人数量/判定胜利的方法名（例如 .OnEnemyDied() 或 .CheckWin()）
            RoundManager.instance.OnEnemyDied();
        }

        // 2. 杀敌给钱（可选，假设击杀给 200 块）
        if (GameEconomy.instance != null)
        {
            // 如果你的 GameEconomy 里有加钱的方法，可以把这句取消注释
            // GameEconomy.instance.AddMoney(200); 
        }

        // ====================================================

        // 3. 触发死亡动画
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        else
        {
            // 如果你忘了在面板里拖入 Animator，控制台会立刻弹黄字提醒你！
            Debug.LogWarning("⚠️ 敌人的 Animator 槽位是空的！请在面板里把带有动画的模型拖给它！");
        }

        // 4. 核心：防止“滑板鞋”尸体
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // 5. 核心：让尸体变回背景，不再挡子弹或攻击玩家
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log("敌人已阵亡");

        // 尸体保留 10 秒后销毁
        Destroy(gameObject, 10f);
    }
}