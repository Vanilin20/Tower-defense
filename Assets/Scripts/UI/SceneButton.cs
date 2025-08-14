using UnityEngine;
using UnityEngine.EventSystems;

// Кнопка в сцені для управління героєм
public class SceneButton : MonoBehaviour, IPointerClickHandler
{
    public enum ButtonType
    {
        ReturnToSpawn,
        ReturnToZone
    }

    [Header("Налаштування")]
    public ButtonType buttonType = ButtonType.ReturnToSpawn;

    [Header("Візуалізація")]
    public SpriteRenderer buttonSprite;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color disabledColor = Color.gray;

    // Локальний стан
    private bool isHovered = false;
    private bool isEnabled = true;

    private void Start()
    {
        UpdateVisual();
        GameManager.OnHeroSelected += OnHeroSelected;
    }

    private void OnDestroy()
    {
        GameManager.OnHeroSelected -= OnHeroSelected;
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void OnHeroSelected(GameObject hero)
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        GameObject selectedHero = GameManager.GetSelectedHero();
        if (selectedHero == null)
        {
            SetEnabled(false);
            return;
        }

        HeroController heroController = selectedHero.GetComponent<HeroController>();
        if (heroController == null)
        {
            SetEnabled(false);
            return;
        }

        bool canBeActive;
        if (buttonType == ButtonType.ReturnToSpawn)
        {
            // Доступна, якщо герой має прив'язану зону і не рухається (незалежно від того, чи він у зоні)
            canBeActive = heroController.HasZone() && !heroController.IsMoving();
        }
        else
        {
            // Доступна, якщо герой має прив'язану зону, не рухається і зараз НЕ у зоні
            canBeActive = heroController.HasZone() && !heroController.IsMoving() && !IsAtZone(heroController);
        }

        SetEnabled(canBeActive);
    }

    private void SetEnabled(bool enabled)
    {
        if (isEnabled != enabled)
        {
            isEnabled = enabled;
            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        if (buttonSprite == null) return;

        if (!isEnabled)
        {
            buttonSprite.color = disabledColor;
        }
        else if (isHovered)
        {
            buttonSprite.color = hoverColor;
        }
        else
        {
            buttonSprite.color = normalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEnabled) return;

        GameObject selectedHero = GameManager.GetSelectedHero();
        if (selectedHero == null) return;

        HeroController heroController = selectedHero.GetComponent<HeroController>();
        if (heroController == null) return;

        if (buttonType == ButtonType.ReturnToSpawn)
        {
            heroController.ReturnToSpawn();
        }
        else
        {
            heroController.ReturnToZone();
        }
    }

    private void OnMouseEnter()
    {
        if (!isEnabled) return;
        isHovered = true;
        UpdateVisual();
    }

    private void OnMouseExit()
    {
        isHovered = false;
        UpdateVisual();
    }

	private bool IsAtZone(HeroController controller)
	{
		// В HeroController немає публічного прапорця isAtZone, тож визначаємо по дистанції до зони
		if (!controller.HasZone()) return false;
		Zone z = controller.GetCurrentZone();
		if (z == null) return false;
		float dist = Vector3.Distance(controller.transform.position, z.transform.position);
		return dist <= controller.stopDistance + 0.05f;
	}
} 