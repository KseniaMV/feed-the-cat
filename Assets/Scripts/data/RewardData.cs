using UnityEngine;

[CreateAssetMenu(fileName = "RewardData", menuName = "Feed The Cat/RewardData")]
public class RewardData : ScriptableObject
{
    [Header("Основная информация")]

    [Tooltip("День награды (1, 2, 3, 4, 5, 6, 7)")]
    public string bonusDay;

    [Tooltip("Тип награды (coins, experience, etc.)")]
    public string bonusType_1;

    public string bonusType_2;

    public string bonusType_3;
    
    [Tooltip("Количество награды")]
    public string bonusCount_1;

    public string bonusCount_2;

    public string bonusCount_3;
    
    [Header("Визуальные элементы")]

    [Tooltip("Изображение бонуса")]
    public Sprite bonusSprite_1;

    public Sprite bonusSprite_2;

    public Sprite bonusSprite_3;
}
