using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Менеджер спавна продуктов питания
/// Управляет созданием новых продуктов в точке спавна согласно правилам игры
/// </summary>
public class FoodSpawner : MonoBehaviour
{
    [Header("Точка спавна")]
    [Tooltip("Позиция, где появляются новые продукты")]
    public Transform spawnPoint;
    
    [Tooltip("Текущий продукт в точке спавна")]
    public FoodItem currentFood;
    
    [Header("Данные продуктов")]
    [Tooltip("Массив всех доступных продуктов")]
    public FoodData[] allFoodData;
    
    [Tooltip("Словарь продуктов по типам для быстрого поиска")]
    private Dictionary<FoodType, FoodData> foodDataDictionary;
    
    [Header("Игровой прогресс")]
    [Tooltip("Открытые продукты в коллекции")]
    public List<FoodType> unlockedFoods = new List<FoodType>();
    
    [Tooltip("Продукты, которые можно спавнить в точке спавна")]
    private List<FoodType> spawnableFoods = new List<FoodType>();
    
    [Header("Настройки спавна")]
    [Tooltip("Задержка перед спавном нового продукта после столкновения (в секундах)")]
    public float spawnDelay = 0.1f;
    
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    [Tooltip("Идет ли процесс спавна нового продукта")]
    public bool isSpawning = false;
    
    [Tooltip("Счетчик спавнов продуктов в текущей игре")]
    private int spawnCount = 0;
    
    [Header("Бустеры")]
    [Tooltip("Есть ли активный бустер в точке спавна")]
    public bool hasActiveBooster = false;
    
    [Header("Границы контейнера")]
    [Tooltip("Левая граница контейнера")]
    public float containerLeftBoundary = -1.7f;
    
    [Tooltip("Правая граница контейнера")]
    public float containerRightBoundary = 1.7f;
    
    [Tooltip("Верхняя граница контейнера")]
    public float containerTopBoundary = 2f;
    
    [Tooltip("Нижняя граница контейнера")]
    public float containerBottomBoundary = -2f;
    
    private void Start()
    {
        InitializeFoodSpawner();
    }
    
    /// <summary>
    /// Инициализирует спавнер продуктов
    /// </summary>
    private void InitializeFoodSpawner()
    {
        InitializeFoodDictionary();
        InitializeUnlockedFoods();
        InitializeSpawnableFoods();
        // Сбрасываем счетчик спавнов при инициализации
        ResetSpawnCount();
        // Не спавним продукт сразу - только когда игра активна
        // SpawnNewFood();
    }
    
    /// <summary>
    /// Сбрасывает счетчик спавнов (вызывается при начале новой игры)
    /// </summary>
    public void ResetSpawnCount()
    {
        spawnCount = 0;
        isSpawning = false; // Сбрасываем флаг спавна
    }
    
    /// <summary>
    /// Сбрасывает список спавнабельных продуктов к начальному состоянию
    /// </summary>
    public void ResetSpawnableFoods()
    {
        // Сбрасываем список спавнабельных продуктов к базовым
        UpdateSpawnableFoods();
    }
    
    /// <summary>
    /// Инициализирует словарь продуктов для быстрого поиска
    /// </summary>
    private void InitializeFoodDictionary()
    {
        foodDataDictionary = new Dictionary<FoodType, FoodData>();
        
        if (allFoodData == null || allFoodData.Length == 0)
        {
            return;
        }
        
        int addedCount = 0;
        foreach (FoodData foodData in allFoodData)
        {
            if (foodData != null)
            {
                foodDataDictionary[foodData.foodType] = foodData;
                addedCount++;
            }
        }
    }
    
    /// <summary>
    /// Инициализирует список открытых продуктов
    /// </summary>
    private void InitializeUnlockedFoods()
    {
        unlockedFoods.Clear();
        
        // Сосиска всегда открыта
        unlockedFoods.Add(FoodType.Sausage);
        
        // Добавляем продукты, которые можно спавнить в начале
        foreach (FoodData foodData in allFoodData)
        {
            if (foodData != null && foodData.canSpawnAtStart)
            {
                if (!unlockedFoods.Contains(foodData.foodType))
                {
                    unlockedFoods.Add(foodData.foodType);
                }
            }
        }
        
    }
    
