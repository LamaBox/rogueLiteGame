using System;
using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    [Header("HP Settings")]
    [SerializeField] private float hp = 50;
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float regenHpPerSecond = 1f;

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

    public void AddHp(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP value cannot be negative");

        hp = Mathf.Min(hp + value, maxHp);
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
        }
        
        ChangeMaxHp(maxHp - value);
    }

    private void ChangeMaxHp(float newHP)
    {
        maxHp = newHP;
        rect.sizeDelta = new Vector2(newHP * WidthPerHp, rect.sizeDelta.y);
        fromHpToSliderCoefficient = 1 / newHP;
    }

    public void ReduceHp(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "HP value cannot be negative");

        hp = Mathf.Max(hp - value, 0);
    }

    public bool IsZeroHp() => hp <= 0;

    private void UpdateSlider()
    {
        slider.value = hp * fromHpToSliderCoefficient;
    }
}