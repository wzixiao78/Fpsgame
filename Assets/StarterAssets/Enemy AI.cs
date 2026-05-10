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
    private Animator anim; // 动画大脑控制中枢

    // 状态记录
    private float nextFireTime = 0f;
    private Transform currentTarget;
    private bool canSeeTarget = false;

    void Start()
    {
        // 1. 获取寻路腿并接管旋转控制（避免与我们自己的瞄准旋转冲突）
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
        }

        // 2. 获取子物体身上的动画大脑
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 如果导航系统没准备好，直接跳过
        if (agent == null || !agent.isOnNavMesh) return;

        // 【核心动画逻辑】实时读取寻路腿的物理速度，除以最大速度转为 0~1 的小数，喂给混合树的 Speed 滑块
        if (anim != null)
        {
            float currentSpeed = agent.velocity.magnitude / agent.speed;
            anim.SetFloat("Speed", currentSpeed);
        }

        // 1. 雷达扫描全场：谁近就锁谁！
        FindNearestTarget();

        // 2. 战场清理判断：如果场上没活人了（玩家和友军死光），就原地待命
        if (currentTarget == null)
        {
            agent.ResetPath();
            return;
        }

        // 3. 视线检测：我看得到当前锁定的目标吗？
        CheckLineOfSight();

        // 4. 核心状态机逻辑
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        // 只有【距离够近】且【没有掩体挡住视线】时才开火
        if (distanceToTarget <= attackRange && canSeeTarget)
        {
            // --- 攻击状态：站桩输出 ---
            agent.ResetPath(); // 停下脚步（速度归零，动画自动切回 Idle）
            RotateTowardsTarget(); // 死死盯住目标

            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            // --- 追逐状态：迈开腿跑 ---
            agent.SetDestination(currentTarget.position); // 往目标位置寻路
            RotateTowardsTarget(); // 跑动中也保持面朝目标
        }
    }

    // 📡 双目标雷达扫描（自动索敌玩家与友军）
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

        // 测距友军（如果友军更近，目标就会无缝切换成友军）
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

    // 👁️ 物理视线检测（防穿墙透视）
    void CheckLineOfSight()
    {
        if (muzzlePoint == null || currentTarget == null) return;

        Vector3 eyePos = muzzlePoint.position;
        Vector3 targetPos = currentTarget.position + Vector3.up * 1.5f; // 瞄准敌人胸口高度
        Vector3 direction = targetPos - eyePos;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, direction, out hit, attackRange + 2f))
        {
            // 只要射线第一眼看到的是玩家、友军，或者是他们身上的血量脚本，就判定为视线无遮挡
            if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("Ally") ||
                hit.transform.GetComponentInParent<PlayerHealth>() != null ||
                hit.transform.GetComponentInParent<AllyHealth>() != null)
            {
                canSeeTarget = true;
                Debug.DrawLine(eyePos, hit.point, Color.green);
            }
            else
            {
                // 如果射线打到了掩体（墙壁、箱子等）
                canSeeTarget = false;
                Debug.DrawLine(eyePos, hit.point, Color.yellow);
            }
        }
        else
        {
            canSeeTarget = false;
        }
    }

    // 🔄 平滑瞄准转向
    void RotateTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0; // 锁死 Y 轴，防止敌人像迈克尔·杰克逊一样倾斜
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * aimSpeed);
    }

    // 🔫 开火与扣血结算
    void Shoot()
    {
        if (muzzlePoint == null || currentTarget == null) return;

        Vector3 fireDirection = (currentTarget.position + Vector3.up * 1.5f) - muzzlePoint.position;
        Debug.DrawRay(muzzlePoint.position, fireDirection, Color.red, 0.5f);

        RaycastHit hit;
        if (Physics.Raycast(muzzlePoint.position, fireDirection, out hit, attackRange + 5f))
        {
            // 防止击中自己
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) return;

            // 获取目标身上的血量脚本（自动区分是玩家还是友军）
            PlayerHealth playerHealth = hit.transform.GetComponentInParent<PlayerHealth>();
            AllyHealth allyHealth = hit.transform.GetComponentInParent<AllyHealth>();

            // 结算伤害
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("【交火】敌人击中了玩家！");
            }
            else if (allyHealth != null)
            {
                allyHealth.TakeDamage(damage);
                Debug.Log("【交火】敌人击中了友军！");
            }
        }
    }
}