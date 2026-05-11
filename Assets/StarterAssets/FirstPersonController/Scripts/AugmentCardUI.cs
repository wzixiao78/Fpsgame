using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentCardUI : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;

    // 这张卡当前代表的海克斯数据
    private AugmentData currentData;

    // 这个方法由发牌员 (AugmentManager) 呼叫，用来刷新卡片上的图文
    public void SetupCard(AugmentData data)
    {
        currentData = data;

        // 替换UI上的文字
        titleText.text = data.augmentName;
        descriptionText.text = data.description;

        // 替换UI上的图片（如果没有配置图片，就隐藏图片框）
        if (data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    // 当玩家点击这张卡片时，按钮会触发这个方法
    public void OnCardClicked()
    {
        if (currentData == null) return;

        Debug.Log("🎯 玩家选择了海克斯：" + currentData.augmentName);

        // 核心：让 GameManager 去执行具体的海克斯效果！
        if (AugmentManager.instance != null)
        {
            AugmentManager.instance.ApplyAugment(currentData); // ⬅️ 执行效果 (比如发枪、加血)
            AugmentManager.instance.CloseAugmentPanel();       // ⬅️ 关闭UI面板，恢复游戏时间
        }
    }
}