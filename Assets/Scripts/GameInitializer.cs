using UnityEngine;

/// <summary>
/// Инициализатор игровых данных
/// Автоматически инициализирует GameDataManager при запуске игры
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Настройки инициализации")]
    [Tooltip("Автоматически инициализировать при старте")]
    public bool autoInitialize = true;
    
    [Tooltip("Выводить подробные логи инициализации")]
    public bool verboseLogging = true;

    private void Awake()
    {
        // Инициализируем менеджер жизненного цикла приложения
        InitializeApplicationLifecycleManager();
        
        // Убеждаемся, что инициализация происходит только один раз
        if (autoInitialize && !GameDataManager.IsInitialized)
        {
            InitializeGameData();
        }
    }

    /// <summary>
    /// Инициализирует менеджер жизненного цикла приложения
    /// </summary>
    private void InitializeApplicationLifecycleManager()
    {
        // Проверяем, существует ли уже менеджер
        // Используем двойную проверку для безопасности (thread-safe pattern)
        if (ApplicationLifecycleManager.Instance == null)
        {
            // Проверяем, нет ли уже менеджера в сцене (на случай если он был создан вручную)
            ApplicationLifecycleManager existingManager = FindFirstObjectByType<ApplicationLifecycleManager>();
            
            if (existingManager == null)
            {
                GameObject lifecycleManagerGO = new GameObject("ApplicationLifecycleManager");
                lifecycleManagerGO.AddComponent<ApplicationLifecycleManager>();
            }
        }
    }

    /// <summary>
    /// Инициализирует игровые данные
    /// </summary>
    public void InitializeGameData()
    {
        if (verboseLogging)
        {
        }

        try
        {
            // Инициализируем центральную систему данных
            GameDataManager.Initialize();
            
            if (verboseLogging)
            {
                LogCurrentData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameInitializer: Ошибка при инициализации: {e.Message}");
        }
    }

    /// <summary>
    /// Выводит текущие данные в лог (для отладки)
    /// </summary>
    private void LogCurrentData()
    {
    }

    /// <summary>
    /// Сбрасывает все игровые данные (для тестирования)
    /// </summary>
    [ContextMenu("Reset All Game Data")]
    public void ResetAllGameData()
    {
        if (Application.isEditor)
        {
            GameDataManager.ResetAllData();
        }
        else
        {
            Debug.LogWarning("GameInitializer: Сброс данных доступен только в редакторе");
        }
    }

    /// <summary>
    /// Добавляет тестовые данные (для тестирования)
    /// </summary>
    [ContextMenu("Add Test Data")]
    public void AddTestData()
    {
        if (Application.isEditor)
        {
            GameDataManager.AddCoins(500);
            GameDataManager.AddExperience(500);
            GameDataManager.AddBoosters("bomb", 5);
            GameDataManager.AddBoosters("paw", 5);
            GameDataManager.AddBoosters("cat", 2);
            
        }
        else
        {
            Debug.LogWarning("GameInitializer: Тестовые данные доступны только в редакторе");
        }
    }
}
