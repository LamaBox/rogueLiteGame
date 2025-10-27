using System;
using UnityEngine;
using UnityEngine.UI;

public class StaminaController : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float stamina = 1;
    [SerializeField] private float maxStamina = 1;
    [SerializeField] private float regenStaminaPerSecond = 0;

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
    
    public void SetStamina(float value)
    {
        if (value > stamina)
        {
            AddStamina(value - stamina);
        } else if (value < stamina)
        {
            ReduceStamina(stamina - value);
        }
    }

    public void SetMaxStamina(float value)
    {
        if (value > maxStamina)
        {
            IncreaseMaxStamina(value - maxStamina);
        }
        else if (value < maxStamina)
        {
            DecreaseMaxStamina(maxStamina - value);
        }
    }

    public void AddStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina value cannot be negative");

        stamina = Mathf.Min(stamina + value, maxStamina);
    }
    
    public void ReduceStamina(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Stamina value cannot be negative");

        stamina = Mathf.Max(stamina - value, 0);
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
            // пока допустим что стамина не может уйти в 0
            return;
        }
        
        ChangeMaxStamina(maxStamina - value);
    }

    private void ChangeMaxStamina(float newHP)
    {
        maxStamina = newHP;
        rect.sizeDelta = new Vector2(newHP * WidthPerHp, rect.sizeDelta.y);
        fromHpToSliderCoefficient = 1 / newHP;
    }
    
    public bool IsZeroStamina() => stamina <= 0;

    private void UpdateSlider()
    {
        slider.value = stamina * fromHpToSliderCoefficient;
    }
}
