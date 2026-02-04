using UnityEngine;

/// <summary>
/// Конфигурация игровой экономики
/// Централизованное хранение всех цен и значений
/// </summary>
[CreateAssetMenu(fileName = "EconomyConfig", menuName = "Game/Economy Config")]
public class EconomyConfig : ScriptableObject
{
    [Header("Цены бустеров (в монетах)")]
    [Tooltip("Цена лапки")]
    public int pawPrice = 75;
    
    [Tooltip("Цена кота-обжорки")]
    public int catPrice = 150;
    

    [Header("Опыт за продукты")]
    [Tooltip("Опыт за сосиску")]
    public int sausageExperience = 1;
    
    [Tooltip("Опыт за яичницу")]
    public int eggsExperience = 10;
    
    [Tooltip("Опыт за сандвич")]
    public int sandwichExperience = 25;
    
    [Tooltip("Опыт за мясной шарик")]
    public int meatballExperience = 40;
    
    [Tooltip("Опыт за суп")]
    public int soupExperience = 70;
    
    [Tooltip("Опыт за курицу")]
    public int chickenExperience = 90;
    
    [Tooltip("Опыт за лосось")]
    public int salmonExperience = 150;
    
    [Tooltip("Опыт за креветку")]
    public int shrimpExperience = 240;
    
    [Tooltip("Опыт за икру")]
    public int caviarExperience = 400;
    
    [Tooltip("Опыт за устрицу")]
    public int oysterExperience = 650;
    
    [Tooltip("Опыт за лобстера")]
    public int lobsterExperience = 1050;

    [Header("Пороги состояний кота")]
    [Tooltip("Опыт для 'Заморил червячка'")]
    public int wormsThreshold = 200;
    
    [Tooltip("Опыт для 'Уже полегче'")]
    public int betterThreshold = 500;
    
    [Tooltip("Опыт для 'Вкусненько'")]
    public int tastyThreshold = 1000;
    
    [Tooltip("Опыт для 'Пузико довольно'")]
    public int satisfiedThreshold = 2000;
    
    [Tooltip("Опыт для 'Надо же, сколько еды'")]
    public int fullThreshold = 4000;
    
    [Tooltip("Опыт для 'Счастливый котик'")]
    public int happyThreshold = 8000;
    
    [Tooltip("Опыт для 'Королевская сытость'")]
    public int royalThreshold = 15000;

    [Header("Награды за цели (в монетах)")]
    [Tooltip("Награда за яичницу")]
    public int eggsGoalReward = 10;
    
    [Tooltip("Награда за сандвич")]
    public int sandwichGoalReward = 15;
    
    [Tooltip("Награда за мясной шарик")]
    public int meatballGoalReward = 25;
    
    [Tooltip("Награда за суп")]
    public int soupGoalReward = 40;
    
    [Tooltip("Награда за курицу")]
    public int chickenGoalReward = 65;
    
    [Tooltip("Награда за лосось")]
    public int salmonGoalReward = 105;
    
    [Tooltip("Награда за креветку")]
    public int shrimpGoalReward = 170;
    
    [Tooltip("Награда за икру")]
    public int caviarGoalReward = 275;
    
    [Tooltip("Награда за устрицу")]
    public int oysterGoalReward = 445;
    
    [Tooltip("Награда за лобстера")]
    public int lobsterGoalReward = 715;

    [Header("Награды за первое открытие (в монетах)")]
    [Tooltip("Награда за первое открытие яичницы")]
    public int eggsFirstDiscoveryReward = 50;
    
    [Tooltip("Награда за первое открытие сандвича")]
    public int sandwichFirstDiscoveryReward = 80;
    
    [Tooltip("Награда за первое открытие мясного шарика")]
    public int meatballFirstDiscoveryReward = 120;
    
    [Tooltip("Награда за первое открытие супа")]
    public int soupFirstDiscoveryReward = 200;
    
    [Tooltip("Награда за первое открытие курицы")]
    public int chickenFirstDiscoveryReward = 320;
    
    [Tooltip("Награда за первое открытие лосося")]
    public int salmonFirstDiscoveryReward = 520;
    
    [Tooltip("Награда за первое открытие креветки")]
    public int shrimpFirstDiscoveryReward = 840;
    
    [Tooltip("Награда за первое открытие икры")]
    public int caviarFirstDiscoveryReward = 1360;
    
    [Tooltip("Награда за первое открытие устрицы")]
    public int oysterFirstDiscoveryReward = 2200;
    
    [Tooltip("Награда за первое открытие лобстера")]
    public int lobsterFirstDiscoveryReward = 3560;

