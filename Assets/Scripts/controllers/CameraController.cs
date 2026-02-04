using UnityEngine;

/// <summary>
/// Контроллер камеры для правильного отображения игрового процесса
/// Обеспечивает корректное отображение продуктов в мировом пространстве
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Настройки камеры")]
    [Tooltip("Целевая позиция камеры")]
    public Vector3 targetPosition = new Vector3(0, 0, -10);
    
    [Tooltip("Размер ортографической камеры")]
    public float orthographicSize = 8f;
    
    [Tooltip("Скорость перемещения камеры")]
    public float moveSpeed = 5f;
    
    [Header("Ссылки")]
    [Tooltip("Ссылка на Canvas")]
    public Canvas uiCanvas;
    
    [Tooltip("Ссылка на GameContainer")]
    public Transform gameContainer;
    
    private Camera mainCamera;
    private bool isInitialized = false;
    
    /// <summary>
    /// Инициализация камеры
    /// </summary>
    void Start()
    {
        InitializeCamera();
    }
    
    /// <summary>
    /// Обновление камеры
    /// </summary>
    void Update()
    {
        if (!isInitialized)
        {
            InitializeCamera();
        }
        
        // Плавно перемещаем камеру к целевой позиции
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Инициализация настроек камеры
    /// </summary>
    private void InitializeCamera()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            return;
        }
        
        // Настраиваем камеру для отображения мирового пространства
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = orthographicSize;
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 100f;
        
        // Позиционируем камеру
        transform.position = targetPosition;
        
        // Настраиваем Canvas для World Space если нужно
        if (uiCanvas != null)
        {
            SetupCanvasForWorldSpace();
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Настройка Canvas для World Space
    /// </summary>
    private void SetupCanvasForWorldSpace()
    {
        if (uiCanvas.renderMode != RenderMode.WorldSpace)
        {
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.worldCamera = mainCamera;
            
            // Настраиваем размер Canvas
            RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.sizeDelta = new Vector2(1920, 1080); // Стандартный размер
                canvasRect.localScale = Vector3.one * 0.01f; // Масштабируем для мирового пространства
            }
            
        }
    }
    
    /// <summary>
    /// Установка целевой позиции камеры
    /// </summary>
    /// <param name="position">Новая позиция</param>
    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
    }
    
    /// <summary>
    /// Установка размера ортографической камеры
    /// </summary>
    /// <param name="size">Новый размер</param>
    public void SetOrthographicSize(float size)
    {
        orthographicSize = size;
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = size;
        }
    }
    
    /// <summary>
    /// Фокус на контейнере
    /// </summary>
    public void FocusOnContainer()
    {
        if (gameContainer != null)
        {
            Vector3 containerPosition = gameContainer.position;
            containerPosition.z = targetPosition.z; // Сохраняем Z координату камеры
            SetTargetPosition(containerPosition);
        }
    }
    
    /// <summary>
    /// Сброс камеры в исходное положение
    /// </summary>
    public void ResetCamera()
    {
        targetPosition = new Vector3(0, 0, -10);
        orthographicSize = 8f;
        InitializeCamera();
    }
}
