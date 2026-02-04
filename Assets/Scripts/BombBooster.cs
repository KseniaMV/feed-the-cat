using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Компонент бомбочки-бустера
/// Уничтожает 3 продукта при соприкосновении с контейнером
/// </summary>
public class BombBooster : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Компоненты")]
    [Tooltip("SpriteRenderer для отображения спрайта бомбочки")]
    public SpriteRenderer spriteRenderer;
    
    [Tooltip("Rigidbody2D для физики бомбочки")]
    public Rigidbody2D rigidBody;
    
    [Tooltip("Collider2D для обнаружения столкновений")]
    public Collider2D bombCollider;
    
    [Header("Состояние")]
    [Tooltip("Находится ли бомбочка в процессе перетаскивания")]
    public bool isDragble = false;
    
    [Tooltip("Находится ли бомбочка в точке спавна")]
    public bool inSpawnPoint = true;
    
    [Tooltip("Была ли бомбочка сброшена (начала падать)")]
    public bool isDroped = false;
    
    [Tooltip("Активирована ли бомбочка")]
    public bool isActivated = false;
    
    [Header("Настройки перетаскивания")]
    [Tooltip("Границы игрового поля для ограничения движения по оси X")]
    private const float LEFT_BOUNDARY = -1.7f;
    private const float RIGHT_BOUNDARY = 1.7f;
    
    [Header("Настройки взрыва")]
    [Tooltip("Радиус обнаружения продуктов для взрыва")]
    public float explosionRadius = 2f;
    
    [Tooltip("Максимальное количество продуктов для уничтожения")]
    public int maxDestroyedItems = 3;
    
    [Tooltip("Эффект взрыва")]
    public GameObject explosionEffect;
    
    [Tooltip("Звук взрыва")]
    public AudioClip explosionSound;
    
    [Header("Ссылки")]
    [Tooltip("Ссылка на спавнер продуктов")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    
    private Camera mainCamera;
    private bool canDrag = true;
    
    // Список продуктов, которые будут уничтожены
    private List<FoodItem> itemsToDestroy = new List<FoodItem>();
    
    private void Start()
    {
        InitializeBombBooster();
    }
    
    /// <summary>
    /// Инициализирует компонент бомбочки
    /// </summary>
    private void InitializeBombBooster()
    {
        CacheCamera();
        CacheGameManager();
        SetupPhysics();
        SetupSprite();
        
        // Тратим бомбочку при спавне
        SpendBombBooster();
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
            gameManager = foodSpawner.gameManager;
        }
        else
        {
            // Если не найден, ищем в сцене
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }
    
    /// <summary>
    /// Настраивает физику бомбочки
    /// </summary>
    private void SetupPhysics()
    {
        if (rigidBody != null)
        {
            rigidBody.mass = 1.0f;
            rigidBody.linearDamping = 0.5f;
            rigidBody.angularDamping = 2f;
            rigidBody.bodyType = RigidbodyType2D.Kinematic; // Начинаем в кинематическом режиме
        }
    }
    
    /// <summary>
    /// Начинает перетаскивание бомбочки
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag || isDroped || !inSpawnPoint) return;
        
        isDragble = true;
        canDrag = false;
    }
    
    /// <summary>
    /// Обрабатывает перетаскивание бомбочки
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
        
        // Убеждаемся, что бомбочка находится в допустимых границах
        Vector3 currentPos = transform.position;
        if (currentPos.x < LEFT_BOUNDARY || currentPos.x > RIGHT_BOUNDARY)
        {
            // Если бомбочка за границами, возвращаем её в допустимую зону
            currentPos.x = Mathf.Clamp(currentPos.x, LEFT_BOUNDARY, RIGHT_BOUNDARY);
            transform.position = currentPos;
        }
        
        DropBomb();
    }
    
    /// <summary>
    /// Обрабатывает клик по бомбочке
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inSpawnPoint && !isDroped && canDrag)
        {
            DropBomb();
        }
    }
    
    /// <summary>
    /// Сбрасывает бомбочку (активирует падение)
    /// </summary>
    private void DropBomb()
    {
        isDragble = false;
        isDroped = true;
        inSpawnPoint = false;
        canDrag = false;
        
        // Очищаем currentFood в спавнере при отпускании бомбочки
        if (foodSpawner != null && foodSpawner.currentFood == null)
        {
            // Бомбочка не является продуктом, но освобождаем спавнер
        }
        
        if (rigidBody != null)
        {
            rigidBody.bodyType = RigidbodyType2D.Dynamic;
            
            // Принудительно сбрасываем скорость, чтобы избежать застревания
            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
            
            // Добавляем небольшую силу вниз для гарантированного падения
            rigidBody.AddForce(Vector2.down * 2f, ForceMode2D.Impulse);
        }
    }
    
    /// <summary>
    /// Обрабатывает столкновения с другими объектами
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что столкновение происходит с продуктом или дном
        bool isFoodCollision = collision.gameObject.layer == LayerMask.NameToLayer("Food");
        bool isGroundCollision = collision.gameObject.CompareTag("Ground");
        
        // Если это не продукт и не дно, игнорируем столкновение
        if (!isFoodCollision && !isGroundCollision)
        {
            return;
        }
        
        // Активируем бомбочку только при соприкосновении с продуктами
        if (!isActivated && isFoodCollision)
        {
            isActivated = true;
            StartCoroutine(ExplodeWithDelay());
        }
        // Если бомбочка упала на дно без соприкосновения с продуктами - просто взрывается без эффекта
        else if (!isActivated && isGroundCollision)
        {
            isActivated = true;
            StartCoroutine(ExplodeWithoutEffectWithDelay());
        }
    }
    
    /// <summary>
    /// Взрывает бомбочку и уничтожает продукты
    /// </summary>
    private void Explode()
    {
        // Уничтожаем только продукты, с которыми бомбочка соприкоснулась
        DestroyContactedItems();
        
        // Создаем эффект взрыва
        CreateExplosionEffect();
        
        // Воспроизводим звук взрыва
        PlayExplosionSound();
        
        // Уничтожаем саму бомбочку
        Destroy(gameObject);
        
        // Запускаем спавн нового продукта
        TriggerNewProductSpawn();
    }
    
    /// <summary>
    /// Уничтожает только продукты, с которыми бомбочка соприкоснулась
    /// </summary>
    private void DestroyContactedItems()
    {
        // Получаем все коллайдеры, с которыми бомбочка соприкасается
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = false;
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = LayerMask.GetMask("Food");
        
        List<Collider2D> contactedColliders = new List<Collider2D>();
        int contactCount = bombCollider.Overlap(contactFilter, contactedColliders);
        
        
        // Ограничиваем количество уничтожаемых продуктов до 3
        int itemsToDestroy = Mathf.Min(contactCount, maxDestroyedItems);
        
        for (int i = 0; i < itemsToDestroy; i++)
        {
            FoodItem foodItem = contactedColliders[i].GetComponent<FoodItem>();
            if (foodItem != null && foodItem.isDroped)
            {
                // Проверяем, произойдет ли слияние после уничтожения
                CheckForMergesAfterDestruction(foodItem);
                
                // Уничтожаем продукт
                foodItem.DestroyFood();
                
            }
        }
    }
    
    
    /// <summary>
    /// Проверяет, произойдет ли слияние после уничтожения продукта
    /// </summary>
    private void CheckForMergesAfterDestruction(FoodItem destroyedItem)
    {
        if (destroyedItem == null || destroyedItem.foodData == null) return;
        
        // Находим соседние продукты того же типа
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(destroyedItem.transform.position, 1.5f);
        
        List<FoodItem> sameTypeItems = new List<FoodItem>();
        
        foreach (Collider2D collider in nearbyColliders)
        {
            FoodItem foodItem = collider.GetComponent<FoodItem>();
            if (foodItem != null && 
                foodItem != destroyedItem && 
                foodItem.isDroped &&
                foodItem.foodData != null &&
                foodItem.foodData.foodType == destroyedItem.foodData.foodType)
            {
                sameTypeItems.Add(foodItem);
            }
        }
        
        // Если есть два продукта одного типа, они могут слиться
        if (sameTypeItems.Count >= 2)
        {
            // Выбираем первые два продукта для слияния
            FoodItem item1 = sameTypeItems[0];
            FoodItem item2 = sameTypeItems[1];
            
            // Проверяем, можно ли их объединить
            if (item1.CanMergeWith(item2))
            {
                // Уведомляем менеджер слияния
                MergeManager mergeManager = FindFirstObjectByType<MergeManager>();
                if (mergeManager != null)
                {
                    mergeManager.TryMerge(item1, item2);
                }
            }
        }
    }
    
    /// <summary>
    /// Создает эффект взрыва
    /// </summary>
    private void CreateExplosionEffect()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            
            // Уничтожаем эффект через некоторое время
            Destroy(effect, 2f);
        }
    }
    
    /// <summary>
    /// Воспроизводит звук взрыва
    /// </summary>
    private void PlayExplosionSound()
    {
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }
    }
    
    /// <summary>
    /// Запускает спавн нового продукта после взрыва
    /// </summary>
    private void TriggerNewProductSpawn()
    {
        if (foodSpawner != null && gameManager != null && gameManager.isGameActive)
        {
            foodSpawner.SpawnNewFood();
        }
    }
    
    /// <summary>
    /// Проверяет, можно ли объединить с другим продуктом (для совместимости с FoodItem)
    /// </summary>
    public bool CanMergeWith(FoodItem otherFood)
    {
        // Бомбочка не может сливаться с продуктами
        return false;
    }
    
    /// <summary>
    /// Настраивает спрайт бомбочки
    /// </summary>
    private void SetupSprite()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer != null)
        {
            // Устанавливаем цвет для видимости
            spriteRenderer.color = Color.white;
        }
        
        // Исправляем только Z-позицию и Order in Layer
        FixBombPosition();
    }
    
    /// <summary>
    /// Исправляет позицию бомбочки для видимости
    /// </summary>
    private void FixBombPosition()
    {
        // Исправляем Z-позицию, чтобы бомбочка была поверх других элементов
        Vector3 position = transform.position;
        position.z = -1f; // Ближе к камере
        transform.position = position;
        
        // Устанавливаем правильный Order in Layer для SpriteRenderer
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10; // Поверх других элементов
        }
        
    }
    
    /// <summary>
    /// Взрывает бомбочку без уничтожения продуктов (когда нет соприкосновения с продуктами)
    /// </summary>
    private void ExplodeWithoutEffect()
    {
        // Создаем эффект взрыва
        CreateExplosionEffect();
        
        // Воспроизводим звук взрыва
        PlayExplosionSound();
        
        // Уничтожаем саму бомбочку
        Destroy(gameObject);
        
        // Запускаем спавн нового продукта
        TriggerNewProductSpawn();
        
    }
    
    /// <summary>
    /// Взрывает бомбочку с задержкой
    /// </summary>
    private System.Collections.IEnumerator ExplodeWithDelay()
    {
        
        // Ждем 0.5 секунды
        yield return new WaitForSeconds(0.5f);
        
        // Взрываем бомбочку
        Explode();
    }
    
    /// <summary>
    /// Взрывает бомбочку без эффекта с задержкой
    /// </summary>
    private System.Collections.IEnumerator ExplodeWithoutEffectWithDelay()
    {
        
        // Ждем 0.5 секунды
        yield return new WaitForSeconds(0.5f);
        
        // Взрываем бомбочку без эффекта
        ExplodeWithoutEffect();
    }
    
    /// <summary>
    /// Тратит бомбочку при спавне
    /// </summary>
    private void SpendBombBooster()
    {
        // Тратим бомбочку через GameDataManager
        bool success = GameDataManager.SpendBoosters("bomb", 1);
        if (success)
        {
            
            // Обновляем UI бустеров
            UpdateBoosterUI();
        }
        else
        {
            Debug.LogError("BombBooster: Не удалось потратить бомбочку!");
        }
    }
    
    /// <summary>
    /// Обновляет UI бустеров
    /// </summary>
    private void UpdateBoosterUI()
    {
        // Ищем BoosterManager в сцене
        BoosterManager boosterManager = FindFirstObjectByType<BoosterManager>();
        if (boosterManager != null)
        {
            boosterManager.UpdateBoosterUI();
        }
        else
        {
            Debug.LogWarning("BombBooster: BoosterManager не найден в сцене!");
        }
    }
    
}
