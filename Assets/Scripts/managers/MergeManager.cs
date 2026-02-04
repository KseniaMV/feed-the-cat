using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Менеджер системы слияния продуктов
/// Обрабатывает логику слияния одинаковых продуктов и создание новых продуктов
/// </summary>
public class MergeManager : MonoBehaviour
{
    [Header("Настройки слияния")]
    [Tooltip("Радиус обнаружения продуктов для слияния")]
    public float mergeRadius = 6f;
    
    [Tooltip("Задержка перед слиянием (в секундах)")]
    public float mergeDelay = 0.1f;
    
    [Tooltip("Эффект слияния (опционально)")]
    public GameObject mergeEffect;
    
    [Tooltip("Звук слияния (опционально)")]
    public AudioClip mergeSound;
    
    [Header("Ссылки")]
    [Tooltip("Ссылка на спавнер продуктов")]
    public FoodSpawner foodSpawner;
    
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    [Tooltip("Родительский объект для эффектов слияния")]
    public Transform effectsParent;
    
    private bool isMerging = false;
    private List<FoodItem> itemsBeingMerged = new List<FoodItem>();
    
    
    /// <summary>
    /// Пытается объединить два продукта
    /// </summary>
    public void TryMerge(FoodItem item1, FoodItem item2)
    {
        if (isMerging || item1 == null || item2 == null || !CanMergeItems(item1, item2))
            return;
        
        StartCoroutine(PerformMergeCoroutine(item1, item2));
    }
    
    /// <summary>
    /// Проверяет, можно ли объединить два продукта
    /// </summary>
    private bool CanMergeItems(FoodItem item1, FoodItem item2)
    {
        if (item1?.foodData == null || item2?.foodData == null) return false;
        if (item1.foodData.foodType != item2.foodData.foodType) return false;
        if (item1.foodData.nextLevelFood == null) return false;
        if (itemsBeingMerged.Contains(item1) || itemsBeingMerged.Contains(item2)) return false;
        
        // Разрешаем слияние с мигающими продуктами - это может спасти от проигрыша
        // Слияние с мигающими продуктами возможно
        
        // Слияние возможно только между продуктами со статусом isDroped = true
        // Падающие продукты (isFalling = true) не участвуют в слиянии
        return item1.isDroped && item2.isDroped;
    }
    
    /// <summary>
    /// Выполняет слияние двух продуктов
    /// </summary>
    private IEnumerator PerformMergeCoroutine(FoodItem item1, FoodItem item2)
    {
        isMerging = true;
        
        // Добавляем продукты в список слияния
        itemsBeingMerged.Add(item1);
        itemsBeingMerged.Add(item2);
        
        // Останавливаем движение продуктов
        StopItemMovement(item1);
        StopItemMovement(item2);
        
        // Ждем задержку перед слиянием
        yield return new WaitForSeconds(mergeDelay);
        
        // Вычисляем позицию для нового продукта
        Vector3 mergePosition = (item1.transform.position + item2.transform.position) / 2f;
        
        // Проверяем и корректируем позицию, чтобы продукт не вышел за границы контейнера
        mergePosition = ClampPositionToContainer(mergePosition, item1.foodData.nextLevelFood);
        
        // Создаем эффект слияния
        CreateMergeEffect(mergePosition);
        
        // Создаем новый продукт более высокого уровня
        CreateMergedItem(item1.foodData.nextLevelFood, mergePosition, item1, item2);
        
        // При слиянии НЕ запускаем спавн - он уже запущен при столкновении
        // currentFood уже очищен в DropFood() при отпускании продукта
        
        // Уничтожаем старые продукты
        DestroyItem(item1);
        DestroyItem(item2);
        
        // Уведомляем GameManager о слиянии
        if (gameManager != null)
        {
            gameManager.OnMergeCompleted(item1.foodData, item1.foodData.nextLevelFood);
        }
        
        // Убираем продукты из списка слияния
        itemsBeingMerged.Remove(item1);
        itemsBeingMerged.Remove(item2);
        
        isMerging = false;
        
    }
    
