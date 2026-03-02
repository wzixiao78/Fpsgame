using UnityEngine;
using TMPro; // 必须引用 TMP

public class ShopManager : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject shopUI;
    public TextMeshProUGUI currentMoneyText;
    public TextMeshProUGUI nextRoundMoneyText; // 新增：下回合低保

    [Header("逻辑引用")]
    public WeaponSwitcher weaponSwitcher;
    public PlayerHealth playerHealth; // 需要引用血量脚本买甲

    [Header("商品价格")]
    public int costVandal = 2900;
    public int costOperator = 4700;
    public int costLightArmor = 400;
    public int costHeavyArmor = 1000;

    private bool isShopOpen = false;
    
    void Start()
    {
        // 游戏一开始，强制把状态设为关闭
        isShopOpen = false;

        // 隐藏商店 UI
        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }

        // 锁定并隐藏鼠标 (为了你能正常转动视角开枪)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {
        // 监听 B 键
        if (Input.GetKeyDown(KeyCode.B))
        {
            // 只有在购买阶段，才允许开关商店！
            if (RoundManager.instance != null && RoundManager.instance.currentState == RoundManager.RoundState.PreparationPhase)
            {
                ToggleShop();
            }
        }

        if (isShopOpen) UpdateShopUI();
    }

    public void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        shopUI.SetActive(isShopOpen);

        if (isShopOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UpdateShopUI();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void UpdateShopUI()
    {
        int money = GameEconomy.instance.currentMoney;

        // 更新当前金币
        if (currentMoneyText) currentMoneyText.text = $"$ {money}";

        // 计算下回合保底：当前存款 + 败北奖励(假设1900)
        int predicted = money + 1900;
        if (nextRoundMoneyText) nextRoundMoneyText.text = $"下回合最低: $ {predicted}";
    }

    

    public void BuyClassic()
    {
        // 经典手枪是免费的，随时领
        weaponSwitcher.SwitchWeapon(0);
        Debug.Log("领取 Classic");
    }

    public void BuyVandal()
    {
        if (GameEconomy.instance.TrySpendMoney(costVandal))
        {
            weaponSwitcher.SwitchWeapon(1);
        }
    }

    public void BuyOperator()
    {
        if (GameEconomy.instance.TrySpendMoney(costOperator))
        {
            weaponSwitcher.SwitchWeapon(2);
        }
    }

    

    public void BuyLightArmor()
    {
        // 如果已经是重甲(50)，就别买轻甲了
        if (playerHealth.currentArmor >= 50) return;

        if (GameEconomy.instance.TrySpendMoney(costLightArmor))
        {
            playerHealth.SetArmor(25); // 设置为轻甲值
            Debug.Log("购买轻甲成功");
        }
    }

    public void BuyHeavyArmor()
    {
        if (playerHealth.currentArmor >= 50) return;

        if (GameEconomy.instance.TrySpendMoney(costHeavyArmor))
        {
            playerHealth.SetArmor(50); // 设置为重甲值
            Debug.Log("购买重甲成功");
        }
    }
}