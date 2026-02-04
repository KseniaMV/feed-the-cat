using UnityEngine;
using RuStore.PayClient;
using RuStore;

/// <summary>
/// Скрипт для инициализации RuStore Pay SDK
/// Теперь использует новый Pay SDK вместо старого BillingClient
/// </summary>
public class RuStoreInitializer : MonoBehaviour
{
    [Header("Настройки RuStore Pay SDK")]
    [Tooltip("ID приложения в консоли RuStore (теперь настраивается в AndroidManifest.xml)")]
    public string consoleApplicationId = "your_app_id_here";
    
    [Tooltip("Схема deeplink для приложения (теперь настраивается в AndroidManifest.xml)")]
    public string deeplinkScheme = "feedthecat";
    
    [Tooltip("Включить подробное логирование")]
    public bool enableLogs = true;

    [Header("Отладка")]
    [Tooltip("Показывать сообщения об инициализации в консоли")]
    public bool showDebugMessages = true;

    private void Start()
    {
        // Инициализируем RuStore только на Android
        if (Application.platform == RuntimePlatform.Android)
        {
            InitializeRuStore();
        }
        else
        {
            if (showDebugMessages)
            {
                Debug.Log("RuStoreInitializer: RuStore поддерживается только на Android. Пропускаем инициализацию.");
            }
        }
    }

    /// <summary>
    /// Инициализирует RuStore Pay SDK
    /// </summary>
    private void InitializeRuStore()
    {
        if (showDebugMessages)
        {
            Debug.Log("RuStoreInitializer: Начинаем инициализацию RuStore Pay SDK...");
        }

        // Проверяем установлен ли RuStore
        if (!RuStorePayClient.Instance.IsRuStoreInstalled())
        {
            Debug.LogError("RuStoreInitializer: RuStore не установлен на устройстве! " +
                          "Установите RuStore из магазина приложений.");
            return;
        }

        if (showDebugMessages)
        {
            Debug.Log("RuStoreInitializer: RuStore найден на устройстве.");
        }

        // Проверяем доступность платежей
        RuStorePayClient.Instance.GetPurchaseAvailability(
            onFailure: (error) =>
            {
                Debug.LogError($"RuStoreInitializer: Ошибка проверки доступности платежей: {error.description}");
            },
            onSuccess: (result) =>
            {
                if (result.isAvailable)
                {
                    if (showDebugMessages)
                    {
                        Debug.Log("RuStoreInitializer: RuStore Pay SDK успешно инициализирован!");
                        Debug.Log($"RuStoreInitializer: App ID: {consoleApplicationId}");
                        Debug.Log($"RuStoreInitializer: Deeplink: {deeplinkScheme}");
                    }
                }
                else
                {
                    Debug.LogError($"RuStoreInitializer: Платежи недоступны: {result.cause?.description}");
                }
            }
        );
    }

    /// <summary>
    /// Проверяет статус авторизации пользователя в RuStore
    /// </summary>
    public void CheckUserAuthorization()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.LogWarning("RuStoreInitializer: Проверка авторизации доступна только на Android");
            return;
        }

        RuStorePayClient.Instance.GetUserAuthorizationStatus(
            onFailure: (error) =>
            {
                Debug.LogError($"RuStoreInitializer: Ошибка проверки авторизации - {error.description}");
            },
            onSuccess: (status) =>
            {
                if (showDebugMessages)
                {
                    Debug.Log($"RuStoreInitializer: Статус авторизации пользователя: {status}");
                }
            }
        );
    }

    /// <summary>
    /// Получает информацию о версии плагина RuStore Pay SDK
    /// </summary>
    public string GetRuStorePluginVersion()
    {
        return RuStorePayClient.PluginVersion;
    }

    private void OnGUI()
    {
        // Показываем информацию об инициализации в режиме отладки
        if (showDebugMessages && Application.isEditor)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"RuStore Plugin Version: {GetRuStorePluginVersion()}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Platform: {Application.platform}");
            GUI.Label(new Rect(10, 50, 300, 20), $"App ID: {consoleApplicationId}");
        }
    }
}
