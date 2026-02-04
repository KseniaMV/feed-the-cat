using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Менеджер состояний кота
/// Централизованно управляет состояниями удовлетворенности кота на основе CatStateData
/// </summary>
public class CatStateManager : MonoBehaviour
{
    [Header("Данные состояний кота")]
    [Tooltip("Массив данных состояний кота (ScriptableObject)")]
    public CatStateData[] catStatesData;
    
    [Header("Текущее состояние")]
    [Tooltip("Текущий уровень удовлетворенности кота")]
    public CatSatisfactionLevel currentSatisfactionLevel = CatSatisfactionLevel.Hungry;
    
    [Tooltip("Последний достигнутый уровень удовлетворенности кота")]
    public CatSatisfactionLevel lastSatisfactionLevel = CatSatisfactionLevel.Hungry;
    
    // Кэшированные данные для быстрого доступа
    private Dictionary<CatSatisfactionLevel, CatStateData> stateDataCache;
    private CatStateData[] sortedStatesByLevel;
    
    // События
    public System.Action<CatSatisfactionLevel, CatStateData> OnSatisfactionLevelChanged;
    public System.Action<CatStateData> OnNewStateUnlocked;
    
    private void Awake()
    {
        InitializeStateData();
    }
    
    private void OnValidate()
    {
        // Валидация данных в редакторе
        if (catStatesData != null)
        {
            ValidateStateData();
        }
    }
    
    /// <summary>
    /// Валидирует данные состояний
    /// </summary>
    private void ValidateStateData()
    {
        if (catStatesData == null) return;
        
        var seenLevels = new HashSet<CatSatisfactionLevel>();
        var seenOrders = new HashSet<int>();
        
        foreach (var stateData in catStatesData)
        {
            if (stateData == null) continue;
            
            // Проверяем дубликаты уровней
            if (seenLevels.Contains(stateData.satisfactionLevel))
            {
                Debug.LogWarning($"CatStateManager: Дубликат уровня удовлетворенности: {stateData.satisfactionLevel}");
            }
            seenLevels.Add(stateData.satisfactionLevel);
            
            // Проверяем дубликаты порядков отображения
            if (seenOrders.Contains(stateData.displayOrder))
            {
                Debug.LogWarning($"CatStateManager: Дубликат порядка отображения: {stateData.displayOrder}");
            }
            seenOrders.Add(stateData.displayOrder);
            
            // Проверяем корректность данных
            if (stateData.requiredExperience < 0)
            {
                Debug.LogWarning($"CatStateManager: Отрицательный опыт для {stateData.stateName}: {stateData.requiredExperience}");
            }
            
            if (stateData.coinReward < 0)
            {
                Debug.LogWarning($"CatStateManager: Отрицательная награда для {stateData.stateName}: {stateData.coinReward}");
            }
        }
    }
    
    /// <summary>
    /// Инициализирует данные состояний и создает кэш для быстрого доступа
    /// </summary>
    private void InitializeStateData()
    {
        if (catStatesData == null || catStatesData.Length == 0)
        {
            Debug.LogError("CatStateManager: Данные состояний кота не настроены!");
            return;
        }
        
        // Создаем кэш для быстрого доступа по уровню удовлетворенности
        stateDataCache = new Dictionary<CatSatisfactionLevel, CatStateData>();
        foreach (var stateData in catStatesData)
        {
            if (stateData != null)
            {
                stateDataCache[stateData.satisfactionLevel] = stateData;
            }
        }
        
        // Сортируем состояния по уровню удовлетворенности для удобства
        sortedStatesByLevel = catStatesData
            .Where(data => data != null)
            .OrderBy(data => (int)data.satisfactionLevel)
            .ToArray();
        
        // Загружаем последний достигнутый уровень
        LoadLastSatisfactionLevel();
    }
    
