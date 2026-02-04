using UnityEngine;

/// <summary>
/// Быстрый сброс игровых данных через консоль
/// Используется для тестирования - можно вызывать из консоли Unity
/// </summary>
public class QuickReset : MonoBehaviour
{
    [Header("Горячие клавиши (только в редакторе)")]
    [Tooltip("Клавиша для быстрого сброса")]
    public KeyCode resetKey = KeyCode.R;
    
    [Tooltip("Клавиша для сброса только прогресса")]
    public KeyCode resetProgressKey = KeyCode.T;
    
    [Tooltip("Клавиша для сброса только монет")]
    public KeyCode resetCoinsKey = KeyCode.C;
    
    [Tooltip("Клавиша для сброса только опыта")]
    public KeyCode resetExperienceKey = KeyCode.E;
    
    private void Update()
    {
        // Работает только в редакторе
        if (!Application.isEditor) return;
        
        // Проверяем нажатие клавиш
        if (Input.GetKeyDown(resetKey))
        {
            ResetAllData();
        }
        
        if (Input.GetKeyDown(resetProgressKey))
        {
            ResetProgressOnly();
        }
        
        if (Input.GetKeyDown(resetCoinsKey))
        {
            ResetCoins();
        }
        
        if (Input.GetKeyDown(resetExperienceKey))
        {
            ResetExperience();
        }
    }
    
    /// <summary>
    /// Сбрасывает все данные
    /// </summary>
    [ContextMenu("Reset All Data")]
    public void ResetAllData()
    {
        GameDataManager.ResetAllData();
    }
    
    /// <summary>
    /// Сбрасывает только прогресс
    /// </summary>
    [ContextMenu("Reset Progress Only")]
    public void ResetProgressOnly()
    {
        
        // Сбрасываем монеты и опыт
        GameDataManager.SetCoins(0);
        GameDataManager.SetExperience(0);
        
        // Очищаем открытые продукты, оставляем только сосиску
        var discoveredFoods = GameDataManager.DiscoveredFoods;
        discoveredFoods.Clear();
        discoveredFoods.Add(FoodType.Sausage);
        
        // Сбрасываем цель (сосиска не может быть целью, так как это дефолтный продукт)
        GameDataManager.SetCurrentGoal(FoodType.Eggs);
        
        // Сбрасываем бустеры (устанавливаем количество в 0)
        // Получаем текущее количество и вычитаем его
        int bombCount = GameDataManager.GetBoosterCount("bomb");
        int pawCount = GameDataManager.GetBoosterCount("paw");
        int catCount = GameDataManager.GetBoosterCount("cat");
        
        if (bombCount > 0) GameDataManager.AddBoosters("bomb", -bombCount);
        if (pawCount > 0) GameDataManager.AddBoosters("paw", -pawCount);
        if (catCount > 0) GameDataManager.AddBoosters("cat", -catCount);
        
        // Сбрасываем состояние кота (включая коллекцию)
        CatStateManager catStateManager = FindFirstObjectByType<CatStateManager>();
        if (catStateManager != null)
        {
            catStateManager.FullReset();
        }
        else
        {
            // Если CatStateManager не найден, все равно очищаем PlayerPrefs
            PlayerPrefs.DeleteKey("LastSatisfactionLevel");
            PlayerPrefs.Save();
        }
        
        // Сбрасываем lastSatisfactionLevel в CatStateManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.catStateManager != null)
        {
            gameManager.catStateManager.FullReset();
        }
        
        // Принудительно обновляем UI после сброса
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.RefreshExperienceRecord();
        }
        
    }
    
    /// <summary>
    /// Сбрасывает только монеты
    /// </summary>
    [ContextMenu("Reset Coins")]
    public void ResetCoins()
    {
        GameDataManager.SetCoins(0);
    }
    
    /// <summary>
    /// Сбрасывает только опыт
    /// </summary>
    [ContextMenu("Reset Experience")]
    public void ResetExperience()
    {
        GameDataManager.SetExperience(0);
    }
    
    /// <summary>
    /// Добавляет монеты для тестирования
    /// </summary>
    [ContextMenu("Add Test Coins")]
    public void AddTestCoins()
    {
        GameDataManager.AddCoins(1000);
    }
    
    /// <summary>
    /// Добавляет опыт для тестирования
    /// </summary>
    [ContextMenu("Add Test Experience")]
    public void AddTestExperience()
    {
        GameDataManager.AddExperience(5000);
    }
    
    /// <summary>
    /// Открывает все продукты для тестирования
    /// </summary>
    [ContextMenu("Unlock All Foods")]
    public void UnlockAllFoods()
    {
        
        // Добавляем все продукты в коллекцию
        GameDataManager.AddDiscoveredFood(FoodType.Eggs);
        GameDataManager.AddDiscoveredFood(FoodType.Sandwich);
        GameDataManager.AddDiscoveredFood(FoodType.Meatball);
        GameDataManager.AddDiscoveredFood(FoodType.Soup);
        GameDataManager.AddDiscoveredFood(FoodType.Chicken);
        GameDataManager.AddDiscoveredFood(FoodType.Salmon);
        GameDataManager.AddDiscoveredFood(FoodType.Shrimp);
        GameDataManager.AddDiscoveredFood(FoodType.Caviar);
        GameDataManager.AddDiscoveredFood(FoodType.Oyster);
        GameDataManager.AddDiscoveredFood(FoodType.Lobster);
        
    }
    
    /// <summary>
    /// Показывает текущее состояние данных
    /// </summary>
    [ContextMenu("Show Current Data")]
    public void ShowCurrentData()
    {
    }
}
