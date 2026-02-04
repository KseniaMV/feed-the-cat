using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI компонент для отображения целей и удовлетворенности кота
/// </summary>
public class GoalUI : MonoBehaviour
{
    [Header("Цель")]
    [Tooltip("Текст текущей цели")]
    public TextMeshProUGUI goalText;
    
    [Tooltip("Иконка цели")]
    public Image goalIcon;
    
    [Header("Удовлетворенность кота")]
    [Tooltip("Текст степени удовлетворенности")]
    public TextMeshProUGUI satisfactionText;
    
    [Tooltip("Слайдер удовлетворенности")]
    public Slider satisfactionSlider;
    
    [Tooltip("Иконка кота")]
    public Image catIcon;
    
    [Header("Опыт и монеты")]
    [Tooltip("Текст опыта")]
    public TextMeshProUGUI experienceText;
    
    [Tooltip("Текст монет")]
    public TextMeshProUGUI coinsText;
    
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (gameManager != null)
        {
            // Подписываемся на события
            gameManager.OnExperienceChanged += UpdateExperienceDisplay;
            gameManager.OnCoinsChanged += UpdateCoinsDisplay;
            gameManager.OnSatisfactionLevelChanged += UpdateSatisfactionDisplay;
            gameManager.OnGoalCompleted += OnGoalCompleted;
            
            // Обновляем отображение
            UpdateAllDisplays();
        }
    }
    
    private void UpdateAllDisplays()
    {
        UpdateExperienceDisplay(gameManager.currentExperience);
        UpdateCoinsDisplay(gameManager.currentCoins);
        UpdateSatisfactionDisplay(gameManager.currentSatisfactionLevel);
        UpdateGoalDisplay();
    }
    
    private void UpdateExperienceDisplay(int experience)
    {
        if (experienceText != null)
        {
            experienceText.text = $"Опыт: {experience}";
        }
    }
    
    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsText != null)
        {
            coinsText.text = $"Монеты: {coins}";
        }
    }
    
    private void UpdateSatisfactionDisplay(CatSatisfactionLevel level)
    {
        if (satisfactionText != null)
        {
            string levelName = GetSatisfactionLevelName(level);
            satisfactionText.text = levelName;
        }
        
        if (satisfactionSlider != null)
        {
            // Вычисляем прогресс до следующей степени
            float progress = CalculateSatisfactionProgress(level);
            satisfactionSlider.value = progress;
        }
    }
    
    private void UpdateGoalDisplay()
    {
        if (goalText != null && gameManager != null)
        {
            string goalName = GetFoodTypeName(gameManager.currentGoal);
            goalText.text = $"Цель: {goalName}";
        }
    }
    
    private void OnGoalCompleted(FoodType completedGoal)
    {
        // Обновляем отображение цели
        UpdateGoalDisplay();
    }
    
    private string GetSatisfactionLevelName(CatSatisfactionLevel level)
    {
        switch (level)
        {
            case CatSatisfactionLevel.Hungry: return "Голодный котик";
            case CatSatisfactionLevel.Worms: return "Заморил червячка";
            case CatSatisfactionLevel.Better: return "Уже полегче";
            case CatSatisfactionLevel.Tasty: return "Вкусненько";
            case CatSatisfactionLevel.Satisfied: return "Пузико довольно!";
            case CatSatisfactionLevel.Full: return "Надо же, сколько еды!";
            case CatSatisfactionLevel.Happy: return "Счастливый котик - сытый котик!";
            case CatSatisfactionLevel.Royal: return "Королевская сытость";
            default: return "Голодный котик";
        }
    }
    
    private string GetFoodTypeName(FoodType foodType)
    {
        if (gameManager != null && gameManager.foodSpawner != null)
        {
            FoodData foodData = gameManager.foodSpawner.GetFoodData(foodType);
            if (foodData != null && !string.IsNullOrEmpty(foodData.foodName))
            {
                return foodData.foodName;
            }
        }
        
        // Fallback на старые значения, если данные из префаба недоступны
        switch (foodType)
        {
            case FoodType.Sausage: return "Сосиска";
            case FoodType.Eggs: return "Яичница";
            case FoodType.Sandwich: return "Сандвич";
            case FoodType.Meatball: return "Мясной шарик";
            case FoodType.Soup: return "Суп";
            case FoodType.Chicken: return "Курица";
            case FoodType.Salmon: return "Лосось";
            case FoodType.Shrimp: return "Креветка";
            case FoodType.Caviar: return "Икра";
            case FoodType.Oyster: return "Устрица";
            case FoodType.Lobster: return "Лобстер";
            default: return "Неизвестно";
        }
    }
    
    private float CalculateSatisfactionProgress(CatSatisfactionLevel currentLevel)
    {
        if (gameManager == null) return 0f;
        
        int currentLevelIndex = (int)currentLevel;
        int nextLevelIndex = currentLevelIndex + 1;
        
        // Если это максимальная степень, показываем 100%
        if (nextLevelIndex >= 8) return 1f;
        
        // Получаем пороги для текущей и следующей степени
        int[] thresholds = { 0, 1500, 3500, 5000, 8500, 13500, 23000, 46500 };
        int currentThreshold = thresholds[currentLevelIndex];
        int nextThreshold = thresholds[nextLevelIndex];
        
        // Вычисляем прогресс
        float progress = (float)(gameManager.currentExperience - currentThreshold) / (nextThreshold - currentThreshold);
        return Mathf.Clamp01(progress);
    }
    
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnExperienceChanged -= UpdateExperienceDisplay;
            gameManager.OnCoinsChanged -= UpdateCoinsDisplay;
            gameManager.OnSatisfactionLevelChanged -= UpdateSatisfactionDisplay;
            gameManager.OnGoalCompleted -= OnGoalCompleted;
        }
    }
}
