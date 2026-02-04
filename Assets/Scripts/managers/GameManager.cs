using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Степени удовлетворенности кота
/// </summary>
public enum CatSatisfactionLevel
{
    Hungry,           // Голодный котик (0 опыта)
    Worms,           // Заморил червячка (1500 опыта)
    Better,          // Уже полегче (3500 опыта)
    Tasty,           // Вкусненько (5000 опыта)
    Satisfied,       // Пузико довольно! (8500 опыта)
    Full,            // Надо же, сколько еды! (13500 опыта)
    Happy,           // Счастливый котик - сытый котик! (23000 опыта)
    Royal            // Королевская сытость (46500 опыта)
}

/// <summary>
/// Главный менеджер игры
/// Координирует работу всех систем игры и управляет общим состоянием
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Ссылки на системы")]
    [Tooltip("Ссылка на спавнер продуктов")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на менеджер слияния")]
    public MergeManager mergeManager;
    
    [Tooltip("Ссылка на менеджер UI")]
    public UIManager uiManager;
    
    [Tooltip("Ссылка на менеджер состояний кота")]
    public CatStateManager catStateManager;
    
    [Tooltip("Ссылка на менеджер бустеров")]
    public BoosterManager boosterManager;
    
    [Tooltip("Ссылка на менеджер ежедневных наград")]
    public DailyRewardManager dailyRewardManager;
    
    [Tooltip("Конфигурация экономики")]
    public EconomyConfig economyConfig;
    
    [Header("Игровое состояние")]
    [Tooltip("Текущий опыт игрока (синхронизируется с GameDataManager)")]
    public int currentExperience => GameDataManager.CurrentExperience;
    
    [Tooltip("Текущие монеты игрока (синхронизируется с GameDataManager)")]
    public int currentCoins => GameDataManager.CurrentCoins;
    
    
    [Tooltip("Игра активна")]
    public bool isGameActive = false;
    
    [Header("Система целей")]
    [Tooltip("Текущая цель игрока (синхронизируется с GameDataManager)")]
    public FoodType currentGoal => GameDataManager.CurrentGoal;
    
    [Tooltip("Открытые продукты в коллекции (синхронизируется с GameDataManager)")]
    public List<FoodType> discoveredFoods => GameDataManager.DiscoveredFoods;
    
    [Tooltip("Список доступных целей")]
    private List<FoodType> availableGoals = new List<FoodType>();
    
    [Header("Система удовлетворенности кота")]
    [Tooltip("Текущая степень удовлетворенности кота")]
    public CatSatisfactionLevel currentSatisfactionLevel = CatSatisfactionLevel.Hungry;
    
    [Header("Система проигрыша")]
    
    [Tooltip("Задержка перед показом экрана проигрыша")]
    public float gameOverDelay = 2.0f;
    
    [Tooltip("Задержка перед показом экрана проигрыша (3 секунды)")]
    public float gameOverScreenDelay = 3.0f;
    
    
    [Header("События")]
    [Tooltip("Вызывается при изменении опыта игрока")]
    public System.Action<int> OnExperienceChanged;
    
    [Tooltip("Вызывается при изменении количества монет")]
    public System.Action<int> OnCoinsChanged;
    
    [Tooltip("Вызывается при разблокировке нового продукта")]
    public System.Action<FoodType> OnNewFoodUnlocked;
    
    [Tooltip("Вызывается при изменении уровня удовлетворенности кота")]
    public System.Action<CatSatisfactionLevel> OnSatisfactionLevelChanged;
    
    [Tooltip("Вызывается при выполнении цели")]
    public System.Action<FoodType> OnGoalCompleted;
    
    [Tooltip("Вызывается при изменении цели")]
    public System.Action<FoodType> OnGoalChanged;
    
    [Tooltip("Вызывается при проигрыше")]
    public System.Action OnGameOver;
    
    // Переменные для системы проигрыша
    private bool isGameOverTriggered = false;
    private Coroutine gameOverCoroutine = null;
    
    // Кэширование для оптимизации
    private FoodItem[] cachedFoodItems;
    private float lastCacheTime = 0f;
    private const float CACHE_UPDATE_INTERVAL = 0.1f; // Обновляем кэш каждые 0.1 секунды
    
    // Пороги для системы проигрыша
    private const float TOP_BOUNDARY = 2.5f; // Верхняя граница контейнера согласно README.md
    
    
    
    private void Start()
    {
        ValidateReferences();
        CheckDailyRewards();
        LoadGameProgress();
        
        if (!discoveredFoods.Contains(FoodType.Sausage))
        {
            discoveredFoods.Add(FoodType.Sausage);
        }
        
        SetupSystemReferences();
        InitializeGoalSystem();
        SetupCatStateManagerEvents();
        
        // Инициализируем кэш продуктов
        cachedFoodItems = new FoodItem[0];
        
        GameDataManager.OnCoinsChanged += OnCoinsChangedFromDataManager;
        GameDataManager.OnBoosterCountChanged += OnBoosterCountChangedFromDataManager;
    }
    
    /// <summary>
    /// Валидирует необходимые ссылки
    /// </summary>
    private void ValidateReferences()
    {
        if (foodSpawner == null)
        {
            Debug.LogError("GameManager: FoodSpawner не назначен!");
        }
        
        if (mergeManager == null)
        {
            Debug.LogError("GameManager: MergeManager не назначен!");
        }
        
        if (uiManager == null)
        {
            Debug.LogError("GameManager: UIManager не назначен!");
        }
        
        if (catStateManager == null)
        {
            Debug.LogWarning("GameManager: CatStateManager не назначен! Будет использована fallback логика.");
        }
        
        if (boosterManager == null)
        {
            Debug.LogError("GameManager: BoosterManager не назначен!");
        }
        
        if (dailyRewardManager == null)
        {
            Debug.LogError("GameManager: DailyRewardManager не назначен!");
        }
        
    }
    
    /// <summary>
    /// Проверяет и показывает ежедневные награды
    /// </summary>
    private void CheckDailyRewards()
    {
        if (dailyRewardManager == null)
        {
            Debug.LogError("GameManager: DailyRewardManager не назначен");
            return;
        }
        
        if (dailyRewardManager.IsRewardClaimedToday()) return;
        
        dailyRewardManager.SetCurrentDailyReward();
        dailyRewardManager.SetNextDayReward();
        dailyRewardManager.ShowRewardPanel();
    }
    
    /// <summary>
    /// Настраивает ссылки между системами
    /// </summary>
    private void SetupSystemReferences()
    {
        if (foodSpawner != null)
        {
            foodSpawner.gameManager = this;
        }
        
        if (mergeManager != null)
        {
            mergeManager.gameManager = this;
        }
    }
    
    /// <summary>
    /// Настраивает события CatStateManager
    /// </summary>
    private void SetupCatStateManagerEvents()
    {
        if (catStateManager != null)
        {
            catStateManager.OnSatisfactionLevelChanged += OnCatSatisfactionLevelChanged;
            catStateManager.OnNewStateUnlocked += OnNewCatStateUnlocked;
        }
    }
    
    
    /// <summary>
    /// Обрабатывает изменения монет из GameDataManager
    /// </summary>
    private void OnCoinsChangedFromDataManager(int newCoins)
    {
        // Передаем событие дальше в UI
        OnCoinsChanged?.Invoke(newCoins);
    }
    
    /// <summary>
    /// Обрабатывает изменения количества бустеров из GameDataManager
    /// </summary>
    private void OnBoosterCountChangedFromDataManager(string boosterType, int newCount)
    {
        // Уведомляем BoosterManager об обновлении UI
        if (boosterManager != null)
        {
            boosterManager.UpdateBoosterUI();
        }
    }
    
    private void LoadGameProgress() { }
    private void SaveGameProgress() { }
    private void LoadDiscoveredFoods() { }
    private void SaveDiscoveredFoods() { }
    
    /// <summary>
    /// Отписывается от событий при уничтожении объекта
    /// </summary>
    private void OnDestroy()
    {
        if (catStateManager != null)
        {
            catStateManager.OnSatisfactionLevelChanged -= OnCatSatisfactionLevelChanged;
            catStateManager.OnNewStateUnlocked -= OnNewCatStateUnlocked;
        }
        
        // Отписываемся от событий GameDataManager
        GameDataManager.OnCoinsChanged -= OnCoinsChangedFromDataManager;
        GameDataManager.OnBoosterCountChanged -= OnBoosterCountChangedFromDataManager;
    }
    
    private void Update()
    {
        // Проверяем продукты в контейнере на пересечение границ
        if (isGameActive && !isGameOverTriggered)
        {
            CheckProductsInContainer();
        }
    }
    
    
    /// <summary>
    /// Проверяет продукты в контейнере на пересечение границ согласно README.md
    /// </summary>
    private void CheckProductsInContainer()
    {
        if (isGameOverTriggered) return;
        
        // Обновляем кэш продуктов с интервалом для оптимизации
        if (Time.time - lastCacheTime > CACHE_UPDATE_INTERVAL)
        {
            FoodItem[] newCache = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            cachedFoodItems = newCache ?? new FoodItem[0]; // Защита от null
            lastCacheTime = Time.time;
        }
        
        FoodItem[] allFoodItems = cachedFoodItems;
        bool hasProductsAboveBoundary = false;
        
        // Проверяем, что кэш продуктов инициализирован
        if (allFoodItems == null)
        {
            return;
        }
        
        foreach (FoodItem foodItem in allFoodItems)
        {
            // Проверяем только продукты со статусом isDroped = true
            // Падающие продукты (isFalling = true) не проверяем на пересечение границы
            if (foodItem == null || !foodItem.isDroped) continue;
            
            float yPosition = foodItem.transform.position.y;
            
            // Проверяем на пересечение верхней границы (y >= 2.5)
            if (yPosition >= TOP_BOUNDARY)
            {
                hasProductsAboveBoundary = true;
                
                // Делаем продукт красным при пересечении границы
                foodItem.SetWarningColor(true);
            }
            else
            {
                // Если продукт не пересек границу, сбрасываем цвет
                foodItem.SetWarningColor(false);
            }
        }
        
        
        // Мгновенный проигрыш при пересечении границы
        if (hasProductsAboveBoundary && !isGameOverTriggered)
        {
            Debug.Log("GameManager: Продукт пересек верхнюю границу - мгновенный проигрыш!");
            
            // Мгновенно останавливаем спавн
            if (foodSpawner != null)
            {
                foodSpawner.isSpawning = false;
                Debug.Log("GameManager: Спавн продуктов остановлен при пересечении границы");
                
                // Отменяем все запланированные Invoke в FoodSpawner
                foodSpawner.CancelInvoke();
                
                // Удаляем продукт с точки спавна, если он есть
                if (foodSpawner.currentFood != null)
                {
                    Debug.Log("GameManager: Удаляем продукт с точки спавна при проигрыше");
                    foodSpawner.currentFood.DestroyFood();
                    foodSpawner.currentFood = null;
                }
            }
            
            TriggerGameOver();
        }
    }
    
    /// <summary>
    /// Обрабатывает вход продукта в триггер (устаревший метод - теперь не используется)
    /// </summary>
    public void OnProductEnterTrigger()
    {
        // Метод оставлен для совместимости, но логика перенесена в CheckProductsInContainer
        Debug.Log("GameManager: OnProductEnterTrigger вызван, но больше не используется");
    }
    
    /// <summary>
    /// Обрабатывает выход продукта из триггера (устаревший метод - теперь не используется)
    /// </summary>
    public void OnProductExitTrigger()
    {
        // Метод оставлен для совместимости, но логика перенесена в CheckProductsInContainer
        Debug.Log("GameManager: OnProductExitTrigger вызван, но больше не используется");
    }
    
    
    
    /// <summary>
    /// Останавливает физику всех продуктов (цвета сохраняются)
    /// </summary>
    private void StopAllProductPhysics()
    {
        FoodItem[] allFoodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        foreach (FoodItem foodItem in allFoodItems)
        {
            if (foodItem != null && foodItem.rigidBody != null)
            {
                // Останавливаем физику
                foodItem.rigidBody.bodyType = RigidbodyType2D.Static;
                foodItem.rigidBody.linearVelocity = Vector2.zero;
                foodItem.rigidBody.angularVelocity = 0f;
                
                // Цвета НЕ сбрасываем - игрок должен увидеть проблему
            }
        }
        Debug.Log("GameManager: Физика всех продуктов остановлена, цвета сохранены");
    }
    
    
    /// <summary>
    /// Запускает проигрыш - мгновенно останавливает игру
    /// </summary>
    private void TriggerGameOver()
    {
        if (isGameOverTriggered) return;
        
        Debug.Log("GameManager: ТРИГГЕР ПРОИГРЫША! Продукт пересек верхнюю границу - мгновенный проигрыш");
        isGameOverTriggered = true;
        isGameActive = false;
        
        
        // Мгновенно останавливаем физику всех продуктов (цвета сохраняются)
        StopAllProductPhysics();
        
        // Спавн уже остановлен в CheckProductsInContainer()
        
        // Запускаем корутину для показа экрана проигрыша через 3 секунды
        gameOverCoroutine = StartCoroutine(ShowGameOverDelayed());
    }
    
    /// <summary>
    /// Показывает экран проигрыша с задержкой (3 секунды)
    /// </summary>
    private System.Collections.IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSeconds(gameOverScreenDelay);
        
        // Показываем панель проигрыша
        uiManager?.ShowGameOverPanel();
        OnGameOver?.Invoke();
        
        // Удаляем все продукты
        ClearAllFoodItems();
    }
    
    // Метод ClearAllFoodItemsDelayed удален - больше не нужен
    
    
    
    
    
    
    /// <summary>
    /// Очищает все продукты с поля (немедленно)
    /// </summary>
    private void ClearAllFoodItems()
    {
        FoodItem[] foodItems = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        foreach (FoodItem item in foodItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Добавляет опыт игроку
    /// </summary>
    public void AddExperience(int amount)
    {
        // Добавляем опыт к игровой сессии (сбрасывается при начале новой игры)
        GameDataManager.AddGameSessionExperience(amount);
        
        // Обновляем состояние кота через CatStateManager на основе игрового опыта
        if (catStateManager != null)
        {
            currentSatisfactionLevel = catStateManager.UpdateSatisfactionLevel(GameDataManager.GameSessionExperience);
        }
        else
        {
            // Fallback на старую логику, если CatStateManager не настроен
            UpdateSatisfactionLevel();
        }
        
        // Уведомляем UI
        OnExperienceChanged?.Invoke(GameDataManager.GameSessionExperience);
        
        // Проверяем разблокировку новых продуктов
        CheckForNewUnlocks();
    }
    
    /// <summary>
    /// Добавляет монеты игроку
    /// </summary>
    public void AddCoins(int amount)
    {
        // Используем GameDataManager для добавления монет
        GameDataManager.AddCoins(amount);
        
        // Уведомляем UI
        OnCoinsChanged?.Invoke(GameDataManager.CurrentCoins);
    }
    
    /// <summary>
    /// Проверяет разблокировку новых продуктов
    /// </summary>
    private void CheckForNewUnlocks()
    {
        // Проверяем разблокировку мясного шарика при 1000 игровом опыте
        if (GameDataManager.GameSessionExperience >= 1000 && foodSpawner != null)
        {
            if (!foodSpawner.IsFoodUnlocked(FoodType.Meatball))
            {
                foodSpawner.UnlockFood(FoodType.Meatball);
            }
        }
        
        // Обновляем список спавнабельных продуктов при изменении опыта
        if (foodSpawner != null)
        {
            foodSpawner.UpdateSpawnableFoods();
        }
    }
    
    /// <summary>
    /// Вызывается при завершении слияния
    /// </summary>
    public void OnMergeCompleted(FoodData originalFood, FoodData mergedFood)
    {
        if (originalFood != null && mergedFood != null)
        {
            // Проверяем, создан ли продукт впервые
            bool isFirstDiscovery = !discoveredFoods.Contains(mergedFood.foodType);
            
            // Начисляем опыт за результирующий продукт (всегда)
            int experienceGain = economyConfig != null ? economyConfig.GetExperienceForFood(mergedFood.foodType) : 0;
            AddExperience(experienceGain);
            
            // Добавляем продукт в коллекцию при первом открытии
            if (isFirstDiscovery)
            {
                GameDataManager.AddDiscoveredFood(mergedFood.foodType);
                
                // Разблокируем продукт для спавна в FoodSpawner
                if (foodSpawner != null)
                {
                    foodSpawner.UnlockFood(mergedFood.foodType);
                }
                
                // Выдаем награду за первое открытие продукта
                int firstDiscoveryReward = economyConfig != null ? economyConfig.GetFirstDiscoveryReward(mergedFood.foodType) : 0;
                if (firstDiscoveryReward > 0)
                {
                    AddCoins(firstDiscoveryReward);
                }
            }
            
            // Проверяем выполнение цели (монеты начисляются только за выполнение целей)
            // И обновляем цель только после добавления продукта в коллекцию
            CheckGoalCompletion(mergedFood.foodType, isFirstDiscovery);
        }
    }
    
    /// <summary>
    /// Инициализирует систему целей
    /// </summary>
    private void InitializeGoalSystem()
    {
        // Убеждаемся, что GameDataManager инициализирован
        if (!GameDataManager.IsInitialized)
        {
            GameDataManager.Initialize();
        }
        
        // Для первого запуска устанавливаем первую цель
        SetNextGoal();
    }
    
    /// <summary>
    /// Устанавливает следующую цель
    /// </summary>
    private void SetNextGoal()
    {
        UpdateAvailableGoals();
        
        if (availableGoals.Count > 0)
        {
            FoodType newGoal = SelectWeightedGoal();
            
            if (newGoal != currentGoal)
            {
                GameDataManager.SetCurrentGoal(newGoal);
                OnGoalChanged?.Invoke(currentGoal);
            }
        }
    }
    
    /// <summary>
    /// Обновляет список доступных целей
    /// </summary>
    private void UpdateAvailableGoals()
    {
        availableGoals.Clear();
        
        // Проверяем, достигнут ли лобстер
        bool lobsterAchieved = discoveredFoods.Contains(FoodType.Lobster);
        
        if (lobsterAchieved)
        {
            // После достижения лобстера - случайные цели из коллекции
            foreach (FoodType foodType in discoveredFoods)
            {
                if (foodType != FoodType.Sausage) // Сосиска не может быть целью
                {
                    availableGoals.Add(foodType);
                }
            }
        }
        else
        {
            // Прогрессивная система целей
            FoodType nextGoal = GetNextProgressiveGoal();
            if (nextGoal != FoodType.Sausage) // Сосиска не может быть целью
            {
                availableGoals.Add(nextGoal);
            }
        }
    }
    
    /// <summary>
    /// Получает следующую прогрессивную цель
    /// </summary>
    private FoodType GetNextProgressiveGoal()
    {
        // Если открыта только сосиска, цель - яичница
        if (discoveredFoods.Count == 1 && discoveredFoods.Contains(FoodType.Sausage))
        {
            return FoodType.Eggs;
        }
        
        // Прогрессивная цепочка продуктов (без сосиски, так как она не может быть целью)
        FoodType[] progressiveChain = { 
            FoodType.Eggs, FoodType.Sandwich, FoodType.Meatball, 
            FoodType.Soup, FoodType.Chicken, FoodType.Salmon, FoodType.Shrimp, 
            FoodType.Caviar, FoodType.Oyster, FoodType.Lobster 
        };
        
        // Находим первый НЕ открытый продукт в цепочке
        for (int i = 0; i < progressiveChain.Length; i++)
        {
            if (!discoveredFoods.Contains(progressiveChain[i]))
            {
                return progressiveChain[i];
            }
        }
        
        // Если все продукты открыты, возвращаем лобстера как финальную цель
        return FoodType.Lobster;
    }
    
    /// <summary>
    /// Выбирает цель с учетом весов (продукты высшего порядка реже)
    /// </summary>
    private FoodType SelectWeightedGoal()
    {
        if (availableGoals.Count == 0) return FoodType.Eggs; // Сосиска не может быть целью
        
        // Если только одна цель, возвращаем её
        if (availableGoals.Count == 1) return availableGoals[0];
        
        // Создаем список с весами
        List<FoodType> weightedGoals = new List<FoodType>();
        
        foreach (FoodType goal in availableGoals)
        {
            // Определяем вес продукта
            int weight = GetFoodWeight(goal);
            
            // Добавляем продукт в список столько раз, сколько его вес
            for (int i = 0; i < weight; i++)
            {
                weightedGoals.Add(goal);
            }
        }
        
        // Выбираем случайную цель из взвешенного списка
        int randomIndex = Random.Range(0, weightedGoals.Count);
        return weightedGoals[randomIndex];
    }
    
    /// <summary>
    /// Получает вес продукта для выбора цели
    /// </summary>
    private int GetFoodWeight(FoodType foodType)
    {
        // Прогрессивная цепочка для определения уровня
        FoodType[] progressiveChain = { 
            FoodType.Sausage, FoodType.Eggs, FoodType.Sandwich, FoodType.Meatball, 
            FoodType.Soup, FoodType.Chicken, FoodType.Salmon, FoodType.Shrimp, 
            FoodType.Caviar, FoodType.Oyster, FoodType.Lobster 
        };
        
        // Находим позицию продукта в цепочке
        int position = -1;
        for (int i = 0; i < progressiveChain.Length; i++)
        {
            if (progressiveChain[i] == foodType)
            {
                position = i;
                break;
            }
        }
        
        if (position == -1) return 1; // Fallback
        
        // Определяем вес на основе категории продукта
        // Базовые продукты: Sausage (0), Eggs (1), Sandwich (2)
        if (position <= 2) // Базовые продукты
        {
            return 2;
        }
        // Средние продукты: Meatball (3), Soup (4), Chicken (5)
        else if (position <= 5) // Средние продукты
        {
            return 2;
        }
        // Редкие продукты: Salmon (6), Shrimp (7)
        // Эпические продукты: Caviar (8), Oyster (9), Lobster (10)
        else // Редкие и эпические продукты
        {
            return 1;
        }
    }
    
    /// <summary>
    /// Проверяет выполнение цели
    /// </summary>
    private void CheckGoalCompletion(FoodType createdFood, bool isFirstDiscovery)
    {
        if (createdFood == currentGoal)
        {
            // Цель выполнена! Выдаем базовую награду за цель
            int goalReward = economyConfig != null ? economyConfig.GetGoalReward(currentGoal) : 0;
            AddCoins(goalReward);
            
            // Уведомляем UI
            OnGoalCompleted?.Invoke(currentGoal);
            
            // Устанавливаем следующую цель только если это было первое открытие
            if (isFirstDiscovery)
            {
                SetNextGoal();
            }
        }
    }
    
    
    /// <summary>
    /// Обработчик изменения состояния кота
    /// </summary>
    private void OnCatSatisfactionLevelChanged(CatSatisfactionLevel newLevel, CatStateData stateData)
    {
        currentSatisfactionLevel = newLevel;
        
        // Выдаем награду за смену уровня удовлетворенности (10% от суммы коллекции)
        if (stateData != null && stateData.coinReward > 0)
        {
            int changeReward = Mathf.RoundToInt(stateData.coinReward * 0.1f);
            if (changeReward > 0)
            {
                AddCoins(changeReward);
                Debug.Log($"Получена награда за смену уровня {stateData.satisfactionLevel}: {changeReward} монет (10% от {stateData.coinReward})");
            }
        }
        
        // Уведомляем UI
        OnSatisfactionLevelChanged?.Invoke(newLevel);
    }
    
    /// <summary>
    /// Обработчик разблокировки нового состояния кота
    /// </summary>
    private void OnNewCatStateUnlocked(CatStateData stateData)
    {
        // Выдаем награду только при первом достижении нового уровня удовлетворенности
        if (stateData != null && stateData.coinReward > 0)
        {
            AddCoins(stateData.coinReward);
            Debug.Log($"Получена награда за первое достижение уровня {stateData.satisfactionLevel}: {stateData.coinReward} монет");
        }
    }
    
    /// <summary>
    /// Обновляет степень удовлетворенности кота (fallback метод)
    /// </summary>
    private void UpdateSatisfactionLevel()
    {
        CatSatisfactionLevel newLevel = GetSatisfactionLevel(GameDataManager.CurrentExperience);
        
        if (newLevel != currentSatisfactionLevel)
        {
            CatSatisfactionLevel oldLevel = currentSatisfactionLevel;
            currentSatisfactionLevel = newLevel;
            
            // Уведомляем UI
            OnSatisfactionLevelChanged?.Invoke(newLevel);
        }
    }
    
    /// <summary>
    /// Определяет степень удовлетворенности по опыту
    /// </summary>
    private CatSatisfactionLevel GetSatisfactionLevel(int experience)
    {
        if (catStateManager != null)
        {
            return catStateManager.GetSatisfactionLevel(experience);
        }
        
        // Fallback на старую логику
        int[] satisfactionThresholds = { 0, 1500, 3500, 5000, 8500, 13500, 23000, 46500 };
        for (int i = satisfactionThresholds.Length - 1; i >= 0; i--)
        {
            if (experience >= satisfactionThresholds[i])
            {
                return (CatSatisfactionLevel)i;
            }
        }
        return CatSatisfactionLevel.Hungry;
    }
    
    /// <summary>
    /// Сбрасывает игру - обнуляет текущий опыт и состояние
    /// </summary>
    public void ResetGame()
    {
        // Сбрасываем только игровой опыт (сохраняем коллекцию состояний кота)
        GameDataManager.ResetGameSessionExperience();
        
        // Сбрасываем текущую степень удовлетворенности кота
        currentSatisfactionLevel = CatSatisfactionLevel.Hungry;
        
        // Сбрасываем состояние в CatStateManager
        if (catStateManager != null)
        {
            catStateManager.ResetToInitialState();
        }
        
        // Сбрасываем список спавнабельных продуктов в FoodSpawner
        if (foodSpawner != null)
        {
            foodSpawner.ResetSpawnableFoods();
        }
        
        // Сбрасываем состояние игры и проигрыша
        ResetGameOverState();
        
        // Сбрасываем активные бустеры
        if (boosterManager != null)
        {
            boosterManager.DeactivateCurrentBooster();
        }
        
        // Очищаем поле от продуктов
        ClearAllFoodItems();
        
        // Сбрасываем счетчик спавнов в FoodSpawner
        if (foodSpawner != null)
        {
            foodSpawner.ResetSpawnCount();
        }
        
        // Уведомляем UI об изменении опыта
        OnExperienceChanged?.Invoke(GameDataManager.GameSessionExperience);
        OnSatisfactionLevelChanged?.Invoke(currentSatisfactionLevel);
    }
    
    /// <summary>
    /// Сбрасывает состояние проигрыша
    /// </summary>
    public void ResetGameOverState()
    {
        isGameOverTriggered = false;
        
        // Останавливаем корутины
        if (gameOverCoroutine != null)
        {
            StopCoroutine(gameOverCoroutine);
            gameOverCoroutine = null;
        }
        
        
        
        // Сбрасываем цвета всех продуктов
        ResetAllProductColors();
        
        Debug.Log("GameManager: Состояние проигрыша сброшено");
    }
    
    /// <summary>
    /// Сбрасывает цвета всех продуктов к исходным
    /// </summary>
    private void ResetAllProductColors()
    {
        if (cachedFoodItems == null) return;
        
        foreach (FoodItem foodItem in cachedFoodItems)
        {
            if (foodItem != null)
            {
                foodItem.SetWarningColor(false);
            }
        }
        Debug.Log("GameManager: Цвета всех продуктов сброшены к исходным");
    }
    
    
    /// <summary>
    /// Запускает игру - активирует спавн продуктов
    /// </summary>
    public void StartGame()
    {
        isGameActive = true;
        
        if (foodSpawner != null)
        {
            foodSpawner.SpawnNewFood();
        }
        else
        {
            Debug.LogError("GameManager: foodSpawner не назначен!");
        }
        
        boosterManager?.UpdateBoosterUI();
    }
    
    /// <summary>
    /// Останавливает игру - деактивирует спавн и очищает поле
    /// </summary>
    public void StopGame()
    {
        isGameActive = false;
        
        if (foodSpawner != null)
        {
            foodSpawner.isSpawning = false;
            foodSpawner.currentFood = null;
        }
        
        ClearAllFoodItems();
    }
    
}