    /// <summary>
    /// Останавливает движение продукта (но сохраняет физику)
    /// </summary>
    private void StopItemMovement(FoodItem item)
    {
        if (item != null && item.rigidBody != null)
        {
            item.rigidBody.linearVelocity = Vector2.zero;
            item.rigidBody.angularVelocity = 0f;
            // НЕ переводим в кинематический режим - оставляем физику активной
            // item.rigidBody.bodyType = RigidbodyType2D.Kinematic; // Убрано!
        }
    }
    
    /// <summary>
    /// Создает эффект слияния
    /// </summary>
    private void CreateMergeEffect(Vector3 position)
    {
        if (mergeEffect != null)
        {
            GameObject effect = Instantiate(mergeEffect, position, Quaternion.identity);
            
            if (effectsParent != null)
            {
                effect.transform.SetParent(effectsParent);
            }
            
            // Уничтожаем эффект через некоторое время
            Destroy(effect, 2f);
            
        }
        
        // Воспроизводим звук слияния
        if (mergeSound != null)
        {
            AudioSource.PlayClipAtPoint(mergeSound, position);
        }
    }
    
    /// <summary>
    /// Создает новый продукт более высокого уровня
    /// </summary>
    private void CreateMergedItem(FoodData nextLevelData, Vector3 position, FoodItem originalItem1, FoodItem originalItem2)
    {
        if (nextLevelData == null || nextLevelData.foodPrefab == null)
        {
            return;
        }
        
        // Создаем новый продукт
        GameObject newItem = Instantiate(nextLevelData.foodPrefab, position, Quaternion.identity);
        FoodItem newFoodItem = newItem.GetComponent<FoodItem>();
        
        if (newFoodItem != null)
        {
            // Настраиваем новый продукт
            newFoodItem.foodData = nextLevelData;
            newFoodItem.isDroped = true;
            newFoodItem.inSpawnPoint = false;
            newFoodItem.isDragble = false;
            newFoodItem.isFalling = false;
            
            
            // Переключаем в динамический режим и добавляем эффект слияния
            if (newFoodItem.rigidBody != null)
            {
                newFoodItem.rigidBody.bodyType = RigidbodyType2D.Dynamic;
                
                // Добавляем эффект "взрыва" при слиянии
                AddMergeExplosionEffect(newFoodItem);
            }
            
            // Настраиваем ссылки
            newFoodItem.foodSpawner = foodSpawner;
            newFoodItem.mergeManager = this;
            
            // Разблокируем новый продукт в спавнере
            foodSpawner?.UnlockFood(nextLevelData.foodType);
            
            
        }
        else
        {
            Destroy(newItem);
        }
    }
    
    /// <summary>
    /// Добавляет эффект "взрыва" при слиянии продуктов
    /// </summary>
    private void AddMergeExplosionEffect(FoodItem newFoodItem)
    {
        if (newFoodItem.rigidBody == null) return;
        
        // Уменьшаем эффект "взрыва" при слиянии - только небольшой вертикальный импульс
        Vector2 explosionForce = new Vector2(
            Random.Range(-0.5f, 0.5f), // Уменьшили горизонтальный импульс
            Random.Range(0.5f, 1.5f)  // Уменьшили вертикальный импульс
        );
        newFoodItem.rigidBody.AddForce(explosionForce, ForceMode2D.Impulse);
        
        // Уменьшаем вращение
        newFoodItem.rigidBody.angularVelocity = Random.Range(-50f, 50f);
        
        // Создаем цепную реакцию от места слияния - только при сильном слиянии
        CreateMergeChainReaction(newFoodItem.transform.position);
    }
    
