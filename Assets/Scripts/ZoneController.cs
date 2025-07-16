using UnityEngine;

public class ZoneController : MonoBehaviour
{
    public bool hasUnit = false;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateAlpha();
    }

    void Update()
    {
        UpdateAlpha(); // можна замінити на подію, якщо хочеш не щокадрово
    }

    private void UpdateAlpha()
    {
        if (sr == null) return;

        Color c = sr.color;
        c.a = hasUnit ? 0.4f : 1f; // Прозора або повна
        sr.color = c;
    }
}
