using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject shopUI;
    public TextMeshProUGUI currentMoneyText;
    public TextMeshProUGUI nextRoundMoneyText;

    [Header("逻辑引用")]
    public WeaponSwitcher weaponSwitcher; // ⚠️ 等下必须在面板里把挂着背包的物体拖给它！
    public PlayerHealth playerHealth;

    [Header("商品价格")]
    public int costVandal = 2900;
    public int costOdin = 3200;    // 👈 奥丁回归！
    public int costShotgun = 2000;
    public int costOperator = 4700;
    public int costLightArmor = 400;
    public int costHeavyArmor = 1000;

    private bool isShopOpen = false;

    void Start()
    {
        isShopOpen = false;
        if (shopUI != null) shopUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
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
        if (currentMoneyText) currentMoneyText.text = $"$ {money}";

        int predicted = money + 1900;
        if (nextRoundMoneyText) nextRoundMoneyText.text = $"下回合最低: $ {predicted}";
    }

    // ================= 购买逻辑区 =================

    public void BuyClassic()
    {
        weaponSwitcher.SwitchWeapon(0); // 0号位：手枪
    }

    // 假设你的奥丁排在第 1 个长枪位
    public void BuyOdin()
    {
        if (GameEconomy.instance.TrySpendMoney(costOdin))
        {
            weaponSwitcher.SwitchWeapon(1);
            Debug.Log("购买奥丁成功！");
        }
    }

    // 假设你的 Vandal 排在第 2 个长枪位
    public void BuyVandal()
    {
        if (GameEconomy.instance.TrySpendMoney(costVandal))
        {
            weaponSwitcher.SwitchWeapon(2);
        }
    }

    // 假设你的霰弹枪排在第 3 个长枪位
    public void BuyShotgun()
    {
        if (GameEconomy.instance.TrySpendMoney(costShotgun))
        {
            weaponSwitcher.SwitchWeapon(3);
            Debug.Log("购买散弹枪成功！");
        }
    }

    public void BuyOperator()
    {
        if (GameEconomy.instance.TrySpendMoney(costOperator))
        {
            weaponSwitcher.SwitchWeapon(4);
        }
    }

    // ... 护甲逻辑保持不变 ...
    public void BuyLightArmor()
    {
        if (playerHealth.currentArmor >= 50) return;
        if (GameEconomy.instance.TrySpendMoney(costLightArmor))
        {
            playerHealth.SetArmor(25);
        }
    }

    public void BuyHeavyArmor()
    {
        if (playerHealth.currentArmor >= 50) return;
        if (GameEconomy.instance.TrySpendMoney(costHeavyArmor))
        {
            playerHealth.SetArmor(50);
        }
    }
}