using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("后坐力恢复设置")]
    public float snappiness = 10f;    // 枪口上抬的爆发力速度
    public float returnSpeed = 5f;    // 枪口自然下坠恢复的速度

    void Update()
    {
        // 让目标旋转角度缓慢回落到 0 (恢复准心)
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

        // 让当前摄像机的旋转平滑地跟上目标旋转
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

        // 应用旋转到摄像机上
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    // 当开枪时，SimpleShoot 会呼叫这个函数
    public void RecoilFire(float recoilForce)
    {
        // 向上抬枪口 (X轴负方向)，并加一点点随机的左右抖动 (Y轴)
        float recoilX = -recoilForce;
        float recoilY = Random.Range(-recoilForce / 3f, recoilForce / 3f);

        targetRotation += new Vector3(recoilX, recoilY, 0);
    }
}