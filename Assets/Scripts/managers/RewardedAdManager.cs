using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YandexMobileAds;
using YandexMobileAds.Base;
using System;

/// <summary>
/// Менеджер рекламы с наградой для получения бустера "Бомбочка"
/// Заменяет покупку за монеты на просмотр рекламы
/// </summary>
public class RewardedAdManager : MonoBehaviour
{
    [Header("Настройки рекламы")]
    [Tooltip("ID рекламного блока для тестирования (замените на реальный R-M-XXXXXX-Y)")]
    public string adUnitId = "demo-rewarded-yandex";
    
    [Tooltip("Включить логирование")]
    public bool enableLogs = true;

    [Header("UI элементы")]
    [Tooltip("Кнопка получения бомбочки за рекламу")]
    public Button getBombButton;
    
    [Tooltip("Текст на кнопке")]
    public TextMeshProUGUI buttonText;
    
    [Tooltip("Индикатор загрузки рекламы")]
    public GameObject loadingIndicator;

    [Header("Настройки награды")]
    [Tooltip("Количество бомбочек за просмотр рекламы")]
    public int bombRewardAmount = 1;

    // События
    public static event Action OnAdLoaded;
    public static event Action<string> OnAdFailedToLoad;
    public static event Action OnAdShown;
    public static event Action<string> OnAdFailedToShow;
    public static event Action OnAdDismissed;
    public static event Action OnRewardEarned;
    // Событие для аналитики: когда игрок не получил награду за рекламу
    public static event Action OnRewardNotEarned;

    // Статический экземпляр для доступа из других скриптов
    public static RewardedAdManager Instance { get; private set; }

    // Компоненты рекламы
    private RewardedAdLoader rewardedAdLoader;
    private RewardedAd rewardedAd;
    
    // Состояние
    private bool isAdLoaded = false;
    private bool isAdLoading = false;
    private bool isAdShowing = false;
    private bool rewardEarned = false;

    // Ссылки на менеджеры
    private ShopBillingManager shopBillingManager;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeReferences();
        SetupLoader();
        SetupButtonListeners();
        UpdateUI();
        
