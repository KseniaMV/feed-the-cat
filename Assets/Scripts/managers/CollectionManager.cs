using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Данные коллекции продуктов
/// </summary>
[System.Serializable]
public class CollectionData
{
    [Tooltip("Название коллекции")]
    public string collectionName;
    
    [Tooltip("Список продуктов в коллекции")]
    public FoodData[] foodItems;
    
    [Tooltip("Описание коллекции")]
    public string description;
}

/// <summary>
/// Менеджер системы коллекций
/// Управляет отображением и состоянием коллекций продуктов
/// </summary>
public class CollectionManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Панель списка коллекций")]
    public GameObject collectionListPanel;
    
    [Tooltip("Контейнер для элементов коллекции")]
    public Transform collectionItemList;
    
    [Tooltip("Префаб элемента коллекции")]
    public GameObject collectionItemPrefab;
    
    [Tooltip("Текст названия коллекции")]
    public TMPro.TextMeshProUGUI collectionTitleText;
    
    [Header("Collections Data")]
    [Tooltip("Базовая коллекция продуктов")]
    public CollectionData basicCollection;
    
    [Tooltip("Данные состояний кота (ScriptableObject)")]
    public CatStateData[] catStatesData;
    
    [Tooltip("Ссылка на менеджер состояний кота")]
    public CatStateManager catStateManager;
    
    public GameManager gameManager;
    private List<CollectionItemUI> currentCollectionItems = new List<CollectionItemUI>();
    
    private void Start()
    {
        // Инициализируем ссылку на GameManager, если она не установлена
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }
    
    /// <summary>
    /// Показывает коллекцию по имени
    /// </summary>
    /// <param name="collectionName">Имя коллекции (basic, catStates)</param>
    public void ShowCollection(string collectionName)
    {
        if (collectionListPanel == null || collectionItemList == null || collectionItemPrefab == null)
        {
            Debug.LogError("CollectionManager: Не все UI элементы настроены!");
            return;
        }
        
        // Очищаем текущие элементы
        ClearCollectionItems();
        
        // Устанавливаем название коллекции
        SetCollectionTitle(collectionName);
        
        // Создаем элементы коллекции
        if (collectionName == "catStates")
        {
            CreateCatStatesCollectionItems();
        }
        else if (collectionName == "basic")
        {
            if (basicCollection == null || basicCollection.foodItems == null)
            {
                Debug.LogError("CollectionManager: Базовая коллекция не настроена!");
                return;
            }
            
            foreach (FoodData foodData in basicCollection.foodItems)
            {
                CreateCollectionItem(foodData);
            }
        }
        else
        {
            Debug.LogError($"CollectionManager: Неизвестная коллекция '{collectionName}'!");
            return;
        }
        
        // Показываем панель
        collectionListPanel.SetActive(true);
    }
    
    
    /// <summary>
    /// Создает элемент коллекции для продукта
    /// </summary>
    /// <param name="foodData">Данные продукта</param>
    private void CreateCollectionItem(FoodData foodData)
    {
        GameObject itemObject = Instantiate(collectionItemPrefab, collectionItemList);
        CollectionItemUI itemUI = itemObject.GetComponent<CollectionItemUI>();
        
        if (itemUI != null)
        {
            // Передаем ссылку на GameManager в CollectionItemUI
            itemUI.gameManager = gameManager;
            
            // Проверяем, открыт ли продукт в коллекции
            bool isUnlocked = gameManager != null && gameManager.discoveredFoods.Contains(foodData.foodType);
            
            // Настраиваем элемент
            itemUI.SetupFoodItem(foodData, isUnlocked);
            currentCollectionItems.Add(itemUI);
        }
    }
    
    /// <summary>
    /// Создает элементы коллекции для состояний кота
    /// </summary>
    private void CreateCatStatesCollectionItems()
    {
        // Приоритет: используем CatStateManager, если доступен
        if (catStateManager != null)
        {
            var sortedStates = catStateManager.GetAllStatesSortedByDisplayOrder();
            
            foreach (CatStateData stateData in sortedStates)
            {
                GameObject itemObject = Instantiate(collectionItemPrefab, collectionItemList);
                CollectionItemUI itemUI = itemObject.GetComponent<CollectionItemUI>();
                
                if (itemUI != null)
                {
                    // Проверяем, достигнуто ли состояние через CatStateManager
                    bool isUnlocked = catStateManager.IsStateUnlocked(stateData.satisfactionLevel);
                    
                    // Настраиваем элемент состояния кота с данными из ScriptableObject
                    itemUI.SetupCatStateItem(stateData, isUnlocked);
                    currentCollectionItems.Add(itemUI);
                }
            }
        }
        else if (catStatesData != null && catStatesData.Length > 0)
        {
            // Fallback на старую логику
            var sortedStates = catStatesData.OrderBy(x => x.displayOrder).ToArray();
            
            foreach (CatStateData stateData in sortedStates)
            {
                GameObject itemObject = Instantiate(collectionItemPrefab, collectionItemList);
                CollectionItemUI itemUI = itemObject.GetComponent<CollectionItemUI>();
                
                if (itemUI != null)
                {
                    // Проверяем, достигнуто ли состояние через CatStateManager
                    bool isUnlocked = gameManager != null && gameManager.catStateManager != null && 
                                    gameManager.catStateManager.IsStateUnlocked(stateData.satisfactionLevel);
                    
                    // Настраиваем элемент состояния кота с данными из ScriptableObject
                    itemUI.SetupCatStateItem(stateData, isUnlocked);
                    currentCollectionItems.Add(itemUI);
                }
            }
        }
        else
        {
            Debug.LogError("CollectionManager: Данные состояний кота не настроены!");
        }
    }
    
    /// <summary>
    /// Очищает текущие элементы коллекции
    /// </summary>
    private void ClearCollectionItems()
    {
        foreach (CollectionItemUI item in currentCollectionItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        
        currentCollectionItems.Clear();
    }
    
    /// <summary>
    /// Закрывает панель коллекции
    /// </summary>
    public void CloseCollectionPanel()
    {
        if (collectionListPanel != null)
            collectionListPanel.SetActive(false);
    }
    
    /// <summary>
    /// Устанавливает название коллекции
    /// </summary>
    /// <param name="collectionName">Имя коллекции</param>
    private void SetCollectionTitle(string collectionName)
    {
        if (collectionTitleText == null)
        {
            Debug.LogWarning("CollectionManager: collectionTitleText не настроен!");
            return;
        }
        
        string title = "";
        switch (collectionName.ToLower())
        {
            case "basic":
                title = "Вкусно и сытно";
                break;
            case "catstates":
                title = "Голодный котик атакует";
                break;
            default:
                title = "Коллекция";
                break;
        }
        
        collectionTitleText.text = title;
    }
    
    /// <summary>
    /// Получает название статуса кота по уровню удовлетворенности
    /// </summary>
    /// <param name="satisfactionLevel">Уровень удовлетворенности</param>
    /// <returns>Название статуса</returns>
    public string GetSatisfactionLevelName(CatSatisfactionLevel satisfactionLevel)
    {
        // Приоритет: используем CatStateManager, если доступен
        if (catStateManager != null)
        {
            return catStateManager.GetStateName(satisfactionLevel);
        }
        
        // Fallback: ищем данные в catStatesData
        if (catStatesData != null)
        {
            var stateData = System.Array.Find(catStatesData, data => data.satisfactionLevel == satisfactionLevel);
            if (stateData != null)
            {
                return stateData.stateName;
            }
        }
        
        // Fallback на хардкод, если ScriptableObject не найден
        switch (satisfactionLevel)
        {
            case CatSatisfactionLevel.Hungry: return "Голодный котик";
            case CatSatisfactionLevel.Worms: return "Заморил червячка";
            case CatSatisfactionLevel.Better: return "Уже полегче";
            case CatSatisfactionLevel.Tasty: return "Вкусненько";
            case CatSatisfactionLevel.Satisfied: return "Пузико довольно!";
            case CatSatisfactionLevel.Full: return "Надо же, сколько еды!";
            case CatSatisfactionLevel.Happy: return "Счастливый котик - сытый котик!";
            case CatSatisfactionLevel.Royal: return "Королевская сытость";
            default: return "Неизвестное состояние";
        }
    }
}