    [Header("Награды за состояния кота (в монетах)")]
    [Tooltip("Награда за 'Заморил червячка'")]
    public int wormsCatReward = 50;
    
    [Tooltip("Награда за 'Уже полегче'")]
    public int betterCatReward = 100;
    
    [Tooltip("Награда за 'Вкусненько'")]
    public int tastyCatReward = 200;
    
    [Tooltip("Награда за 'Пузико довольно'")]
    public int satisfiedCatReward = 400;
    
    [Tooltip("Награда за 'Надо же, сколько еды'")]
    public int fullCatReward = 600;
    
    [Tooltip("Награда за 'Счастливый котик'")]
    public int happyCatReward = 850;
    
    [Tooltip("Награда за 'Королевскую сытость'")]
    public int royalCatReward = 1400;

    /// <summary>
    /// Получает опыт за продукт
    /// </summary>
    public int GetExperienceForFood(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Sausage: return sausageExperience;
            case FoodType.Eggs: return eggsExperience;
            case FoodType.Sandwich: return sandwichExperience;
            case FoodType.Meatball: return meatballExperience;
            case FoodType.Soup: return soupExperience;
            case FoodType.Chicken: return chickenExperience;
            case FoodType.Salmon: return salmonExperience;
            case FoodType.Shrimp: return shrimpExperience;
            case FoodType.Caviar: return caviarExperience;
            case FoodType.Oyster: return oysterExperience;
            case FoodType.Lobster: return lobsterExperience;
            default: return 0;
        }
    }

    /// <summary>
    /// Получает награду за цель
    /// </summary>
    public int GetGoalReward(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Eggs: return eggsGoalReward;
            case FoodType.Sandwich: return sandwichGoalReward;
            case FoodType.Meatball: return meatballGoalReward;
            case FoodType.Soup: return soupGoalReward;
            case FoodType.Chicken: return chickenGoalReward;
            case FoodType.Salmon: return salmonGoalReward;
            case FoodType.Shrimp: return shrimpGoalReward;
            case FoodType.Caviar: return caviarGoalReward;
            case FoodType.Oyster: return oysterGoalReward;
            case FoodType.Lobster: return lobsterGoalReward;
            default: return 0;
        }
    }

    /// <summary>
    /// Получает награду за первое открытие
    /// </summary>
    public int GetFirstDiscoveryReward(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Eggs: return eggsFirstDiscoveryReward;
            case FoodType.Sandwich: return sandwichFirstDiscoveryReward;
            case FoodType.Meatball: return meatballFirstDiscoveryReward;
            case FoodType.Soup: return soupFirstDiscoveryReward;
            case FoodType.Chicken: return chickenFirstDiscoveryReward;
            case FoodType.Salmon: return salmonFirstDiscoveryReward;
            case FoodType.Shrimp: return shrimpFirstDiscoveryReward;
            case FoodType.Caviar: return caviarFirstDiscoveryReward;
            case FoodType.Oyster: return oysterFirstDiscoveryReward;
            case FoodType.Lobster: return lobsterFirstDiscoveryReward;
            default: return 0;
        }
    }

    /// <summary>
    /// Получает награду за состояние кота
    /// </summary>
    public int GetCatStateReward(CatSatisfactionLevel level)
    {
        switch (level)
        {
            case CatSatisfactionLevel.Worms: return wormsCatReward;
            case CatSatisfactionLevel.Better: return betterCatReward;
            case CatSatisfactionLevel.Tasty: return tastyCatReward;
            case CatSatisfactionLevel.Satisfied: return satisfiedCatReward;
            case CatSatisfactionLevel.Full: return fullCatReward;
            case CatSatisfactionLevel.Happy: return happyCatReward;
            case CatSatisfactionLevel.Royal: return royalCatReward;
            default: return 0;
        }
    }

    /// <summary>
    /// Получает порог опыта для состояния кота
    /// </summary>
    public int GetCatStateThreshold(CatSatisfactionLevel level)
    {
        switch (level)
        {
            case CatSatisfactionLevel.Hungry: return 0;
            case CatSatisfactionLevel.Worms: return wormsThreshold;
            case CatSatisfactionLevel.Better: return betterThreshold;
            case CatSatisfactionLevel.Tasty: return tastyThreshold;
            case CatSatisfactionLevel.Satisfied: return satisfiedThreshold;
            case CatSatisfactionLevel.Full: return fullThreshold;
            case CatSatisfactionLevel.Happy: return happyThreshold;
            case CatSatisfactionLevel.Royal: return royalThreshold;
            default: return 0;
        }
    }
}
