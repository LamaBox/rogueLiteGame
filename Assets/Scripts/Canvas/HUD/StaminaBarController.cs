using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaController : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float stamina = 50;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenStaminaPerSecond = 1f;

    private float fromHpToSliderCoefficient = 0.01f;
    private const float WidthPerHp = 3.4f;

    private Slider slider;
    private RectTransform rect;

    void Awake()
    {
        slider = GetComponent<Slider>();
        rect = GetComponent<RectTransform>();
        if (slider == null)
            Debug.LogError("Не удалось получить компонент Slider");
        if (rect == null)
            Debug.LogError("Не удалось получить компонент Transform");
    }

    void Start()
    {
        UpdateSlider();
    }

    private void FixedUpdate()
    {
        AddStamina(regenStaminaPerSecond * Time.fixedDeltaTime);
        UpdateSlider();
        ChangeMaxStamina(maxStamina);
    }

    public void AddStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina value cannot be negative");

        stamina = Mathf.Min(stamina + value, maxStamina);
    }

    public void IncreaseRegenStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina regen value cannot be negative");
        
        regenStaminaPerSecond = Mathf.Max(0, regenStaminaPerSecond + value);
    }

    public void DecreaseRegenStamina(float value)
    {
        if (value < 0)  
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina regen value cannot be negative");
        
        regenStaminaPerSecond = Mathf.Max(0, regenStaminaPerSecond - value);
    }

    public void IncreaseMaxStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina max value cannot be negative");
        ChangeMaxStamina(maxStamina + value);
    }

    public void DecreaseMaxStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina max value cannot be negative");

        if (value >= maxStamina)
        {
            // TODO
            // тут персонаж должен умирать
        }
        
        ChangeMaxStamina(maxStamina - value);
    }

    private void ChangeMaxStamina(float newHP)
    {
        maxStamina = newHP;
        rect.sizeDelta = new Vector2(newHP * WidthPerHp, rect.sizeDelta.y);
        fromHpToSliderCoefficient = 1 / newHP;
    }

    public void ReduceStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina value cannot be negative");

        stamina = Mathf.Max(stamina - value, 0);
    }

    public bool IsZeroStamina() => stamina <= 0;

    private void UpdateSlider()
    {
        slider.value = stamina * fromHpToSliderCoefficient;
    }
}
