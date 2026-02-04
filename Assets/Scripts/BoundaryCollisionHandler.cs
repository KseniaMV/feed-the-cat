using UnityEngine;

/// <summary>
/// Обрабатывает столкновения продуктов с стенками контейнера
/// Добавляет дополнительные эффекты при столкновении (звук, частицы)
/// 
/// ТРЕБОВАНИЯ К СТЕНКЕ:
/// - Collider2D (настроенный в редакторе)
/// - Rigidbody2D с Body Type = Static (настроенный в редакторе)
/// - PhysicsMaterial2D с нужными настройками отскока и трения (созданный в редакторе)
/// </summary>
public class BoundaryCollisionHandler : MonoBehaviour
{
    [Header("Настройки эффектов")]
    [Tooltip("Сила отскока при столкновении")]
    public float bounceForce = 2f;
    
    [Tooltip("Демпфирование скорости после отскока")]
    [Range(0f, 1f)]
    public float velocityDamping = 0.8f;
    
    [Tooltip("Минимальная скорость для создания эффекта отскока")]
    public float minBounceVelocity = 1f;
    
    [Tooltip("Эффект частиц при столкновении (опционально)")]
    public GameObject collisionEffect;
    
    [Tooltip("Звук столкновения (опционально)")]
    public AudioClip collisionSound;
    
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что столкновение происходит с продуктом
        FoodItem foodItem = collision.gameObject.GetComponent<FoodItem>();
        if (foodItem == null || !foodItem.isDroped) return;
        
        // Получаем Rigidbody2D продукта
        Rigidbody2D foodRigidbody = collision.rigidbody;
        if (foodRigidbody == null) return;
        
        // Проверяем скорость столкновения
        float collisionVelocity = collision.relativeVelocity.magnitude;
        if (collisionVelocity < minBounceVelocity) return;
        
        // Создаем эффект отскока
        CreateBounceEffect(collision, foodRigidbody);
        
        // Создаем визуальный эффект
        CreateVisualEffect(collision.contacts[0].point);
        
        // Воспроизводим звук
        PlayCollisionSound(collision.contacts[0].point);
        
    }
    
    /// <summary>
    /// Создает эффект отскока
    /// </summary>
    private void CreateBounceEffect(Collision2D collision, Rigidbody2D foodRigidbody)
    {
        // Вычисляем направление отскока
        Vector2 bounceDirection = collision.contacts[0].normal;
        
        // Применяем силу отскока
        Vector2 bounceForceVector = bounceDirection * bounceForce * collision.relativeVelocity.magnitude;
        foodRigidbody.AddForce(bounceForceVector, ForceMode2D.Impulse);
        
        // Применяем демпфирование скорости
        foodRigidbody.linearVelocity *= velocityDamping;
        
        // Ограничиваем максимальную скорость
        if (foodRigidbody.linearVelocity.magnitude > 10f)
        {
            foodRigidbody.linearVelocity = foodRigidbody.linearVelocity.normalized * 10f;
        }
    }
    
    /// <summary>
    /// Создает визуальный эффект при столкновении
    /// </summary>
    private void CreateVisualEffect(Vector2 contactPoint)
    {
        if (collisionEffect != null)
        {
            GameObject effect = Instantiate(collisionEffect, contactPoint, Quaternion.identity);
            
            // Уничтожаем эффект через некоторое время
            Destroy(effect, 1f);
        }
    }
    
    /// <summary>
    /// Воспроизводит звук столкновения
    /// </summary>
    private void PlayCollisionSound(Vector2 contactPoint)
    {
        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, contactPoint);
        }
    }
}
