using UnityEngine;
using System;

/// <summary>
/// Утилита для тестирования системы ежедневных наград
/// Используйте Context Menu в редакторе для быстрого тестирования
/// </summary>
public class DailyRewardTester : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Ссылка на DailyRewardManager")]
    public DailyRewardManager dailyRewardManager;

    private void Start()
    {
        // Автоматически находим DailyRewardManager если не назначен
        if (dailyRewardManager == null)
        {
            dailyRewardManager = FindFirstObjectByType<DailyRewardManager>();
        }
    }

    /// <summary>
    /// Сбрасывает прогресс наград для тестирования
    /// </summary>
    [ContextMenu("1. Сбросить прогресс наград")]
    public void ResetRewardProgress()
    {
        if (dailyRewardManager != null)
        {
            dailyRewardManager.ResetDailyRewardProgress();
            Debug.Log("DailyRewardTester: Прогресс наград сброшен");
        }
        else
        {
            Debug.LogError("DailyRewardTester: DailyRewardManager не найден!");
        }
    }

    /// <summary>
    /// Показывает панель наград
    /// </summary>
    [ContextMenu("2. Показать панель наград")]
    public void ShowRewardPanel()
    {
        if (dailyRewardManager != null)
        {
            dailyRewardManager.ShowRewardPanel();
            Debug.Log("DailyRewardTester: Панель наград показана");
        }
        else
        {
            Debug.LogError("DailyRewardTester: DailyRewardManager не найден!");
        }
    }

    /// <summary>
    /// Скрывает панель наград
    /// </summary>
    [ContextMenu("3. Скрыть панель наград")]
    public void HideRewardPanel()
    {
        if (dailyRewardManager != null)
        {
            dailyRewardManager.HideRewardPanel();
            Debug.Log("DailyRewardTester: Панель наград скрыта");
        }
        else
        {
            Debug.LogError("DailyRewardTester: DailyRewardManager не найден!");
        }
    }

    /// <summary>
    /// Выводит информацию о текущем дне награды
    /// </summary>
    [ContextMenu("4. Показать текущий день")]
    public void ShowCurrentDay()
    {
        if (dailyRewardManager != null)
        {
            int currentDay = dailyRewardManager.GetCurrentRewardDay();
            bool claimed = dailyRewardManager.IsRewardClaimedToday();
            
            Debug.Log($"DailyRewardTester: Текущий день награды: {currentDay}");
            Debug.Log($"DailyRewardTester: Награда получена сегодня: {(claimed ? "Да" : "Нет")}");
            Debug.Log($"DailyRewardTester: Последний вход: {PlayerPrefs.GetString("LastLoginDate", "Нет данных")}");
        }
        else
        {
            Debug.LogError("DailyRewardTester: DailyRewardManager не найден!");
        }
    }

    /// <summary>
    /// Симулирует следующий день (для тестирования)
    /// </summary>
    [ContextMenu("5. Симулировать следующий день")]
    public void SimulateNextDay()
    {
        DateTime tomorrow = DateTime.Now.AddDays(1);
        PlayerPrefs.SetString("LastLoginDate", tomorrow.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt("RewardClaimedToday", 0);
        PlayerPrefs.Save();
        
        Debug.Log($"DailyRewardTester: Дата изменена на: {tomorrow:yyyy-MM-dd}");
        Debug.Log("DailyRewardTester: Перезапустите сцену для применения изменений");
    }

    /// <summary>
    /// Симулирует пропуск дня (для тестирования)
    /// </summary>
    [ContextMenu("6. Симулировать пропуск 2+ дней")]
    public void SimulateSkippedDays()
    {
        DateTime threeDaysAgo = DateTime.Now.AddDays(-3);
        PlayerPrefs.SetString("LastLoginDate", threeDaysAgo.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt("RewardClaimedToday", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"DailyRewardTester: Дата последнего входа установлена на: {threeDaysAgo:yyyy-MM-dd}");
        Debug.Log("DailyRewardTester: При следующем запуске прогресс должен сброситься на день 1");
        Debug.Log("DailyRewardTester: Перезапустите сцену для применения изменений");
    }

    /// <summary>
    /// Устанавливает конкретный день награды (для тестирования)
    /// </summary>
    [ContextMenu("7. Установить день 7")]
    public void SetDay7()
    {
        PlayerPrefs.SetInt("CurrentRewardDay", 7);
        PlayerPrefs.SetString("LastLoginDate", DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.SetInt("RewardClaimedToday", 0);
        PlayerPrefs.SetInt("FirstTimePlayer", 0);
        PlayerPrefs.Save();
        
        Debug.Log("DailyRewardTester: Установлен день 7");
        Debug.Log("DailyRewardTester: Перезапустите сцену для применения изменений");
    }

    /// <summary>
    /// Показывает все сохраненные данные
    /// </summary>
    [ContextMenu("8. Показать все данные PlayerPrefs")]
    public void ShowAllPlayerPrefs()
    {
        Debug.Log("=== DailyRewardTester: PlayerPrefs данные ===");
        Debug.Log($"LastLoginDate: {PlayerPrefs.GetString("LastLoginDate", "Нет данных")}");
        Debug.Log($"CurrentRewardDay: {PlayerPrefs.GetInt("CurrentRewardDay", -1)}");
        Debug.Log($"RewardClaimedToday: {PlayerPrefs.GetInt("RewardClaimedToday", -1)}");
        Debug.Log($"FirstTimePlayer: {PlayerPrefs.GetInt("FirstTimePlayer", -1)}");
        Debug.Log("===========================================");
    }

    /// <summary>
    /// Очищает все данные PlayerPrefs (осторожно!)
    /// </summary>
    [ContextMenu("9. ОЧИСТИТЬ ВСЕ PlayerPrefs (ОСТОРОЖНО!)")]
    public void ClearAllPlayerPrefs()
    {
        if (Application.isEditor)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.LogWarning("DailyRewardTester: ВСЕ данные PlayerPrefs очищены! Перезапустите сцену.");
        }
        else
        {
            Debug.LogError("DailyRewardTester: Очистка всех PlayerPrefs доступна только в редакторе!");
        }
    }

    /// <summary>
    /// Добавляет тестовые бустеры для проверки
    /// </summary>
    [ContextMenu("10. Добавить тестовые бустеры")]
    public void AddTestBoosters()
    {
        GameDataManager.AddBoosters("bomb", 3);
        GameDataManager.AddBoosters("paw", 2);
        GameDataManager.AddBoosters("cat", 1);
        GameDataManager.AddCoins(100);
        
        Debug.Log("DailyRewardTester: Добавлены тестовые бустеры:");
        Debug.Log("  - Бомбы: 3");
        Debug.Log("  - Лапки: 2");
        Debug.Log("  - Коты: 1");
        Debug.Log("  - Монеты: 100");
    }

    /// <summary>
    /// Показывает текущее количество бустеров
    /// </summary>
    [ContextMenu("11. Показать количество бустеров")]
    public void ShowBoosterCounts()
    {
        Debug.Log("=== DailyRewardTester: Текущие бустеры ===");
        Debug.Log($"Бомбы: {GameDataManager.GetBoosterCount("bomb")}");
        Debug.Log($"Лапки: {GameDataManager.GetBoosterCount("paw")}");
        Debug.Log($"Коты: {GameDataManager.GetBoosterCount("cat")}");
        Debug.Log($"Монеты: {GameDataManager.CurrentCoins}");
        Debug.Log("=========================================");
    }
}

