using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour // Или добавь это в PlayerMovement
{
    // Этот метод нужно привязать в PlayerInput (Invoke Unity Events) -> Action "Pause"
    public void OnPauseInput(InputAction.CallbackContext context)
    {
        // Обязательная проверка фазы нажатия!
        if (context.performed)
        {
            // Ищем наше меню и переключаем его
            if (GameMenuSystem.Instance != null)
            {
                GameMenuSystem.Instance.TogglePauseManual();
            }
        }
    }
}