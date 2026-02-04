using UnityEngine;
using System.Collections;

/// <summary>
/// Компонент кота-обжорки-бустера
/// Появляется на сцене, проигрывает анимацию и уничтожает все продукты в контейнере
/// </summary>
public class FatCatBooster : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Задержка перед уничтожением продуктов после окончания анимации")]
    public float destroyDelay = 0.5f;
    
    [Tooltip("Активирован ли бустер")]
    private bool isActivated = false;
    
    [Header("Ссылки")]
    [Tooltip("Ссылка на спавнер продуктов")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    [Tooltip("Ссылка на менеджер бустеров")]
    public BoosterManager boosterManager;
    
    private void Start()
    {
        InitializeFatCatBooster();
    }
    
    /// <summary>
    /// Инициализирует компонент кота-обжорки
    /// </summary>
    private void InitializeFatCatBooster()
    {
        CacheGameManager();
        
        // Тратим бустер кота при спавне
        SpendCatBooster();
    }
    
    /// <summary>
    /// Кэширует ссылку на GameManager для оптимизации
    /// </summary>
    private void CacheGameManager()
    {
        // Сначала пробуем через foodSpawner
        if (foodSpawner != null && foodSpawner.gameManager != null)
        {
            gameManager = foodSpawner.gameManager;
        }
        else
        {
            // Если не найден, ищем в сцене
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        // Ищем boosterManager, если не назначен
        if (boosterManager == null && gameManager != null)
        {
            boosterManager = gameManager.boosterManager;
        }
    }
    
    /// <summary>
    /// Вызывается в конце анимации через Animation Event
    /// </summary>
    public void OnAnimationEnd()
    {
        if (isActivated)
        {
            return;
        }
        
        isActivated = true;
        
        // Запускаем корутину для уничтожения продуктов с задержкой
        StartCoroutine(DestroyFoodsAfterDelay());
    }
    
    /// <summary>
    /// Уничтожает все продукты в контейнере с задержкой
    /// </summary>
    private IEnumerator DestroyFoodsAfterDelay()
    {
        
        // Ждем указанное время
        yield return new WaitForSeconds(destroyDelay);
        
        // Уничтожаем все продукты в контейнере
        DestroyAllFoodsInContainer();
        
        // Уничтожаем самого кота
        Destroy(gameObject);
        
        // Запускаем спавн нового продукта
        TriggerNewProductSpawn();
    }
    
    /// <summary>
    /// Уничтожает все продукты в контейнере (кроме продукта на точке спавна)
    /// </summary>
    private void DestroyAllFoodsInContainer()
    {
        
        // Находим все FoodItem в сцене
        FoodItem[] allFoods = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        
        int destroyedCount = 0;
        
        foreach (FoodItem food in allFoods)
        {
            // Уничтожаем только продукты в контейнере (не на точке спавна)
            if (food != null && food.isDroped && !food.inSpawnPoint)
            {
                food.DestroyFood();
                destroyedCount++;
            }
        }
        
    }
    
    /// <summary>
    /// Запускает спавн нового продукта после действия бустера
    /// </summary>
    private void TriggerNewProductSpawn()
    {
        if (foodSpawner != null && gameManager != null && gameManager.isGameActive)
        {
            foodSpawner.SpawnNewFood();
        }
    }
    
    /// <summary>
    /// Тратит бустер кота при спавне
    /// </summary>
    private void SpendCatBooster()
    {
        // Тратим кота через GameDataManager
        bool success = GameDataManager.SpendBoosters("cat", 1);
        if (success)
        {
            
            // Обновляем UI бустеров
            UpdateBoosterUI();
        }
        else
        {
            Debug.LogError("FatCatBooster: Не удалось потратить бустер кота!");
        }
    }
    
    /// <summary>
    /// Обновляет UI бустеров
    /// </summary>
    private void UpdateBoosterUI()
    {
        if (boosterManager != null)
        {
            boosterManager.UpdateBoosterUI();
        }
        else
        {
            Debug.LogWarning("FatCatBooster: BoosterManager не найден!");
        }
    }
}

