using UnityEngine;

public static class TutorialData
{
    public  static bool is_shop_and_collection_tutorial_and = false;

    public  static bool is_game_tutorial_and = false;

    public  static bool is_collections_tutorial_and = false;

    public  static bool is_shop_tutorial_and = false;

    public  static void SetShopAndCollectionTutorialEnd() {
        is_shop_and_collection_tutorial_and = true;
    }

    public  static void SetGameTutorialEnd() {
        is_game_tutorial_and = true;
    }

    public  static void SetShopTutorialEnd() {
        is_shop_tutorial_and = true;
    }

    public  static void SetCollectionTutorialEnd() {
        is_collections_tutorial_and = true;
    }

}
