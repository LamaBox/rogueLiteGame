using UnityEngine;
using UnityEngine.UI;
using ResourceData = PlayerDataStructures.ResourceData;
using ResourceType = PlayerDataStructures.ResourceType;

namespace Canvas.HUD
{
    public class PlayerResourceHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider manaSlider;
        [SerializeField] private Slider staminaSlider;
        
        [Header("Debug")]
        [SerializeField] private bool enableLogging = false;
        
        [SerializeField] private PlayerData playerData;
        
        private HPBarController hPBarController;
        private MannaBarController mannaBarController;
        private StaminaController staminaController;

        private void Awake()
        {
            InitializeControllers();
        }

        private void Start()
        {
            if (enableLogging)
            {
                Debug.Log($"Controllers initialized - HP: {hPBarController != null}, Mana: {mannaBarController != null}, Stamina: {staminaController != null}");
            }
        }
        
        private void InitializeControllers()
        {
            if (healthSlider != null)
                hPBarController = healthSlider.GetComponent<HPBarController>();
            else
                Debug.LogWarning("No health slider reference");
            
            if (manaSlider != null)
                mannaBarController = manaSlider.GetComponent<MannaBarController>();
            else
                Debug.LogWarning("No mana slider reference");
            
            if (staminaSlider != null)
                staminaController = staminaSlider.GetComponent<StaminaController>();
            else 
                Debug.LogWarning("No stamina slider reference");
        }
        
        // private void OnEnable()
        // {
        //     playerData.OnResourceChanged += HandleResourceChanged;
        //     if (enableLogging)
        //         Debug.Log("Subscribed to PlayerData.OnResourceChanged");
        // }
        //
        // private void OnDisable()
        // {
        //     playerData.OnResourceChanged -= HandleResourceChanged;
        //     if (enableLogging)
        //         Debug.Log("Unsubscribed from PlayerData.OnResourceChanged");
        // }
        
        private void HandleResourceChanged(ResourceData resourceData)
        {
            if (enableLogging)
                Debug.Log($"Resource event received: {resourceData.Type} = {resourceData.Current}/{resourceData.Max}");

            switch (resourceData.Type)
            {
                case ResourceType.Health:
                    UpdateHealthUI(resourceData);
                    break;
                case ResourceType.Mana:
                    UpdateManaUI(resourceData);
                    break;
                case ResourceType.Stamina:
                    UpdateStaminaUI(resourceData);
                    break;
            }

            if (enableLogging)
                Debug.Log($"Resource event processed: {resourceData.Type}");
        }

        private void UpdateHealthUI(ResourceData healthData)
        {
            if (hPBarController == null)
            {
                Debug.LogError("HPBarController is null!");
                return;
            }

            float curHp = healthData.Current;
            float maxHp = healthData.Max;
            hPBarController.SetHp(curHp);
            hPBarController.SetMaxHp(maxHp);
            
            if (enableLogging)
                Debug.Log($"Health UI updated: {curHp}/{maxHp}");
        }

        private void UpdateManaUI(ResourceData manaData)
        {
            if (mannaBarController == null)
            {
                Debug.LogError("MannaBarController is null!");
                return;
            }

            float curMana = manaData.Current;
            float maxMana = manaData.Max;
            mannaBarController.SetManna(curMana);
            mannaBarController.SetMaxManna(maxMana);
            
            if (enableLogging)
                Debug.Log($"Mana UI updated: {curMana}/{maxMana}");
        }

        private void UpdateStaminaUI(ResourceData staminaData)
        {
            if (staminaController == null)
            {
                Debug.LogError("StaminaController is null!");
                return;
            }

            float curStamina = staminaData.Current;
            float maxStamina = staminaData.Max;
            staminaController.SetStamina(curStamina);
            staminaController.SetMaxStamina(maxStamina);
            
            if (enableLogging)
                Debug.Log($"Stamina UI updated: {curStamina}/{maxStamina}");
        }
    }
}