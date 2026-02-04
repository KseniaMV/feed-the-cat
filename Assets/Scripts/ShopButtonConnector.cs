using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт для подключения кнопок магазина к системе платежей RuStore
/// Добавьте этот скрипт на каждый элемент покупки в ShopItemContainer
/// </summary>
public class ShopButtonConnector : MonoBehaviour
{
    [Header("Настройки кнопки")]
    [Tooltip("ID продукта для покупки (coins_pack, pack_1, pack_2, pack_3)")]
    public string productId = "";
    
    [Tooltip("Кнопка покупки")]
    public Button purchaseButton;
    
    [Tooltip("Текст кнопки (для отображения статуса)")]
    public TMPro.TextMeshProUGUI buttonText;
    
    [Header("Сообщения")]
    [Tooltip("Панель сообщений (опционально)")]
    public GameObject messagePanel;
    
    [Tooltip("Текст сообщения")]
    public TMPro.TextMeshProUGUI messageText;

    private ShopBillingManager billingManager;

    private void Start()
    {
        InitializeReferences();
        SetupButton();
        SubscribeToEvents();
    }

    /// <summary>
    /// Инициализирует ссылки на менеджеры
    /// </summary>
    private void InitializeReferences()
    {
        billingManager = ShopBillingManager.Instance;
        
        if (billingManager == null)
        {
            Debug.LogError("ShopButtonConnector: ShopBillingManager не найден!");
        }
    }

    /// <summary>
    /// Настраивает кнопку покупки
    /// </summary>
    private void SetupButton()
    {
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseButtonClick);
        }
        else
        {
            Debug.LogWarning("ShopButtonConnector: Кнопка покупки не назначена!");
        }
    }

    /// <summary>
    /// Подписывается на события платежей
    /// </summary>
    private void SubscribeToEvents()
    {
        ShopBillingManager.OnPurchaseSuccessful += OnPurchaseSuccessful;
        ShopBillingManager.OnPurchaseFailed += OnPurchaseFailed;
    }

    /// <summary>
    /// Обработчик нажатия кнопки покупки
    /// </summary>
    private void OnPurchaseButtonClick()
    {
        if (string.IsNullOrEmpty(productId))
        {
            Debug.LogError("ShopButtonConnector: ProductId не установлен!");
            ShowMessage("Ошибка: ID продукта не установлен", false);
            return;
        }

        if (billingManager == null)
        {
            Debug.LogError("ShopButtonConnector: BillingManager не найден!");
            ShowMessage("Ошибка: Платежная система недоступна", false);
            return;
        }

        if (!billingManager.IsBillingInitialized())
        {
            Debug.LogWarning("ShopButtonConnector: BillingManager не инициализирован");
            ShowMessage("Платежная система не готова. Попробуйте позже.", false);
            return;
        }

        
        // Отключаем кнопку на время обработки
        SetButtonEnabled(false);
        UpdateButtonText("Обработка...");
        
        // Вызываем соответствующую функцию покупки
        switch (productId)
        {
            case "coins_pack":
                billingManager.BuyCoinsPack();
                break;
            case "pack_1":
                billingManager.BuyPack1();
                break;
            case "pack_2":
                billingManager.BuyPack2();
                break;
            case "pack_3":
                billingManager.BuyPack3();
                break;
            default:
                Debug.LogError($"ShopButtonConnector: Неизвестный productId: {productId}");
                ShowMessage("Ошибка: Неизвестный продукт", false);
                SetButtonEnabled(true);
                UpdateButtonText("Купить");
                break;
        }
    }

    /// <summary>
    /// Обработчик успешной покупки
    /// </summary>
    private void OnPurchaseSuccessful(string purchasedProductId, int coins, int bombs, int knives, int cats)
    {
        // Проверяем, относится ли успешная покупка к нашему продукту
        if (purchasedProductId == productId)
        {
            
            // Формируем сообщение о награде
            string rewardMessage = "Покупка успешна!\nПолучено:\n";
            
            if (coins > 0) rewardMessage += $"{coins} монет\n";
            if (bombs > 0) rewardMessage += $"{bombs} бомбочек\n";
            if (knives > 0) rewardMessage += $"{knives} нож-вилок\n";
            if (cats > 0) rewardMessage += $"{cats} котов-обжорок\n";
            
            ShowMessage(rewardMessage, true);
            UpdateButtonText("Куплено!");
            
            // Включаем кнопку обратно через несколько секунд
            Invoke(nameof(ResetButton), 3f);
        }
    }

    /// <summary>
    /// Обработчик ошибки покупки
    /// </summary>
    private void OnPurchaseFailed(string errorMessage)
    {
        Debug.LogError($"ShopButtonConnector: Ошибка покупки {productId}: {errorMessage}");
        
        ShowMessage($"Ошибка покупки:\n{errorMessage}", false);
        SetButtonEnabled(true);
        UpdateButtonText("Купить");
    }

    /// <summary>
    /// Сбрасывает кнопку в исходное состояние
    /// </summary>
    private void ResetButton()
    {
        SetButtonEnabled(true);
        UpdateButtonText("Купить");
        HideMessage();
    }

    /// <summary>
    /// Включает/отключает кнопку
    /// </summary>
    private void SetButtonEnabled(bool enabled)
    {
        if (purchaseButton != null)
        {
            purchaseButton.interactable = enabled;
        }
    }

    /// <summary>
    /// Обновляет текст кнопки
    /// </summary>
    private void UpdateButtonText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }

    /// <summary>
    /// Показывает сообщение пользователю
    /// </summary>
    private void ShowMessage(string message, bool isSuccess)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messageText.color = isSuccess ? Color.green : Color.red;
            messagePanel.SetActive(true);
            
            // Автоматически скрываем сообщение через 5 секунд
            Invoke(nameof(HideMessage), 5f);
        }
        else
        {
            // Если нет панели сообщений, выводим в консоль
        }
    }

    /// <summary>
    /// Скрывает сообщение
    /// </summary>
    private void HideMessage()
    {
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    /// <summary>
    /// Устанавливает ID продукта (для использования из других скриптов)
    /// </summary>
    public void SetProductId(string newProductId)
    {
        productId = newProductId;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        ShopBillingManager.OnPurchaseSuccessful -= OnPurchaseSuccessful;
        ShopBillingManager.OnPurchaseFailed -= OnPurchaseFailed;
        
        // Отписываемся от событий кнопки
        if (purchaseButton != null)
        {
            purchaseButton.onClick.RemoveListener(OnPurchaseButtonClick);
        }
    }
}
