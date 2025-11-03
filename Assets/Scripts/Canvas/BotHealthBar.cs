using System;
using UnityEngine;

public class BotHealthBar : MonoBehaviour
{
    [SerializeField] GameObject bot;
    [SerializeField] Transform fillAmount;

    private Transform transf;
    
    private BotBase botBase;

    void Start()
    {
        transf = GetComponent<RectTransform>();
        botBase = bot.GetComponent<BotBase>();
        botBase.OnHealthChanged += OnHealthChanged;
    }
    void Update()
    {
        transf.position = new Vector3(botBase.transform.position.x, botBase.transform.position.y + 2, 0);
    }

    private void OnHealthChanged(float current, float max, float percentage)
    {
        fillAmount.localScale = new Vector3(percentage, 1, 1);
    }
}
