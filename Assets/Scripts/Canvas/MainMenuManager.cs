using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject settingsCanvas;

    void Start()
    {
        mainMenuCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        MusicSystem.Instance.PlayMusic(0);
    }
    
    public void PlayGame()
    {
        // Бросаем монетку: выпадет 0 или 1
        int roll = Random.Range(0, 2); 

        if (roll == 0)
        {
            Debug.Log("Загрузка Level");
            SceneManager.LoadScene("Level");
        }
        else
        {
            Debug.Log("Загрузка Level2");
            SceneManager.LoadScene("Level2");
        }
    }

    public void OpenSettings()
    {
        mainMenuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}