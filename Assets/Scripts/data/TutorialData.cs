using UnityEngine;

public static class TutorialData
{
    public  static bool is_collection_tutorial_and = false;

    public  static bool is_shop_tutorial_and = false;

    public  static bool is_game_tutorial_and = false;

    public  static bool is_tutorial_complete = false;

    public  static void SetCollectionTutorialEnd() {
        is_collection_tutorial_and = true;
    }

    public  static void SetShopTutorialEnd() {
        is_shop_tutorial_and = true;
    }

    public  static void SetGameTutorialEnd() {
        is_game_tutorial_and = true;
    }

    public  static void SetTutorialComlete() {
        if(is_shop_tutorial_and == true &
        is_collection_tutorial_and == true &
        is_game_tutorial_and == true) {
            is_tutorial_complete = true;
        }
    }
}
