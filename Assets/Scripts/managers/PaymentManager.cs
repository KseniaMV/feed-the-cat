using UnityEngine;
using System;
using System.Collections.Generic;
using RuStore.PayClient;
using RuStore;

/// <summary>
/// Новый менеджер платежей, использующий RuStore Pay SDK
/// Заменяет старый BillingClient SDK на новый Pay SDK
/// </summary>
public class PaymentManager : MonoBehaviour
{
    [Header("Настройки RuStore Pay SDK")]
    [Tooltip("ID приложения в консоли RuStore")]
    public string consoleApplicationId = "your_app_id_here";
    
    [Tooltip("Схема deeplink для приложения")]
    public string deeplinkScheme = "feedthecat";
    
    [Tooltip("Включить логирование")]
    public bool enableLogs = true;

    // События платежей
    [Tooltip("Вызывается при успешной покупке")]
    public static event Action<string, int, int, int, int> OnPurchaseSuccessful; // productId, coins, bombs, paws, cats
    
    [Tooltip("Вызывается при ошибке покупки")]
    public static event Action<string> OnPurchaseFailed;

    // Статический экземпляр для доступа из других скриптов
    public static PaymentManager Instance { get; private set; }

    // Флаги состояния
    private bool isInitialized = false;
    private bool isInitializing = false;

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
        InitializePaySDK();
        SetupEventListeners();
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

        if (gameManager == null)
        {
            Debug.LogWarning("PaymentManager: GameManager не найден");
        }

