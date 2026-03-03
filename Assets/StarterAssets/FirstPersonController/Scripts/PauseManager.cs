using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 兼容你的 Starter Assets 新输入系统
#endif

public class PauseManager : MonoBehaviour
{
    [Header("拖入 Canvas 下的 Settings_Panel 预制体")]
    public GameObject settingsPanel;

    private bool isPaused = false;

    void Start()
    {
        // 确保进游戏时，设置面板是隐藏的
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void Update()
    {
        // 监听 ESC 键 (兼容新老输入系统)
        bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) escPressed = true;
#else
        if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
#endif

        if (escPressed)
        {
            if (isPaused) ResumeGame(); // 如果已经暂停了，就恢复
            else PauseGame();           // 如果没暂停，就呼出菜单
        }
    }

    // ====== 暂停与恢复控制 ======

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;           // 冻结游戏里的时间！敌人和子弹都会停下
        settingsPanel.SetActive(true); // 呼出你原汁原味的设置面板

        // 解锁鼠标，让玩家可以点击面板
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 这个函数用来给设置面板里的 "Back" 按钮连线
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;            //  恢复时间流逝
        settingsPanel.SetActive(false); // 隐藏面板

        // 重新锁定并隐藏鼠标，继续战斗
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ====== 退出与设置联动 ======

    // 这个函数用来给设置面板里的 "Exit" 按钮连线
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f; // ⚠️ 极其重要：切场景前必须把时间恢复正常，否则回大厅也会卡死！
        SceneManager.LoadScene("MainMenu_Scene");
    }

    // 接管音量滑动条
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    // 接管灵敏度滑动条
    public void SetSensitivity(float value)
    {
        MainMenuManager.currentSensitivity = value; // 把新数据写进咱们大厅建好的内存里
    }
}