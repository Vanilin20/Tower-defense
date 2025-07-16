using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GoldPickup : MonoBehaviour
{
    private int goldAmount = 1;
    private bool canPickup = true;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D goldCollider;
    
    public float pickupCooldown = 0.1f;
    public float lifetime = 30f;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        goldCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer не знайдено!");
        if (goldCollider == null) Debug.LogError("Collider2D не знайдено!");
        
        // 🕐 Авто-знищення (працює навіть на паузі)
        StartCoroutine(DestroyAfterRealTime());
    }
    
    void Update()
    {
        // 🎯 Перевіряємо клік навіть коли гра на паузі (новий Input System)
        if (Mouse.current.leftButton.wasPressedThisFrame && canPickup)
        {
            CheckMouseClick();
        }
    }
    
    void CheckMouseClick()
    {
        // Отримуємо позицію миші через новий Input System
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        
        // Перевіряємо через bounds
        if (goldCollider != null && goldCollider.bounds.Contains(worldPos))
        {
            Debug.Log("Клік по золоту!");
            PickupGold();
            return; // Виходимо, щоб не викликати двічі
        }
        
        // Альтернативний метод через Raycast
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            Debug.Log("Raycast попав в золото!");
            PickupGold();
        }
    }
    
    public void SetGoldAmount(int amount)
    {
        goldAmount = amount;
    }
    
    void PickupGold()
    {
        Debug.Log("PickupGold викликано!");
        
        if (!canPickup) return;
        canPickup = false;
        
        // 💎 Додаємо золото в менеджер
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldAmount);
            Debug.Log($"Додано {goldAmount} золота!");
        }
        else
        {
            Debug.LogError("GoldManager.Instance не знайдено!");
        }
        
        // 🎨 Ефект підбору (працює навіть на паузі)
        StartCoroutine(PickupEffectRealTime());
    }
    
    // ✨ Ефект підбору використовуючи real time
    IEnumerator PickupEffectRealTime()
    {
        if (goldCollider != null)
            goldCollider.enabled = false;
            
        float duration = 0.3f;
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * 1.2f;
        Color startColor = spriteRenderer.color;
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // 👈 unscaledDeltaTime!
            float progress = timer / duration;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            spriteRenderer.color = currentColor;
            
            transform.Translate(Vector2.up * 2f * Time.unscaledDeltaTime); // 👈 unscaledDeltaTime!
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // ⏰ Авто-знищення використовуючи real time
    IEnumerator DestroyAfterRealTime()
    {
        float timer = 0f;
        
        while (timer < lifetime)
        {
            timer += Time.unscaledDeltaTime; // 👈 unscaledDeltaTime!
            yield return null;
        }
        
        if (gameObject != null)
        {
            StartCoroutine(FadeOutRealTime());
        }
    }
    
    // 🌟 Зникнення використовуючи real time
    IEnumerator FadeOutRealTime()
    {
        float fadeSpeed = 2f;
        
        while (spriteRenderer.color.a > 0f)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a -= fadeSpeed * Time.unscaledDeltaTime; // 👈 unscaledDeltaTime!
            spriteRenderer.color = currentColor;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // ❌ OnMouseDown НЕ ПРАЦЮЄ коли Time.timeScale = 0!
    void OnMouseDown()
    {
        Debug.Log("OnMouseDown (може не працювати на паузі)");
        if (canPickup)
        {
            PickupGold();
        }
    }
}