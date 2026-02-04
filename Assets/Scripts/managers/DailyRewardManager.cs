using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailyRewardManager : MonoBehaviour
{
    [Header("Основная информация о наградах")]
    [Tooltip("Массив наград для дней 1-7")]
    public RewardData[] rewardData;
    [Tooltip("Стартовая награда (день 0)")]
    public RewardData startGameReward;

    [Header("Ссылки на UI элементы")]
    [Tooltip("Панель наград")]
    public GameObject rewardPanel;
    [Tooltip("Панель бонусов текущего дня")]
    public GameObject bonusPanel;
    [Tooltip("Панель бонусов следующего дня")]
    public GameObject nextDayBonusPanel;
    [Tooltip("Титул панели наград")]
    public TMP_Text rewardText;
    
    [Header("Префабы")]
    [Tooltip("Префаб бонуса для текущего дня")]
    public GameObject bonusPrefab;
    [Tooltip("Префаб бонуса для следующего дня")]
    public GameObject nextDayBonusPrefab;

    [Tooltip("Титул стартового бонуса")]
    public string startRewardText = "Стартовый бонус";
    [Tooltip("Титул ежедневного бонуса")]
    public string dailyRewardText = "Ежедневный бонус";

    [Header("Данные по датам")]
    public int currentDay = 0;
    public int nextDay;
    
    // Ключи для сохранения в PlayerPrefs
    private const string LAST_LOGIN_DATE_KEY = "LastLoginDate";
    private const string CURRENT_REWARD_DAY_KEY = "CurrentRewardDay";
    private const string REWARD_CLAIMED_TODAY_KEY = "RewardClaimedToday";
    private const string FIRST_TIME_PLAYER_KEY = "FirstTimePlayer";

    // Текущие данные награды
    private RewardData currentRewardData;
    private RewardData nextDayRewardData;
    
    void Start()
    {
        Debug.Log($"DailyRewardManager: Start() вызван. Time.timeScale = {Time.timeScale}");
        // Инициализация теперь управляется GameManager
        InitializeDailyRewardSystem();
    }

    /// <summary>
    /// Инициализирует систему ежедневных наград (вызывается из GameManager)
    /// </summary>
    public void InitializeDailyRewardSystem()
    {
        Debug.Log("DailyRewardManager: Инициализация системы ежедневных наград");
        CheckDailyReward();
    }
    
    /// <summary>
    /// Проверяет и обновляет систему ежедневных наград
    /// </summary>
    private void CheckDailyReward()
    {
        Debug.Log("=== DailyRewardManager: Начинаем CheckDailyReward() ===");
        
        // Проверяем, первый ли раз игрок запускает игру
        bool isFirstTime = PlayerPrefs.GetInt(FIRST_TIME_PLAYER_KEY, 1) == 1;
        Debug.Log($"DailyRewardManager: isFirstTime = {isFirstTime}");
        
        if (isFirstTime)
        {
            // Первый запуск - выдаем стартовый бонус (день 0)
            currentDay = 0;
            PlayerPrefs.SetInt(FIRST_TIME_PLAYER_KEY, 0);
            PlayerPrefs.SetInt(CURRENT_REWARD_DAY_KEY, 0);
            PlayerPrefs.SetString(LAST_LOGIN_DATE_KEY, DateTime.Now.ToString("yyyy-MM-dd"));
            PlayerPrefs.SetInt(REWARD_CLAIMED_TODAY_KEY, 0);
            PlayerPrefs.Save();
            
            Debug.Log("DailyRewardManager: Первый запуск игры. Установлен день 0 (стартовый бонус)");
            Debug.Log($"DailyRewardManager: REWARD_CLAIMED_TODAY_KEY установлен в 0 (награда НЕ получена)");
        }
        else
        {
            // Загружаем сохраненные данные
            string lastLoginDate = PlayerPrefs.GetString(LAST_LOGIN_DATE_KEY, "");
            int savedRewardDay = PlayerPrefs.GetInt(CURRENT_REWARD_DAY_KEY, 1);
            int currentClaimedFlag = PlayerPrefs.GetInt(REWARD_CLAIMED_TODAY_KEY, 0);
            
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
            
            Debug.Log($"DailyRewardManager: Загружены данные - lastLoginDate: {lastLoginDate}, savedRewardDay: {savedRewardDay}, currentClaimedFlag: {currentClaimedFlag}, todayDate: {todayDate}");
            
            // Проверяем, новый ли это день
            if (lastLoginDate != todayDate)
            {
                // Новый день - проверяем, был ли пропуск
                if (!string.IsNullOrEmpty(lastLoginDate))
                {
                    DateTime lastLogin = DateTime.Parse(lastLoginDate);
                    DateTime today = DateTime.Now.Date;
                    int daysDifference = (today - lastLogin).Days;
                    
                    if (daysDifference > 1)
                    {
                        // Пропущен день - сбрасываем на день 1
                        currentDay = 1;
                        Debug.Log($"DailyRewardManager: Пропущено дней: {daysDifference}. Прогресс сброшен на день 1");
                    }
                    else if (daysDifference == 1)
                    {
                        // Следующий день - увеличиваем счетчик
                        if (savedRewardDay == 0)
                        {
                            // После стартового бонуса идет день 1
                            currentDay = 1;
                        }
                        else
                        {
                            // Циклические дни 1-7
                            currentDay = (savedRewardDay % 7) + 1;
                        }
                        Debug.Log($"DailyRewardManager: Новый день. Предыдущий день: {savedRewardDay}, текущий день: {currentDay}");
                    }
                }
                else
                {
                    currentDay = 1;
                }
                
                // Обновляем данные
                PlayerPrefs.SetString(LAST_LOGIN_DATE_KEY, todayDate);
                PlayerPrefs.SetInt(CURRENT_REWARD_DAY_KEY, currentDay);
                PlayerPrefs.SetInt(REWARD_CLAIMED_TODAY_KEY, 0); // Награда еще не получена
                PlayerPrefs.Save();
            }
            else
            {
                // Тот же день - загружаем сохраненный день
                currentDay = savedRewardDay;
                Debug.Log($"DailyRewardManager: Тот же день ({todayDate}), день награды: {currentDay}");
                Debug.Log($"DailyRewardManager: Флаг получения награды остается: {currentClaimedFlag} (0=не получена, 1=получена)");
            }
        }
        
        // Вычисляем следующий день
        if (currentDay == 0)
        {
            nextDay = 1;
        }
        else
        {
            nextDay = (currentDay % 7) + 1;
        }
        
        // Проверяем статус получения награды
        bool rewardClaimed = IsRewardClaimedToday();
        Debug.Log($"DailyRewardManager: Текущий день награды: {currentDay}, следующий день: {nextDay}, награда получена: {rewardClaimed}");
        
        // Устанавливаем данные наград
        SetCurrentDailyReward();
        SetNextDayReward();
        
        // Показ панели теперь управляется GameManager
        Debug.Log($"DailyRewardManager: Инициализация завершена. Награда получена: {rewardClaimed}");
    }

    /// <summary>
    /// Устанавливает текущую награду (на текущий день)
    /// </summary>
    public void SetCurrentDailyReward()
    {
        // Определяем данные награды для текущего дня
        if (currentDay == 0)
        {
            currentRewardData = startGameReward;
            if (rewardText != null)
            {
                rewardText.text = startRewardText;
            }
        }
        else
        {
            // Дни 1-7 (индексы массива 0-6)
            int rewardIndex = currentDay - 1;
            if (rewardIndex >= 0 && rewardIndex < rewardData.Length)
            {
                currentRewardData = rewardData[rewardIndex];
                if (rewardText != null)
                {
                    rewardText.text = dailyRewardText + " - День " + currentDay;
                }
            }
            else
            {
                Debug.LogError($"DailyRewardManager: Нет данных награды для дня {currentDay}");
                return;
            }
        }
        
        // Заполняем UI для текущей награды
        if (currentRewardData != null && bonusPanel != null)
        {
            FillBonusPanel(bonusPanel, currentRewardData);
        }
    }

    /// <summary>
    /// Устанавливает награду на следующий день
    /// </summary>
    public void SetNextDayReward()
    {
        // Определяем данные награды для следующего дня (дни 1-7)
        int rewardIndex = nextDay - 1;
        if (rewardIndex >= 0 && rewardIndex < rewardData.Length)
        {
            nextDayRewardData = rewardData[rewardIndex];
            
            // Заполняем UI для следующей награды
            if (nextDayRewardData != null && nextDayBonusPanel != null)
            {
                FillBonusPanel(nextDayBonusPanel, nextDayRewardData);
            }
        }
        else
        {
            Debug.LogError($"DailyRewardManager: Нет данных награды для следующего дня {nextDay}");
        }
    }

    /// <summary>
    /// Заполняет панель бонусов данными из RewardData
    /// </summary>
    private void FillBonusPanel(GameObject panel, RewardData reward)
    {
        if (panel == null || reward == null) return;
        
        // Определяем нужный префаб
        GameObject prefabToUse = (panel == bonusPanel) ? bonusPrefab : nextDayBonusPrefab;
        
        if (prefabToUse == null)
        {
            Debug.LogError("DailyRewardManager: Префаб бонуса не назначен!");
            return;
        }
        
        // Получаем все BonusController в панели
        BonusController[] bonusControllers = panel.GetComponentsInChildren<BonusController>(true);
        
        // Если контроллеров нет или их меньше 3 - создаем недостающие
        if (bonusControllers.Length < 3)
        {
            // Удаляем старые, если есть
            foreach (var controller in bonusControllers)
            {
                if (controller != null)
                    Destroy(controller.gameObject);
            }
            
            // Создаем 3 новых
            bonusControllers = new BonusController[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject bonusObj = Instantiate(prefabToUse, panel.transform);
                bonusControllers[i] = bonusObj.GetComponent<BonusController>();
                
                if (bonusControllers[i] == null)
                {
                    Debug.LogError($"DailyRewardManager: Префаб {prefabToUse.name} не содержит компонент BonusController!");
                }
            }
        }
        
        // Массивы данных из RewardData
        string[] bonusTypes = { reward.bonusType_1, reward.bonusType_2, reward.bonusType_3 };
        string[] bonusCounts = { reward.bonusCount_1, reward.bonusCount_2, reward.bonusCount_3 };
        Sprite[] bonusSprites = { reward.bonusSprite_1, reward.bonusSprite_2, reward.bonusSprite_3 };
        
        // Заполняем контроллеры
        for (int i = 0; i < bonusControllers.Length && i < 3; i++)
        {
            if (bonusControllers[i] == null) continue;
            
            if (!string.IsNullOrEmpty(bonusTypes[i]))
            {
                bonusControllers[i].gameObject.SetActive(true);
                
                // Устанавливаем изображение
                if (bonusControllers[i].bonusImage != null && bonusSprites[i] != null)
                {
                    bonusControllers[i].bonusImage.sprite = bonusSprites[i];
                }
                
                // Устанавливаем количество
                if (bonusControllers[i].bonusCount != null)
                {
                    bonusControllers[i].bonusCount.text = bonusCounts[i];
                }
            }
            else
            {
                // Скрываем неиспользуемые слоты (если нет названия типа бонуса)
                bonusControllers[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Показывает панель наград
    /// </summary>
    public void ShowRewardPanel()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            Debug.Log("DailyRewardManager: Панель наград показана");
        }
    }

    /// <summary>
    /// Скрывает панель наград
    /// </summary>
    public void HideRewardPanel()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
            Debug.Log("DailyRewardManager: Панель наград скрыта");
        }
    }

    /// <summary>
    /// Выдает награду игроку
    /// </summary>
    public void GetReward()
    {
        // Проверяем, не получена ли уже награда сегодня
        if (IsRewardClaimedToday())
        {
            Debug.LogWarning("DailyRewardManager: Награда уже была получена сегодня! Повторное получение невозможно.");
            HideRewardPanel();
            return;
        }
        
        if (currentRewardData == null)
        {
            Debug.LogError("DailyRewardManager: Нет данных награды для выдачи");
            return;
        }
        
        // Массивы данных из RewardData
        string[] bonusTypes = { currentRewardData.bonusType_1, currentRewardData.bonusType_2, currentRewardData.bonusType_3 };
        string[] bonusCounts = { currentRewardData.bonusCount_1, currentRewardData.bonusCount_2, currentRewardData.bonusCount_3 };
        
        // Выдаем награды
        for (int i = 0; i < 3; i++)
        {
            if (!string.IsNullOrEmpty(bonusTypes[i]) && !string.IsNullOrEmpty(bonusCounts[i]))
            {
                int count = 0;
                if (int.TryParse(bonusCounts[i], out count) && count > 0)
                {
                    GiveRewardToPlayer(bonusTypes[i], count);
                }
            }
        }
        
        // Отмечаем, что награда получена
        PlayerPrefs.SetInt(REWARD_CLAIMED_TODAY_KEY, 1);
        PlayerPrefs.Save();
        
        string todayDate = DateTime.Now.ToString("yyyy-MM-dd");
        Debug.Log($"DailyRewardManager: Награда за день {currentDay} получена. Дата: {todayDate}. Флаг REWARD_CLAIMED_TODAY_KEY установлен в 1");
        
        // Закрываем панель наград
        HideRewardPanel();
    }

    /// <summary>
    /// Выдает конкретную награду игроку
    /// </summary>
    private void GiveRewardToPlayer(string bonusType, int count)
    {
        switch (bonusType.ToLower())
        {
            case "bomb":
                GameDataManager.AddBoosters("bomb", count);
                Debug.Log($"DailyRewardManager: Выдано {count} бустеров 'bomb'");
                break;
                
            case "paw":
                GameDataManager.AddBoosters("paw", count);
                Debug.Log($"DailyRewardManager: Выдано {count} бустеров 'paw'");
                break;
                
            case "cat":
                GameDataManager.AddBoosters("cat", count);
                Debug.Log($"DailyRewardManager: Выдано {count} бустеров 'cat'");
                break;
                
            case "coins":
                GameDataManager.AddCoins(count);
                Debug.Log($"DailyRewardManager: Выдано {count} монет");
                break;
                
            default:
                Debug.LogWarning($"DailyRewardManager: Неизвестный тип награды: {bonusType}");
                break;
        }
    }

    /// <summary>
    /// Сбрасывает прогресс ежедневных наград (для тестирования)
    /// </summary>
    public void ResetDailyRewardProgress()
    {
        PlayerPrefs.DeleteKey(LAST_LOGIN_DATE_KEY);
        PlayerPrefs.DeleteKey(CURRENT_REWARD_DAY_KEY);
        PlayerPrefs.DeleteKey(REWARD_CLAIMED_TODAY_KEY);
        PlayerPrefs.DeleteKey(FIRST_TIME_PLAYER_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("DailyRewardManager: Прогресс ежедневных наград сброшен");
        
        // Перезапускаем проверку
        CheckDailyReward();
    }

    /// <summary>
    /// Проверяет, получена ли награда сегодня
    /// </summary>
    public bool IsRewardClaimedToday()
    {
        int claimedFlag = PlayerPrefs.GetInt(REWARD_CLAIMED_TODAY_KEY, 0);
        return claimedFlag == 1;
    }

    /// <summary>
    /// Возвращает текущий день награды
    /// </summary>
    public int GetCurrentRewardDay()
    {
        return currentDay;
    }
}
