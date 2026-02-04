using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер свайпов для управления продуктом на точке спавна
/// Позволяет игроку управлять продуктом свайпами по оси X в пределах контейнера
/// </summary>
public class SwipeController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Ссылки")]
    [Tooltip("Ссылка на FoodSpawner для получения текущего продукта")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на камеру для конвертации координат")]
    public Camera mainCamera;
    
    [Header("Настройки свайпов")]
    [Tooltip("Минимальное расстояние для начала свайпа")]
    public float minSwipeDistance = 10f;
    
    [Tooltip("Чувствительность свайпа (множитель скорости)")]
    public float swipeSensitivity = 1f;
    
    [Header("Границы контейнера")]
    [Tooltip("Левая граница контейнера в мировых координатах")]
    public float leftBoundary = -1.7f;
    
    [Tooltip("Правая граница контейнера в мировых координатах")]
    public float rightBoundary = 1.7f;
    
    [Tooltip("Верхняя граница контейнера в мировых координатах")]
    public float topBoundary = 2f;
    
    [Tooltip("Нижняя граница контейнера в мировых координатах")]
    public float bottomBoundary = -2f;
    
    // Состояние свайпа
    private bool isSwipeActive = false;
    private Vector2 startTouchPosition;
    private Vector2 currentTouchPosition;
    private FoodItem currentFoodItem;
    
    private void Start()
    {
        InitializeSwipeController();
    }
    
    private void Update()
    {
        // Обновляем ссылку на текущий продукт
        UpdateCurrentFoodItem();
        
        // Обрабатываем ввод мышки для тестирования в редакторе
        HandleMouseInput();
    }
    
    /// <summary>
    /// Инициализирует контроллер свайпов
    /// </summary>
    private void InitializeSwipeController()
    {
        // Получаем ссылку на камеру если не установлена
        if (mainCamera == null)
        {
            mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
        }
        
        // Получаем ссылку на FoodSpawner если не установлена
        if (foodSpawner == null)
        {
            foodSpawner = FindFirstObjectByType<FoodSpawner>();
        }
        
        // Получаем границы контейнера из FoodSpawner
        if (foodSpawner != null)
        {
            foodSpawner.GetContainerBounds(out leftBoundary, out rightBoundary, out topBoundary, out bottomBoundary);
        }
        
        // Проверяем корректность границ
        if (leftBoundary >= rightBoundary || bottomBoundary >= topBoundary)
        {
            Debug.LogError($"SwipeController: Некорректные границы контейнера! Left: {leftBoundary}, Right: {rightBoundary}, Top: {topBoundary}, Bottom: {bottomBoundary}");
        }
    }
    
    /// <summary>
    /// Обновляет ссылку на текущий продукт
    /// </summary>
    private void UpdateCurrentFoodItem()
    {
        if (foodSpawner != null)
        {
            currentFoodItem = foodSpawner.currentFood;
        }
    }
    
    /// <summary>
    /// Обрабатывает ввод мышки для тестирования в редакторе
    /// </summary>
    private void HandleMouseInput()
    {
        // Проверяем камеру
        if (mainCamera == null)
        {
            return;
        }
        
        // Проверяем, есть ли активный бустер лапки - если да, игнорируем ввод
        if (IsPawBoosterActive())
        {
            return;
        }
        
        // Проверяем, есть ли продукт на точке спавна
        if (currentFoodItem == null || !currentFoodItem.inSpawnPoint || currentFoodItem.isDroped)
        {
            // Сбрасываем состояние свайпа если продукт исчез
            if (isSwipeActive)
            {
                isSwipeActive = false;
            }
            return;
        }
        
        // Обрабатываем нажатие мышки
        if (Input.GetMouseButtonDown(0))
        {
            // Проверяем, что нет активного свайпа от касаний
            if (isSwipeActive)
            {
                return;
            }
            
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0; // Убираем Z координату
            
            // Проверяем, находится ли клик в пределах контейнера
            if (IsPositionInContainer(mouseWorldPos))
            {
                // Начинаем свайп мышкой
                isSwipeActive = true;
                startTouchPosition = Input.mousePosition;
                currentTouchPosition = Input.mousePosition;
                
                // Проверяем и уничтожаем активный бустер (кроме бомбочки)
                CheckAndDestroyActiveBooster();
                
            }
        }
        
        // Обрабатываем перетаскивание мышкой
        if (isSwipeActive && Input.GetMouseButton(0))
        {
            Vector2 previousTouchPosition = currentTouchPosition;
            currentTouchPosition = Input.mousePosition;
            
            // Вычисляем смещение по оси X относительно предыдущей позиции
            float deltaX = (currentTouchPosition.x - previousTouchPosition.x) * swipeSensitivity;
            
            // Конвертируем в мировые координаты
            Vector3 worldDelta = mainCamera.ScreenToWorldPoint(new Vector3(deltaX, 0, 0)) - 
                                mainCamera.ScreenToWorldPoint(Vector3.zero);
            
            // Вычисляем новую позицию продукта
            Vector3 newPosition = currentFoodItem.transform.position;
            newPosition.x = Mathf.Clamp(newPosition.x + worldDelta.x, leftBoundary, rightBoundary);
            
            // Применяем новую позицию
            currentFoodItem.transform.position = newPosition;
        }
        
        // Обрабатываем отпускание мышки
        if (isSwipeActive && Input.GetMouseButtonUp(0))
        {
            // Завершаем свайп
            isSwipeActive = false;
            
            // Сбрасываем продукт
            DropFood();
            
        }
    }
    
    /// <summary>
    /// Обрабатывает начало касания
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // Проверяем камеру
        if (mainCamera == null)
        {
            return;
        }
        
        // Проверяем, есть ли активный бустер лапки - если да, игнорируем ввод
        if (IsPawBoosterActive())
        {
            return;
        }
        
        // Проверяем, есть ли продукт на точке спавна
        if (currentFoodItem == null || !currentFoodItem.inSpawnPoint || currentFoodItem.isDroped)
        {
            // Сбрасываем состояние свайпа если продукт исчез
            if (isSwipeActive)
            {
                isSwipeActive = false;
            }
            return;
        }
        
        // Проверяем, что нет активного свайпа от мышки
        if (isSwipeActive)
        {
            return;
        }
        
        // Проверяем, находится ли касание в пределах контейнера
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(eventData.position);
        if (!IsPositionInContainer(worldPosition))
        {
            return;
        }
        
        // Начинаем свайп
        isSwipeActive = true;
        startTouchPosition = eventData.position;
        currentTouchPosition = eventData.position;
        
        // Проверяем и уничтожаем активный бустер (кроме бомбочки)
        CheckAndDestroyActiveBooster();
        
    }
    
    /// <summary>
    /// Обрабатывает перетаскивание
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isSwipeActive || currentFoodItem == null)
        {
            return;
        }
        
        Vector2 previousTouchPosition = currentTouchPosition;
        currentTouchPosition = eventData.position;
        
        // Вычисляем смещение по оси X относительно предыдущей позиции
        float deltaX = (currentTouchPosition.x - previousTouchPosition.x) * swipeSensitivity;
        
        // Конвертируем в мировые координаты
        Vector3 worldDelta = mainCamera.ScreenToWorldPoint(new Vector3(deltaX, 0, 0)) - 
                            mainCamera.ScreenToWorldPoint(Vector3.zero);
        
        // Вычисляем новую позицию продукта
        Vector3 newPosition = currentFoodItem.transform.position;
        newPosition.x = Mathf.Clamp(newPosition.x + worldDelta.x, leftBoundary, rightBoundary);
        
        // Применяем новую позицию
        currentFoodItem.transform.position = newPosition;
    }
    
    /// <summary>
    /// Обрабатывает окончание касания
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSwipeActive || currentFoodItem == null)
        {
            return;
        }
        
        // Завершаем свайп
        isSwipeActive = false;
        
        // Сбрасываем продукт
        DropFood();
        
    }
    
    /// <summary>
    /// Проверяет, находится ли позиция в пределах контейнера
    /// </summary>
    private bool IsPositionInContainer(Vector3 worldPosition)
    {
        return worldPosition.x >= leftBoundary && 
               worldPosition.x <= rightBoundary &&
               worldPosition.y >= bottomBoundary && 
               worldPosition.y <= topBoundary;
    }
    
    /// <summary>
    /// Проверяет и уничтожает активный бустер при начале свайпа
    /// </summary>
    private void CheckAndDestroyActiveBooster()
    {
        if (currentFoodItem == null || !currentFoodItem.inSpawnPoint)
        {
            return;
        }
        
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager?.boosterManager == null)
        {
            return;
        }
        
        BoosterManager boosterManager = gameManager.boosterManager;
        if (!boosterManager.IsBoosterActive())
        {
            return;
        }
        
        string activeBoosterType = boosterManager.GetActiveBoosterType();
        if (activeBoosterType == "bomb")
        {
            return; // Бомбочка не уничтожается
        }
        
        boosterManager.DeactivateCurrentBooster();
    }
    
    /// <summary>
    /// Сбрасывает продукт (активирует падение)
    /// </summary>
    private void DropFood()
    {
        if (currentFoodItem == null)
        {
            return;
        }
        
        // Убеждаемся, что продукт находится в допустимых границах
        Vector3 currentPos = currentFoodItem.transform.position;
        if (currentPos.x < leftBoundary || currentPos.x > rightBoundary)
        {
            currentPos.x = Mathf.Clamp(currentPos.x, leftBoundary, rightBoundary);
            currentFoodItem.transform.position = currentPos;
        }
        
        // Вызываем метод сброса продукта
        currentFoodItem.DropFood();
    }
    
    /// <summary>
    /// Устанавливает границы контейнера
    /// </summary>
    public void SetContainerBounds(float left, float right, float top, float bottom)
    {
        leftBoundary = left;
        rightBoundary = right;
        topBoundary = top;
        bottomBoundary = bottom;
    }
    
    /// <summary>
    /// Проверяет, активен ли свайп
    /// </summary>
    public bool IsSwipeActive()
    {
        return isSwipeActive;
    }
    
    /// <summary>
    /// Принудительно завершает свайп
    /// </summary>
    public void ForceEndSwipe()
    {
        if (isSwipeActive)
        {
            isSwipeActive = false;
            if (currentFoodItem != null)
            {
                DropFood();
            }
        }
    }
    
    /// <summary>
    /// Принудительно сбрасывает состояние свайпа без сброса продукта
    /// </summary>
    public void ResetSwipeState()
    {
        isSwipeActive = false;
    }
    
    /// <summary>
    /// Проверяет, активен ли бустер лапки
    /// </summary>
    private bool IsPawBoosterActive()
    {
        // Ищем активный PawTargetController в сцене
        PawTargetController pawTarget = FindFirstObjectByType<PawTargetController>();
        if (pawTarget != null)
        {
            return true;
        }
        
        return false;
    }
    
    private void OnDestroy()
    {
        // Сбрасываем состояние при уничтожении объекта
        if (isSwipeActive)
        {
            isSwipeActive = false;
        }
    }
}
