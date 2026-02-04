using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

/// <summary>
/// Скрипт для настройки YandexMobileAds SDK
/// Этот скрипт нужно добавить на сцену для автоматической настройки рекламы
/// YandexMobileAds SDK не требует явной инициализации, но нужна настройка параметров
/// </summary>
public class YandexAdsInitializer : MonoBehaviour
{
    [Header("Настройки YandexMobileAds")]
    [Tooltip("Включить логирование")]
    public bool enableLogs = true;
    
    [Tooltip("Включить отладочную информацию")]
    public bool enableDebugInfo = true;

    private void Awake()
    {
        // Инициализируем YandexMobileAds только на мобильных платформах
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            InitializeYandexAds();
        }
        else
        {
            if (enableLogs)
                Debug.Log("YandexAdsInitializer: YandexMobileAds поддерживается только на мобильных платформах. Пропускаем инициализацию.");
        }
    }

    /// <summary>
    /// Инициализирует YandexMobileAds SDK
    /// </summary>
    private void InitializeYandexAds()
    {
        if (enableLogs)
            Debug.Log("YandexAdsInitializer: Настраиваем YandexMobileAds SDK...");

        try
        {
            // YandexMobileAds SDK не требует явной инициализации
            // Настраиваем базовые параметры
            MobileAds.SetUserConsent(true); // Разрешаем сбор данных для персонализации рекламы
            MobileAds.SetLocationConsent(true); // Разрешаем сбор геолокации для таргетинга
            MobileAds.SetAgeRestrictedUser(false); // Пользователь не является несовершеннолетним
            
            if (enableLogs)
                Debug.Log("YandexAdsInitializer: YandexMobileAds SDK настроен!");
                
            OnInitializationCompleted("SDK настроен успешно");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"YandexAdsInitializer: Ошибка настройки YandexMobileAds SDK: {e.Message}");
            OnInitializationCompleted($"Ошибка: {e.Message}");
        }
    }

    /// <summary>
    /// Обработчик завершения инициализации
    /// </summary>
    private void OnInitializationCompleted(object status)
    {
        if (enableLogs)
        {
            Debug.Log("YandexAdsInitializer: Инициализация YandexMobileAds завершена");
            Debug.Log($"YandexAdsInitializer: Статус инициализации: {status}");
        }

        if (enableDebugInfo)
        {
            // Выводим дополнительную отладочную информацию
            Debug.Log($"YandexAdsInitializer: Платформа: {Application.platform}");
            Debug.Log($"YandexAdsInitializer: Версия Unity: {Application.unityVersion}");
        }
    }

    /// <summary>
    /// Получает информацию о версии YandexMobileAds SDK
    /// </summary>
    public string GetSDKVersion()
    {
        try
        {
            // Попытка получить версию SDK (если доступно)
            return "YandexMobileAds Unity Plugin";
        }
        catch
        {
            return "Версия недоступна";
        }
    }

    private void OnGUI()
    {
        if (enableDebugInfo && Application.isEditor)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"YandexMobileAds SDK: {GetSDKVersion()}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Платформа: {Application.platform}");
        }
    }
}
