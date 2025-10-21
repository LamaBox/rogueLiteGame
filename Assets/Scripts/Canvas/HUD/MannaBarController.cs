using System;
using UnityEngine;
using UnityEngine.UI;

public class MannaBarController : MonoBehaviour
{
    [Header("Manna Settings")]
    [SerializeField] private float manna = 50;
    [SerializeField] private float maxManna = 100f;
    [SerializeField] private float regenMannaPerSecond = 1f;

    private float fromMannaToSliderCoefficient = 0.01f;
    private const float WidthPerManna = 3.4f;

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
        AddManna(regenMannaPerSecond * Time.fixedDeltaTime);
        UpdateSlider();
        ChangeMaxManna(maxManna);
    }

    public void AddManna(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Manna value cannot be negative");

        manna = Mathf.Min(manna + value, maxManna);
    }

    public void IncreaseRegenManna(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Manna regen value cannot be negative");
        
        regenMannaPerSecond = Mathf.Max(0, regenMannaPerSecond + value);
    }

    public void DecreaseRegenManna(float value)
    {
        if (value < 0)  
            throw new ArgumentOutOfRangeException(nameof(value), "Manna regen value cannot be negative");
        
        regenMannaPerSecond = Mathf.Max(0, regenMannaPerSecond - value);
    }

    public void IncreaseMaxManna(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Manna max value cannot be negative");
        ChangeMaxManna(maxManna + value);
    }

    public void DecreaseMaxManna(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Manna max value cannot be negative");

        if (value >= maxManna)
        {
            // TODO
            // тут персонаж должен умирать
        }
        
        ChangeMaxManna(maxManna - value);
    }

    private void ChangeMaxManna(float newHP)
    {
        maxManna = newHP;
        rect.sizeDelta = new Vector2(newHP * WidthPerManna, rect.sizeDelta.y);
        fromMannaToSliderCoefficient = 1 / newHP;
    }

    public void ReduceManna(float value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Manna value cannot be negative");

        manna = Mathf.Max(manna - value, 0);
    }

    public bool IsZeroHp() => manna <= 0;

    private void UpdateSlider()
    {
        slider.value = manna * fromMannaToSliderCoefficient;
    }
}
