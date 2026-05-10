using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;
    public enum RoundState { PreparationPhase, CombatPhase, RoundEnd }
    public RoundState currentState;

    [Header("比赛计分系统")]
    public int playerScore = 0;
    public int enemyScore = 0;
    public int scoreToWin = 13;
    private bool isMatchOver = false;
    private int enemiesAlive = 0;
    private int alliesAlive = 0; // (新增) 记录己方存活人数

    [Header("出生点设置")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public Transform[] playerSpawnPoints;
    public GameObject player;

    [Header("友军空降设置")]
    public GameObject allyPrefab;
    public Transform[] allySpawnPoints;

    [Header("UI设置")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI centerMessageText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;

    // ==========================================
    [Header("UI 头像设置 (新增)")]
    public Sprite kayoIcon;             // Kayo 头像图片
    public GameObject playerIconsGroup; // 己方头像组 (PlayerIconsGroup)
    public GameObject enemyIconsGroup;  // 敌人头像组 (EnemyIconsGroup)
    // ==========================================

    [Header("时间设置")]
    public float preparationTime = 20f;
    public float combatTime = 120f;
    private float currentTimer;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 没有任何延迟，开局直接强行执行准备阶段！
        StartPreparationPhase();
    }

    void Update()
    {
        if (isMatchOver) return;

        if (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            if (currentState == RoundState.PreparationPhase) StartCombatPhase();
            else if (currentState == RoundState.CombatPhase) PlayerWonRound();
        }
    }

    public void StartPreparationPhase()
    {
        currentState = RoundState.PreparationPhase;
        currentTimer = preparationTime;
        if (centerMessageText != null) centerMessageText.text = "PREPARATION PHASE";

        // 🧹 1. 彻底清理敌军
        EnemyHealth[] allEnemies = FindObjectsOfType<EnemyHealth>();
        foreach (EnemyHealth enemy in allEnemies)
        {
            Destroy(enemy.gameObject);
        }

        // 🧹 2. 核心修复：拔掉旧友军的“物理插头”，防止与新友军发生碰撞爆炸！
        AllyBrain[] oldAllies = FindObjectsOfType<AllyBrain>();
        foreach (AllyBrain ally in oldAllies)
        {
            // 瞬间关闭旧友军的所有碰撞体和导航，让他们在物理世界“立即消失”
            Collider[] cols = ally.GetComponentsInChildren<Collider>();
            foreach (Collider c in cols) c.enabled = false;

            UnityEngine.AI.NavMeshAgent agent = ally.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            Destroy(ally.gameObject);
        }

        UpdateScoreUI();

        // 🏥 3. 恢复玩家状态
        if (player != null && playerSpawnPoints.Length > 0)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            int randomIndex = Random.Range(0, playerSpawnPoints.Length);
            player.transform.position = playerSpawnPoints[randomIndex].position;
            player.transform.rotation = playerSpawnPoints[randomIndex].rotation;

            if (cc != null) cc.enabled = true;

            SimpleShoot shootScript = player.GetComponentInChildren<SimpleShoot>();
            if (shootScript != null) shootScript.ResetAmmo();

            PlayerHealth healthScript = player.GetComponent<PlayerHealth>();
            if (healthScript != null) healthScript.ResetState();
        }

        // 🪂 4. 踏踏实实地、没有一丝延迟地空降新友军！
        ForceSpawnAllies();
    }

    void ForceSpawnAllies()
    {
        if (allyPrefab == null || allySpawnPoints.Length == 0) return;

        // 直接在这个干净的瞬间，把友军砸在出生点上
        for (int i = 0; i < allySpawnPoints.Length; i++)
        {
            if (allySpawnPoints[i] != null)
            {
                Instantiate(allyPrefab, allySpawnPoints[i].position, allySpawnPoints[i].rotation);
            }
        }
    }

    public void StartCombatPhase()
    {
        currentState = RoundState.CombatPhase;
        currentTimer = combatTime;
        if (centerMessageText != null) centerMessageText.text = "COMBAT PHASE!";

        // 🧮 (新增) 计算本回合的己方总人数 = 玩家(1) + 友军数量
        alliesAlive = 1 + allySpawnPoints.Length;

        if (enemyPrefab != null && spawnPoints.Length > 0)
        {
            enemiesAlive = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Instantiate(enemyPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            }
        }

        // 🖼️ (新增) 初始化双方头像 UI
        InitIcons();
    }

    public void PlayerWonRound()
    {
        if (currentState == RoundState.RoundEnd || isMatchOver) return;
        playerScore++;
        StartCoroutine(HandleRoundEnd("PLAYER WON THE ROUND"));
    }

    public void EnemyWonRound()
    {
        if (currentState == RoundState.RoundEnd || isMatchOver) return;
        enemyScore++;
        StartCoroutine(HandleRoundEnd("ENEMY WON THE ROUND"));
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;

        // 💀 (新增) 熄灭一个敌人头像
        UpdateSurvivalUI(enemyIconsGroup, enemiesAlive);

        if (enemiesAlive <= 0) PlayerWonRound();
    }

    // ==========================================
    // (新增) 己方阵亡与 UI 控制核心代码区
    // ==========================================

    public void OnAllyDied()
    {
        alliesAlive--;

        // 💀 (新增) 熄灭一个己方头像
        UpdateSurvivalUI(playerIconsGroup, alliesAlive);

        // 如果你想让己方死光时判定敌人赢，可以取消下面这句的注释：
        // if (alliesAlive <= 0) EnemyWonRound();
    }

    void InitIcons()
    {
        // 激活并重置所有图标，隐藏多余的白框
        ResetGroupIcons(playerIconsGroup, alliesAlive);
        ResetGroupIcons(enemyIconsGroup, enemiesAlive);
    }

    void ResetGroupIcons(GameObject group, int aliveCount)
    {
        if (group == null) return;

        Image[] icons = group.GetComponentsInChildren<Image>(true);

        for (int i = 0; i < icons.Length; i++)
        {
            if (i < aliveCount)
            {
                icons[i].gameObject.SetActive(true);
                icons[i].sprite = kayoIcon;
                icons[i].color = Color.white;
            }
            else
            {
                icons[i].gameObject.SetActive(false);
            }
        }
    }

    void UpdateSurvivalUI(GameObject group, int currentAlive)
    {
        if (group == null) return;
        Image[] icons = group.GetComponentsInChildren<Image>(true);

        if (currentAlive >= 0 && currentAlive < icons.Length)
        {
            // 变成半透明暗色，表示已阵亡
            icons[currentAlive].color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

    // ==========================================

    IEnumerator HandleRoundEnd(string message)
    {
        currentState = RoundState.RoundEnd;
        if (centerMessageText != null) centerMessageText.text = message;
        yield return new WaitForSeconds(3f);
        CheckMatchWinner();
    }

    private void CheckMatchWinner()
    {
        if (playerScore >= scoreToWin && (playerScore - enemyScore) >= 2)
        {
            isMatchOver = true;
            if (centerMessageText != null) centerMessageText.text = $"MATCH VICTORY!\n{playerScore} : {enemyScore}";
        }
        else if (enemyScore >= scoreToWin && (enemyScore - playerScore) >= 2)
        {
            isMatchOver = true;
            if (centerMessageText != null) centerMessageText.text = $"MATCH DEFEAT...\n{playerScore} : {enemyScore}";
        }
        else
        {
            StartPreparationPhase();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTimer / 60F);
            int seconds = Mathf.FloorToInt(currentTimer - minutes * 60);
            string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (currentState == RoundState.PreparationPhase) timerText.text = "BUY PHASE - " + timeString;
            else timerText.text = timeString;
        }
    }

    void UpdateScoreUI()
    {
        if (playerScoreText != null) playerScoreText.text = playerScore.ToString();
        if (enemyScoreText != null) enemyScoreText.text = enemyScore.ToString();
    }
}