using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Основное меню")]
    public GameObject mainMenuPanel;
    [Tooltip("Кнопка загрузки игры")]
    public Button playButton;

    [Header("Панель игры")]
    public GameObject gamePanel;

    [Header("Кнопки панели игры")]
    public Button musicOnButton;
    public Button musicOffButton;
    public Button collectionButton;
    public Button shopButton;
    public Button homeButton;

    [Header("Игровая информация на панели игры")]
    [Tooltip("Показывает количество монет")]
    public TextMeshProUGUI coinsCountText;
    
    [Tooltip("Показывает уровень удовлетворенности кота")]
    public TextMeshProUGUI experienceText;
    [Tooltip("Шкала удовлетворенности кота")]
    public Slider experienceSlider;
    [Tooltip("Иконка цели")]
    public Image goalImage;
    [Tooltip("Показывает награду за достижение цели")]
    public TextMeshProUGUI goalRewardText;

    [Header("Панель коллекций")]
    public GameObject collectionPanel;
    [Tooltip("Показывает рекорд игрока (опыт)")]
    public TextMeshProUGUI  experienceRecordText;


    [Header("Панель магазина")]
    public GameObject shopPanel;
    
    [Header("Панель проигрыша")]
    public GameObject gameOverPanel;

    [Header("Модальные окна")]

    private GameManager gameManager;
    private CollectionManager collectionManager;
    
    private void Start()
    {
        InitializeGameManager();
        ShowMainMenu();
    }
    
    /// <summary>
    /// Инициализирует GameManager и подписывается на события
    /// </summary>
    private void InitializeGameManager()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        collectionManager = FindFirstObjectByType<CollectionManager>();
        
        if (gameManager != null)
        {
            SubscribeToGameEvents();
            UpdateInitialUI();
            UpdateExperienceRecordDisplay(); // Обновляем отображение рекорда при инициализации
        }
    }
    
    /// <summary>
    /// Подписывается на события GameManager
    /// </summary>
    private void SubscribeToGameEvents()
    {
        gameManager.OnCoinsChanged += UpdateCoinsDisplay;
        gameManager.OnExperienceChanged += UpdateExperienceDisplay;
        gameManager.OnSatisfactionLevelChanged += UpdateSatisfactionDisplay;
        gameManager.OnGoalCompleted += OnGoalCompleted;
        gameManager.OnGoalChanged += OnGoalChanged;
    }
    
    /// <summary>
    /// Обновляет UI с начальными значениями
    /// </summary>
    private void UpdateInitialUI()
    {
        if (!GameDataManager.IsInitialized)
        {
            GameDataManager.Initialize();
        }
        
        UpdateCoinsDisplay(gameManager.currentCoins);
        UpdateExperienceDisplay(GameDataManager.GameSessionExperience);
        UpdateSatisfactionDisplay(gameManager.currentSatisfactionLevel);
        UpdateGoalIcon();
        UpdateGoalRewardText();
    }
    
    private void UpdateCoinsDisplay(int coins)
    {
        if (coinsCountText != null)
        {
            coinsCountText.text = coins.ToString();
        }
    }
    
    private void UpdateExperienceDisplay(int experience)
    {
        if (gameManager != null)
        {
            UpdateSatisfactionDisplay(gameManager.currentSatisfactionLevel);
        }
    }
    
    private void UpdateSatisfactionDisplay(CatSatisfactionLevel level)
    {
        if (experienceText != null && collectionManager != null)
        {
            string levelName = collectionManager.GetSatisfactionLevelName(level);
            experienceText.text = levelName;
        }
        
        if (experienceSlider != null && gameManager != null)
        {
            float progress = CalculateSatisfactionProgress(level);
            experienceSlider.value = progress;
        }
        
        // Обновляем отображение рекорда при изменении состояния кота
        UpdateExperienceRecordDisplay();
    }
    
    /// <summary>
    /// Обновляет отображение последнего статуса кота (рекорд)
    /// </summary>
    private void UpdateExperienceRecordDisplay()
    {
        if (experienceRecordText != null && gameManager?.catStateManager != null)
        {
            string recordName = gameManager.catStateManager.GetLastUnlockedLevelName();
            experienceRecordText.text = recordName;
        }
    }
    
    /// <summary>
    /// Публичный метод для принудительного обновления отображения рекорда
    /// </summary>
    public void RefreshExperienceRecord()
    {
        UpdateExperienceRecordDisplay();
    }
    
    
    private void UpdateGoalIcon()
    {
        if (goalImage != null && gameManager != null)
        {
            // Получаем спрайт для текущей цели
            Sprite goalSprite = GetFoodTypeSprite(gameManager.currentGoal);
            
            if (goalSprite != null)
            {
                goalImage.sprite = goalSprite;
            }
        }
    }
    
    private Sprite GetFoodTypeSprite(FoodType foodType)
    {
        if (gameManager != null && gameManager.foodSpawner != null)
        {
            // Получаем FoodData для данного типа продукта
            FoodData foodData = gameManager.foodSpawner.GetFoodData(foodType);
            
            if (foodData != null && foodData.foodSprite != null)
            {
                return foodData.foodSprite;
            }
        }
        
        return null;
    }
    
    private void OnGoalCompleted(FoodType completedGoal)
    {
        UpdateGoalIcon();
        UpdateGoalRewardText();
    }
    
    private void OnGoalChanged(FoodType newGoal)
    {
        UpdateGoalIcon();
        UpdateGoalRewardText();
    }
    
    /// <summary>
    /// Обновляет текст награды за достижение цели
    /// </summary>
    private void UpdateGoalRewardText()
    {
        if (goalRewardText != null && gameManager != null && gameManager.economyConfig != null)
        {
            int goalReward = gameManager.economyConfig.GetGoalReward(gameManager.currentGoal);
            if (goalReward > 0)
            {
                goalRewardText.text = $"+{goalReward}";
            }
            else
            {
                goalRewardText.text = "";
            }
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
        if (gameManager == null || gameManager.catStateManager == null) return 0f;
        
        // Используем CatStateManager для получения правильного прогресса на основе игрового опыта
        return gameManager.catStateManager.GetProgressToNextLevel(GameDataManager.GameSessionExperience);
    }
    
    /// <summary>
    /// Показывает главное меню при загрузке игры
    /// </summary>
    private void ShowMainMenu()
    {
        // Показываем главное меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        // Скрываем панель игры
        if (gamePanel != null)
            gamePanel.SetActive(false);
        
        // Скрываем другие панели
        if (collectionPanel != null)
            collectionPanel.SetActive(false);
        if (shopPanel != null)
            shopPanel.SetActive(false);
        
        // Показываем кнопки магазина и коллекции
        if (shopButton != null)
            shopButton.gameObject.SetActive(true);
        if (collectionButton != null)
            collectionButton.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameEvents();
    }
    
    /// <summary>
    /// Отписывается от событий GameManager
    /// </summary>
    private void UnsubscribeFromGameEvents()
    {
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged -= UpdateCoinsDisplay;
            gameManager.OnExperienceChanged -= UpdateExperienceDisplay;
            gameManager.OnSatisfactionLevelChanged -= UpdateSatisfactionDisplay;
            gameManager.OnGoalCompleted -= OnGoalCompleted;
            gameManager.OnGoalChanged -= OnGoalChanged;
        }
    }


    /// <summary>
    /// Отключает музыку и звуки в игре (вызывается по клику на musicOffButton, подключено в инспекторе)
    /// </summary>
    public void TurnOffMusicAndSounds()
    {
        // Отключаем все аудиоисточники на сцене
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in audioSources)
        {
            source.mute = true;
        }
    }

    /// <summary>
    /// Включает музыку и звуки в игре (вызывается по клику на musicOnButton, подключено в инспекторе)
    /// </summary>
    public void TurnOnMusicAndSounds()
    {
        // Включаем все аудиоисточники на сцене
        AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (AudioSource source in audioSources)
        {
            source.mute = false;
        }
    }

    // Функции для управления панелями магазина и коллекции (без подписки на события кнопок)
    public void OnShopButtonClicked()
    {
        // Открываем панель магазина, закрываем коллекцию
        if (shopPanel != null)
            shopPanel.SetActive(true);
        if (collectionPanel != null)
            collectionPanel.SetActive(false);
            
        // Обновляем UI бустеров при открытии магазина
        var shopBillingManager = ShopBillingManager.Instance;
        if (shopBillingManager != null)
        {
            shopBillingManager.InitializeBoosterUI();
        }
        
        // Обновляем отображение монет при открытии магазина
        UpdateCoinsDisplay(gameManager.currentCoins);
        
        // Обновляем UI магазина бустеров
        var boosterShopManager = FindFirstObjectByType<BoosterShopManager>();
        if (boosterShopManager != null)
        {
            boosterShopManager.RefreshShopUI();
        }
    }

    public void OnCollectionButtonClicked()
    {
        // Открываем панель коллекции, закрываем магазин
        if (collectionPanel != null)
            collectionPanel.SetActive(true);
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    
    /// <summary>
    /// Универсальный обработчик кнопок коллекций
    /// </summary>
    /// <param name="collectionName">Имя коллекции (basic, catStates)</param>
    public void OnCollectionButtonClicked(string collectionName)
    {
        if (collectionManager != null)
        {
            // Принудительно обновляем отображение рекорда перед показом коллекции
            RefreshExperienceRecord();
            collectionManager.ShowCollection(collectionName);
        }
    }
    
    /// <summary>
    /// Закрывает панель списка коллекций
    /// </summary>
    public void CloseCollectionListPanel()
    {
        if (collectionManager != null)
        {
            collectionManager.CloseCollectionPanel();
        }
    }

    public void OnHomeButtonClicked()
    {
        // Завершаем игру: останавливаем спавн продуктов и очищаем поле
        if (gameManager != null)
        {
            gameManager.StopGame();
            // Обновляем рекорд при возврате в меню
            UpdateExperienceRecordDisplay();
        }

        // Открываем панель главного меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        // Закрываем все открытые панели
        if (shopPanel != null)
            shopPanel.SetActive(false);
        if (collectionPanel != null)
            collectionPanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(false);

        // Показываем кнопки магазина и коллекции, если они скрыты
        if (shopButton != null)
            shopButton.gameObject.SetActive(true);
        if (collectionButton != null)
            collectionButton.gameObject.SetActive(true);
    }

    public void OnStartGameButtonClicked()
    {
        if (gamePanel != null)
        {
            gamePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("UIManager: gamePanel не назначен!");
        }

        if (gameManager != null)
        {
            gameManager.ResetGame();
            gameManager.StartGame();
            UpdateExperienceDisplay(GameDataManager.GameSessionExperience);
        }
        else
        {
            Debug.LogError("UIManager: gameManager не назначен!");
        }

        shopButton?.gameObject.SetActive(false);
        collectionButton?.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Показывает панель проигрыша
    /// </summary>
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("UIManager: gameOverPanel не назначен!");
        }
    }
    
    /// <summary>
    /// Возвращает игрока в главное меню из панели проигрыша
    /// </summary>
    public void ReturnToMainMenuFromGameOver()
    {
        
        // Закрываем панель проигрыша
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Сбрасываем состояние проигрыша в GameManager
        if (gameManager != null)
        {
            gameManager.ResetGameOverState();
        }
        
        // Вызываем основную логику возврата в меню
        OnHomeButtonClicked();
    }



}
