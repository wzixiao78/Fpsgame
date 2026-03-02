using UnityEngine;

public class GameEconomy : MonoBehaviour
{
    public static GameEconomy instance;

    public int currentMoney = 800; // 初始资金 $800
    public int maxMoney = 9000;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (currentMoney > maxMoney) currentMoney = maxMoney;

        UpdateMoneyUI();
    }

    public bool TrySpendMoney(int cost)
    {
        if (currentMoney >= cost)
        {
            currentMoney -= cost;
            UpdateMoneyUI();
            return true; // 购买成功
        }
        return false; // 穷，买不起
    }

    void UpdateMoneyUI()
    {
        if (PlayerHUD.instance != null)
            PlayerHUD.instance.UpdateMoney(currentMoney);
    }
}