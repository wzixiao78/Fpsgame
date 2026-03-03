using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public int selectedWeapon = 0;

    void Start()
    {
        SelectWeapon();
    }


    public void SwitchWeapon(int weaponIndex)
    {
        // 防止越界报错 (比如你只做了3把枪，却想切到索引5)
        if (weaponIndex >= transform.childCount)
        {
            Debug.LogWarning($"武器索引 {weaponIndex} 超出范围！当前只有 {transform.childCount} 把枪。");
            return;
        }

        selectedWeapon = weaponIndex;
        SelectWeapon();
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
                weapon.gameObject.SetActive(true);
            else
                weapon.gameObject.SetActive(false);
            i++;
        }
    }
}