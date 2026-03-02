using UnityEngine;
using System.Collections;

public class SimpleShoot : MonoBehaviour
{
    [Header(" 插入武器数据芯片")]
    public GunData gunData;

   

    [Header("引用设置")]
    public Camera playerCamera;
    public CameraRecoil recoilScript; // 拖入挂着 CameraRecoil 摇晃脚本的摄像机
    public Transform muzzlePoint;     // 枪口位置
    public GameObject muzzleFlashPrefab; // 火光特效
    public AudioSource audioSource;
    public AudioClip shootSound;      // 可以给不同枪放不同音效
    public GameObject hitMarkerUI;

    // 内部运行时的状态变量（不需要在面板修改）
    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        // 游戏刚开始时，读取芯片里的弹匣和备弹上限
        if (gunData != null)
        {
            currentAmmo = gunData.maxAmmoInMag;
            reserveAmmo = gunData.maxReserveAmmo;
        }
        UpdateUI();
    }

    void Update()
    {
        // 准备阶段不准开枪
        if (RoundManager.instance != null && RoundManager.instance.currentState != RoundManager.RoundState.CombatPhase)
            return;

        if (gunData == null || isReloading) return;

        // 连射检测：读取芯片里的 fireRate
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                nextFireTime = Time.time + gunData.fireRate;
                Shoot();
            }
            else if (currentAmmo <= 0 && Input.GetMouseButtonDown(0))
            {
                TryReload();
            }
        }

        // 手动换弹
        if (Input.GetKeyDown(KeyCode.R)) TryReload();
    }

    void Shoot()
    {
        currentAmmo--;
        UpdateUI();

        // 播放专属音效和火光
        if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);
        if (muzzleFlashPrefab && muzzlePoint)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            Destroy(flash, 0.1f);
        }

        // 💥 读取芯片里的后坐力 (recoilForce)
        if (recoilScript != null && gunData != null)
        {
            recoilScript.RecoilFire(gunData.recoilForce);
        }

        //  读取芯片里的散布 (spread)
        Vector3 shootDirection = playerCamera.transform.forward;
        shootDirection += playerCamera.transform.right * Random.Range(-gunData.spread, gunData.spread);
        shootDirection += playerCamera.transform.up * Random.Range(-gunData.spread, gunData.spread);
        shootDirection.Normalize();

        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Player");

        // 读取芯片里的射程 (range)
        if (Physics.Raycast(playerCamera.transform.position, shootDirection, out hit, gunData.range, layerMask))
        {

            if (hit.collider.CompareTag("Ally"))
            {
                Debug.Log("停火！那是自己人！");
                return; // 遇到友军，直接中止这颗子弹的后续伤害逻辑！
            }

            BodyPartHitbox hitbox = hit.transform.GetComponent<BodyPartHitbox>();
            if (hitbox != null)
            {
                hitbox.OnHit(gunData.damage); // 读取芯片里的伤害 (damage)
                TriggerHitMarker();
            }
            else
            {
                EnemyHealth enemy = hit.transform.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(gunData.damage);
                    TriggerHitMarker();
                }
            }
        }
    }

    void TryReload()
    {
        if (isReloading || currentAmmo == gunData.maxAmmoInMag || reserveAmmo <= 0) return;
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        // 读取芯片里的换弹时间 (reloadTime)
        yield return new WaitForSeconds(gunData.reloadTime);

        int bulletsNeeded = gunData.maxAmmoInMag - currentAmmo;
        int bulletsToReload = Mathf.Min(bulletsNeeded, reserveAmmo);

        currentAmmo += bulletsToReload;
        reserveAmmo -= bulletsToReload;

        isReloading = false;
        UpdateUI();
    }

    // 回合重置子弹
    public void ResetAmmo()
    {
        if (gunData != null)
        {
            currentAmmo = gunData.maxAmmoInMag;
            reserveAmmo = gunData.maxReserveAmmo;
            isReloading = false;
            UpdateUI();
        }
    }

    void TriggerHitMarker()
    {
        if (hitMarkerUI != null)
        {
            hitMarkerUI.SetActive(true);
            CancelInvoke("HideHitMarker");
            Invoke("HideHitMarker", 0.1f);
        }
    }

    void HideHitMarker() => hitMarkerUI?.SetActive(false);

    void UpdateUI()
    {
        if (PlayerHUD.instance != null && gunData != null)
        {
            PlayerHUD.instance.UpdateAmmo(currentAmmo, reserveAmmo);
        }
    }
}