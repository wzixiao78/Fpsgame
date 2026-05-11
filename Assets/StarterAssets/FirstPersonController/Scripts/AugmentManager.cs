using UnityEngine;
using System.Collections.Generic;

public class AugmentManager : MonoBehaviour
{
    // 这个 instance 就是为了让卡片能随时找到它！
    public static AugmentManager instance;
    [Header("全局状态")]
    public bool isLoneWolfActive = false; // 记录是否已经献祭了队友
    [Header("海克斯设置")]
    public List<AugmentData> allAugments; // 这里放你创建的所有海克斯数据方块
    public GameObject augmentPanel;       // 整个海克斯UI面板
    public AugmentCardUI card1;           // 左边的卡片
    public AugmentCardUI card2;           // 右边的卡片

    [Header("玩家引用")]
    public GameObject player;             // 拖入你的玩家模型

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 游戏刚开始时，隐藏海克斯面板
        if (augmentPanel != null) augmentPanel.SetActive(false);
    }

    // 测试用：按 T 键随时呼出海克斯选择面板！
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            // 确保裁判员 (RoundManager) 存在
            if (RoundManager.instance != null)
            {
                // 💡 核心修复：用双方的比分推算出当前是第几回合！
                // 玩家分数 + 敌人分数 + 1 = 当前回合数
                int currentRound = RoundManager.instance.playerScore + RoundManager.instance.enemyScore + 1;

                // 只有第 1 回合和第 6 回合，才允许弹出面板！
                if (currentRound == 1 || currentRound == 6)
                {
                    TriggerAugmentSelection();
                }
                else
                {
                    Debug.Log($"当前是第 {currentRound} 回合，只有第1和第6回合能选海克斯！");
                }
            }
        }
    }

    public void TriggerAugmentSelection()
    {
        if (allAugments.Count < 2) return;

        // 随机抽两张不重复的卡
        int randomIndex1 = Random.Range(0, allAugments.Count);
        int randomIndex2;
        do
        {
            randomIndex2 = Random.Range(0, allAugments.Count);
        } while (randomIndex1 == randomIndex2);

        // 喂数据给卡片
        card1.SetupCard(allAugments[randomIndex1]);
        card2.SetupCard(allAugments[randomIndex2]);

        // 弹出版面并暂停游戏
        augmentPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        augmentPanel.SetActive(true);

        // 【核心解锁代码】
        Cursor.lockState = CursorLockMode.None; // 解除锁定
        Cursor.visible = true;                  // 显示鼠标指针


        Time.timeScale = 0f;
    }

    // 关闭面板并恢复时间
    public void CloseAugmentPanel()
    {
        augmentPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 核心执行逻辑：根据海克斯的类型给予奖励
    public void ApplyAugment(AugmentData data)
    {
        if (player == null) player = GameObject.FindWithTag("Player");
        if (player == null) return;

        switch (data.type)
        {
            case AugmentType.SacrificeForHealth:
                // 1. 寻找所有打上 "Ally" 标签的队友
                isLoneWolfActive = true;
                GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally"); 
                int sacrificedCount = 0;

                foreach (GameObject ally in allies)
                {
                    // 💥 直接抹杀！
                    Destroy(ally);
                    sacrificedCount++;
                }

                Debug.Log($"孤狼协议生效！成功献祭了 {sacrificedCount} 个队友！");

                // 2. 找到玩家的 PlayerHealth 脚本，注入三倍血量！
                PlayerHealth myHealth = player.GetComponent<PlayerHealth>();
                if (myHealth != null)
                {
                    myHealth.maxHealth = myHealth.maxHealth * (int)data.value; // 上限翻倍

                    // ⬅️ 核心：直接呼叫你写好的方法！不仅自动回满血，还会瞬间刷新屏幕UI！
                    myHealth.ResetHealth();

                    Debug.Log($"🩸 力量涌现！玩家最大血量飙升至：{myHealth.maxHealth}");
                }
                else
                {
                    Debug.LogError("找不到玩家的 PlayerHealth 脚本，加血失败！");
                }

                break; 

                // 你以后还可以在这里加 GiveWeapon(发枪), IncreaseDamage(加伤害) 等等...
        }
    }
}