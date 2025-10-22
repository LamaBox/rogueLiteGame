using System;
using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private float hp = 1;
    [SerializeField] private float maxHp = 1;
    [SerializeField] private float regenHpPerSecond = 0;

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
        AddHp(regenHpPerSecond * Time.fixedDeltaTime);
        UpdateSlider();
        ChangeMaxHp(maxHp);
    }
    
    public void SetHp(float value)
    {
        if (value > hp)
        {
            AddHp(value - hp);
        } else if (value < hp)
        {
            ReduceHp(hp - value);
        }
    }

    public void SetMaxHp(float value)
    {
        if (value > maxHp)
        {
            IncreaseMaxHP(value - maxHp);
        }
        else if (value < maxHp)
        {
            DecreaseMaxHP(maxHp - value);
        }
    }

    public void AddHp(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP value cannot be negative");

        hp = Mathf.Min(hp + value, maxHp);
    }
    
    public void ReduceHp(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP value cannot be negative");

        hp = Mathf.Max(hp - value, 0);
    }
    
    
    public void IncreaseRegenHp(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP regen value cannot be negative");
        
        regenHpPerSecond = Mathf.Max(0, regenHpPerSecond + value);
    }
    
    public void DecreaseRegenHp(float value)
    {
        if (value < 0)  
            throw new ArgumentOutOfRangeException(nameof(value), "HP regen value cannot be negative");
        
        regenHpPerSecond = Mathf.Max(0, regenHpPerSecond - value);
    }

    public void IncreaseMaxHP(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP max value cannot be negative");
        ChangeMaxHp(maxHp + value);
    }

    public void DecreaseMaxHP(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP max value cannot be negative");

        if (value >= maxHp)
        {
            // TODO
            // тут персонаж должен умирать
            return;
        }
        
        ChangeMaxHp(maxHp - value);
    }

    private void ChangeMaxHp(float newHP)
    {
        maxHp = newHP;
        rect.sizeDelta = new Vector2(newHP * WidthPerHp, rect.sizeDelta.y);
        fromHpToSliderCoefficient = 1 / newHP;
    }
    
    public bool IsZeroHp() => hp <= 0;

    private void UpdateSlider()
    {
        slider.value = hp * fromHpToSliderCoefficient;
    }
}