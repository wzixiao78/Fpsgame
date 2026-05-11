using UnityEngine;

// 这里定义了海克斯的各种类型（包括咱们加的献祭队友）
public enum AugmentType
{
    AddMaxHealth,
    GiveWeapon,
    IncreaseDamage,
    EconomyBoost,
    SacrificeForHealth
}

// 这个标签让你可以右键直接创建海克斯数据卡
[CreateAssetMenu(fileName = "New Augment", menuName = "Augment Data")]
public class AugmentData : ScriptableObject
{
    [Header("海克斯基础信息")]
    public string augmentName;      // 名字

    [TextArea(3, 5)]
    public string description;      // 描述

    public Sprite icon;             // 图标

    [Header("海克斯效果设置")]
    public AugmentType type;        // 效果类型
    public float value;             // 效果数值 (比如填 3 代表 3倍血量)
}