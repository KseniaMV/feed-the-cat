using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Скрипт для тестирования интеграции рекламы с наградой
/// Помогает проверить работу всех компонентов
/// </summary>
public class AdIntegrationTester : MonoBehaviour
{
    [Header("UI элементы для тестирования")]
    [Tooltip("Кнопка тестирования загрузки рекламы")]
    public Button testLoadAdButton;
    
    [Tooltip("Кнопка тестирования показа рекламы")]
    public Button testShowAdButton;
    
    [Tooltip("Кнопка принудительной перезагрузки рекламы")]
    public Button testReloadAdButton;
    
    [Tooltip("Текст статуса рекламы")]
    public TextMeshProUGUI statusText;
    
    [Tooltip("Текст логов")]
    public TextMeshProUGUI logText;

    [Header("Настройки тестирования")]
    [Tooltip("Включить автоматическое тестирование")]
    public bool enableAutoTest = false;
    
    [Tooltip("Интервал автоматического тестирования (секунды)")]
    public float autoTestInterval = 30f;

    private float lastAutoTestTime = 0f;
    private string logMessages = "";

    private void Start()
    {
        SetupButtonListeners();
        SubscribeToEvents();
        UpdateStatus();
    }

    private void Update()
    {
        if (enableAutoTest && Time.time - lastAutoTestTime > autoTestInterval)
        {
            RunAutoTest();
            lastAutoTestTime = Time.time;
        }
    }

    /// <summary>
    /// Настраивает обработчики кнопок
    /// </summary>
    private void SetupButtonListeners()
    {
        if (testLoadAdButton != null)
            testLoadAdButton.onClick.AddListener(TestLoadAd);
            
        if (testShowAdButton != null)
            testShowAdButton.onClick.AddListener(TestShowAd);
            
        if (testReloadAdButton != null)
            testReloadAdButton.onClick.AddListener(TestReloadAd);
    }

    /// <summary>
    /// Подписывается на события рекламы
    /// </summary>
    private void SubscribeToEvents()
    {
        RewardedAdManager.OnAdLoaded += OnAdLoaded;
        RewardedAdManager.OnAdFailedToLoad += OnAdFailedToLoad;
        RewardedAdManager.OnAdShown += OnAdShown;
        RewardedAdManager.OnAdFailedToShow += OnAdFailedToShow;
        RewardedAdManager.OnAdDismissed += OnAdDismissed;
        RewardedAdManager.OnRewardEarned += OnRewardEarned;
        RewardedAdManager.OnRewardNotEarned += OnRewardNotEarned;
    }

    /// <summary>
    /// Тестирует загрузку рекламы
    /// </summary>
    private void TestLoadAd()
    {
        AddLog("Тестирование загрузки рекламы...");
        
        if (RewardedAdManager.Instance != null)
        {
            RewardedAdManager.Instance.RequestRewardedAd();
        }
        else
        {
            AddLog("ОШИБКА: RewardedAdManager не найден!");
        }
    }

    /// <summary>
    /// Тестирует показ рекламы
    /// </summary>
    private void TestShowAd()
    {
        AddLog("Тестирование показа рекламы...");
        
        if (RewardedAdManager.Instance != null)
        {
            if (RewardedAdManager.Instance.IsAdAvailable())
            {
                RewardedAdManager.Instance.ShowRewardedAd();
            }
            else
            {
                AddLog("ПРЕДУПРЕЖДЕНИЕ: Реклама недоступна для показа");
            }
        }
        else
        {
            AddLog("ОШИБКА: RewardedAdManager не найден!");
        }
    }

    /// <summary>
    /// Тестирует перезагрузку рекламы
    /// </summary>
    private void TestReloadAd()
    {
        AddLog("Тестирование перезагрузки рекламы...");
        
        if (RewardedAdManager.Instance != null)
        {
            RewardedAdManager.Instance.ForceReloadAd();
        }
        else
        {
            AddLog("ОШИБКА: RewardedAdManager не найден!");
        }
    }

    /// <summary>
    /// Запускает автоматическое тестирование
    /// </summary>
    private void RunAutoTest()
    {
        AddLog("=== АВТОМАТИЧЕСКОЕ ТЕСТИРОВАНИЕ ===");
        TestLoadAd();
    }

    /// <summary>
    /// Обработчики событий рекламы
    /// </summary>
    private void OnAdLoaded()
    {
        AddLog("✓ Реклама загружена успешно");
        UpdateStatus();
    }

    private void OnAdFailedToLoad(string error)
    {
        AddLog($"✗ Ошибка загрузки рекламы: {error}");
        UpdateStatus();
    }

    private void OnAdShown()
    {
        AddLog("✓ Реклама показана");
        UpdateStatus();
    }

    private void OnAdFailedToShow(string error)
    {
        AddLog($"✗ Ошибка показа рекламы: {error}");
        UpdateStatus();
    }

    private void OnAdDismissed()
    {
        AddLog("✓ Реклама закрыта");
        UpdateStatus();
    }

    private void OnRewardEarned()
    {
        AddLog("✓ НАГРАДА ПОЛУЧЕНА! Бомбочка добавлена");
        UpdateStatus();
    }

    private void OnRewardNotEarned()
    {
        AddLog("⚠ Награда НЕ получена (игрок не досмотрел рекламу)");
        UpdateStatus();
    }

    /// <summary>
    /// Добавляет сообщение в лог
    /// </summary>
    private void AddLog(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        logMessages = $"[{timestamp}] {message}\n{logMessages}";
        
        if (logText != null)
        {
            logText.text = logMessages;
        }
        
        Debug.Log($"AdIntegrationTester: {message}");
    }

    /// <summary>
    /// Обновляет статус
    /// </summary>
    private void UpdateStatus()
    {
        if (statusText != null && RewardedAdManager.Instance != null)
        {
            string status = RewardedAdManager.Instance.IsAdAvailable() ? "Доступна" : "Недоступна";
            statusText.text = $"Статус рекламы: {status}";
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        RewardedAdManager.OnAdLoaded -= OnAdLoaded;
        RewardedAdManager.OnAdFailedToLoad -= OnAdFailedToLoad;
        RewardedAdManager.OnAdShown -= OnAdShown;
        RewardedAdManager.OnAdFailedToShow -= OnAdFailedToShow;
        RewardedAdManager.OnAdDismissed -= OnAdDismissed;
        RewardedAdManager.OnRewardEarned -= OnRewardEarned;
        RewardedAdManager.OnRewardNotEarned -= OnRewardNotEarned;
    }
}