        // Предзагружаем рекламу
        RequestRewardedAd();
    }

    /// <summary>
    /// Инициализирует ссылки на менеджеры
    /// </summary>
    private void InitializeReferences()
    {
        shopBillingManager = ShopBillingManager.Instance;

        if (shopBillingManager == null)
        {
            Debug.LogWarning("RewardedAdManager: ShopBillingManager не найден!");
        }
    }

    /// <summary>
    /// Настраивает загрузчик рекламы
    /// </summary>
    private void SetupLoader()
    {
        rewardedAdLoader = new RewardedAdLoader();
        rewardedAdLoader.OnAdLoaded += HandleAdLoaded;
        rewardedAdLoader.OnAdFailedToLoad += HandleAdFailedToLoad;
    }

    /// <summary>
    /// Настраивает обработчики кнопок
    /// </summary>
    private void SetupButtonListeners()
    {
        if (getBombButton != null)
        {
            getBombButton.onClick.AddListener(ShowRewardedAd);
        }
    }

    /// <summary>
    /// Запрашивает загрузку рекламы
    /// </summary>
    public void RequestRewardedAd()
    {
        if (isAdLoading || isAdLoaded)
        {
            if (enableLogs)
                Debug.Log("RewardedAdManager: Реклама уже загружается или загружена");
            return;
        }

        if (enableLogs)
            Debug.Log("RewardedAdManager: Запрашиваем загрузку рекламы...");

        isAdLoading = true;
        isAdLoaded = false;
        rewardEarned = false;

        // Настраиваем параметры запроса
        AdRequestConfiguration adRequestConfiguration = new AdRequestConfiguration.Builder(adUnitId).Build();
        
        // Загружаем рекламу
        rewardedAdLoader.LoadAd(adRequestConfiguration);
        
        UpdateUI();
    }

    /// <summary>
    /// Показывает рекламу с наградой
    /// </summary>
    public void ShowRewardedAd()
    {
        if (!isAdLoaded)
        {
            if (enableLogs)
                Debug.LogWarning("RewardedAdManager: Реклама не загружена!");
            
            // Пытаемся загрузить рекламу
            RequestRewardedAd();
            return;
        }

        if (isAdShowing)
        {
            if (enableLogs)
                Debug.LogWarning("RewardedAdManager: Реклама уже показывается!");
            return;
        }

        if (enableLogs)
            Debug.Log("RewardedAdManager: Показываем рекламу...");

        isAdShowing = true;
        rewardEarned = false;
        
        // Показываем рекламу
        rewardedAd.Show();
        
        UpdateUI();
    }

    /// <summary>
    /// Обработчик успешной загрузки рекламы
    /// </summary>
    private void HandleAdLoaded(object sender, RewardedAdLoadedEventArgs args)
    {
        if (enableLogs)
            Debug.Log("RewardedAdManager: Реклама успешно загружена!");

        isAdLoading = false;
        isAdLoaded = true;
        rewardedAd = args.RewardedAd;

        // Настраиваем обработчики событий рекламы
        SetupAdEventHandlers();

        OnAdLoaded?.Invoke();
        UpdateUI();
    }

    /// <summary>
    /// Обработчик ошибки загрузки рекламы
    /// </summary>
    private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        if (enableLogs)
            Debug.LogError($"RewardedAdManager: Ошибка загрузки рекламы: {args.Message}");

        isAdLoading = false;
        isAdLoaded = false;
        rewardedAd = null;

        OnAdFailedToLoad?.Invoke(args.Message);
        UpdateUI();
    }

    /// <summary>
    /// Настраивает обработчики событий рекламы
    /// </summary>
    private void SetupAdEventHandlers()
    {
        if (rewardedAd == null) return;

        rewardedAd.OnAdShown += HandleAdShown;
        rewardedAd.OnAdFailedToShow += HandleAdFailedToShow;
        rewardedAd.OnAdDismissed += HandleAdDismissed;
        rewardedAd.OnAdClicked += HandleAdClicked;
        rewardedAd.OnAdImpression += HandleAdImpression;
        rewardedAd.OnRewarded += HandleRewarded;
    }

    /// <summary>
    /// Обработчик показа рекламы
    /// </summary>
    private void HandleAdShown(object sender, EventArgs args)
    {
        if (enableLogs)
            Debug.Log("RewardedAdManager: Реклама показана");

        OnAdShown?.Invoke();
    }

    /// <summary>
    /// Обработчик ошибки показа рекламы
    /// </summary>
    private void HandleAdFailedToShow(object sender, AdFailureEventArgs args)
    {
        if (enableLogs)
            Debug.LogError($"RewardedAdManager: Ошибка показа рекламы: {args.Message}");

        isAdShowing = false;
        rewardEarned = false;

        OnAdFailedToShow?.Invoke(args.Message);
        UpdateUI();

        // Загружаем новую рекламу
        RequestRewardedAd();
    }

    /// <summary>
    /// Обработчик закрытия рекламы
    /// </summary>
    private void HandleAdDismissed(object sender, EventArgs args)
    {
        if (enableLogs)
            Debug.Log("RewardedAdManager: Реклама закрыта");

        isAdShowing = false;

        // Проверяем, получил ли игрок награду
        if (rewardEarned)
        {
            if (enableLogs)
                Debug.Log("RewardedAdManager: Игрок получил награду!");
            
            GiveBombReward();
            OnRewardEarned?.Invoke();
        }
        else
        {
            if (enableLogs)
                Debug.Log("RewardedAdManager: Игрок не получил награду (не досмотрел рекламу)");
            
            OnRewardNotEarned?.Invoke();
        }

        OnAdDismissed?.Invoke();
        
        // Очищаем ресурсы и загружаем новую рекламу
        DestroyRewardedAd();
        RequestRewardedAd();
    }

    /// <summary>
    /// Обработчик клика по рекламе
    /// </summary>
    private void HandleAdClicked(object sender, EventArgs args)
    {
        if (enableLogs)
            Debug.Log("RewardedAdManager: Клик по рекламе");
    }

    /// <summary>
    /// Обработчик показа impression
    /// </summary>
    private void HandleAdImpression(object sender, ImpressionData impressionData)
    {
        if (enableLogs)
            Debug.Log("RewardedAdManager: Impression зафиксирован");
    }

    /// <summary>
    /// Обработчик получения награды
    /// </summary>
    private void HandleRewarded(object sender, Reward args)
    {
        if (enableLogs)
            Debug.Log($"RewardedAdManager: Награда получена! Тип: {args.type}, Количество: {args.amount}");

        rewardEarned = true;
    }

    /// <summary>
    /// Выдает награду игроку
    /// </summary>
    private void GiveBombReward()
    {
        // Добавляем бомбочку через GameDataManager (статический класс)
        GameDataManager.AddBoosters("bomb", bombRewardAmount);
        
        if (enableLogs)
            Debug.Log($"RewardedAdManager: Выдано {bombRewardAmount} бомбочек за просмотр рекламы");

        // Обновляем UI через ShopBillingManager
        if (shopBillingManager != null)
        {
            shopBillingManager.InitializeBoosterUI();
        }
    }

    /// <summary>
    /// Уничтожает текущую рекламу
    /// </summary>
    private void DestroyRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        
        isAdLoaded = false;
        isAdShowing = false;
        rewardEarned = false;
    }

    /// <summary>
    /// Обновляет UI элементы
    /// </summary>
    private void UpdateUI()
    {
        // Обновляем состояние кнопки
        if (getBombButton != null)
        {
            getBombButton.interactable = isAdLoaded && !isAdShowing;
        }

        // Обновляем текст кнопки
        if (buttonText != null)
        {
            if (isAdLoading)
            {
                buttonText.text = "Загрузка...";
            }
            else if (isAdShowing)
            {
                buttonText.text = "Показ рекламы...";
            }
            else if (isAdLoaded)
            {
                buttonText.text = $"Получить {bombRewardAmount} бомбочку";
            }
            else
            {
                buttonText.text = "Реклама недоступна";
            }
        }

        // Обновляем индикатор загрузки
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(isAdLoading);
        }
    }

    /// <summary>
    /// Проверяет, доступна ли реклама
    /// </summary>
    public bool IsAdAvailable()
    {
        return isAdLoaded && !isAdShowing;
    }

    /// <summary>
    /// Принудительно загружает новую рекламу
    /// </summary>
    public void ForceReloadAd()
    {
        if (enableLogs)
            Debug.Log("RewardedAdManager: Принудительная перезагрузка рекламы");

        DestroyRewardedAd();
        RequestRewardedAd();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (rewardedAdLoader != null)
        {
            rewardedAdLoader.OnAdLoaded -= HandleAdLoaded;
            rewardedAdLoader.OnAdFailedToLoad -= HandleAdFailedToLoad;
        }

        // Очищаем ресурсы
        DestroyRewardedAd();
    }
}
