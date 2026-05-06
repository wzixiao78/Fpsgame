using UnityEngine;
using UnityEngine.UI; // 👇 新增：必须引入 UI 命名空间才能控制按钮

public class WeaponSwitcher : MonoBehaviour
{
    [Header("经济系统 (新增)")]
    public int currentMoney = 800;       // 你的初始资金
    public int odinPrice = 3200;         // 奥丁的价格
    public Button odinBuyButton;         // 留给 UI 按钮的插槽

    [Header("基础设置")]
    public int selectedWeapon = 1;

    [Header("武器解锁状态")]
    public bool hasMainWeapon = false;
    public bool hasSecondaryWeapon = true;

    [Header("动画系统")]
    public Animator playerAnimator;

    void Start()
    {
        SwitchWeapon(selectedWeapon);
    }

    void Update()
    {
        // 👇 新增：每帧都在偷偷检查你的钱包！
        // 如果插槽里有按钮，且钱包里的钱 >= 奥丁的价格，按钮就激活；否则就锁死变灰！
        if (odinBuyButton != null)
        {
            odinBuyButton.interactable = (currentMoney >= odinPrice);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (hasMainWeapon) SwitchWeapon(0);
            else Debug.Log("未购买主武器，无法切换！");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (hasSecondaryWeapon) SwitchWeapon(1);
        }
    }

    public void SwitchWeapon(int weaponIndex)
    {
        if (weaponIndex >= transform.childCount) return;

        selectedWeapon = weaponIndex;
        SelectWeapon();

        if (playerAnimator != null)
        {
            playerAnimator.SetInteger("WeaponType", weaponIndex);
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(i == selectedWeapon);
            i++;
        }
    }

    // 👇 修改：购买逻辑现在变得非常“现实”了
    public void BuyMainWeapon()
    {
        if (currentMoney >= odinPrice)
        {
            currentMoney -= odinPrice; // 扣钱
            hasMainWeapon = true;      // 解锁
            SwitchWeapon(0);           // 拿在手里
            Debug.Log($"购买成功！花费 {odinPrice}，剩余资产：${currentMoney}");
        }
        else
        {
            Debug.Log("余额不足，想零元购？没门！");
        }
    }
}