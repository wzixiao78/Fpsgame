using UnityEngine;
using UnityEngine.AI;

public class AllyBrain : MonoBehaviour
{
    [Header("战斗属性设置")]
    public float aimSpeed = 5f;       // 转身瞄准速度
    public float attackRange = 10f;   // 开火射程
    public float fireRate = 1f;       // 射速 (每秒开火次数)
    public float damage = 10f;        // 武器伤害

    [Header("引用设置")]
    public Transform muzzlePoint;     // 枪口位置（必须在预制体里拖入！）
    private NavMeshAgent agent;
    private Animator anim;            // 动画大脑

    // 状态记录
    private Transform targetEnemy;
    private float nextFireTime = 0f;
    private bool canSeeTarget = false;

    void Start()
    {
        // 1. 获取寻路组件并撕掉自带的旋转封印
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
        }

        // 2. 获取动画组件
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1. 雷达扫描找敌人
        FindNearestEnemy();

        // 2. 如果场上没敌人，原地待命
        if (targetEnemy == null)
        {
            if (agent != null && agent.isOnNavMesh) agent.ResetPath();
            if (anim != null) anim.SetFloat("Speed", 0); // 停下奔跑动画
            return;
        }

        // 3. 视线检测：看没看到敌人？
        CheckLineOfSight();

        // 4. 动画联动：把实际移动速度转换给动画机
        bool hasLegs = (agent != null && agent.isOnNavMesh);
        if (hasLegs && anim != null)
        {
            float currentSpeed = agent.velocity.magnitude / agent.speed;
            anim.SetFloat("Speed", currentSpeed);
        }

        // 5. 战斗决策树
        float distanceToTarget = Vector3.Distance(transform.position, targetEnemy.position);

        // 如果【距离够近】且【没被墙挡住】，就开火！
        if (distanceToTarget <= attackRange && canSeeTarget)
        {
            // --- 站桩输出模式 ---
            if (hasLegs) agent.ResetPath(); // 停下脚步

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            // --- 冲锋追逐模式 ---
            if (hasLegs) agent.SetDestination(targetEnemy.position);
        }
    }

    // 🌟 强制接管视角指向（解决木头人不转身的问题）
    void LateUpdate()
    {
        if (targetEnemy != null)
        {
            Vector3 direction = (targetEnemy.position - transform.position).normalized;
            direction.y = 0; // 锁死Y轴，保证身体直立
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * aimSpeed);
            }
        }
    }

    // 📡 寻找最近的敌人
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
        targetEnemy = nearest != null ? nearest.transform : null;
    }

    // 👁️ 视线检测（防穿墙开火）
    void CheckLineOfSight()
    {
        if (muzzlePoint == null || targetEnemy == null) return;

        Vector3 eyePos = muzzlePoint.position;
        Vector3 targetPos = targetEnemy.position + Vector3.up * 1.5f; // 瞄准敌人胸口
        Vector3 direction = targetPos - eyePos;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, direction, out hit, attackRange + 2f))
        {
            // 如果射线打到的是 Enemy 标签，或者它身上有 EnemyHealth 脚本
            if (hit.transform.CompareTag("Enemy") || hit.transform.GetComponentInParent<EnemyHealth>() != null)
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

    // 🔫 开火与伤害结算
    void Shoot()
    {
        if (muzzlePoint == null || targetEnemy == null) return;

        Vector3 fireDirection = (targetEnemy.position + Vector3.up * 1.5f) - muzzlePoint.position;
        Debug.DrawRay(muzzlePoint.position, fireDirection, Color.red, 0.5f);

        RaycastHit hit;
        if (Physics.Raycast(muzzlePoint.position, fireDirection, out hit, attackRange + 5f))
        {
            // 防止打中自己
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) return;

            EnemyHealth enemyHealth = hit.transform.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"<color=#00FF00>【交火】队友 {gameObject.name} 狠狠地击中了敌人！</color>");
            }
        }
    }
}