using UnityEngine;

public class BodyPartHitbox : MonoBehaviour
{
    [Header("部位设置")]
    public EnemyHealth mainHealthScript; // 引用主血条
    public float damageMultiplier = 1.0f; // 伤害倍率 (头=2, 腿=0.5)

    public void OnHit(float baseDamage)
    {
        if (mainHealthScript != null)
        {
            float finalDamage = baseDamage * damageMultiplier;
            Debug.Log($"<color=red>打中了 {gameObject.name} ! 倍率: {damageMultiplier}</color>");
            mainHealthScript.TakeDamage(finalDamage);
        }
    }
}