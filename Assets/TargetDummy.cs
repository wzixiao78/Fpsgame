using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    [Header("Attributes")] 
    public float health = 50f;

    
    public void TakeDamage(float amount)
    {
        health -= amount;

       
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = Color.red;
            Invoke("ResetColor", 0.1f); 
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    void ResetColor()
    {
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    void Die()
    {
        Debug.Log("Enemy Died!"); 
        Destroy(gameObject);
    }
}