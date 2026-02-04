using UnityEngine;
using System;

#if UNITY_ANDROID && !UNITY_EDITOR
using RuStore;
using RuStore.AppUpdate;
#endif

/// <summary>
/// Менеджер для проверки и управления обновлениями игры через RuStore
/// Автоматически проверяет наличие обновлений при запуске игры
/// </summary>
public class GameUpdateManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameUpdatePanel;  // Панель с кнопкой "Обновить"
    [SerializeField] private GameObject mainGameUI;        // Основной интерфейс игры (MainMenuPanel)
    
    [Header("Update Settings")]
    [SerializeField] private bool checkOnStart = true;     // Проверять при запуске
    [SerializeField] private float checkDelay = 0.5f;      // Задержка перед проверкой (секунды)
    
    [Header("Update Mode")]
    [SerializeField] private bool forceUpdate = true;      // Принудительное обновление (IMMEDIATE)
    
    [Header("Retry Settings")]
    [SerializeField] private int maxRetryAttempts = 2;     // Максимальное количество попыток
    [SerializeField] private float retryDelay = 3f;        // Задержка между попытками (секунды)
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    private int currentRetryAttempt = 0;
    private bool isCheckingForUpdates = false;
    
    private RuStoreAppUpdateManager updateClient;
    private AppUpdateInfo currentUpdateInfo;
    #endif
    
    private void Awake()
    {
        // Инициализация RuStore AppUpdate Client
        InitializeUpdateClient();
    }
    
    private void Start()
    {
        if (checkOnStart)
        {
            // Добавляем небольшую задержку для корректной инициализации
            Invoke(nameof(CheckForUpdates), checkDelay);
        }
        else
        {
            // Если проверка отключена, сразу показываем основной UI
            ShowMainGameUI();
        }
    }
    
    /// <summary>
    /// Инициализация клиента обновлений
    /// </summary>
    private void InitializeUpdateClient()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            updateClient = RuStoreAppUpdateManager.Instance;
            if (updateClient.Init())
            {
                Debug.Log("[GameUpdateManager] RuStore AppUpdate Client initialized successfully");
            }
            else
            {
                Debug.LogError("[GameUpdateManager] Failed to initialize RuStore AppUpdate Client");
                updateClient = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameUpdateManager] Failed to initialize update client: {e.Message}");
        }
        #else
        Debug.LogWarning("[GameUpdateManager] RuStore SDK only works on Android devices");
        #endif
    }
    
    /// <summary>
    /// Проверка доступности обновлений
    /// Можно вызвать вручную из кода или UI
    /// </summary>
    public void CheckForUpdates()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        
        // Защита от множественных одновременных вызовов
        if (isCheckingForUpdates)
        {
            Debug.LogWarning("[GameUpdateManager] Update check already in progress");
            return;
        }
        
        // Проверка наличия интернет-соединения
        if (!IsInternetAvailable())
        {
            Debug.LogWarning("[GameUpdateManager] No internet connection available");
            ShowMainGameUI();
            return;
        }
        
        if (updateClient == null)
        {
            Debug.LogWarning("[GameUpdateManager] Update client is not initialized");
            ShowMainGameUI();
            return;
        }
        
        isCheckingForUpdates = true;
        Debug.Log("[GameUpdateManager] Checking for updates...");
        
        // Получаем информацию о доступных обновлениях
        updateClient.GetAppUpdateInfo(
            onFailure: OnUpdateCheckFailed,
            onSuccess: OnUpdateInfoReceived
        );
        #else
        // В Unity Editor или не на Android - пропускаем проверку
        Debug.Log("[GameUpdateManager] Skipping update check (not on Android device)");
        ShowMainGameUI();
        #endif
    }
    
    /// <summary>
    /// Проверка доступности интернета
    /// </summary>
    private bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Обработка полученной информации об обновлении
    /// </summary>
    private void OnUpdateInfoReceived(AppUpdateInfo updateInfo)
    {
        isCheckingForUpdates = false;
        currentRetryAttempt = 0; // Сбрасываем счетчик попыток при успехе
        
        currentUpdateInfo = updateInfo;
        
        Debug.Log($"[GameUpdateManager] Update info received:");
        Debug.Log($"  - Available version code: {updateInfo.availableVersionCode}");
        Debug.Log($"  - Install status: {updateInfo.installStatus}");
        Debug.Log($"  - Update availability: {updateInfo.updateAvailability}");
        
        // Проверяем, доступно ли обновление
        bool isUpdateAvailable = updateInfo.updateAvailability == AppUpdateInfo.UpdateAvailability.UPDATE_AVAILABLE;
        
        if (isUpdateAvailable)
        {
            Debug.Log("[GameUpdateManager] Update is available!");
            ShowUpdatePanel();
        }
        else
        {
            Debug.Log("[GameUpdateManager] No updates available. Showing main game UI.");
            ShowMainGameUI();
        }
    }
    
    /// <summary>
    /// Обработка ошибки проверки обновлений
    /// </summary>
    private void OnUpdateCheckFailed(RuStoreError error)
    {
        isCheckingForUpdates = false;
        
        Debug.LogError($"[GameUpdateManager] Failed to check for updates (attempt {currentRetryAttempt + 1}/{maxRetryAttempts + 1}):");
        Debug.LogError($"  - Name: {error.name}");
        Debug.LogError($"  - Description: {error.description}");
        
        // Попытка повторной проверки
        if (currentRetryAttempt < maxRetryAttempts)
        {
            currentRetryAttempt++;
            Debug.Log($"[GameUpdateManager] Retrying in {retryDelay} seconds...");
            Invoke(nameof(RetryCheckForUpdates), retryDelay);
        }
        else
        {
            Debug.LogWarning("[GameUpdateManager] Max retry attempts reached. Showing main game UI.");
            currentRetryAttempt = 0; // Сбрасываем для следующей проверки
            ShowMainGameUI();
        }
    }
    
    /// <summary>
    /// Повторная попытка проверки обновлений
    /// </summary>
    private void RetryCheckForUpdates()
    {
        Debug.Log("[GameUpdateManager] Retrying update check...");
        CheckForUpdates();
    }
    #endif
    
    /// <summary>
    /// Запуск процесса обновления (вызывается кнопкой "Обновить")
    /// </summary>
    public void StartUpdate()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (updateClient == null || currentUpdateInfo == null)
        {
            Debug.LogError("[GameUpdateManager] Cannot start update: client or update info is null");
            OpenAppInRuStore(); // Запасной вариант
            return;
        }
        
        Debug.Log("[GameUpdateManager] Starting update flow...");
        
        // Определяем тип обновления
        UpdateType updateType = forceUpdate ? UpdateType.IMMEDIATE : UpdateType.FLEXIBLE;
        Debug.Log($"[GameUpdateManager] Update type: {updateType}");
        
        // Запускаем процесс обновления
        // IMMEDIATE - полноэкранный диалог, после установки приложение перезапустится
        // FLEXIBLE - обновление в фоне, игрок может продолжать играть
        updateClient.StartUpdateFlow(
            updateType,
            onFailure: OnUpdateFailed,
            onSuccess: OnUpdateStarted
        );
        #else
        Debug.Log("[GameUpdateManager] Update simulation (Editor mode)");
        OpenAppInRuStore();
        #endif
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Обновление успешно запущено
    /// </summary>
    private void OnUpdateStarted(UpdateFlowResult result)
    {
        Debug.Log($"[GameUpdateManager] Update flow started successfully. Result: {result}");
        
        if (result == UpdateFlowResult.RESULT_OK)
        {
            if (forceUpdate)
            {
                // При IMMEDIATE режиме приложение будет автоматически перезапущено
                Debug.Log("[GameUpdateManager] App will restart after update installation");
            }
            else
            {
                // При FLEXIBLE режиме можно продолжить игру
                Debug.Log("[GameUpdateManager] Update downloading in background");
                ShowMainGameUI();
                
                // Можно запустить периодическую проверку статуса
                InvokeRepeating(nameof(CheckFlexibleUpdateStatus), 5f, 5f);
            }
        }
        else if (result == UpdateFlowResult.RESULT_CANCELED)
        {
            Debug.LogWarning("[GameUpdateManager] Update flow was canceled");
            ShowMainGameUI();
        }
        else if (result == UpdateFlowResult.RESULT_ACTIVITY_NOT_FOUND)
        {
            Debug.LogError("[GameUpdateManager] RuStore not installed or version too old");
            OpenAppInRuStore();
        }
    }
    
    /// <summary>
    /// Ошибка при запуске обновления
    /// </summary>
    private void OnUpdateFailed(RuStoreError error)
    {
        Debug.LogError($"[GameUpdateManager] Update failed:");
        Debug.LogError($"  - Name: {error.name}");
        Debug.LogError($"  - Description: {error.description}");
        
        // Запасной вариант - открываем страницу приложения в RuStore
        OpenAppInRuStore();
    }
    
    /// <summary>
    /// Проверка статуса гибкого обновления (для FLEXIBLE режима)
    /// </summary>
    private void CheckFlexibleUpdateStatus()
    {
        if (updateClient == null || currentUpdateInfo == null) 
        {
            CancelInvoke(nameof(CheckFlexibleUpdateStatus));
            return;
        }
        
        updateClient.GetAppUpdateInfo(
            onFailure: (error) => {
                Debug.LogWarning($"[GameUpdateManager] Failed to check flexible update status: {error.description}");
            },
            onSuccess: (info) => {
                if (info.installStatus == AppUpdateInfo.InstallStatus.DOWNLOADED)
                {
                    Debug.Log("[GameUpdateManager] Update downloaded, ready to install");
                    CancelInvoke(nameof(CheckFlexibleUpdateStatus));
                    
                    // Можно показать уведомление и установить обновление
                    CompleteFlexibleUpdate();
                }
                else
                {
                    Debug.Log($"[GameUpdateManager] Flexible update status: {info.installStatus}");
                }
            }
        );
    }
    
    /// <summary>
    /// Завершение гибкого обновления (установка)
    /// </summary>
    private void CompleteFlexibleUpdate()
    {
        if (updateClient == null) return;
        
        Debug.Log("[GameUpdateManager] Completing flexible update...");
        updateClient.CompleteUpdate(
            UpdateType.FLEXIBLE,
            onFailure: (error) => {
                Debug.LogError($"[GameUpdateManager] Failed to complete update: {error.description}");
            }
        );
        // Приложение будет перезапущено
    }
    #endif
    
    /// <summary>
    /// Открытие страницы приложения в RuStore (запасной вариант)
    /// </summary>
    private void OpenAppInRuStore()
    {
        try
        {
            string packageName = Application.identifier;
            string url = $"rustore://apps/{packageName}";
            
            Debug.Log($"[GameUpdateManager] Opening RuStore app page: {url}");
            Application.OpenURL(url);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameUpdateManager] Failed to open RuStore: {e.Message}");
        }
    }
    
    /// <summary>
    /// Показать панель обновления и скрыть основной интерфейс
    /// </summary>
    private void ShowUpdatePanel()
    {
        if (gameUpdatePanel != null)
        {
            gameUpdatePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameUpdateManager] Game Update Panel reference is not set!");
        }
        
        if (mainGameUI != null)
        {
            mainGameUI.SetActive(false);
        }
        
        Debug.Log("[GameUpdateManager] Update panel shown");
    }
    
    /// <summary>
    /// Показать основной интерфейс игры и скрыть панель обновления
    /// </summary>
    private void ShowMainGameUI()
    {
        if (gameUpdatePanel != null)
        {
            gameUpdatePanel.SetActive(false);
        }
        
        if (mainGameUI != null)
        {
            mainGameUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[GameUpdateManager] Main Game UI reference is not set!");
        }
        
        Debug.Log("[GameUpdateManager] Main game UI shown");
    }
    
    /// <summary>
    /// Принудительная проверка обновлений (для вызова из UI настроек)
    /// </summary>
    public void ManualCheckForUpdates()
    {
        Debug.Log("[GameUpdateManager] Manual update check requested");
        CheckForUpdates();
    }
    
    private void OnDestroy()
    {
        // Останавливаем периодические проверки
        CancelInvoke();
    }
}

