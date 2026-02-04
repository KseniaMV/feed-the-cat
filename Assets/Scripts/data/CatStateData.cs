using UnityEngine;

/// <summary>
/// ScriptableObject для хранения данных о состояниях удовлетворенности кота
/// Содержит информацию о состоянии, его требованиях и наградах
/// </summary>
[CreateAssetMenu(fileName = "New Cat State Data", menuName = "Feed The Cat/Cat State Data")]
public class CatStateData : ScriptableObject
{
    [Header("Основная информация")]
    [Tooltip("Уровень удовлетворенности кота")]
    public CatSatisfactionLevel satisfactionLevel;
    
    [Tooltip("Название состояния для отображения")]
    public string stateName;
    
    [Tooltip("Описание состояния")]
    public string description;
    
    [Header("Требования")]
    [Tooltip("Количество опыта, необходимое для достижения этого состояния")]
    public int requiredExperience;
    
    [Header("Визуальные элементы")]
    [Tooltip("Спрайт состояния кота")]
    public Sprite catStateSprite;

    [Header("Награды")]
    [Tooltip("Количество монет за достижение этого состояния")]
    public int coinReward;

    [Header("Дополнительная информация")]
    [Tooltip("Порядок отображения в коллекции (0 - первое)")]
    public int displayOrder = 0;
    
    [Tooltip("Является ли это состояние начальным (дефолтным)")]
    public bool isDefaultState = false;
    
    [Tooltip("Особые эффекты при достижении состояния")]
    public string specialEffects;
}
