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

        if (enemyPrefab == null || spawnPoints.Length == 0) return;
        enemiesAlive = spawnPoints.Length;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Instantiate(enemyPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
        }
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
        if (enemiesAlive <= 0) PlayerWonRound();
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