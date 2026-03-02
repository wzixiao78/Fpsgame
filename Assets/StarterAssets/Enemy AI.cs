using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("难度设置")]
    public float aimSpeed = 5f;
    public float attackRange = 8f;
    public float fireRate = 1f;
    public float damage = 10f;

    [Header("引用")]
    public Transform muzzlePoint;
    private NavMeshAgent agent;
    private float nextFireTime = 0f;

    // 👇 新增：当前锁定的目标（可能是玩家，也可能是友军）
    private Transform currentTarget;

    // 用来记录“我看见目标了吗”
    private bool canSeeTarget = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        // 1. 每一帧雷达扫描全场：谁近就锁谁！
        FindNearestTarget();

        // 如果场上没活人了，就原地待命
        if (currentTarget == null)
        {
            agent.ResetPath();
            return;
        }

        // 2. 检查：我看得到当前锁定的目标吗？
        CheckLineOfSight();

        // 3. 状态机逻辑
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        // 只有【距离够近】且【看得见】才攻击
        if (distanceToTarget <= attackRange && canSeeTarget)
        {
            // --- 攻击状态 ---
            agent.ResetPath(); // 停下
            RotateTowardsTarget(); // 瞄准

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            // --- 追逐状态 ---
            agent.SetDestination(currentTarget.position);
            RotateTowardsTarget();
        }
    }

    // 📡 新增：双目标雷达扫描
    void FindNearestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");

        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        // 测距玩家
        foreach (GameObject p in players)
        {
            if (p == null) continue;
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < shortestDistance)
            {
                shortestDistance = d;
                nearestTarget = p.transform;
            }
        }

        // 测距友军（如果友军更近，目标就会切换成友军）
        foreach (GameObject a in allies)
        {
            if (a == null) continue;
            float d = Vector3.Distance(transform.position, a.transform.position);
            if (d < shortestDistance)
            {
                shortestDistance = d;
                nearestTarget = a.transform;
            }
        }

        currentTarget = nearestTarget;
    }

    void CheckLineOfSight()
    {
        if (muzzlePoint == null || currentTarget == null) return;

        Vector3 eyePos = muzzlePoint.position;
        Vector3 targetPos = currentTarget.position + Vector3.up * 1.5f; // 瞄准胸口
        Vector3 direction = targetPos - eyePos;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, direction, out hit, attackRange + 2f))
        {
            // 👇 修改：既检测 Player，也检测 Ally
            if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Ally") ||
                hit.transform.GetComponentInParent<PlayerHealth>() != null ||
                hit.transform.GetComponentInParent<AllyHealth>() != null)
            {
                canSeeTarget = true;
                Debug.DrawLine(eyePos, hit.point, Color.green);
            }
            else
            {
                canSeeTarget = false;
                Debug.DrawLine(eyePos, hit.point, Color.yellow);
            }
        }
        else
        {
            canSeeTarget = false;
        }
    }

    void RotateTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction == Vector3.zero) return;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * aimSpeed);
    }

    void Shoot()
    {
        if (muzzlePoint == null || currentTarget == null)
        {
            return;
        }

        Vector3 fireDirection = (currentTarget.position + Vector3.up * 1.5f) - muzzlePoint.position;
        Debug.DrawRay(muzzlePoint.position, fireDirection, Color.red, 0.5f);

        RaycastHit hit;
        if (Physics.Raycast(muzzlePoint.position, fireDirection, out hit, attackRange + 5f))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return;
            }

            //  修改：判断打中的是玩家还是友军，分别扣血
            PlayerHealth playerHealth = hit.transform.GetComponentInParent<PlayerHealth>();
            AllyHealth allyHealth = hit.transform.GetComponentInParent<AllyHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("【成功】敌人击中了玩家！");
            }
            else if (allyHealth != null)
            {
                allyHealth.TakeDamage(damage);
                Debug.Log("【成功】敌人击中了友军！");
            }
        }
    }
}