    /// <summary>
    /// Инициализирует список продуктов, которые можно спавнить в точке спавна
    /// </summary>
    private void InitializeSpawnableFoods()
    {
        // Используем общий метод для инициализации
        UpdateSpawnableFoods();
    }
    
    
    
    /// <summary>
    /// Создает новый продукт в точке спавна (упрощенная логика)
    /// </summary>
    public void SpawnNewFood()
    {
        
        // Проверяем, активна ли игра
        if (gameManager != null && !gameManager.isGameActive)
        {
            return;
        }
        
        // Проверяем, что нет текущего продукта и не идет спавн
        if (currentFood != null || isSpawning)
        {
            return;
        }
        
        // Атомарно устанавливаем флаг спавна для предотвращения race condition
        isSpawning = true;
        
        // Используем Invoke для задержки
        Invoke(nameof(CreateFood), spawnDelay);
    }
    
    /// <summary>
    /// Создает продукт (вызывается через Invoke)
    /// </summary>
    private void CreateFood()
    {
        // Дополнительная проверка: не создаем продукт, если игра неактивна или спавн отключен
        if (gameManager != null && !gameManager.isGameActive)
        {
            isSpawning = false;
            return;
        }
        
        if (!isSpawning)
        {
            return;
        }
        
        // Выбираем случайный продукт из доступных
        FoodType randomFoodType = GetRandomUnlockedFood();
        FoodData foodData = GetFoodData(randomFoodType);
        if (foodData != null && foodData.foodPrefab != null)
        {
            // Создаем новый продукт
            GameObject newFoodObject = Instantiate(foodData.foodPrefab, spawnPoint.position, Quaternion.identity);
            currentFood = newFoodObject.GetComponent<FoodItem>();
            
            if (currentFood != null)
            {
                // Настраиваем продукт согласно README.md
                currentFood.foodData = foodData;
                // Продукт на точке спавна имеет статус inSpawnPoint = true и isDroped = false и isFalling = false и isDragble = false
                currentFood.inSpawnPoint = true;
                currentFood.isDroped = false;
                currentFood.isFalling = false;
                currentFood.isDragble = false;
                
                // Устанавливаем ссылки на менеджеры
                currentFood.foodSpawner = this;
                if (gameManager != null && gameManager.mergeManager != null)
                {
                    currentFood.mergeManager = gameManager.mergeManager;
                }
                
                // Переключаем в кинематический режим до начала перетаскивания
                if (currentFood.rigidBody != null)
                {
                    currentFood.rigidBody.bodyType = RigidbodyType2D.Kinematic;
                }
                
            }
            else
            {
                Destroy(newFoodObject);
            }
        }
        
        isSpawning = false;
    }
    
    /// <summary>
    /// Выбирает случайный продукт из доступных для спавна согласно новой логике
    /// </summary>
    private FoodType GetRandomUnlockedFood()
    {
        // Новая логика спавна согласно требованиям:
        // 1. В начале игры всегда спавнятся две сосиски подряд
        // 2. Затем случайно из доступных продуктов
        // 3. Мясной шарик спавнится в два раза реже базовых продуктов
        
        if (gameManager == null) return FoodType.Sausage;
        
        // Увеличиваем счетчик спавнов
        spawnCount++;
        
        // Первые два спавна - всегда сосиска
        if (spawnCount <= 2)
        {
            return FoodType.Sausage;
        }
        
        // Далее - случайно из доступных продуктов с учетом редкости
        if (spawnableFoods.Count == 0)
        {
            return FoodType.Sausage;
        }
        
        // Создаем взвешенный список для выбора
        List<FoodType> weightedFoods = new List<FoodType>();
        
        foreach (FoodType foodType in spawnableFoods)
        {
            if (foodType == FoodType.Meatball)
            {
                // Мясной шарик добавляем один раз (в два раза реже)
                weightedFoods.Add(foodType);
            }
            else
            {
                // Базовые продукты добавляем дважды (в два раза чаще)
                weightedFoods.Add(foodType);
                weightedFoods.Add(foodType);
            }
        }
        
        // Выбираем случайный продукт из взвешенного списка
        int randomIndex = Random.Range(0, weightedFoods.Count);
        FoodType selectedFood = weightedFoods[randomIndex];
        
        return selectedFood;
    }
    