        if (uiManager == null)
        {
            Debug.LogWarning("PaymentManager: UIManager не найден");
        }
    }

    /// <summary>
    /// Инициализирует RuStore Pay SDK
    /// </summary>
    private void InitializePaySDK()
    {
        if (isInitialized || isInitializing)
        {
            return;
        }

        isInitializing = true;

        // Проверяем платформу (только Android)
        if (Application.platform != RuntimePlatform.Android)
        {
            OnPurchaseFailed?.Invoke("RuStore поддерживается только на Android");
            isInitializing = false;
            return;
        }

        // Проверяем установлен ли RuStore
        if (!RuStorePayClient.Instance.IsRuStoreInstalled())
        {
            OnPurchaseFailed?.Invoke("RuStore не установлен. Установите RuStore для совершения покупок.");
            isInitializing = false;
            return;
        }

        // Проверяем доступность платежей
        RuStorePayClient.Instance.GetPurchaseAvailability(
            onFailure: (error) =>
            {
                Debug.LogError($"PaymentManager: Ошибка проверки доступности платежей: {error.description}");
                OnPurchaseFailed?.Invoke("Платежи недоступны");
                isInitializing = false;
            },
            onSuccess: (result) =>
            {
                if (result.isAvailable)
                {
                    isInitialized = true;
                    Debug.Log("PaymentManager: RuStore Pay SDK успешно инициализирован");
                }
                else
                {
                    Debug.LogError($"PaymentManager: Платежи недоступны: {result.cause?.description}");
                    OnPurchaseFailed?.Invoke("Платежи недоступны");
                }
                isInitializing = false;
            }
        );
    }

    /// <summary>
    /// Покупка пакета монеток
    /// </summary>
    public void BuyCoinsPack()
    {
        PurchasePackage("coins_pack");
    }

    /// <summary>
    /// Покупка пакета 1
    /// </summary>
    public void BuyPack1()
    {
        PurchasePackage("pack_1");
    }

    /// <summary>
    /// Покупка пакета 2
    /// </summary>
    public void BuyPack2()
    {
        PurchasePackage("pack_2");
    }

    /// <summary>
    /// Покупка пакета 3
    /// </summary>
    public void BuyPack3()
    {
        PurchasePackage("pack_3");
    }

    /// <summary>
    /// Универсальный метод покупки пакета
    /// </summary>
    private void PurchasePackage(string productId)
    {
        if (!isInitialized)
        {
            OnPurchaseFailed?.Invoke("Платежная система не инициализирована");
            return;
        }

        var package = GetPackageInfo(productId);
        if (package == null)
        {
            OnPurchaseFailed?.Invoke("Пакет не найден");
            return;
        }

        // Создаем параметры покупки для нового Pay SDK
        var purchaseParams = new ProductPurchaseParams(
            productId: new ProductId(productId),
            quantity: new Quantity(1),
            developerPayload: new DeveloperPayload($"purchase_{productId}_{DateTime.Now.Ticks}"),
            orderId: new OrderId($"order_{productId}_{DateTime.Now.Ticks}")
        );

        // Вызываем метод покупки из нового Pay SDK
        RuStorePayClient.Instance.Purchase(
            parameters: purchaseParams,
            preferredPurchaseType: PreferredPurchaseType.ONE_STEP,
            onFailure: OnPurchaseError,
            onSuccess: OnPurchaseSuccess
        );
    }

    /// <summary>
    /// Обработчик успешной покупки
    /// </summary>
    private void OnPurchaseSuccess(ProductPurchaseResult result)
    {
        Debug.Log($"PaymentManager: Покупка успешна - ProductId: {result.productId.value}, PurchaseId: {result.purchaseId.value}");
        
        // Для одностадийной оплаты товар сразу выдается
        // Для двухстадийной оплаты нужно подтвердить покупку
        if (result.purchaseType == PurchaseType.ONE_STEP)
        {
            ProcessPurchaseReward(result.productId.value);
        }
        else if (result.purchaseType == PurchaseType.TWO_STEP)
        {
            // Подтверждаем двухстадийную покупку
            ConfirmTwoStepPurchase(result.purchaseId, result.productId.value);
        }
    }

    /// <summary>
    /// Обработчик ошибки покупки
    /// </summary>
    private void OnPurchaseError(RuStoreError error)
    {
        string errorMessage = GetUserFriendlyErrorMessage(error);
        OnPurchaseFailed?.Invoke(errorMessage);
    }

    /// <summary>
    /// Подтверждает двухстадийную покупку
    /// </summary>
    private void ConfirmTwoStepPurchase(PurchaseId purchaseId, string productId)
    {
        RuStorePayClient.Instance.ConfirmTwoStepPurchase(
            purchaseId: purchaseId,
            developerPayload: new DeveloperPayload($"confirm_{productId}_{DateTime.Now.Ticks}"),
            onFailure: (error) =>
            {
                Debug.LogError($"PaymentManager: Ошибка подтверждения покупки: {error.description}");
                OnPurchaseFailed?.Invoke("Ошибка подтверждения покупки");
            },
            onSuccess: () =>
            {
                Debug.Log("PaymentManager: Покупка успешно подтверждена");
                ProcessPurchaseReward(productId);
            }
        );
    }

    /// <summary>
    /// Обрабатывает награду за покупку
    /// </summary>
    private void ProcessPurchaseReward(string productId)
    {
        var package = GetPackageInfo(productId);
        if (package == null)
        {
            return;
        }

        // Выдаем монеты
        if (package.coins > 0 && gameManager != null)
        {
            gameManager.AddCoins(package.coins);
        }

        // Выдаем бустеры
        if (package.bombs > 0)
        {
            AddBoosters("bomb", package.bombs);
        }
        if (package.paws > 0)
        {
            AddBoosters("paw", package.paws);
        }
        if (package.cats > 0)
        {
            AddBoosters("cat", package.cats);
        }

        // Уведомляем о успешной покупке
        OnPurchaseSuccessful?.Invoke(productId, package.coins, package.bombs, package.paws, package.cats);
    }

    /// <summary>
    /// Добавляет бустеры игроку
    /// </summary>
    private void AddBoosters(string boosterType, int count)
    {
        // Получаем текущее количество бустеров
        int currentCount = GetCurrentBoosterCount(boosterType);
        int newCount = currentCount + count;
        
        // Сохраняем новое количество
        SaveBoosterCount(boosterType, newCount);
        
        // Обновляем UI
        UpdateBoosterUI(boosterType, newCount);
    }

    /// <summary>
    /// Получает текущее количество бустеров
    /// </summary>
    private int GetCurrentBoosterCount(string boosterType)
    {
        return GameDataManager.GetBoosterCount(boosterType);
    }

    /// <summary>
    /// Сохраняет количество бустеров
    /// </summary>
    private void SaveBoosterCount(string boosterType, int count)
    {
        // Данные теперь сохраняются через GameDataManager автоматически
    }

    /// <summary>
    /// Обновляет UI бустеров
    /// </summary>
    private void UpdateBoosterUI(string boosterType, int count)
    {
        // Обновляем UI через ShopBillingManager, если он существует
        if (ShopBillingManager.Instance != null)
        {
            ShopBillingManager.Instance.InitializeBoosterUI();
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
    /// Проверяет инициализирован ли payment manager
    /// </summary>
    public bool IsPaymentInitialized()
    {
        return isInitialized;
    }

    /// <summary>
    /// Преобразует техническую ошибку в понятное пользователю сообщение
    /// </summary>
    private string GetUserFriendlyErrorMessage(RuStoreError error)
    {
        // Обрабатываем новые типы ошибок Pay SDK
        if (error is RuStorePaymentException paymentException)
        {
            if (paymentException is RuStorePaymentException.ProductPurchaseCancelled)
            {
                return "Покупка отменена пользователем";
            }
            else if (paymentException is RuStorePaymentException.ProductPurchaseException)
            {
                return "Ошибка при обработке покупки";
            }
            else if (paymentException is RuStorePaymentException.RuStorePayInvalidConsoleAppId)
            {
                return "Неверный ID приложения. Обратитесь к разработчику";
            }
            else if (paymentException is RuStorePaymentException.RuStorePaySignatureException)
            {
                return "Ошибка безопасности. Попробуйте позже";
            }
        }

        // Обрабатываем стандартные ошибки
        switch (error.name)
        {
            case "RuStoreNotInstalledException":
                return "RuStore не установлен. Пожалуйста, установите RuStore для совершения покупок.";
            
            case "RuStoreOutdatedException":
                return "Устаревшая версия RuStore. Пожалуйста, обновите приложение.";
            
            case "RuStoreUserUnauthorizedException":
                return "Необходимо войти в аккаунт RuStore для совершения покупок.";
            
            case "RuStoreRequestLimitReached":
                return "Слишком много запросов. Подождите немного и попробуйте снова.";
            
            default:
                return $"Ошибка покупки: {error.description}";
        }
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
