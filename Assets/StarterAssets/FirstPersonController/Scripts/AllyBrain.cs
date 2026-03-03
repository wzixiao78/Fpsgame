using UnityEngine;
using UnityEngine.AI;

public class AllyBrain : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform targetEnemy;
    public float attackRange = 10f;

    // 用于控制日志频率，防止刷屏卡死
    private float logTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null) return;

        FindNearestEnemy();

        // ======= 逻辑执行区 =======
        if (targetEnemy != null)
        {
            float dist = Vector3.Distance(transform.position, targetEnemy.position);
            if (dist > attackRange)
            {
                agent.SetDestination(targetEnemy.position);
            }
            else
            {
                agent.ResetPath();
            }
        }

        logTimer += Time.deltaTime;
        if (logTimer >= 1f)
        {
            logTimer = 0f; // 重置计时器

            if (targetEnemy == null)
            {
                Debug.LogWarning($"<color=yellow>【诊断】队友 {gameObject.name} 报告：我看不到任何带有 'Enemy' 标签的活人！我只能罚站。</color>");
            }
            else
            {
                float currentDist = Vector3.Distance(transform.position, targetEnemy.position);
                Debug.Log($"<color=cyan>【诊断】队友 {gameObject.name} 报告：我锁定了 {targetEnemy.name} (距离:{currentDist})。当前移动速度: {agent.velocity.magnitude}</color>");

                // 深度物理检测
                if (agent.velocity.magnitude < 0.1f && currentDist > attackRange)
                {
                    Debug.LogError($"<color=red>【致命报错】我明明接到了冲锋指令，但我走不动！一定是 Animator 的 Root Motion 没关，或者被 Rigidbody 锁死了！</color>");
                }
            }
        }
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortest = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject e in enemies)
        {
            if (e == null) continue;
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < shortest)
            {
                shortest = d;
                nearest = e;
            }
        }
        if (nearest != null) targetEnemy = nearest.transform;
        else targetEnemy = null;
    }
}