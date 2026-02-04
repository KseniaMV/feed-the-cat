using UnityEngine;
using System;
using TMPro;

/// <summary>
/// Менеджер платежей для существующего UI магазина игры
/// Теперь использует новый PaymentManager с Pay SDK
/// </summary>
public class ShopBillingManager : MonoBehaviour
{
    [Header("Настройки RuStore (устарело - используйте PaymentManager)")]
    [Tooltip("ID приложения в консоли RuStore (устарело)")]
    public string consoleApplicationId = "your_app_id_here";
    
    [Tooltip("Схема deeplink для приложения (устарело)")]
    public string deeplinkScheme = "feedthecat";
    
    [Tooltip("Включить логирование (устарело)")]
    public bool enableLogs = true;

    [Header("UI элементы бустеров")]
    [Tooltip("Ссылка на поле количества бомбочек в UI")]
    public TextMeshProUGUI bombCountText;
    
    [Tooltip("Ссылка на поле количества лапок в UI")]
    public TextMeshProUGUI pawCountText;
    
    [Tooltip("Ссылка на поле количества котов-обжорок в UI")]
    public TextMeshProUGUI catCountText;

    // События платежей (теперь перенаправляются из PaymentManager)
    [Tooltip("Вызывается при успешной покупке")]
    public static event Action<string, int, int, int, int> OnPurchaseSuccessful; // productId, coins, bombs, paws, cats
    
    [Tooltip("Вызывается при ошибке покупки")]
    public static event Action<string> OnPurchaseFailed;

    // Статический экземпляр для доступа из других скриптов
    public static ShopBillingManager Instance { get; private set; }

    // Ссылка на новый PaymentManager
    private PaymentManager paymentManager;

    // Ссылки на менеджеры
    private GameManager gameManager;
    private UIManager uiManager;

    [Header("Конфигурация пакетов")]
    [Tooltip("Конфигурация пакетов магазина")]
    public ShopPackageConfig shopPackageConfig;

    // Данные о пакетах для покупки (берется из конфигурации)
    private ShopPackage[] shopPackages => shopPackageConfig != null ? shopPackageConfig.GetAllPackages() : GetDefaultPackages();

    /// <summary>
    /// Получает пакеты по умолчанию (fallback)
    /// </summary>
    private ShopPackage[] GetDefaultPackages()
    {
        return new ShopPackage[]
        {
            new ShopPackage
            {
                productId = "coins_pack",
                packageName = "Пакет монеток",
                description = "1500 монет для быстрого старта",
                coins = 1500,
                bombs = 0,
                paws = 0,
                cats = 0,
                price = "99.00",
                currency = "RUB"
            },
            new ShopPackage
            {
                productId = "pack_1",
                packageName = "Пакет 1",
                description = "10 бомбочек + 10 лапок + 2 кота-обжорки (без рекламы!)",
                coins = 0,
                bombs = 10,
                paws = 10,
                cats = 2,
                price = "199.00",
                currency = "RUB"
            },
            new ShopPackage
            {
                productId = "pack_2",
                packageName = "Пакет 2",
                description = "15 бомбочек + 15 лапок + 5 котов-обжорок (без рекламы!)",
                coins = 0,
                bombs = 15,
                paws = 15,
                cats = 5,
                price = "299.00",
                currency = "RUB"
            },
            new ShopPackage
            {
                productId = "pack_3",
                packageName = "Пакет 3",
                description = "25 бомбочек + 25 лапок + 10 котов-обжорок (без рекламы!)",
                coins = 0,
                bombs = 25,
                paws = 25,
                cats = 10,
                price = "499.00",
                currency = "RUB"
            }
        };
    }