    /// <summary>
    /// Создает цепную реакцию от места слияния
    /// </summary>
    private void CreateMergeChainReaction(Vector3 mergePosition)
    {
        Collider2D[] nearbyFoods = Physics2D.OverlapCircleAll(mergePosition, 1.2f); // Уменьшили радиус
        
        foreach (Collider2D collider in nearbyFoods)
        {
            FoodItem nearbyFood = collider.GetComponent<FoodItem>();
            if (nearbyFood != null && nearbyFood.rigidBody != null)
            {
                Vector2 direction = (nearbyFood.transform.position - mergePosition).normalized;
                float force = Random.Range(0.3f, 0.8f); // Уменьшили силу цепной реакции
                nearbyFood.rigidBody.AddForce(direction * force, ForceMode2D.Impulse);
            }
        }
    }
    
    /// <summary>
    /// Уничтожает продукт
    /// </summary>
    private void DestroyItem(FoodItem item)
    {
        if (item != null)
            Destroy(item.gameObject);
    }
    
    /// <summary>
    /// Проверяет, можно ли объединить продукты в данной позиции
    /// </summary>
    public bool CanMergeAtPosition(Vector3 position)
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, mergeRadius);
        Dictionary<FoodType, int> foodCounts = new Dictionary<FoodType, int>();
        
        foreach (Collider2D collider in nearbyColliders)
        {
            FoodItem foodItem = collider.GetComponent<FoodItem>();
            if (foodItem != null && foodItem.foodData != null && !itemsBeingMerged.Contains(foodItem))
            {
                FoodType foodType = foodItem.foodData.foodType;
                if (!foodCounts.ContainsKey(foodType))
                    foodCounts[foodType] = 0;
                foodCounts[foodType]++;
            }
        }
        
        // Проверяем, есть ли достаточно продуктов одного типа для слияния
        foreach (var kvp in foodCounts)
        {
            if (kvp.Value >= 2) // Нужно минимум 2 одинаковых продукта
            {
                // Проверяем, есть ли следующий уровень для этого типа
                FoodData foodData = foodSpawner?.GetFoodData(kvp.Key);
                if (foodData != null && foodData.nextLevelFood != null)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Получает список продуктов, которые можно объединить в данной позиции
    /// </summary>
    public List<FoodItem> GetMergeableItemsAtPosition(Vector3 position)
    {
        List<FoodItem> result = new List<FoodItem>();
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, mergeRadius);
        
        foreach (Collider2D collider in nearbyColliders)
        {
            FoodItem foodItem = collider.GetComponent<FoodItem>();
            if (foodItem != null && !itemsBeingMerged.Contains(foodItem))
            {
                result.Add(foodItem);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Ограничивает позицию продукта границами контейнера с учетом размера продукта
    /// </summary>
    private Vector3 ClampPositionToContainer(Vector3 position, FoodData foodData)
    {
        if (foodSpawner == null || foodData?.foodPrefab == null)
        {
            return position;
        }
        
        // Получаем границы контейнера
        foodSpawner.GetContainerBounds(out float leftBoundary, out float rightBoundary, out float topBoundary, out float bottomBoundary);
        
        // Получаем размер продукта из префаба
        GameObject prefab = foodData.foodPrefab;
        Collider2D prefabCollider = prefab.GetComponent<Collider2D>();
        
        float halfWidth = 0.3f; // Значение по умолчанию
        if (prefabCollider != null)
        {
            // Получаем размер коллайдера
            Bounds bounds = prefabCollider.bounds;
            halfWidth = bounds.size.x * 0.5f;
        }
        
        // Ограничиваем позицию с учетом размера продукта
        float clampedX = Mathf.Clamp(position.x, 
            leftBoundary + halfWidth,   // Левая граница + половина ширины продукта
            rightBoundary - halfWidth); // Правая граница - половина ширины продукта
        
        // Y координату оставляем без изменений (продукт должен упасть естественно)
        return new Vector3(clampedX, position.y, position.z);
    }
    
    /// <summary>
    /// Проверяет, есть ли физические границы контейнера
    /// </summary>
    private bool HasPhysicalBoundaries()
    {
        // Ищем объекты с тегом ContainerBoundary
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("ContainerBoundary");
        return boundaries.Length > 0;
    }
    
}
