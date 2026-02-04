using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Компонент для управления отдельным продуктом питания
/// Обрабатывает перемещение, сброс и взаимодействие с другими продуктами
/// 
/// Система проигрыша использует проверку позиции продуктов в контейнере.
/// </summary>
public class FoodItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Данные продукта")]
    [Tooltip("Данные о продукте (тип, спрайт, награды)")]
    public FoodData foodData;
    
    [Header("Компоненты")]
    [Tooltip("SpriteRenderer для отображения спрайта продукта")]
    public SpriteRenderer spriteRenderer;
    
    [Tooltip("Rigidbody2D для физики продукта")]
    public Rigidbody2D rigidBody;
    
    [Tooltip("Collider2D для обнаружения столкновений")]
    public Collider2D foodCollider;
    
    [Tooltip("Animator для анимаций продукта (опционально, для будущей анимации мигания)")]
    public Animator animator;
    
    [Header("Состояние продукта (согласно README.md)")]
    [Tooltip("Продукт на точке спавна имеет статус inSpawnPoint = true и isDroped = false и isFalling = false и isDragble = false")]
    public bool inSpawnPoint = true;
    
    [Tooltip("Продукт, который перемещается игроком имеет статус isDragble = true и inSpawnPoint = true и isDroped = false и isFalling = false")]
    public bool isDragble = false;
    
    [Tooltip("Продукт, который игрок отпустил после перемещения имеет статус isFalling = true и isDragble = false и inSpawnPoint = false isDroped = false")]
    public bool isFalling = false;
    
    [Tooltip("Продукт, который был коснулся другого продукта или дна контейнера имеет статус isDroped = true и isFalling = false и isDragble = false и inSpawnPoint = false")]
    public bool isDroped = false;
    
    [Tooltip("Флаг для предотвращения множественного вызова спавна")]
    private bool hasTriggeredSpawn = false;
    
    [Tooltip("Исходный цвет продукта")]
    private Color originalColor = Color.white;
    
    [Tooltip("Находится ли продукт в зоне предупреждения")]
    public bool isInWarningZone = false;
    
    
    
    [Header("Настройки перетаскивания")]
    private const float LEFT_BOUNDARY = -1.7f;
    private const float RIGHT_BOUNDARY = 1.7f;
    
    [Tooltip("Ссылка на спавнер продуктов")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на менеджер слияния")]
    public MergeManager mergeManager;
    
    [Tooltip("Кэшированная ссылка на GameManager для оптимизации")]
    private GameManager cachedGameManager;
    
    private Camera mainCamera;
    private bool canDrag = true;
    
    private void Start()
    {
        InitializeFoodItem();
        
        // Проверяем компоненты для отладки триггеров
        Collider2D foodCollider = GetComponent<Collider2D>();
        if (foodCollider == null)
        {
            Debug.LogError($"FoodItem: {foodData?.foodType} не имеет Collider2D!");
        }
    }
    
    private void Update()
    {
        // Обновление продукта (система проигрыша удалена)
    }
    
    
    
    
    /// <summary>
    /// Инициализирует компонент продукта
    /// </summary>
    private void InitializeFoodItem()
    {
        CacheCamera();
        CacheGameManager();
        SetupPhysicsByType();
        SaveOriginalColor();
    }
    
    
    /// <summary>
    /// Кэширует ссылку на камеру для оптимизации
    /// </summary>
    private void CacheCamera()
    {
        mainCamera = Camera.main ?? FindFirstObjectByType<Camera>();
    }
    
    /// <summary>
    /// Кэширует ссылку на GameManager для оптимизации
    /// </summary>
    private void CacheGameManager()
    {
        // Сначала пробуем через foodSpawner
        if (foodSpawner != null && foodSpawner.gameManager != null)
        {
            cachedGameManager = foodSpawner.gameManager;
        }
        else
        {
            // Если не найден, ищем в сцене
            cachedGameManager = FindFirstObjectByType<GameManager>();
        }
    }
    
    /// <summary>
    /// Получает кэшированную ссылку на GameManager, обновляя её при необходимости
    /// </summary>
    private GameManager GetGameManager()
    {
        if (cachedGameManager == null)
        {
            CacheGameManager();
        }
        return cachedGameManager;
    }
    
    /// <summary>
    /// Проверяет, активна ли игра (оптимизированная проверка)
    /// </summary>
    private bool IsGameActive()
    {
        GameManager gameManager = GetGameManager();
        return gameManager != null && gameManager.isGameActive;
    }
    
    /// <summary>
    /// Настраивает физику в зависимости от типа продукта
    /// </summary>
    public void SetupPhysicsByType()
    {
        if (rigidBody == null || foodData == null) return;
        
        switch (foodData.foodType)
        {
            case FoodType.Sausage:
                rigidBody.mass = 0.8f;
                rigidBody.linearDamping = 0.5f;
                rigidBody.angularDamping = 2f;
                break;
                
            case FoodType.Eggs:
                rigidBody.mass = 0.6f;
                rigidBody.linearDamping = 0.3f;
                rigidBody.angularDamping = 1f;
                break;
                
            case FoodType.Sandwich:
                rigidBody.mass = 1.0f;
                rigidBody.linearDamping = 0.7f;
                rigidBody.angularDamping = 3f;
                break;
                
            case FoodType.Meatball:
                rigidBody.mass = 1.2f;
                rigidBody.linearDamping = 0.8f;
                rigidBody.angularDamping = 4f;
                break;
                
            default:
                // Настройки по умолчанию для других продуктов
                rigidBody.mass = 1.0f;
                rigidBody.linearDamping = 0.5f;
                rigidBody.angularDamping = 2f;
                break;
        }
    }
    
    /// <summary>
    /// Начинает перетаскивание продукта
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag || isDroped || !inSpawnPoint) return;
        
        // Проверяем и уничтожаем активный бустер (кроме бомбочки)
        CheckAndDestroyActiveBooster();
        
        // Продукт, который перемещается игроком имеет статус isDragble = true и inSpawnPoint = true и isDroped = false и isFalling = false
        isDragble = true;
        canDrag = false;
    }
    
    /// <summary>
    /// Обрабатывает перетаскивание продукта
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragble || isDroped) return;
        
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(eventData.position);
        worldPosition.z = transform.position.z;
        
        // Ограничиваем горизонтальное перемещение
        worldPosition.x = Mathf.Clamp(worldPosition.x, LEFT_BOUNDARY, RIGHT_BOUNDARY);
        
        // Не изменяем Y координату во время перетаскивания
        worldPosition.y = transform.position.y;
        
        transform.position = worldPosition;
    }
    
    /// <summary>
    /// Завершает перетаскивание и активирует падение
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragble || isDroped) return;
        
        // Убеждаемся, что продукт находится в допустимых границах
        Vector3 currentPos = transform.position;
        if (currentPos.x < LEFT_BOUNDARY || currentPos.x > RIGHT_BOUNDARY)
        {
            // Если продукт за границами, возвращаем его в допустимую зону
            currentPos.x = Mathf.Clamp(currentPos.x, LEFT_BOUNDARY, RIGHT_BOUNDARY);
            transform.position = currentPos;
        }
        
        DropFood();
    }
    
    /// <summary>
    /// Обрабатывает клик по продукту
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inSpawnPoint && !isDroped && canDrag)
        {
            // Проверяем и уничтожаем активный бустер (кроме бомбочки)
            CheckAndDestroyActiveBooster();
            
            DropFood();
        }
    }
    
    /// <summary>
    /// Проверяет и уничтожает активный бустер при клике/перетаскивании продукта на точке спавна
    /// Исключение: бомбочка, так как она спавнится на месте спавна продукции
    /// </summary>
    private void CheckAndDestroyActiveBooster()
    {
        if (!inSpawnPoint) return;
        
        GameManager gameManager = GetGameManager();
        if (gameManager?.boosterManager == null) return;
        
        BoosterManager boosterManager = gameManager.boosterManager;
        if (!boosterManager.IsBoosterActive()) return;
        
        string activeBoosterType = boosterManager.GetActiveBoosterType();
        if (activeBoosterType == "bomb") return;
        
        boosterManager.DeactivateCurrentBooster();
    }
    
    /// <summary>
    /// Сбрасывает продукт (активирует падение)
    /// </summary>
    public void DropFood()
    {
        // Продукт, который игрок отпустил после перемещения имеет статус isFalling = true и isDragble = false и inSpawnPoint = false isDroped = false
        isDragble = false;
        isFalling = true;
        inSpawnPoint = false;
        canDrag = false;
        
        if (foodSpawner?.currentFood == this)
        {
            foodSpawner.currentFood = null;
        }
        
        if (rigidBody == null) return;
        
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.AddForce(Vector2.down * 2f, ForceMode2D.Impulse);
        
        AddRealisticDropPhysics();
    }
    
    /// <summary>
    /// Принудительно заставляет продукт упасть (используется при застревании)
    /// </summary>
    public void ForceFall()
    {
        if (rigidBody == null || isDroped) return;
        
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.angularVelocity = 0f;
        rigidBody.AddForce(Vector2.down * 5f, ForceMode2D.Impulse);
        
        isDragble = false;
        isFalling = true;
        inSpawnPoint = false;
        canDrag = false;
        
        if (foodSpawner?.currentFood == this)
        {
            foodSpawner.currentFood = null;
        }
    }
    
    /// <summary>
    /// Добавляет реалистичную физику при падении продукта
    /// </summary>
    private void AddRealisticDropPhysics()
    {
        if (rigidBody == null) return;
        
        rigidBody.angularVelocity = Random.Range(-30f, 30f);
        
        if (rigidBody.linearVelocity.y > -1f)
        {
            rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, -2f);
        }
    }
    
    /// <summary>
    /// Обрабатывает столкновения с другими объектами
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isFoodCollision = collision.gameObject.layer == LayerMask.NameToLayer("Food");
        bool isGroundCollision = collision.gameObject.CompareTag("Ground");
        
        if (!isFoodCollision && !isGroundCollision) return;
        
        if (!isDroped)
        {
            // Продукт, который был коснулся другого продукта или дна контейнера имеет статус isDroped = true и isFalling = false и isDragble = false и inSpawnPoint = false
            isDroped = true;
            isFalling = false;
            isDragble = false;
            inSpawnPoint = false;
            
            // ВСЕГДА очищаем ссылку на текущий продукт и запускаем спавн
            if (foodSpawner?.currentFood == this)
            {
                foodSpawner.currentFood = null;
            }
            TriggerNewProductSpawn();
        }
        
        HandleSoftLanding(collision);
        
        if (isFoodCollision)
        {
            FoodItem otherFood = collision.gameObject.GetComponent<FoodItem>();
            if (otherFood != null)
            {
                // Обычная обработка столкновений - слияние с мигающими продуктами разрешено
                HandleFoodCollision(otherFood);
            }
        }
    }
    
    /// <summary>
    /// Обрабатывает вход в триггер (устаревший метод - больше не используется)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Логика триггеров перенесена в GameManager.CheckProductsInContainer()
        // Метод оставлен для совместимости
    }
    
    /// <summary>
    /// Обрабатывает выход из триггера (устаревший метод - больше не используется)
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Логика триггеров перенесена в GameManager.CheckProductsInContainer()
        // Метод оставлен для совместимости
    }
    
    
    
    
    /// <summary>
    /// Обрабатывает эффект мягкого приземления
    /// </summary>
    private void HandleSoftLanding(Collision2D collision)
    {
        if (rigidBody == null || collision.contacts.Length == 0) return;
        
        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce <= 3f) return;
        
        Vector2 bounceForce = -collision.relativeVelocity * 0.1f;
        rigidBody.AddForce(bounceForce, ForceMode2D.Impulse);
        
        if (impactForce > 5f)
        {
            TransferMomentumToNearbyFoods(collision.contacts[0].point);
        }
    }
    
    /// <summary>
    /// Передает импульс соседним продуктам (цепная реакция)
    /// </summary>
    private void TransferMomentumToNearbyFoods(Vector3 impactPoint)
    {
        Collider2D[] nearbyFoods = Physics2D.OverlapCircleAll(impactPoint, 1.0f);
        
        foreach (Collider2D collider in nearbyFoods)
        {
            FoodItem nearbyFood = collider.GetComponent<FoodItem>();
            if (nearbyFood == null || nearbyFood == this || nearbyFood.rigidBody == null) continue;
            
            Vector2 direction = (nearbyFood.transform.position - impactPoint).normalized;
            float force = Random.Range(0.2f, 0.6f);
            nearbyFood.rigidBody.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// Обрабатывает столкновение с другим продуктом
    /// </summary>
    private void HandleFoodCollision(FoodItem otherFood)
    {
        if (otherFood?.foodData == null || foodData == null) return;
        
        if (CanMergeWith(otherFood))
        {
            mergeManager?.TryMerge(this, otherFood);
        }
    }
    
    /// <summary>
    /// Обрабатывает столкновение с мигающим продуктом - слияние невозможно
    /// </summary>
    private void HandleFlashingProductCollision(FoodItem otherFood)
    {
        
        // Применяем сильный отскок только к продукту на спавне
        if (inSpawnPoint && rigidBody != null)
        {
            Vector2 bounceDirection = (transform.position - otherFood.transform.position).normalized;
            float bounceForce = 8f; // Сильная сила отскока
            rigidBody.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
        }
        // Для продуктов в контейнере - просто не даем им слиться (CanMergeWith уже запрещает)
    }
    
    /// <summary>
    /// Проверяет, можно ли объединить с другим продуктом
    /// </summary>
    public bool CanMergeWith(FoodItem otherFood)
    {
        if (otherFood?.foodData == null || foodData?.nextLevelFood == null) return false;
        
        bool sameTag = gameObject.CompareTag(otherFood.gameObject.tag);
        bool sameType = foodData.foodType == otherFood.foodData.foodType;
        
        // Слияние возможно только между продуктами со статусом isDroped = true
        // Падающие продукты (isFalling = true) не участвуют в слиянии
        bool canMerge = isDroped && otherFood.isDroped;
        
        return sameTag && sameType && canMerge;
    }
    
    /// <summary>
    /// Запускает спавн нового продукта при столкновении
    /// </summary>
    private void TriggerNewProductSpawn()
    {
        
        // Спавн запускается для всех упавших продуктов (isDroped = true)
        if (!isDroped || foodSpawner == null) 
        {
            return;
        }
        if (isDragble || inSpawnPoint) 
        {
            return;
        }
        if (hasTriggeredSpawn) 
        {
            return;
        }
        if (foodSpawner.currentFood != null || foodSpawner.isSpawning) 
        {
            return;
        }
        
        if (!IsGameActive()) 
        {
            return;
        }
        
        hasTriggeredSpawn = true;
        foodSpawner.SpawnNewFood();
    }
    
    /// <summary>
    /// Уничтожает продукт (используется при слиянии)
    /// </summary>
    public void DestroyFood()
    {
        if (foodSpawner?.currentFood == this)
        {
            foodSpawner.currentFood = null;
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Создает новый продукт более высокого уровня
    /// </summary>
    public void CreateMergedFood(Vector3 position)
    {
        if (foodData?.nextLevelFood?.foodPrefab == null) return;
        
        GameObject newFoodObject = Instantiate(foodData.nextLevelFood.foodPrefab, position, Quaternion.identity);
        FoodItem newFoodItem = newFoodObject.GetComponent<FoodItem>();
        
        if (newFoodItem != null)
        {
            SetupMergedFood(newFoodItem);
        }
    }
    
    /// <summary>
    /// Настраивает параметры объединенного продукта
    /// </summary>
    private void SetupMergedFood(FoodItem newFoodItem)
    {
        newFoodItem.foodData = foodData.nextLevelFood;
        newFoodItem.isDroped = true;
        newFoodItem.inSpawnPoint = false;
        newFoodItem.isDragble = false;
        newFoodItem.isFalling = false;
        
        if (newFoodItem.rigidBody != null)
        {
            newFoodItem.rigidBody.bodyType = RigidbodyType2D.Dynamic;
            newFoodItem.SetupPhysicsByType();
        }
        
        // Сохраняем исходный цвет для нового продукта
        newFoodItem.SaveOriginalColor();
    }
    
    /// <summary>
    /// Сохраняет исходный цвет продукта
    /// </summary>
    private void SaveOriginalColor()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    /// <summary>
    /// Устанавливает цвет продукта (красный или исходный) - оптимизировано для избежания избыточных вызовов
    /// </summary>
    public void SetWarningColor(bool isWarning)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"FoodItem: Продукт {foodData?.foodType} не имеет SpriteRenderer! Не можем изменить цвет.");
            return;
        }
        
        // Оптимизация: меняем цвет только если состояние действительно изменилось
        if (isWarning && !isInWarningZone)
        {
            spriteRenderer.color = Color.red;
            isInWarningZone = true;
        }
        else if (!isWarning && isInWarningZone)
        {
            spriteRenderer.color = originalColor;
            isInWarningZone = false;
        }
        // Если состояние не изменилось - ничего не делаем (оптимизация)
    }
}



