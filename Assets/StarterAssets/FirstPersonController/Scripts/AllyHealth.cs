using UnityEngine;
using UnityEngine.AI; // 需要引入 AI 命名空间来控制寻路组件

public class AllyHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public float maxHealth = 100f;
    private float currentHealth;

    // 缓存组件引用
    private Animator anim;
    private NavMeshAgent agent;
    private Collider col;
    private AllyBrain brain;

    // 死亡标记，防止被鞭尸多次触发死亡逻辑
    private bool isDead = false;

    void Start()
    {
        // 出生时满血
        currentHealth = maxHealth;

        // 初始化时获取身上的所有关键组件
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<Collider>();
        brain = GetComponent<AllyBrain>();
    }

    // 留给敌人攻击时调用的扣血接口
    public void TakeDamage(float damage)
    {
        // 如果已经死了，就不再接受伤害（防鞭尸）
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"<color=blue>友军受到攻击！受到 {damage} 点伤害，剩余血量: {currentHealth}</color>");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("<color=grey>一名友军阵亡了...</color>");

        // 1. 触发死亡动画！
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 2. 打断狗腿：关闭寻路，防止尸体在地上滑行追踪敌人
        if (agent != null)
        {
            agent.enabled = false;
        }

        // 3. 强制关机：关闭大脑逻辑，防止死人继续开枪
        if (brain != null)
        {
            brain.enabled = false;
        }

        // 4. 虚化肉体：关闭碰撞体，这样尸体就不会阻挡后面人的子弹了
        if (col != null)
        {
            col.enabled = false;
        }

        // 5. 延迟收尸：不要直接 Destroy(gameObject)！
        // 留出 3 秒钟的时间让死亡动画播完，然后再让尸体消失。你可以根据动画长短调整这个时间。
        Destroy(gameObject, 3f);
    }
}