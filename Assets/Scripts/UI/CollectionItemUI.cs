using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI компонент для отображения элемента коллекции
/// Поддерживает как продукты, так и состояния кота
/// </summary>
public class CollectionItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Изображение продукта")]
    public Image collectionItemImage;

    [Tooltip("Название достижения/продукта")]
    public TextMeshProUGUI collectionItemDescription;
    
    [Tooltip("Описание продукта/достижения")]
    public TextMeshProUGUI collectionItemTitle;
    
    [Tooltip("Иконка закрытого статуса")]
    public GameObject collectionItemStatusClosedIcon;
    
    [Tooltip("Иконка открытого статуса")]
    public GameObject collectionItemStatusOpenIcon;
    
    [Tooltip("Награда (монеты)")]
    public TextMeshProUGUI rewardInfoText;

    [Tooltip("Блок с информацией о награде>")]
    public GameObject rewardBlock;
    
    private FoodData currentFoodData;
    private CatStateData currentCatStateData;
    
    [Tooltip("Ссылка на GameManager для получения наград за первое открытие")]
    public GameManager gameManager;
    
    /// <summary>
    /// Настраивает элемент для продукта
    /// </summary>
    /// <param name="foodData">Данные продукта</param>
    /// <param name="isUnlocked">Открыт ли продукт</param>
    public void SetupFoodItem(FoodData foodData, bool isUnlocked)
    {
        currentFoodData = foodData;
        
        // Устанавливаем изображение
        if (collectionItemImage != null && foodData.foodSprite != null)
        {
            collectionItemImage.sprite = foodData.foodSprite;
        }
        
        // Устанавливаем название (титул)
        if (collectionItemTitle != null)
        {
            collectionItemTitle.text = foodData.foodName;
        }
        
        // Устанавливаем описание
        if (collectionItemDescription != null)
        {
            collectionItemDescription.text = foodData.description;
        }
        
        // Устанавливаем статус
        SetUnlockStatus(isUnlocked);
        
        // Устанавливаем награду и блок награды
        bool isDefaultProduct = foodData.foodType == FoodType.Sausage;
        
        // Получаем награду за первое открытие, если GameManager доступен
        int firstDiscoveryReward = 0;
        int goalReward = 0;
        if (gameManager != null && gameManager.economyConfig != null)
        {
            firstDiscoveryReward = gameManager.economyConfig.GetFirstDiscoveryReward(foodData.foodType);
            goalReward = gameManager.economyConfig.GetGoalReward(foodData.foodType);
        }
        
        // Показываем награду за первое открытие, если она есть, иначе награду за цель
        // Для сосиски не показываем награду вообще (isDefaultProduct = true)
        string rewardText = "";
        if (!isDefaultProduct)
        {
            rewardText = firstDiscoveryReward > 0 ? firstDiscoveryReward.ToString() : goalReward.ToString();
        }
        SetRewardDisplay(!isDefaultProduct, rewardText);
    }
    
    /// <summary>
    /// Настраивает элемент для состояния кота (новая версия с ScriptableObject)
    /// </summary>
    /// <param name="catStateData">Данные состояния кота</param>
    /// <param name="isUnlocked">Достигнуто ли состояние</param>
    public void SetupCatStateItem(CatStateData catStateData, bool isUnlocked)
    {
        currentCatStateData = catStateData;
        
        // Устанавливаем изображение
        if (collectionItemImage != null && catStateData.catStateSprite != null)
        {
            collectionItemImage.sprite = catStateData.catStateSprite;
        }
        
        // Устанавливаем название (титул)
        if (collectionItemTitle != null)
        {
            collectionItemTitle.text = catStateData.stateName;
        }
        
        // Устанавливаем описание
        if (collectionItemDescription != null)
        {
            collectionItemDescription.text = catStateData.description;
        }
        
        // Устанавливаем статус
        SetUnlockStatus(isUnlocked);
        
        // Устанавливаем награду и блок награды
        bool isDefaultState = catStateData.isDefaultState || catStateData.satisfactionLevel == CatSatisfactionLevel.Hungry;
        SetRewardDisplay(!isDefaultState, catStateData.coinReward.ToString());
    }
    
    
    /// <summary>
    /// Устанавливает статус открытости элемента
    /// </summary>
    /// <param name="isUnlocked">Открыт ли элемент</param>
    private void SetUnlockStatus(bool isUnlocked)
    {
        if (collectionItemStatusClosedIcon != null)
            collectionItemStatusClosedIcon.SetActive(!isUnlocked);
            
        if (collectionItemStatusOpenIcon != null)
            collectionItemStatusOpenIcon.SetActive(isUnlocked);
    }
    
    /// <summary>
    /// Устанавливает отображение блока с наградой
    /// </summary>
    /// <param name="showReward">Показывать ли награду</param>
    /// <param name="rewardText">Текст награды</param>
    private void SetRewardDisplay(bool showReward, string rewardText = "")
    {
        if (rewardBlock != null)
        {
            rewardBlock.SetActive(showReward);
        }
        
        if (rewardInfoText != null && showReward)
        {
            rewardInfoText.text = rewardText;
        }
    }
    
}
