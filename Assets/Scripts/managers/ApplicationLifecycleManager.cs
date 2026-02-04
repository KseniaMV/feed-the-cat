using UnityEngine;

/// <summary>
/// Менеджер жизненного цикла приложения
/// Обрабатывает события паузы/возобновления и возвращает игрока в стартовое меню
/// </summary>
public class ApplicationLifecycleManager : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Включить логирование событий жизненного цикла")]
    public bool enableLogging = true;
    
    [Tooltip("Задержка перед возвратом в меню (в секундах)")]
    public float returnToMenuDelay = 0.2f;

    private static ApplicationLifecycleManager instance;
    public static ApplicationLifecycleManager Instance => instance;

    private bool wasPaused = false;
    private bool isReturningToMenu = false;

    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Обрабатывает паузу приложения (Android/iOS)
    /// </summary>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (enableLogging)
        {
            Debug.Log($"ApplicationLifecycleManager: OnApplicationPause - {pauseStatus}");
        }
        
        wasPaused = pauseStatus;
        
        if (!pauseStatus)
        {
            // Приложение восстановлено - возвращаем в меню
            OnApplicationResumed();
        }
    }

    /// <summary>
    /// Обрабатывает потерю/возврат фокуса приложения (Desktop/Editor)
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (enableLogging)
        {
            Debug.Log($"ApplicationLifecycleManager: OnApplicationFocus - {hasFocus}");
        }
        
        // На Android/iOS OnApplicationPause вызывается первым, поэтому проверяем wasPaused
        // На Desktop/Editor OnApplicationFocus может быть единственным событием
        if (hasFocus)
        {
            if (wasPaused)
            {
                // Приложение восстановлено после паузы
                OnApplicationResumed();
                wasPaused = false;
            }
        }
        else
        {
            // Приложение потеряло фокус (например, Alt+Tab на Desktop)
            wasPaused = true;
        }
    }

    /// <summary>
    /// Вызывается при восстановлении приложения
    /// </summary>
    private void OnApplicationResumed()
    {
        // Предотвращаем множественные вызовы
        if (isReturningToMenu)
        {
            if (enableLogging)
            {
                Debug.Log("ApplicationLifecycleManager: Возврат в меню уже выполняется, пропускаем повторный вызов");
            }
            return;
        }

        if (enableLogging)
        {
            Debug.Log("ApplicationLifecycleManager: Приложение восстановлено, возвращаем в стартовое меню...");
        }
        
        // Отменяем предыдущий вызов, если он есть
        CancelInvoke(nameof(ReturnToMainMenu));
        
        // Устанавливаем флаг, что мы возвращаемся в меню
        isReturningToMenu = true;
        
        // Небольшая задержка для стабильности, затем возвращаем в меню
        Invoke(nameof(ReturnToMainMenu), returnToMenuDelay);
    }

    /// <summary>
    /// Возвращает игрока в стартовое меню
    /// </summary>
    private void ReturnToMainMenu()
    {
        // Проверяем, что объект не уничтожен
        if (this == null || gameObject == null)
        {
            return;
        }

        // Сбрасываем флаг
        isReturningToMenu = false;

        // Находим UIManager
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        
        if (uiManager != null)
        {
            // Проверяем, не находимся ли мы уже в меню
            // Это предотвращает лишние вызовы и возможные проблемы
            if (uiManager.mainMenuPanel != null && uiManager.mainMenuPanel.activeInHierarchy)
            {
                if (enableLogging)
                {
                    Debug.Log("ApplicationLifecycleManager: Игрок уже в главном меню, повторный возврат не требуется");
                }
                return;
            }

            // Вызываем метод возврата в меню (тот же, что используется кнопкой Home)
            uiManager.OnHomeButtonClicked();
            
            if (enableLogging)
            {
                Debug.Log("ApplicationLifecycleManager: Игрок возвращен в стартовое меню");
            }
        }
        else
        {
            Debug.LogWarning("ApplicationLifecycleManager: UIManager не найден, не удалось вернуть в меню");
        }
    }

    private void OnDestroy()
    {
        // Отменяем все запланированные вызовы
        CancelInvoke();
        
        if (instance == this)
        {
            instance = null;
        }
    }
}