    private void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeReferences();
        SetupEventListeners();
        SetupPaymentManagerEvents();
    }
    
    /// <summary>
    /// Настраивает обработчики событий
    /// </summary>
    private void SetupEventListeners()
    {
        // Подписываемся на события изменения количества бустеров
        GameDataManager.OnBoosterCountChanged += OnBoosterCountChanged;
    }
    
    /// <summary>
    /// Обработчик изменения количества бустеров
    /// </summary>
    private void OnBoosterCountChanged(string boosterType, int newCount)
    {
        UpdateBoosterUI(boosterType, newCount);
    }

    /// <summary>
    /// Инициализирует ссылки на менеджеры
    /// </summary>
    private void InitializeReferences()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        uiManager = FindFirstObjectByType<UIManager>();
        paymentManager = FindFirstObjectByType<PaymentManager>();

        if (gameManager == null)
        {
            Debug.LogWarning("ShopBillingManager: GameManager не найден");
        }

        if (uiManager == null)
        {
            Debug.LogWarning("ShopBillingManager: UIManager не найден");
        }

        if (paymentManager == null)
        {
            Debug.LogError("ShopBillingManager: PaymentManager не найден! Создайте объект с PaymentManager.");
        }
    }

    /// <summary>
    /// Настраивает события PaymentManager
    /// </summary>
    private void SetupPaymentManagerEvents()
    {
        if (paymentManager != null)
        {
            // Перенаправляем события из PaymentManager
            PaymentManager.OnPurchaseSuccessful += (productId, coins, bombs, paws, cats) =>
            {
                OnPurchaseSuccessful?.Invoke(productId, coins, bombs, paws, cats);
            };

            PaymentManager.OnPurchaseFailed += (errorMessage) =>
            {
                OnPurchaseFailed?.Invoke(errorMessage);
            };
        }
    }


    /// <summary>
    /// Покупка пакета монеток
    /// </summary>
    public void BuyCoinsPack()
    {
        if (paymentManager != null)
        {
            paymentManager.BuyCoinsPack();
        }
        else
        {
            OnPurchaseFailed?.Invoke("PaymentManager не инициализирован");
        }
    }

    /// <summary>
    /// Покупка пакета 1
    /// </summary>
    public void BuyPack1()
    {
        if (paymentManager != null)
        {
            paymentManager.BuyPack1();
        }
        else
        {
            OnPurchaseFailed?.Invoke("PaymentManager не инициализирован");
        }
    }

    /// <summary>
    /// Покупка пакета 2
    /// </summary>
    public void BuyPack2()
    {
        if (paymentManager != null)
        {
            paymentManager.BuyPack2();
        }
        else
        {
            OnPurchaseFailed?.Invoke("PaymentManager не инициализирован");
        }
    }

    /// <summary>
    /// Покупка пакета 3
    /// </summary>
    public void BuyPack3()
    {
        if (paymentManager != null)
        {
            paymentManager.BuyPack3();
        }
        else
        {
            OnPurchaseFailed?.Invoke("PaymentManager не инициализирован");
        }
    }


    /// <summary>
    /// Добавляет бустеры игроку (теперь делегирует в PaymentManager)
    /// </summary>
    private void AddBoosters(string boosterType, int count)
    {
        if (paymentManager != null)
        {
            // Используем GameDataManager напрямую
            GameDataManager.AddBoosters(boosterType, count);
        }
    }

    /// <summary>
    /// Получает текущее количество бустеров
    /// </summary>
    private int GetCurrentBoosterCount(string boosterType)
    {
        return GameDataManager.GetBoosterCount(boosterType);
    }

    /// <summary>
    /// Обновляет UI бустеров
    /// </summary>
    private void UpdateBoosterUI(string boosterType, int count)
    {
        switch (boosterType)
        {
            case "bomb":
                if (bombCountText != null)
                    bombCountText.text = count.ToString();
                break;
            case "paw":
                if (pawCountText != null)
                    pawCountText.text = count.ToString();
                break;
            case "cat":
                if (catCountText != null)
                    catCountText.text = count.ToString();
                break;
        }
    }

    /// <summary>
    /// Получает информацию о пакете по ID
    /// </summary>
    private ShopPackage GetPackageInfo(string productId)
    {
        foreach (var package in shopPackages)
        {
            if (package.productId == productId)
                return package;
        }
        return null;
    }

    /// <summary>
    /// Получает все доступные пакеты
    /// </summary>
    public ShopPackage[] GetAllPackages()
    {
        return shopPackages;
    }

    /// <summary>
    /// Проверяет инициализирован ли billing (теперь делегирует в PaymentManager)
    /// </summary>
    public bool IsBillingInitialized()
    {
        return paymentManager != null && paymentManager.IsPaymentInitialized();
    }


    /// <summary>
    /// Инициализирует UI бустеров при запуске
    /// </summary>
    public void InitializeBoosterUI()
    {
        UpdateBoosterUI("bomb", GetCurrentBoosterCount("bomb"));
        UpdateBoosterUI("paw", GetCurrentBoosterCount("paw"));
        UpdateBoosterUI("cat", GetCurrentBoosterCount("cat"));
    }

    /// <summary>
    /// Добавляет один бустер (для покупки за монеты)
    /// </summary>
    public void AddSingleBooster(string boosterType)
    {
        GameDataManager.AddBoosters(boosterType, 1);
    }

    private void OnDestroy()
    {
        // Отписываемся от событий GameDataManager
        GameDataManager.OnBoosterCountChanged -= OnBoosterCountChanged;
        
        // Отписываемся от событий при уничтожении объекта
        OnPurchaseSuccessful = null;
        OnPurchaseFailed = null;
    }
}

