using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Менеджер системы бустеров
/// Управляет активацией и использованием бустеров в игре
/// </summary>
public class BoosterManager : MonoBehaviour
{
    [Header("UI элементы бустеров")]
    [Tooltip("Кнопка активации бомбочки")]
    public Button bombButton;
    
    [Tooltip("Кнопка активации лапки")]
    public Button pawButton;
    
    [Tooltip("Кнопка активации кота")]
    public Button catButton;
    
    [Tooltip("Текст с количеством бомбочек")]
    public TextMeshProUGUI bombCountText;
    
    [Tooltip("Текст с количеством лапок")]
    public TextMeshProUGUI pawCountText;
    
    [Tooltip("Текст с количеством котов")]
    public TextMeshProUGUI catCountText;
    
    [Header("Префабы бустеров")]
    [Tooltip("Префаб бомбочки")]
    public GameObject bombPrefab;

    [Tooltip("Префаб кота-обжорки")]
    public GameObject fatCatPrefab;
    
    [Header("Префабы лапки")]
    [Tooltip("Префаб лапки-прицела")]
    public GameObject pawTargetPrefab;
    
    [Tooltip("Префаб анимированной лапки")]
    public GameObject pawAnimationPrefab;
    
    [Header("Ссылки")]
    [Tooltip("Ссылка на спавнер продуктов")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    [Tooltip("Точка спавна бустеров")]
    public Transform boosterSpawnPoint;
    
    // Текущий активный бустер
    private GameObject currentBooster = null;
    private bool isBoosterActive = false;
    
    private void Start()
    {
        InitializeBoosterManager();
        SetupButtonListeners();
        SetupEventListeners();
        UpdateBoosterUI();
    }
    
    /// <summary>
    /// Инициализирует менеджер бустеров
    /// </summary>
    private void InitializeBoosterManager()
    {
        // Валидация ссылок
        if (foodSpawner == null)
        {
            foodSpawner = FindFirstObjectByType<FoodSpawner>();
        }
        
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        
        if (boosterSpawnPoint == null)
        {
            // Используем точку спавна продуктов как fallback
            boosterSpawnPoint = foodSpawner?.spawnPoint;
        }
    }
    
    /// <summary>
    /// Настраивает обработчики кнопок
    /// </summary>
    private void SetupButtonListeners()
    {
        if (bombButton != null)
        {
            bombButton.onClick.AddListener(ActivateBombBooster);
        }
        
        if (pawButton != null)
        {
            pawButton.onClick.AddListener(ActivatePawBooster);
        }
        
        if (catButton != null)
        {
            catButton.onClick.AddListener(ActivateCatBooster);
        }
    }
    
    /// <summary>
    /// Настраивает обработчики событий
    /// </summary>
    private void SetupEventListeners()
    {
        // Подписываемся на события изменения количества бустеров
        GameDataManager.OnBoosterCountChanged += OnBoosterCountChanged;
    }
    
    /// <summary>
    /// Обработчик изменения количества бустеров
    /// </summary>
    private void OnBoosterCountChanged(string boosterType, int newCount)
    {
        UpdateBoosterUI();
    }
    
    /// <summary>
    /// Активирует бомбочку-бустер
    /// </summary>
    public void ActivateBombBooster()
    {
        // Проверяем, есть ли бомбочки у игрока
        int bombCount = GameDataManager.GetBoosterCount("bomb");
        if (bombCount <= 0)
        {
            return;
        }
        
        // Проверяем, что игра активна
        if (gameManager == null)
        {
            return;
        }
        
        if (!gameManager.isGameActive)
        {
            return;
        }
        
        // Проверяем, что нет активного бустера
        if (isBoosterActive)
        {
            return;
        }
        
        // Проверяем, что нет продукта в точке спавна
        if (foodSpawner != null && foodSpawner.currentFood != null)
        {
            // Очищаем текущий продукт при активации бустера
            foodSpawner.ClearCurrentFood();
        }
        
        // Бомбочка будет потрачена при спавне в BombBooster
        
        // Создаем бомбочку
        CreateBombBooster();
    }
    
    /// <summary>
    /// Активирует лапку-бустер
    /// </summary>
    public void ActivatePawBooster()
    {
        // Проверяем, есть ли лапки у игрока
        int pawCount = GameDataManager.GetBoosterCount("paw");
        if (pawCount <= 0)
        {
            return;
        }
        
        // Проверяем, что игра активна
        if (gameManager == null || !gameManager.isGameActive)
        {
            return;
        }
        
        // Проверяем, что нет активного бустера
        if (isBoosterActive)
        {
            return;
        }
        
        // Создаем лапку напрямую (без префаба)
        CreatePawBooster();
    }
    
    /// <summary>
    /// Создает лапку-прицел
    /// </summary>
    private void CreatePawBooster()
    {
        if (pawTargetPrefab == null)
        {
            return;
        }
        
        if (boosterSpawnPoint == null)
        {
            return;
        }
        
        // Создаем лапку-прицел из префаба в центре экрана
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        GameObject pawTargetObject = Instantiate(pawTargetPrefab, pawTargetPrefab.transform.position, Quaternion.identity);
        PawTargetController pawTargetController = pawTargetObject.GetComponent<PawTargetController>();
        
        if (pawTargetController == null)
        {
            Destroy(pawTargetObject);
            return;
        }
        
        // Тратим лапку сразу при активации
        if (!GameDataManager.SpendBoosters("paw", 1))
        {
            Destroy(pawTargetObject);
            return;
        }
        
        // Настраиваем ссылки
        pawTargetController.gameManager = gameManager;
        pawTargetController.boosterManager = this;
        pawTargetController.pawAnimationPrefab = pawAnimationPrefab;
        
        // Устанавливаем флаг активности
        isBoosterActive = true;
        currentBooster = pawTargetObject;
        
        // Уведомляем FoodSpawner о наличии активного бустера
        if (foodSpawner != null)
        {
            foodSpawner.SetActiveBooster(true);
        }
        
        // Подписываемся на уничтожение лапки
        StartCoroutine(MonitorBoosterDestruction(pawTargetObject));
    }
    
    /// <summary>
    /// Активирует кота-обжорку-бустера
    /// </summary>
    public void ActivateCatBooster()
    {
        // Проверяем, есть ли коты у игрока
        int catCount = GameDataManager.GetBoosterCount("cat");
        if (catCount <= 0)
        {
            return;
        }
        
        // Проверяем, что игра активна
        if (gameManager == null)
        {
            return;
        }
        
        if (!gameManager.isGameActive)
        {
            return;
        }
        
        // Проверяем, что нет активного бустера
        if (isBoosterActive)
        {
            return;
        }
        
        // Проверяем, что нет продукта в точке спавна
        if (foodSpawner != null && foodSpawner.currentFood != null)
        {
            // Очищаем текущий продукт при активации бустера
            foodSpawner.ClearCurrentFood();
        }
        
        // Создаем кота-обжорку
        CreateCatBooster();
    }
    
    /// <summary>
    /// Вызывается при завершении работы бустера
    /// </summary>
    public void OnBoosterFinished()
    {
        // Сбрасываем флаг активности
        isBoosterActive = false;
        currentBooster = null;
        
        // Уведомляем FoodSpawner о завершении бустера
        if (foodSpawner != null)
        {
            foodSpawner.SetActiveBooster(false);
        }
        
        // Обновляем UI кнопок
        UpdateBoosterUI();
    }
    
    /// <summary>
    /// Создает кота-обжорку в точке спавна
    /// </summary>
    private void CreateCatBooster()
    {
        if (fatCatPrefab == null)
        {
            return;
        }
        
        if (boosterSpawnPoint == null)
        {
            return;
        }
        
        // Создаем кота-обжорку
        GameObject catObject = Instantiate(fatCatPrefab, boosterSpawnPoint.position, Quaternion.identity);
        FatCatBooster fatCatBooster = catObject.GetComponent<FatCatBooster>();
        
        if (fatCatBooster != null)
        {
            // Настраиваем ссылки
            fatCatBooster.foodSpawner = foodSpawner;
            fatCatBooster.gameManager = gameManager;
            fatCatBooster.boosterManager = this;
            
            // Устанавливаем флаг активности
            isBoosterActive = true;
            currentBooster = catObject;
            
            // Уведомляем FoodSpawner о наличии активного бустера
            if (foodSpawner != null)
            {
                foodSpawner.SetActiveBooster(true);
            }
            
            // Подписываемся на уничтожение кота
            StartCoroutine(MonitorBoosterDestruction(catObject));
        }
        else
        {
            Destroy(catObject);
        }
    }
    
    /// <summary>
    /// Создает бомбочку в точке спавна
    /// </summary>
    private void CreateBombBooster()
    {
        if (bombPrefab == null)
        {
            return;
        }
        
        if (boosterSpawnPoint == null)
        {
            return;
        }
        
        // Создаем бомбочку
        GameObject bombObject = Instantiate(bombPrefab, boosterSpawnPoint.position, Quaternion.identity);
        BombBooster bombBooster = bombObject.GetComponent<BombBooster>();
        
        if (bombBooster != null)
        {
            // Настраиваем ссылки
            bombBooster.foodSpawner = foodSpawner;
            bombBooster.gameManager = gameManager;
            
            // Устанавливаем флаг активности
            isBoosterActive = true;
            currentBooster = bombObject;
            
            // Уведомляем FoodSpawner о наличии активного бустера
            if (foodSpawner != null)
            {
                foodSpawner.SetActiveBooster(true);
            }
            
            // Подписываемся на уничтожение бомбочки
            StartCoroutine(MonitorBoosterDestruction(bombObject));
        }
        else
        {
            Destroy(bombObject);
        }
    }
    
    /// <summary>
    /// Отслеживает уничтожение бустера
    /// </summary>
    private System.Collections.IEnumerator MonitorBoosterDestruction(GameObject booster)
    {
        // Ждем, пока бустер не будет уничтожен
        while (booster != null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Освобождаем флаг активности
        isBoosterActive = false;
        currentBooster = null;
        
        // Уведомляем FoodSpawner об отсутствии активного бустера
        if (foodSpawner != null)
        {
            foodSpawner.SetActiveBooster(false);
        }
        
        // Обновляем UI
        UpdateBoosterUI();
    }
    
    /// <summary>
    /// Обновляет UI бустеров
    /// </summary>
    public void UpdateBoosterUI()
    {
        // Обновляем количество бомбочек
        if (bombCountText != null)
        {
            int bombCount = GameDataManager.GetBoosterCount("bomb");
            bombCountText.text = bombCount.ToString();
        }
        
        // Обновляем количество лапок
        if (pawCountText != null)
        {
            int pawCount = GameDataManager.GetBoosterCount("paw");
            pawCountText.text = pawCount.ToString();
        }
        
        // Обновляем количество котов
        if (catCountText != null)
        {
            int catCount = GameDataManager.GetBoosterCount("cat");
            catCountText.text = catCount.ToString();
        }
        
        // Обновляем состояние кнопок
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Обновляет состояние кнопок бустеров
    /// </summary>
    private void UpdateButtonStates()
    {
        // Проверяем доступность бомбочки
        if (bombButton != null)
        {
            int bombCount = GameDataManager.GetBoosterCount("bomb");
            bool gameActive = gameManager == null || gameManager.isGameActive;
            // Бомбочка может использоваться даже если есть продукт на точке спавна (она его заменит)
            bool canUseBomb = bombCount > 0 && !isBoosterActive && gameActive;
            
            bombButton.interactable = canUseBomb;
        }
        
        // Проверяем доступность лапки
        if (pawButton != null)
        {
            int pawCount = GameDataManager.GetBoosterCount("paw");
            bool canUsePaw = pawCount > 0 && !isBoosterActive && 
                            (gameManager == null || gameManager.isGameActive);
            
            pawButton.interactable = canUsePaw;
        }
        
        // Проверяем доступность кота
        if (catButton != null)
        {
            int catCount = GameDataManager.GetBoosterCount("cat");
            bool canUseCat = catCount > 0 && !isBoosterActive && 
                             (gameManager == null || gameManager.isGameActive);
            
            catButton.interactable = canUseCat;
        }
    }
    
    /// <summary>
    /// Принудительно деактивирует текущий бустер
    /// </summary>
    public void DeactivateCurrentBooster()
    {
        if (currentBooster != null)
        {
            Destroy(currentBooster);
        }
        
        isBoosterActive = false;
        currentBooster = null;
        
        // Уведомляем FoodSpawner об отсутствии активного бустера
        if (foodSpawner != null)
        {
            foodSpawner.SetActiveBooster(false);
        }
        
        UpdateBoosterUI();
    }
    
    /// <summary>
    /// Проверяет, активен ли бустер
    /// </summary>
    public bool IsBoosterActive()
    {
        return isBoosterActive;
    }
    
    /// <summary>
    /// Получает тип активного бустера
    /// </summary>
    public string GetActiveBoosterType()
    {
        if (!isBoosterActive || currentBooster == null)
        {
            return "";
        }
        
        if (currentBooster.GetComponent<BombBooster>() != null)
        {
            return "bomb";
        }
        
        if (currentBooster.GetComponent<PawTargetController>() != null)
        {
            return "paw";
        }
        
        if (currentBooster.GetComponent<FatCatBooster>() != null)
        {
            return "cat";
        }
        
        return "unknown";
    }
    
    private void OnDestroy()
    {
        // Отписываемся от событий
        GameDataManager.OnBoosterCountChanged -= OnBoosterCountChanged;
    }
}
