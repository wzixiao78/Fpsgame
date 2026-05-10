using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("基础设置")]
    public int selectedWeapon = 0; // 默认拿什么枪（0 = 手枪）

    [Header("武器解锁状态（新系统）")]
    // 这个数组的长度会自动适应你 Gun 节点下的枪械数量
    public bool[] unlockedWeapons;

    [Header("动画系统")]
    public Animator playerAnimator;

    void Start()
    {
        // 1. 强制初始化数组长度（确保和你的子物体数量一致）
        unlockedWeapons = new bool[transform.childCount];

        // 2. 暴力清空所有解锁状态，确保万无一失
        for (int i = 0; i < unlockedWeapons.Length; i++)
        {
            unlockedWeapons[i] = false;
        }

        // 3. 只给 0 号位（最上面的手枪）发准入证
        unlockedWeapons[0] = true;

        // 4. 强制执行一次切枪，把模型切换到手枪
        selectedWeapon = 0;
        SelectWeapon();

        // 5. 同步动画状态
        if (playerAnimator != null)
        {
            playerAnimator.SetInteger("WeaponType", 0);
        }

        Debug.Log("武器系统已重置：当前仅手枪可用。");
    }

    void Update()
    {
        // 按 1 键切主武器 (长枪/霰弹)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 遍历查找你当前买过的第一把长枪（索引 > 0 的武器）
            for (int i = 1; i < unlockedWeapons.Length; i++)
            {
                if (unlockedWeapons[i])
                {
                    SwitchWeapon(i);
                    break; // 找到一把就立刻切出来
                }
            }
        }

        // 按 2 键切副武器 (手枪)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (unlockedWeapons[0]) SwitchWeapon(0);
        }
    }

    // 这个方法专门提供给 ShopManager 购买成功后调用
    public void SwitchWeapon(int weaponIndex)
    {
        // 防止索引越界报错
        if (weaponIndex < 0 || weaponIndex >= transform.childCount) return;

        // 核心逻辑：一旦切了这把枪，就相当于在背包里永久解锁了它！
        if (weaponIndex < unlockedWeapons.Length)
        {
            unlockedWeapons[weaponIndex] = true;
        }

        selectedWeapon = weaponIndex;
        SelectWeapon(); // 切换模型显示

        // 切换动画：手枪是 0，只要不是手枪（长枪/霰弹/狙击），统一用步枪的持枪动作（1）
        if (playerAnimator != null)
        {
            int animType = (weaponIndex == 0) ? 0 : 1;
            playerAnimator.SetInteger("WeaponType", animType);
        }
    }

    // 负责显示当前选中的模型，隐藏其他的
    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            weapon.gameObject.SetActive(i == selectedWeapon);
            i++;
        }
    }
}