using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    // Публичная статическая ссылка для доступа из любого скрипта
    public static ScoreCounter Instance { get; private set; }

    // Переменная для хранения текущего счета
    private int _currentScore = 0;

    private void Awake()
    {
        // Реализация Синглтона (без DontDestroyOnLoad)
        if (Instance != null && Instance != this)
        {
            Destroy(this); // Уничтожаем дубликат, если он вдруг появился
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Метод 1: Начисление очков.
    /// Можно передавать отрицательное значение, чтобы отнять очки.
    /// </summary>
    /// <param name="amount">Количество очков</param>
    public void AddScore(int amount)
    {
        _currentScore += amount;
        
        // Для удобства можно выводить в консоль
        Debug.Log($"Очки изменены. Текущий счет: {_currentScore}");
    }

    /// <summary>
    /// Метод 2: Проверка (получение) текущего счета.
    /// </summary>
    /// <returns>Текущее количество очков</returns>
    public int GetScore()
    {
        return _currentScore;
    }
    
    // Опционально: Метод для полного сброса очков (если нужно)
    public void ResetScore()
    {
        _currentScore = 0;
    }
}