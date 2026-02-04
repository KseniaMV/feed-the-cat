using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Контроллер лапки-прицела
/// Управляет перемещением прицела в пределах контейнера
/// </summary>
public class PawTargetController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ссылки")]
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    [Tooltip("Ссылка на менеджер бустеров")]
    public BoosterManager boosterManager;
    
    [Tooltip("Префаб анимированной лапки")]
    public GameObject pawAnimationPrefab;
    
    [Header("Границы игрового поля")]
    [Tooltip("Минимальная граница по оси X")]
    public float minX = -1.7f;
    
    [Tooltip("Максимальная граница по оси X")]
    public float maxX = 1.7f;
    
    [Tooltip("Минимальная граница по оси Y")]
    public float minY = -2.7f;
    
    [Tooltip("Максимальная граница по оси Y")]
    public float maxY = 2.1f;
    
    // Состояние
    private bool isActive = false;
    private bool isDragble = false;
    private Camera mainCamera;
    private FoodType selectedFoodType = FoodType.Sausage;
    
    private void Start()
    {
        InitializeTarget();
    }
    
    /// <summary>
    /// Инициализирует прицел
    /// </summary>
    private void InitializeTarget()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("PawTargetController: Камера не найдена!");
            return;
        }
        
        // Настраиваем коллайдер как триггер, чтобы не было физических столкновений
        SetupColliderAsTrigger();
        
        // Активируем прицел
        ActivateTarget();
        
    }
    
    /// <summary>
    /// Настраивает коллайдер лапки как триггер
    /// </summary>
    private void SetupColliderAsTrigger()
    {
        Collider2D pawCollider = GetComponent<Collider2D>();
        if (pawCollider != null)
        {
            pawCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("PawTargetController: Коллайдер не найден! Drag-события могут не работать.");
        }
        
        // УДАЛЯЕМ Rigidbody2D если он есть - он не нужен для drag-системы
        Rigidbody2D pawRigidbody = GetComponent<Rigidbody2D>();
        if (pawRigidbody != null)
        {
            Debug.LogWarning("PawTargetController: Найден Rigidbody2D - удаляем его (не нужен для лапки)");
            Destroy(pawRigidbody);
        }
    }
    
    /// <summary>
    /// Игнорирует триггеры границ (чтобы лапка не уничтожалась)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Игнорируем все триггеры - лапка не должна на них реагировать
    }
    
    /// <summary>
    /// Игнорирует столкновения (на случай если границы не триггеры)
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Игнорируем все столкновения - лапка не должна на них реагировать
    }
    
    /// <summary>
    /// Начинает перетаскивание лапки-прицела
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isActive) return;
        
        isDragble = true;
    }
    
    /// <summary>
    /// Обрабатывает перетаскивание лапки-прицела
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isActive || !isDragble) return;
        
        // Получаем позицию курсора в мировых координатах
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(eventData.position);
        worldPosition.z = 0;
        
        // Ограничиваем движение прицела в пределах игрового поля
        Vector3 clampedPosition = ClampPositionToGameField(worldPosition);
        
        // Применяем позицию (лапка точно следует за курсором)
        transform.position = clampedPosition;
    }
    
    /// <summary>
    /// Завершает перетаскивание и выбирает цель
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isActive || !isDragble) return;
        
        isDragble = false;
        
        // Выбираем цель под лапкой
        SelectTarget();
    }
    
    /// <summary>
    /// Ограничивает позицию в пределах игрового поля
    /// </summary>
    private Vector3 ClampPositionToGameField(Vector3 position)
    {
        // Ограничиваем позицию лапки в пределах заданных границ
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    /// <summary>
    /// Выбирает цель для уничтожения
    /// </summary>
    private void SelectTarget()
    {
        // Получаем коллайдер лапки
        CircleCollider2D pawCollider = GetComponent<CircleCollider2D>();
        
        Collider2D[] hitColliders;
        
        if (pawCollider != null)
        {
            // Используем радиус коллайдера лапки для более точного определения цели
            // Учитываем масштаб объекта
            float effectiveRadius = pawCollider.radius * transform.localScale.x;
            Vector2 centerPosition = (Vector2)transform.position + pawCollider.offset * transform.localScale.x;
            
            hitColliders = Physics2D.OverlapCircleAll(centerPosition, effectiveRadius);
            
        }
        else
        {
            // Fallback: используем точечную проверку, если коллайдер не найден
            hitColliders = Physics2D.OverlapPointAll(transform.position);
            Debug.LogWarning("PawTargetController: Коллайдер не найден, используется точечная проверка");
        }
        
        // Ищем продукт, игнорируя саму лапку
        FoodItem targetFoodItem = null;
        foreach (Collider2D collider in hitColliders)
        {
            // Пропускаем саму лапку
            if (collider.gameObject == gameObject) continue;
            
            FoodItem foodItem = collider.GetComponent<FoodItem>();
            if (foodItem != null && foodItem.foodData != null && foodItem.isDroped)
            {
                targetFoodItem = foodItem;
                break;
            }
        }
        
        if (targetFoodItem != null)
        {
            selectedFoodType = targetFoodItem.foodData.foodType;
            
            // Запускаем анимацию большой лапки
            StartPawAnimation();
        }
        else
        {
            Debug.LogWarning("PawTargetController: Под прицелом нет продукта в контейнере!");
            CancelTargeting();
        }
    }
    
    /// <summary>
    /// Запускает анимацию большой лапки
    /// </summary>
    private void StartPawAnimation()
    {
        if (pawAnimationPrefab == null)
        {
            Debug.LogError("PawTargetController: Префаб анимированной лапки не назначен!");
            CancelTargeting();
            return;
        }
        
        // Создаем анимированную лапку
        GameObject pawAnimationObject = Instantiate(pawAnimationPrefab, transform.position, Quaternion.identity);
        PawAnimationController pawAnimationController = pawAnimationObject.GetComponent<PawAnimationController>();
        
        if (pawAnimationController != null)
        {
            // Настраиваем ссылки
            pawAnimationController.gameManager = gameManager;
            pawAnimationController.boosterManager = boosterManager;
            pawAnimationController.selectedFoodType = selectedFoodType;
            
            // Запускаем анимацию
            pawAnimationController.StartAnimation();
        }
        else
        {
            Debug.LogError("PawTargetController: У префаба анимированной лапки нет компонента PawAnimationController!");
            Destroy(pawAnimationObject);
        }
        
        // Завершаем работу прицела
        FinishTargeting();
    }
    
    /// <summary>
    /// Отменяет выбор цели
    /// </summary>
    private void CancelTargeting()
    {
        FinishTargeting();
    }
    
    /// <summary>
    /// Завершает работу прицела
    /// </summary>
    private void FinishTargeting()
    {
        isActive = false;
        
        
        // Уведомляем BoosterManager о завершении
        if (boosterManager != null)
        {
            boosterManager.OnBoosterFinished();
        }
        
        // Уничтожаем прицел
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Активирует прицел
    /// </summary>
    public void ActivateTarget()
    {
        isActive = true;
    }
}