    /// <summary>
    /// Получает данные продукта по типу
    /// </summary>
    public FoodData GetFoodData(FoodType foodType)
    {
        if (foodDataDictionary == null)
        {
            InitializeFoodDictionary();
        }
        
        return foodDataDictionary?.TryGetValue(foodType, out FoodData foodData) == true ? foodData : null;
    }
    
    
    /// <summary>
    /// Разблокирует новый продукт
    /// </summary>
    public void UnlockFood(FoodType foodType)
    {
        if (unlockedFoods.Contains(foodType)) return;
        
        unlockedFoods.Add(foodType);
        
        // Обновляем список спавнабельных продуктов при разблокировке
        UpdateSpawnableFoods();
        
        gameManager?.OnNewFoodUnlocked?.Invoke(foodType);
    }
    
    /// <summary>
    /// Проверяет, разблокирован ли продукт
    /// </summary>
    public bool IsFoodUnlocked(FoodType foodType)
    {
        return unlockedFoods.Contains(foodType);
    }
    
    /// <summary>
    /// Обновляет список продуктов, которые можно спавнить
    /// </summary>
    public void UpdateSpawnableFoods()
    {
        spawnableFoods.Clear();
        
        // Добавляем только базовые продукты для спавна (сосиска, яичница, сандвич)
        foreach (FoodType foodType in unlockedFoods)
        {
            if (IsBasicFood(foodType))
            {
                spawnableFoods.Add(foodType);
            }
        }
        
        // Добавляем мясной шарик только если он разблокирован И игрок набрал 1000 опыта
        if (unlockedFoods.Contains(FoodType.Meatball) && gameManager != null && GameDataManager.GameSessionExperience >= 1000)
        {
            spawnableFoods.Add(FoodType.Meatball);
        }
    }
    
    /// <summary>
    /// Проверяет, является ли продукт базовым (доступным для спавна)
    /// </summary>
    /// <param name="foodType">Тип продукта</param>
    /// <returns>True если продукт базовый</returns>
    private bool IsBasicFood(FoodType foodType)
    {
        // Только базовые продукты могут спавниться
        return foodType == FoodType.Sausage || 
               foodType == FoodType.Eggs || 
               foodType == FoodType.Sandwich;
    }
    
    /// <summary>
    /// Устанавливает флаг активного бустера
    /// </summary>
    public void SetActiveBooster(bool active)
    {
        hasActiveBooster = active;
    }
    
    /// <summary>
    /// Проверяет, можно ли спавнить новый продукт
    /// </summary>
    public bool CanSpawnNewFood()
    {
        return currentFood == null && !isSpawning && !hasActiveBooster;
    }
    
    /// <summary>
    /// Принудительно очищает текущий продукт (используется при активации бустера)
    /// </summary>
    public void ClearCurrentFood()
    {
        if (currentFood != null)
        {
            Destroy(currentFood.gameObject);
            currentFood = null;
        }
    }
    
    /// <summary>
    /// Получает границы контейнера для свайп-контроллера
    /// </summary>
    public void GetContainerBounds(out float left, out float right, out float top, out float bottom)
    {
        left = containerLeftBoundary;
        right = containerRightBoundary;
        top = containerTopBoundary;
        bottom = containerBottomBoundary;
    }
    
    /// <summary>
    /// Устанавливает границы контейнера
    /// </summary>
    public void SetContainerBounds(float left, float right, float top, float bottom)
    {
        containerLeftBoundary = left;
        containerRightBoundary = right;
        containerTopBoundary = top;
        containerBottomBoundary = bottom;
    }
    
    
}