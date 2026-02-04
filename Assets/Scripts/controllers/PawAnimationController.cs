using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Контроллер анимированной лапки
/// Управляет анимацией и уничтожением продуктов
/// </summary>
public class PawAnimationController : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Ссылка на менеджер игры")]
    public GameManager gameManager;
    
    [Tooltip("Ссылка на менеджер бустеров")]
    public BoosterManager boosterManager;
    
    [Tooltip("Выбранный тип продукта для уничтожения")]
    public FoodType selectedFoodType = FoodType.Sausage;
    
    private void Start()
    {
    }
    
    /// <summary>
    /// Запускает анимацию
    /// </summary>
    public void StartAnimation()
    {
        // Анимация управляется через Animation Events в префабе
    }
    
    /// <summary>
    /// Вызывается из Animation Event - уничтожает продукты выбранного типа
    /// </summary>
    public void OnPawAnimationComplete()
    {
        
        // Уничтожаем все продукты выбранного типа
        DestroyFoodsOfType(selectedFoodType);
    }
    
    /// <summary>
    /// Вызывается из Animation Event - завершает анимацию
    /// </summary>
    public void OnPawAnimationFinished()
    {
        
        // Уничтожаем объект анимации
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Уничтожает все продукты выбранного типа
    /// </summary>
    private void DestroyFoodsOfType(FoodType foodType)
    {
        // Находим все продукты в сцене
        FoodItem[] allFoods = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        
        int destroyedCount = 0;
        
        foreach (FoodItem foodItem in allFoods)
        {
            if (foodItem != null && foodItem.foodData != null && foodItem.foodData.foodType == foodType)
            {
                // Проверяем, что продукт в контейнере (не на спавне)
                if (foodItem.isDroped)
                {
                    // Уничтожаем продукт
                    Destroy(foodItem.gameObject);
                    destroyedCount++;
                }
            }
        }
        
        
        // Проверяем слияния после уничтожения
        if (gameManager != null && gameManager.mergeManager != null)
        {
            // Запускаем проверку слияний через корутину
            StartCoroutine(CheckForMergesAfterDelay());
        }
    }
    
    /// <summary>
    /// Проверяет слияния после задержки
    /// </summary>
    private System.Collections.IEnumerator CheckForMergesAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Небольшая задержка для стабилизации
        
        // Находим все продукты и проверяем возможные слияния
        FoodItem[] allFoods = FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
        
        foreach (FoodItem foodItem in allFoods)
        {
            if (foodItem != null && foodItem.isDroped)
            {
                // Проверяем, можно ли слить этот продукт
                if (gameManager.mergeManager.CanMergeAtPosition(foodItem.transform.position))
                {
                    List<FoodItem> mergeableItems = gameManager.mergeManager.GetMergeableItemsAtPosition(foodItem.transform.position);
                    if (mergeableItems.Count >= 2)
                    {
                        // Находим пару для слияния
                        for (int i = 0; i < mergeableItems.Count - 1; i++)
                        {
                            for (int j = i + 1; j < mergeableItems.Count; j++)
                            {
                                if (mergeableItems[i].foodData != null && mergeableItems[j].foodData != null &&
                                    mergeableItems[i].foodData.foodType == mergeableItems[j].foodData.foodType)
                                {
                                    gameManager.mergeManager.TryMerge(mergeableItems[i], mergeableItems[j]);
                                    yield return new WaitForSeconds(0.1f); // Задержка между слияниями
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
