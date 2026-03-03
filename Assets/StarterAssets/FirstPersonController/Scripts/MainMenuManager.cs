using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // 代码认识 TMP_Dropdown 下拉菜单

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject exitPanel;
    public GameObject lobbyPanel;    // 👈 备战大厅面板

    [Header("设置滑动条引用")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    //  存入内存的静态数据（跨场景通用！）
    public static float currentSensitivity = 2f;
    public static int selectedMapIndex = 0;       // 记住选了第几个地图 (0是第一个)
    public static int selectedDifficulty = 1;     // 记住难度 (默认 1，比如普通难度)

    void Start()
    {
        // 游戏开局状态
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        exitPanel.SetActive(false);
        lobbyPanel.SetActive(false);

        if (volumeSlider != null) volumeSlider.value = AudioListener.volume;
        if (sensitivitySlider != null) sensitivitySlider.value = currentSensitivity;
    }

    // ====== 主菜单与面板切换 ======

    // 打开备战大厅
    public void OpenLobby()
    {
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    // 从大厅退回主菜单
    public void CloseLobby()
    {
        lobbyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenSettings() { mainMenuPanel.SetActive(false); settingsPanel.SetActive(true); }
    public void CloseSettings() { settingsPanel.SetActive(false); mainMenuPanel.SetActive(true); }
    public void ShowExitPrompt() { exitPanel.SetActive(true); }
    public void CancelExit() { exitPanel.SetActive(false); }
    public void ConfirmQuit() { Application.Quit(); }

    // ====== 设置核心功能 ======
    public void SetVolume(float value) { AudioListener.volume = value; }
    public void SetSensitivity(float value) { currentSensitivity = value; }

    // ====== 备战大厅核心功能 ======

    // 当下拉菜单改变时，记住选中的地图编号
    public void SetMap(int index)
    {
        selectedMapIndex = index;
    }

    // 当下拉菜单改变时，记住选中的难度编号
    public void SetDifficulty(int index)
    {
        selectedDifficulty = index;
    }

    // 终极按钮：带着数据加载战场！
    // 🚀 终极按钮：根据玩家选择的地图，加载不同的场景！
    public void LaunchGame()
    {
        // selectedMapIndex 是玩家在下拉菜单里选的编号（0是第一项，1是第二项）
        if (selectedMapIndex == 0)
        {
            Debug.Log("Realoding：Tutorial ");
            SceneManager.LoadScene("Tutorial_Scene"); //  确保名字和你的教程场景一模一样！
        }
        else if (selectedMapIndex == 1)
        {
            Debug.Log("Realoding: Transportship");
            SceneManager.LoadScene("MainGame_Scene"); //  确保名字和你的实战场景一模一样！
        }
        else
        {
            // 如果以后你加了第三张地图，可以在这里继续写 else if (selectedMapIndex == 2) ...
            Debug.LogWarning("未知的地图编号！");
        }
    }
}