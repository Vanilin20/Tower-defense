using UnityEngine;

public class HomeBase : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Базу атакують! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Базу знищено!");
            // додай тут Game Over або знищення
            Destroy(gameObject);
        }
    }
}