    /// <summary>
    /// Обновляет состояние кота на основе текущего опыта
    /// </summary>
    /// <param name="currentExperience">Текущий опыт игрока</param>
    /// <returns>Новый уровень удовлетворенности</returns>
    public CatSatisfactionLevel UpdateSatisfactionLevel(int currentExperience)
    {
        CatSatisfactionLevel newLevel = GetSatisfactionLevel(currentExperience);
        
        if (newLevel != currentSatisfactionLevel)
        {
            CatSatisfactionLevel oldLevel = currentSatisfactionLevel;
            currentSatisfactionLevel = newLevel;
            
            // Обновляем последний достигнутый уровень, если достигнут новый
            if (newLevel > lastSatisfactionLevel)
            {
                lastSatisfactionLevel = newLevel;
                SaveLastSatisfactionLevel();
                
                // Уведомляем о разблокировке нового состояния
                var newStateData = GetStateData(newLevel);
                if (newStateData != null)
                {
                    OnNewStateUnlocked?.Invoke(newStateData);
                }
            }
            
            // Уведомляем об изменении состояния
            var stateData = GetStateData(newLevel);
            OnSatisfactionLevelChanged?.Invoke(newLevel, stateData);
        }
        
        return currentSatisfactionLevel;
    }
    
    /// <summary>
    /// Определяет уровень удовлетворенности на основе опыта
    /// </summary>
    /// <param name="experience">Количество опыта</param>
    /// <returns>Уровень удовлетворенности</returns>
    public CatSatisfactionLevel GetSatisfactionLevel(int experience)
    {
        if (sortedStatesByLevel == null || sortedStatesByLevel.Length == 0)
        {
            return CatSatisfactionLevel.Hungry;
        }
        
        // Ищем максимальный достижимый уровень
        for (int i = sortedStatesByLevel.Length - 1; i >= 0; i--)
        {
            if (experience >= sortedStatesByLevel[i].requiredExperience)
            {
                return sortedStatesByLevel[i].satisfactionLevel;
            }
        }
        
        return CatSatisfactionLevel.Hungry;
    }
    
    /// <summary>
    /// Получает данные состояния по уровню удовлетворенности
    /// </summary>
    /// <param name="level">Уровень удовлетворенности</param>
    /// <returns>Данные состояния или null, если не найдены</returns>
    public CatStateData GetStateData(CatSatisfactionLevel level)
    {
        if (stateDataCache != null && stateDataCache.ContainsKey(level))
        {
            return stateDataCache[level];
        }
        
        return null;
    }
    
    /// <summary>
    /// Получает награду за достижение уровня удовлетворенности
    /// </summary>
    /// <param name="level">Уровень удовлетворенности</param>
    /// <returns>Количество монет в награду</returns>
    public int GetStateReward(CatSatisfactionLevel level)
    {
        var stateData = GetStateData(level);
        return stateData?.coinReward ?? 0;
    }
    
    /// <summary>
    /// Получает название состояния
    /// </summary>
    /// <param name="level">Уровень удовлетворенности</param>
    /// <returns>Название состояния</returns>
    public string GetStateName(CatSatisfactionLevel level)
    {
        var stateData = GetStateData(level);
        return stateData?.stateName ?? "Неизвестное состояние";
    }
    
    /// <summary>
    /// Получает описание состояния
    /// </summary>
    /// <param name="level">Уровень удовлетворенности</param>
    /// <returns>Описание состояния</returns>
    public string GetStateDescription(CatSatisfactionLevel level)
    {
        var stateData = GetStateData(level);
        return stateData?.description ?? "";
    }
    
    /// <summary>
    /// Получает спрайт состояния
    /// </summary>
    /// <param name="level">Уровень удовлетворенности</param>
    /// <returns>Спрайт состояния</returns>
    public Sprite GetStateSprite(CatSatisfactionLevel level)
    {
        var stateData = GetStateData(level);
        return stateData?.catStateSprite;
    }
    
    /// <summary>
    /// Проверяет, достигнут ли уровень удовлетворенности
    /// </summary>
    /// <param name="level">Уровень для проверки</param>
    /// <returns>True, если уровень достигнут</returns>
    public bool IsStateUnlocked(CatSatisfactionLevel level)
    {
        return lastSatisfactionLevel >= level;
    }
    
