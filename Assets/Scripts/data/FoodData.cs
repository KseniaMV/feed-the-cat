using UnityEngine;

/// <summary>
/// ScriptableObject для хранения данных о продуктах питания
/// Содержит информацию о типе продукта, его свойствах и наградах
/// </summary>
[CreateAssetMenu(fileName = "New Food Data", menuName = "Feed The Cat/Food Data")]
public class FoodData : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Тип продукта (sausage, eggs, sandwich, etc.)")]
    public FoodType foodType;
    
    [Tooltip("Название продукта для отображения")]
    public string foodName;
    
    [Tooltip("Описание продукта")]
    public string description;
    
    [Header("Категория и редкость")]
    [Tooltip("Категория продукта (базовый, средний, редкий, эпический)")]
    public FoodCategory category;
    
    [Tooltip("Редкость продукта (1-4)")]
    [Range(1, 4)]
    public int rarity = 1;
    
    [Header("Визуальные элементы")]
    [Tooltip("Спрайт продукта")]
    public Sprite foodSprite;
    
    [Tooltip("Префаб продукта")]
    public GameObject foodPrefab;
    
    [Header("Игровые характеристики")]
    [Tooltip("Опыт за слияние этого продукта")]
    public int experienceReward;
    
    [Header("Слияние")]
    [Tooltip("Продукт, который получается при слиянии двух одинаковых продуктов")]
    public FoodData nextLevelFood;
    
    [Tooltip("Требуется ли для спавна этого продукта определенный уровень опыта")]
    public int requiredExperience = 0;
    
    [Tooltip("Может ли этот продукт спавниться в начале игры")]
    public bool canSpawnAtStart = false;
}

/// <summary>
/// Перечисление типов продуктов питания
/// </summary>
public enum FoodType
{
    Sausage,     // Сосиска подкопченная
    Eggs,        // Яичница с сосиской и овощами
    Sandwich,    // Сандвич с тунцом и салатом
    Meatball,    // Фрикаделька мясная в томатном соусе с базеликом
    Soup,        // Супчик с фрикадельками и зеленью
    Chicken,     // Куриная грудка на гриле с овощами
    Salmon,      // Лосось на гриле с овощами
    Shrimp,      // Креветка в кляре
    Caviar,      // Тарталетка с красной икрой
    Oyster,      // Свежая устрица
    Lobster      // Королевский лобстер
}

/// <summary>
/// Категории продуктов питания
/// </summary>
public enum FoodCategory
{
    Basic,       // Базовые продукты
    Medium,      // Средние продукты
    Rare,        // Редкие продукты
    Epic         // Эпические продукты
}
