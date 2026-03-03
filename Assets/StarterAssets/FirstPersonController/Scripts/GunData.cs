using UnityEngine;

// 这一行极其重要！它能让你的 Unity 右键菜单里多出一个创建武器数据的选项
[CreateAssetMenu(fileName = "New Gun", menuName = "Weapon/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("基础信息")]
    public string gunName = "新武器";

    [Header("伤害与射程")]
    [Tooltip("子弹单发伤害")]
    public int damage = 30;

    [Tooltip("子弹最大有效射程")]
    public float range = 200f;

    [Header("射速与换弹")]
    [Tooltip("射速：开两枪之间的间隔秒数。数值越小射速越快(如0.1是步枪，1.5是狙击)")]
    public float fireRate = 0.1f;

    [Tooltip("换弹需要花费的时间(秒)")]
    public float reloadTime = 2f;

    [Header(" 弹药系统")]
    [Tooltip("一个弹匣能装多少发子弹")]
    public int maxAmmoInMag = 30;

    [Tooltip("玩家最多能带多少发备用子弹")]
    public int maxReserveAmmo = 90;

    [Header("弹道与后坐力 (硬核手感)")]
    [Tooltip("准星散布大小：0代表指哪打哪(激光枪)，0.05代表步枪连射散布")]
    [Range(0f, 0.2f)] // 变成一个滑动条，防止填太大子弹飞到脑后
    public float spread = 0.02f;

    [Tooltip("后坐力大小：开枪时镜头往上抬的力度")]
    [Range(0f, 10f)]
    public float recoilForce = 2f;
}