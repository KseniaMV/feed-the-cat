using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Централизованный менеджер игровых данных
/// Static класс для глобального доступа ко всем игровым данным
/// </summary>
public static class GameDataManager
{
    #region Events - События для уведомления об изменениях
    
    // События для монет
    public static event Action<int> OnCoinsChanged;
    public static event Action<int, int> OnCoinsAdded; // (amount, newTotal)
    public static event Action<int, int> OnCoinsSpent; // (amount, newTotal)
    
    // События для опыта
    public static event Action<int> OnExperienceChanged;
    
    // События для бустеров
    public static event Action<string, int> OnBoosterCountChanged; // (boosterType, newCount)
    public static event Action<string, int> OnBoostersAdded; // (boosterType, amount)
    
    // События для коллекции
    public static event Action<FoodType> OnNewFoodUnlocked;
    public static event Action<List<FoodType>> OnDiscoveredFoodsChanged;
    
    #endregion

    #region Coins - Управление монетами
    
    private static int _currentCoins = 0;
    private const string COINS_SAVE_KEY = "PlayerCoins";
    
    /// <summary>
    /// Текущее количество монет (только для чтения)
    /// </summary>
    public static int CurrentCoins => _currentCoins;
    
    /// <summary>
    /// Добавляет монеты игроку
    /// </summary>
    public static int AddCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка добавить {amount} монет (должно быть больше 0)");
            return _currentCoins;
        }

        int oldCoins = _currentCoins;
        _currentCoins += amount;
        
        // Сохраняем изменения
        SaveCoins();
        
        // Уведомляем о изменениях
        OnCoinsChanged?.Invoke(_currentCoins);
        OnCoinsAdded?.Invoke(amount, _currentCoins);
        
        return _currentCoins;
    }
    
    /// <summary>
    /// Тратит монеты игрока
    /// </summary>
    public static bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка потратить {amount} монет (должно быть больше 0)");
            return false;
        }

        if (!CanAffordCoins(amount))
        {
            Debug.LogWarning($"GameDataManager: Недостаточно монет для траты {amount}. Доступно: {_currentCoins}");
            return false;
        }

        int oldCoins = _currentCoins;
        _currentCoins -= amount;
        
        // Сохраняем изменения
        SaveCoins();
        
        // Уведомляем о изменениях
        OnCoinsChanged?.Invoke(_currentCoins);
        OnCoinsSpent?.Invoke(amount, _currentCoins);
        
        return true;
    }
    
    /// <summary>
    /// Проверяет, может ли игрок позволить себе покупку
    /// </summary>
    public static bool CanAffordCoins(int amount)
    {
        return _currentCoins >= amount;
    }
    
    /// <summary>
    /// Устанавливает точное количество монет
    /// </summary>
    public static void SetCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка установить отрицательное количество монет: {amount}");
            amount = 0;
        }

        int oldCoins = _currentCoins;
        _currentCoins = amount;
        
        // Сохраняем изменения
        SaveCoins();
        
        // Уведомляем о изменениях
        OnCoinsChanged?.Invoke(_currentCoins);
        
    }
    
    /// <summary>
    /// Сохраняет монеты
    /// </summary>
    private static void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_SAVE_KEY, _currentCoins);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Загружает монеты
    /// </summary>
    private static void LoadCoins()
    {
        int savedCoins = PlayerPrefs.GetInt(COINS_SAVE_KEY, 0);
        _currentCoins = savedCoins;
    }
    
    #endregion

    #region Experience - Управление опытом
    
    private static int _currentExperience = 0;
    private static int _gameSessionExperience = 0; // Опыт текущей игровой сессии (сбрасывается)
    private const string EXPERIENCE_SAVE_KEY = "PlayerExperience";
    
    /// <summary>
    /// Текущий опыт игрока (сохраняется между сессиями)
    /// </summary>
    public static int CurrentExperience => _currentExperience;
    
    /// <summary>
    /// Опыт текущей игровой сессии (сбрасывается при начале новой игры)
    /// </summary>
    public static int GameSessionExperience => _gameSessionExperience;
    
    /// <summary>
    /// Добавляет опыт игроку (сохраняется между сессиями)
    /// </summary>
    public static int AddExperience(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка добавить {amount} опыта (должно быть больше 0)");
            return _currentExperience;
        }

        int oldExperience = _currentExperience;
        _currentExperience += amount;
        
        // Сохраняем изменения
        SaveExperience();
        
        // Уведомляем о изменениях
        OnExperienceChanged?.Invoke(_currentExperience);
        
        return _currentExperience;
    }
    
    /// <summary>
    /// Добавляет опыт к текущей игровой сессии (сбрасывается при начале новой игры)
    /// </summary>
    public static int AddGameSessionExperience(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка добавить {amount} игрового опыта (должно быть больше 0)");
            return _gameSessionExperience;
        }

        int oldExperience = _gameSessionExperience;
        _gameSessionExperience += amount;
        
        // Уведомляем о изменениях
        OnExperienceChanged?.Invoke(_gameSessionExperience);
        
        return _gameSessionExperience;
    }
    
    /// <summary>
    /// Сбрасывает опыт игровой сессии (вызывается при начале новой игры)
    /// </summary>
    public static void ResetGameSessionExperience()
    {
        _gameSessionExperience = 0;
    }
    
    /// <summary>
    /// Устанавливает точное количество опыта
    /// </summary>
    public static void SetExperience(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка установить отрицательный опыт: {amount}");
            amount = 0;
        }

        int oldExperience = _currentExperience;
        _currentExperience = amount;
        
        // Сохраняем изменения
        SaveExperience();
        
        // Уведомляем о изменениях
        OnExperienceChanged?.Invoke(_currentExperience);
        
    }
    
    /// <summary>
    /// Сохраняет опыт
    /// </summary>
    private static void SaveExperience()
    {
        PlayerPrefs.SetInt(EXPERIENCE_SAVE_KEY, _currentExperience);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Загружает опыт
    /// </summary>
    private static void LoadExperience()
    {
        _currentExperience = PlayerPrefs.GetInt(EXPERIENCE_SAVE_KEY, 0);
    }
    
    #endregion

    #region Boosters - Управление бустерами
    
    private const string BOMB_BOOSTER_KEY = "booster_bomb_count";
    private const string PAW_BOOSTER_KEY = "booster_paw_count";
    private const string CAT_BOOSTER_KEY = "booster_cat_count";
    
    /// <summary>
    /// Получает текущее количество бустера
    /// </summary>
    public static int GetBoosterCount(string boosterType)
    {
        string key = GetBoosterKey(boosterType);
        return PlayerPrefs.GetInt(key, 0);
    }
    
    /// <summary>
    /// Добавляет бустеры игроку
    /// </summary>
    public static int AddBoosters(string boosterType, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка добавить {amount} бустеров {boosterType} (должно быть больше 0)");
            return GetBoosterCount(boosterType);
        }

        int oldCount = GetBoosterCount(boosterType);
        int newCount = oldCount + amount;
        
        // Сохраняем новое количество
        string key = GetBoosterKey(boosterType);
        PlayerPrefs.SetInt(key, newCount);
        PlayerPrefs.Save();
        
        // Уведомляем о изменениях
        OnBoosterCountChanged?.Invoke(boosterType, newCount);
        OnBoostersAdded?.Invoke(boosterType, amount);
        
        return newCount;
    }
    
    /// <summary>
    /// Тратит бустеры игрока
    /// </summary>
    public static bool SpendBoosters(string boosterType, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"GameDataManager: Попытка потратить {amount} бустеров {boosterType} (должно быть больше 0)");
            return false;
        }

        int currentCount = GetBoosterCount(boosterType);
        if (currentCount < amount)
        {
            Debug.LogWarning($"GameDataManager: Недостаточно бустеров {boosterType} для траты {amount}. Доступно: {currentCount}");
            return false;
        }

        int newCount = currentCount - amount;
        
        // Сохраняем новое количество
        string key = GetBoosterKey(boosterType);
        PlayerPrefs.SetInt(key, newCount);
        PlayerPrefs.Save();
        
        // Уведомляем о изменениях
        OnBoosterCountChanged?.Invoke(boosterType, newCount);
        
        return true;
    }
    
    /// <summary>
    /// Проверяет, есть ли достаточно бустеров
    /// </summary>
    public static bool HasEnoughBoosters(string boosterType, int amount)
    {
        return GetBoosterCount(boosterType) >= amount;
    }
    
    /// <summary>
    /// Получает ключ для сохранения бустера
    /// </summary>
    private static string GetBoosterKey(string boosterType)
    {
        switch (boosterType.ToLower())
        {
            case "bomb": return BOMB_BOOSTER_KEY;
            case "paw": return PAW_BOOSTER_KEY;
            case "cat": return CAT_BOOSTER_KEY;
            default: 
                Debug.LogWarning($"GameDataManager: Неизвестный тип бустера: {boosterType}");
                return $"booster_{boosterType}_count";
        }
    }
    
    #endregion

    #region Collection - Управление коллекцией
    
    private static List<FoodType> _discoveredFoods = new List<FoodType>();
    private const string DISCOVERED_FOODS_COUNT_KEY = "DiscoveredFoodsCount";
    private const string DISCOVERED_FOOD_KEY = "DiscoveredFood_{0}";
    
    /// <summary>
    /// Список открытых продуктов (только для чтения)
    /// </summary>
    public static List<FoodType> DiscoveredFoods => new List<FoodType>(_discoveredFoods);
    
    /// <summary>
    /// Добавляет продукт в коллекцию
    /// </summary>
    public static bool AddDiscoveredFood(FoodType foodType)
    {
        if (_discoveredFoods.Contains(foodType))
        {
            Debug.LogWarning($"GameDataManager: Продукт {foodType} уже открыт в коллекции");
            return false;
        }

        _discoveredFoods.Add(foodType);
        
        // Сохраняем изменения
        SaveDiscoveredFoods();
        
        // Уведомляем о изменениях
        OnNewFoodUnlocked?.Invoke(foodType);
        OnDiscoveredFoodsChanged?.Invoke(new List<FoodType>(_discoveredFoods));
        
        return true;
    }
    
    /// <summary>
    /// Проверяет, открыт ли продукт в коллекции
    /// </summary>
    public static bool IsFoodDiscovered(FoodType foodType)
    {
        return _discoveredFoods.Contains(foodType);
    }
    
    /// <summary>
    /// Сохраняет открытые продукты
    /// </summary>
    private static void SaveDiscoveredFoods()
    {
        // Сохраняем количество открытых продуктов
        PlayerPrefs.SetInt(DISCOVERED_FOODS_COUNT_KEY, _discoveredFoods.Count);
        
        // Сохраняем каждый открытый продукт
        for (int i = 0; i < _discoveredFoods.Count; i++)
        {
            string key = string.Format(DISCOVERED_FOOD_KEY, i);
            PlayerPrefs.SetInt(key, (int)_discoveredFoods[i]);
        }
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Загружает открытые продукты
    /// </summary>
    private static void LoadDiscoveredFoods()
    {
        _discoveredFoods.Clear();
        
        // Загружаем количество открытых продуктов
        int foodCount = PlayerPrefs.GetInt(DISCOVERED_FOODS_COUNT_KEY, 0);
        
        for (int i = 0; i < foodCount; i++)
        {
            string key = string.Format(DISCOVERED_FOOD_KEY, i);
            int foodType = PlayerPrefs.GetInt(key, -1);
            
            if (foodType >= 0 && foodType < System.Enum.GetValues(typeof(FoodType)).Length)
            {
                _discoveredFoods.Add((FoodType)foodType);
            }
        }
        
        // Сосиска всегда открыта
        if (!_discoveredFoods.Contains(FoodType.Sausage))
        {
            _discoveredFoods.Add(FoodType.Sausage);
        }
        
    }
    
    #endregion

    #region Goals - Управление целями
    
    private static FoodType _currentGoal = FoodType.Eggs; // Сосиска не может быть целью
    private const string CURRENT_GOAL_KEY = "CurrentGoal";
    
    /// <summary>
    /// Текущая цель игрока
    /// </summary>
    public static FoodType CurrentGoal => _currentGoal;
    
    /// <summary>
    /// Устанавливает текущую цель
    /// </summary>
    public static void SetCurrentGoal(FoodType goal)
    {
        if (_currentGoal != goal)
        {
            _currentGoal = goal;
            
            // Сохраняем изменения
            PlayerPrefs.SetInt(CURRENT_GOAL_KEY, (int)_currentGoal);
            PlayerPrefs.Save();
            
        }
    }
    
    /// <summary>
    /// Загружает текущую цель
    /// </summary>
    private static void LoadCurrentGoal()
    {
        _currentGoal = (FoodType)PlayerPrefs.GetInt(CURRENT_GOAL_KEY, (int)FoodType.Eggs); // Сосиска не может быть целью
    }
    
    #endregion

    #region Initialization - Инициализация системы
    
    private static bool _isInitialized = false;
    
    /// <summary>
    /// Проверяет, инициализирована ли система данных
    /// </summary>
    public static bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// Инициализирует все игровые данные
    /// Должен вызываться при запуске игры
    /// </summary>
    public static void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("GameDataManager: Система уже инициализирована");
            return;
        }
        
        
        // Загружаем все данные
        LoadCoins();
        LoadExperience();
        LoadDiscoveredFoods();
        LoadCurrentGoal();
        
        _isInitialized = true;
    }
    
    /// <summary>
    /// Сбрасывает все игровые данные (для тестирования)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void ResetAllData()
    {
        
        // Сбрасываем данные в памяти
        _currentCoins = 0;
        _currentExperience = 0;
        _discoveredFoods.Clear();
        _discoveredFoods.Add(FoodType.Sausage); // Сосиска всегда открыта
        _currentGoal = FoodType.Sausage;
        
        // Очищаем PlayerPrefs
        PlayerPrefs.DeleteKey(COINS_SAVE_KEY);
        PlayerPrefs.DeleteKey(EXPERIENCE_SAVE_KEY);
        PlayerPrefs.DeleteKey(DISCOVERED_FOODS_COUNT_KEY);
        PlayerPrefs.DeleteKey(CURRENT_GOAL_KEY);
        PlayerPrefs.DeleteKey(BOMB_BOOSTER_KEY);
        PlayerPrefs.DeleteKey(PAW_BOOSTER_KEY);
        PlayerPrefs.DeleteKey(CAT_BOOSTER_KEY);
        
        // Очищаем все ключи открытых продуктов
        int foodCount = PlayerPrefs.GetInt(DISCOVERED_FOODS_COUNT_KEY, 0);
        for (int i = 0; i < foodCount; i++)
        {
            string key = string.Format(DISCOVERED_FOOD_KEY, i);
            PlayerPrefs.DeleteKey(key);
        }
        
        PlayerPrefs.Save();
        
        // Уведомляем об изменениях
        OnCoinsChanged?.Invoke(_currentCoins);
        OnExperienceChanged?.Invoke(_currentExperience);
        OnDiscoveredFoodsChanged?.Invoke(new List<FoodType>(_discoveredFoods));
        
    }
    
    #endregion
}
