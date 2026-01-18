using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuSystem : MonoBehaviour
{
    public static GameMenuSystem Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject image; // Фон
    [SerializeField] private GameObject lose;
    [SerializeField] private GameObject win;
    
    [Header("HUD")]
    [SerializeField] private GameObject hpBar;
    [SerializeField] private GameObject mnBar;
    [SerializeField] private GameObject scoreBar;
    
    private PlayerData pdata;
    private GameObject player;

    private bool isPaused = false;
    private bool isGameEnded = false; // Флаг: игра закончена (победа или смерть)
    
    private int score = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResumeGamePhysics(); // Сброс времени при старте
        
        // Скрываем все лишнее
        SetObjectActive(menuPause, false);
        SetObjectActive(settings, false);
        SetObjectActive(image, false);
        SetObjectActive(lose, false);
        SetObjectActive(win, false);
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            pdata = player.GetComponent<PlayerData>();
            if (pdata != null)
            {
                pdata.OnDead += LoseGame;
            }
        }
        else
        {
            Debug.LogWarning("GameMenuSystem: Игрок не найден!");
        }
    }
    
    // Вызывается из скрипта Input игрока
    public void TogglePauseManual()
    {
        // Если игра уже закончилась (выиграли или проиграли), паузу открывать нельзя
        if (isGameEnded) return;

        if (!isPaused)
            PauseGame();
        else
            ResumeGame();
    }
    
    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        SetObjectActive(menuPause, true);
        SetObjectActive(image, true);
    }

    private void ResumeGame()
    {
        ResumeGamePhysics();
        SetObjectActive(menuPause, false);
        SetObjectActive(image, false);
        SetObjectActive(settings, false);
    }

    private void ResumeGamePhysics()
    {
        isPaused = false;
        Time.timeScale = 1;
    }
    
    public void ResumeButton()
    {
        ResumeGame();
    }

    public void ToSettings()
    {
        menuPause.SetActive(false);
        settings.SetActive(true);
    }

    public void ToMainPause()
    {
        menuPause.SetActive(true);
        settings.SetActive(false);
    }

    public void Quit()
    {
        ResumeGamePhysics(); // Важно!
        if (RoomManager.Instance != null) RoomManager.Instance.ResetRun();
        if (RunContextSystem.Instance != null) RunContextSystem.Instance.ResetRun();
        SceneManager.LoadScene("MainMenu");
    }
    
    private void LoseGame()
    {
        if (isGameEnded) return; // Защита от двойного вызова
        EndGameSequence();
        
        SetObjectActive(lose, true);
        Debug.Log("Game Over: Lose");
    }

    public void WinGame()
    {
        if (isGameEnded) return;
        EndGameSequence();
        
        SetObjectActive(win, true);
        Debug.Log("Game Over: Win");
    }

    // Общая логика для конца игры, чтобы не дублировать код
    private void EndGameSequence()
    {
        isGameEnded = true; // Блокируем паузу
        
        // Отписываемся сразу, чтобы не было ошибок
        if (pdata != null) pdata.OnDead -= LoseGame;

        Time.timeScale = 0;
        SetObjectActive(image, true);
        
        // Скрываем интерфейс
        SetObjectActive(hpBar, false);
        SetObjectActive(mnBar, false);
        SetObjectActive(scoreBar, false);
        
        // Отключаем игрока
        if (player != null) player.SetActive(false);
    }

    void OnDestroy()
    {
        // ВАЖНО: Проверка на null перед отпиской
        if (pdata != null)
        {
            pdata.OnDead -= LoseGame;
        }
    }

    // Вспомогательный метод, чтобы не писать if (obj != null) каждый раз
    private void SetObjectActive(GameObject obj, bool isActive)
    {
        if (obj != null) obj.SetActive(isActive);
    }
}