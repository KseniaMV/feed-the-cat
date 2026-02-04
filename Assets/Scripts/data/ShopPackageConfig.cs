using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Данные о пакете для покупки в магазине
/// </summary>
[System.Serializable]
public class ShopPackage
{
    [Header("Основная информация")]
    [Tooltip("ID продукта в магазине")]
    public string productId;
    
    [Tooltip("Название пакета")]
    public string packageName;
    
    [Tooltip("Описание пакета")]
    public string description;

    [Header("Содержимое пакета")]
    [Tooltip("Количество монет")]
    public int coins;
    
    [Tooltip("Количество бомбочек")]
    public int bombs;
    
    [Tooltip("Количество лапок")]
    public int paws;
    
    [Tooltip("Количество котов-обжорок")]
    public int cats;

    [Header("Цена")]
    [Tooltip("Цена в рублях")]
    public string price;
    
    [Tooltip("Валюта")]
    public string currency = "RUB";
}

/// <summary>
/// Конфигурация пакетов магазина (4 базовых пакета)
/// </summary>
[CreateAssetMenu(fileName = "ShopPackageConfig", menuName = "Game/Shop Package Config")]
public class ShopPackageConfig : ScriptableObject
{
    [Header("Пакеты магазина")]
    [Tooltip("Список всех пакетов (4 пакета: монеты + 3 пакета с бустерами)")]
    public List<ShopPackage> shopPackages;

    /// <summary>
    /// Получает все пакеты
    /// </summary>
    public ShopPackage[] GetAllPackages()
    {
        return shopPackages.ToArray();
    }

    /// <summary>
    /// Получает пакет по ID
    /// </summary>
    public ShopPackage GetPackageById(string productId)
    {
        foreach (ShopPackage package in shopPackages)
        {
            if (package.productId == productId)
                return package;
        }
        
        return null;
    }
}