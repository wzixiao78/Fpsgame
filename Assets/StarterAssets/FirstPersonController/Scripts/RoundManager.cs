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
    private int alliesAlive = 0;

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

    [Header("击杀反馈设置")]
    public GameObject killBanner; 

    [Header("UI 头像设置")]
    public Sprite kayoIcon;
    public GameObject playerIconsGroup;
    public GameObject enemyIconsGroup;

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

        // 🧹 2. 清理旧友军
        AllyBrain[] oldAllies = FindObjectsOfType<AllyBrain>();
        foreach (AllyBrain ally in oldAllies)
        {
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

        // 🧮 4. 提前计算人数，保证 UI 瞬间亮起
        enemiesAlive = spawnPoints.Length; // 敌人人数

        // 🌟 孤狼协议判断：如果生效了，友军人数强制设为 1（只有玩家自己）
        if (AugmentManager.instance != null && AugmentManager.instance.isLoneWolfActive)
        {
            alliesAlive = 1;
        }
        else
        {
            alliesAlive = 1 + allySpawnPoints.Length;
        }

        // 🖼️ 5. 瞬间刷新头像 UI（移到了准备阶段）
        InitIcons();

        // 🪂 6. 尝试空降友军
        ForceSpawnAllies();
    }

    void ForceSpawnAllies()
    {
        if (allyPrefab == null || allySpawnPoints.Length == 0) return;

        // 🌟 孤狼协议拦截：如果你是孤狼，直接 return 罢工，绝对不生队友！
        if (AugmentManager.instance != null && AugmentManager.instance.isLoneWolfActive)
        {
            Debug.Log("孤狼协议生效中，本局永久不再生成队友！");
            return;
        }

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

        // 敌人真正在这里生成
        if (enemyPrefab != null && spawnPoints.Length > 0)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Instantiate(enemyPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            }
        }
    }

    public void PlayerWonRound()
    {
        if (currentState == RoundState.RoundEnd || isMatchOver) return;
        playerScore++;
        if (GameEconomy.instance != null)
        {
            GameEconomy.instance.AddMoney(3000);
            Debug.Log("<color=green>回合胜利！奖励 $3000</color>");
        }

        StartCoroutine(HandleRoundEnd("PLAYER WON THE ROUND"));
    }

    public void EnemyWonRound()
    {
        if (currentState == RoundState.RoundEnd || isMatchOver) return;
        enemyScore++;

        
        if (GameEconomy.instance != null)
        {
            GameEconomy.instance.AddMoney(1900);
            Debug.Log("<color=orange>回合失败。低保 $1900</color>");
        }

        StartCoroutine(HandleRoundEnd("ENEMY WON THE ROUND"));
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        UpdateSurvivalUI(enemyIconsGroup, enemiesAlive);

        // 👇 新增：触发击杀图标闪现！
        StartCoroutine(ShowKillBanner());

        if (enemiesAlive <= 0) PlayerWonRound();
    }

    // 👇 新增：控制图标显示和消失的魔法（协程）
    IEnumerator ShowKillBanner()
    {
        if (killBanner != null)
        {
            killBanner.SetActive(true); 

            yield return new WaitForSeconds(1.5f); // 在屏幕上停留 1.5 秒

            killBanner.SetActive(false); // 隐藏图标
        }
    }

    public void OnAllyDied()
    {
        alliesAlive--;
        UpdateSurvivalUI(playerIconsGroup, alliesAlive);
    }

    void InitIcons()
    {
        ResetGroupIcons(playerIconsGroup, alliesAlive);
        ResetGroupIcons(enemyIconsGroup, enemiesAlive);
    }

    void ResetGroupIcons(GameObject group, int aliveCount)
    {
        if (group == null) return;

        Image[] icons = group.GetComponentsInChildren<Image>(true);

        // 如果 group 自己也带了 Image（比如背景底板），为了防止报错，我们跳过第 0 个（自身）
        // 如果你的头像本身就在最高层，把 i = 1 改回 i = 0
        int startIdx = (group.GetComponent<Image>() != null) ? 1 : 0;
        int iconCount = 0;

        for (int i = startIdx; i < icons.Length; i++)
        {
            if (iconCount < aliveCount)
            {
                icons[i].gameObject.SetActive(true);
                icons[i].sprite = kayoIcon;
                icons[i].color = Color.white;
            }
            else
            {
                icons[i].gameObject.SetActive(false);
            }
            iconCount++;
        }
    }

    void UpdateSurvivalUI(GameObject group, int currentAlive)
    {
        if (group == null) return;
        Image[] icons = group.GetComponentsInChildren<Image>(true);

        int startIdx = (group.GetComponent<Image>() != null) ? 1 : 0;

        // 当人数减少时，把对应的头像变灰
        int targetIndex = startIdx + currentAlive;
        if (targetIndex >= startIdx && targetIndex < icons.Length)
        {
            icons[targetIndex].color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }
    }

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