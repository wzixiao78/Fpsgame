using UnityEngine;
// 如果报错说找不到 StarterAssets，请把下面这一行注释掉，或者检查你是不是用的官方的第一人称包
using StarterAssets;

public class TacticalMovement : MonoBehaviour
{
    [Header("设置")]
    public FirstPersonController fpsController; // 引用官方的控制器脚本
    public CharacterController charController;  // 引用角色身体

    [Header("速度设置")]
    public float runSpeed = 6.0f;   // 默认跑步速度 (快)
    public float walkSpeed = 2.5f;  // 静步速度 (慢，且无声)
    public float crouchSpeed = 1.5f; // 下蹲速度 (最慢)

    [Header("下蹲设置")]
    public float standHeight = 1.8f;   // 站立高度
    public float crouchHeight = 0.9f;  // 下蹲高度
    public float heightChangeSpeed = 10f; // 蹲下/起立的动画速度
    public Transform cameraRoot;       // 摄像机的父物体 (用来降低视角)
    public float crouchCamOffset = -0.4f; // 下蹲时摄像机降低多少

    // 状态标记 (给动画和音效系统用的)
    public bool isCrouching { get; private set; }
    public bool isWalking { get; private set; }

    private float defaultCamY;
    private float targetHeight;
    private float targetCamY;

    void Start()
    {
        // 自动获取组件
        if (fpsController == null) fpsController = GetComponent<FirstPersonController>();
        if (charController == null) charController = GetComponent<CharacterController>();

        // 记录初始状态
        if (charController != null) standHeight = charController.height;
        if (cameraRoot != null) defaultCamY = cameraRoot.localPosition.y;

        targetHeight = standHeight;
        targetCamY = defaultCamY;
    }

    void Update()
    {
        HandleStance();
        UpdateSpeed();
        SmoothTransition();
    }

    void HandleStance()
    {
        // 1. 处理下蹲 (按住 Left Control)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            isWalking = false; // 蹲下时覆盖静步状态

            targetHeight = crouchHeight;
            targetCamY = defaultCamY + crouchCamOffset;
        }
        else
        {
            isCrouching = false;
            targetHeight = standHeight;
            targetCamY = defaultCamY;

            // 2. 处理静步 (只有没蹲下时，按住 Left Shift 才是静步)
            // 默认是跑步，按住 Shift 变慢
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isWalking = true;
            }
            else
            {
                isWalking = false; // 松开Shift，恢复默认跑步
            }
        }
    }

    void UpdateSpeed()
    {
        if (fpsController == null) return;

        float targetSpeed = runSpeed; // 默认是跑步

        // 1. 判断我们要用什么速度
        if (isCrouching)
        {
            targetSpeed = crouchSpeed;
        }
        else if (isWalking)
        {
            targetSpeed = walkSpeed;
        }

        // 2. 【关键修复】同时修改 MoveSpeed 和 SprintSpeed
        // 这样无论官方脚本认为现在是“走”还是“跑”，它都只能用我们给的速度
        fpsController.MoveSpeed = targetSpeed;
        fpsController.SprintSpeed = targetSpeed;
    }

    void SmoothTransition()
    {
        // 平滑改变胶囊体高度
        if (charController != null)
        {
            charController.height = Mathf.Lerp(charController.height, targetHeight, Time.deltaTime * heightChangeSpeed);
            // 修正中心点，保证是从头顶缩下来，而不是两头缩
            charController.center = new Vector3(0, charController.height / 2f, 0);
        }

        // 平滑改变摄像机高度
        if (cameraRoot != null)
        {
            Vector3 newPos = cameraRoot.localPosition;
            newPos.y = Mathf.Lerp(newPos.y, targetCamY, Time.deltaTime * heightChangeSpeed);
            cameraRoot.localPosition = newPos;
        }
    }
}