    /// <summary>
    /// Получает прогресс до следующего уровня
    /// </summary>
    /// <param name="currentExperience">Текущий опыт</param>
    /// <returns>Прогресс от 0 до 1</returns>
    public float GetProgressToNextLevel(int currentExperience)
    {
        var currentStateData = GetStateData(currentSatisfactionLevel);
        if (currentStateData == null) return 0f;
        
        // Находим следующий уровень
        var nextStateData = GetNextStateData(currentSatisfactionLevel);
        if (nextStateData == null) return 1f; // Максимальный уровень достигнут
        
        int currentRequired = currentStateData.requiredExperience;
        int nextRequired = nextStateData.requiredExperience;
        int progress = currentExperience - currentRequired;
        int totalNeeded = nextRequired - currentRequired;
        
        return Mathf.Clamp01((float)progress / totalNeeded);
    }
    
    /// <summary>
    /// Получает данные следующего состояния
    /// </summary>
    /// <param name="currentLevel">Текущий уровень</param>
    /// <returns>Данные следующего состояния или null, если это максимальный уровень</returns>
    private CatStateData GetNextStateData(CatSatisfactionLevel currentLevel)
    {
        if (sortedStatesByLevel == null) return null;
        
        for (int i = 0; i < sortedStatesByLevel.Length - 1; i++)
        {
            if (sortedStatesByLevel[i].satisfactionLevel == currentLevel)
            {
                return sortedStatesByLevel[i + 1];
            }
        }
        
        return null; // Максимальный уровень достигнут
    }
    
    /// <summary>
    /// Сбрасывает состояние кота к начальному
    /// </summary>
    public void ResetToInitialState()
    {
        currentSatisfactionLevel = CatSatisfactionLevel.Hungry;
        // lastSatisfactionLevel не сбрасываем, так как это прогресс игрока
    }
    
    /// <summary>
    /// Полностью сбрасывает состояние кота, включая прогресс
    /// </summary>
    public void FullReset()
    {
        currentSatisfactionLevel = CatSatisfactionLevel.Hungry;
        lastSatisfactionLevel = CatSatisfactionLevel.Hungry;
        // Очищаем сохраненные данные
        PlayerPrefs.DeleteKey("LastSatisfactionLevel");
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Сохраняет последний достигнутый уровень в PlayerPrefs
    /// </summary>
    private void SaveLastSatisfactionLevel()
    {
        PlayerPrefs.SetInt("LastSatisfactionLevel", (int)lastSatisfactionLevel);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Загружает последний достигнутый уровень из PlayerPrefs
    /// </summary>
    private void LoadLastSatisfactionLevel()
    {
        int savedLevel = PlayerPrefs.GetInt("LastSatisfactionLevel", 0);
        lastSatisfactionLevel = (CatSatisfactionLevel)savedLevel;
    }
    
    /// <summary>
    /// Получает все данные состояний, отсортированные по порядку отображения
    /// </summary>
    /// <returns>Массив данных состояний</returns>
    public CatStateData[] GetAllStatesSortedByDisplayOrder()
    {
        if (catStatesData == null) return new CatStateData[0];
        
        return catStatesData
            .Where(data => data != null)
            .OrderBy(data => data.displayOrder)
            .ToArray();
    }
    
    /// <summary>
    /// Получает последний разблокированный уровень удовлетворенности (рекорд)
    /// </summary>
    /// <returns>Последний разблокированный уровень</returns>
    public CatSatisfactionLevel GetLastUnlockedLevel()
    {
        return lastSatisfactionLevel;
    }
    
    /// <summary>
    /// Получает название последнего разблокированного уровня (рекорд)
    /// </summary>
    /// <returns>Название последнего разблокированного уровня</returns>
    public string GetLastUnlockedLevelName()
    {
        var stateData = GetStateData(lastSatisfactionLevel);
        return stateData?.stateName ?? "Голодный котик";
    }
    
    /// <summary>
    /// Очищает события при уничтожении объекта
    /// </summary>
    private void OnDestroy()
    {
        OnSatisfactionLevelChanged = null;
        OnNewStateUnlocked = null;
    }
}
