using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI контроллер для панели обновления игры
/// Управляет отображением информации об обновлении и кнопками
/// </summary>
public class GameUpdatePanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button updateButton;
    [SerializeField] private Button laterButton;  // Опционально, для гибкого обновления
    
    [Header("Texts")]
    [SerializeField] private string titleRu = "Доступно обновление!";
    [SerializeField] private string descriptionRu = "Для продолжения игры необходимо установить последнюю версию";
    [SerializeField] private string updateButtonTextRu = "Обновить";
    [SerializeField] private string laterButtonTextRu = "Позже";
    
    [Header("Animation")]
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private float animationDuration = 0.3f;
    
    private GameUpdateManager updateManager;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        // Получаем или добавляем CanvasGroup для анимации
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Настраиваем UI
        SetupUI();
    }
    
    private void OnEnable()
    {
        // Сбрасываем состояние кнопки при каждой активации
        ResetButtonState();
        
        // Анимация появления панели
        if (useScaleAnimation)
        {
            AnimateIn();
        }
    }
    
    private void OnDisable()
    {
        // Останавливаем все анимации при деактивации
        StopAllCoroutines();
        
        // Сбрасываем визуальное состояние
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Получение ссылки на GameUpdateManager (ленивая инициализация)
    /// </summary>
    private GameUpdateManager GetUpdateManager()
    {
        if (updateManager == null)
        {
            // Используем новый метод вместо устаревшего FindObjectOfType
            updateManager = FindFirstObjectByType<GameUpdateManager>();
            
            if (updateManager == null)
            {
                Debug.LogError("[GameUpdatePanelUI] GameUpdateManager not found in scene!");
            }
            else
            {
                Debug.Log("[GameUpdatePanelUI] GameUpdateManager found successfully");
            }
        }
        
        return updateManager;
    }
    
    /// <summary>
    /// Сброс состояния кнопки
    /// </summary>
    private void ResetButtonState()
    {
        if (updateButton != null)
        {
            updateButton.interactable = true;
        }
        
        if (laterButton != null)
        {
            laterButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Настройка UI элементов
    /// </summary>
    private void SetupUI()
    {
        // Устанавливаем тексты
        if (titleText != null)
        {
            titleText.text = titleRu;
        }
        else
        {
            Debug.LogWarning("[GameUpdatePanelUI] Title Text is not assigned!");
        }
            
        if (descriptionText != null)
        {
            descriptionText.text = descriptionRu;
        }
        else
        {
            Debug.LogWarning("[GameUpdatePanelUI] Description Text is not assigned!");
        }
        
        // Настраиваем кнопку "Обновить"
        SetupUpdateButton();
        
        // Настраиваем кнопку "Позже" (если есть)
        SetupLaterButton();
    }
    
    /// <summary>
    /// Настройка кнопки "Обновить"
    /// </summary>
    private void SetupUpdateButton()
    {
        if (updateButton != null)
        {
            // Очищаем старые слушатели
            updateButton.onClick.RemoveAllListeners();
            
            // Добавляем новый слушатель
            updateButton.onClick.AddListener(OnUpdateButtonClicked);
            
            // Устанавливаем текст кнопки
            TextMeshProUGUI buttonText = updateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = updateButtonTextRu;
            }
            
            Debug.Log("[GameUpdatePanelUI] Update button configured");
        }
        else
        {
            Debug.LogError("[GameUpdatePanelUI] Update Button is not assigned!");
        }
    }
    
    /// <summary>
    /// Настройка кнопки "Позже" (опционально)
    /// </summary>
    private void SetupLaterButton()
    {
        if (laterButton != null)
        {
            // Очищаем старые слушатели
            laterButton.onClick.RemoveAllListeners();
            
            // Добавляем новый слушатель
            laterButton.onClick.AddListener(OnLaterButtonClicked);
            
            // Устанавливаем текст кнопки
            TextMeshProUGUI buttonText = laterButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = laterButtonTextRu;
            }
            
            Debug.Log("[GameUpdatePanelUI] Later button configured");
        }
        else
        {
            // Кнопка "Позже" опциональна, не логируем ошибку
            Debug.Log("[GameUpdatePanelUI] Later button not assigned (optional)");
        }
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки "Обновить"
    /// </summary>
    private void OnUpdateButtonClicked()
    {
        Debug.Log("[GameUpdatePanelUI] Update button clicked");
        
        // Отключаем кнопку для предотвращения повторных нажатий
        if (updateButton != null)
        {
            updateButton.interactable = false;
        }
        
        // Получаем менеджер (ленивая инициализация)
        var manager = GetUpdateManager();
        
        // Запускаем обновление через менеджер
        if (manager != null)
        {
            manager.StartUpdate();
        }
        else
        {
            Debug.LogError("[GameUpdatePanelUI] Cannot start update - GameUpdateManager not found!");
            
            // Включаем кнопку обратно, если менеджер не найден
            if (updateButton != null)
            {
                updateButton.interactable = true;
            }
        }
    }
    
    /// <summary>
    /// Обработчик нажатия кнопки "Позже"
    /// Используется только для гибких обновлений (FLEXIBLE)
    /// </summary>
    private void OnLaterButtonClicked()
    {
        Debug.Log("[GameUpdatePanelUI] Later button clicked");
        
        // Можно запустить гибкое обновление в фоне
        // и позволить игроку продолжить играть
        
        // Скрываем панель
        gameObject.SetActive(false);
        
        // Опционально: можно вызвать метод для гибкого обновления
        // if (updateManager != null)
        // {
        //     updateManager.StartFlexibleUpdate();
        // }
    }
    
    /// <summary>
    /// Анимация появления панели
    /// </summary>
    private void AnimateIn()
    {
        if (canvasGroup == null) return;
        
        // Останавливаем предыдущую анимацию, если есть
        StopAllCoroutines();
        
        // Запускаем новую анимацию
        StartCoroutine(AnimateInCoroutine());
    }
    
    /// <summary>
    /// Корутина анимации появления
    /// </summary>
    private System.Collections.IEnumerator AnimateInCoroutine()
    {
        // Начальное состояние
        float startScale = 0.8f;
        float endScale = 1f;
        float startAlpha = 0f;
        float endAlpha = 1f;
        
        transform.localScale = Vector3.one * startScale;
        canvasGroup.alpha = startAlpha;
        
        float elapsedTime = 0f;
        
        // Анимация
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            
            // Easing функция (ease out back)
            float scale = Mathf.Lerp(startScale, endScale, EaseOutBack(t));
            transform.localScale = Vector3.one * scale;
            
            // Easing функция для alpha (ease out quad)
            float alpha = Mathf.Lerp(startAlpha, endAlpha, EaseOutQuad(t));
            canvasGroup.alpha = alpha;
            
            yield return null;
        }
        
        // Финальное состояние
        transform.localScale = Vector3.one * endScale;
        canvasGroup.alpha = endAlpha;
    }
    
    /// <summary>
    /// Easing функция: Ease Out Back
    /// </summary>
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    /// <summary>
    /// Easing функция: Ease Out Quad
    /// </summary>
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    /// <summary>
    /// Публичный метод для обновления текстов (для локализации)
    /// </summary>
    public void SetTexts(string title, string description, string updateButtonText = null, string laterButtonText = null)
    {
        if (titleText != null && !string.IsNullOrEmpty(title))
        {
            titleText.text = title;
        }
        
        if (descriptionText != null && !string.IsNullOrEmpty(description))
        {
            descriptionText.text = description;
        }
        
        if (updateButton != null && !string.IsNullOrEmpty(updateButtonText))
        {
            TextMeshProUGUI buttonText = updateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = updateButtonText;
            }
        }
        
        if (laterButton != null && !string.IsNullOrEmpty(laterButtonText))
        {
            TextMeshProUGUI buttonText = laterButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = laterButtonText;
            }
        }
    }
    
    /// <summary>
    /// Показать/скрыть кнопку "Позже"
    /// </summary>
    public void SetLaterButtonVisible(bool visible)
    {
        if (laterButton != null)
        {
            laterButton.gameObject.SetActive(visible);
        }
    }
}

