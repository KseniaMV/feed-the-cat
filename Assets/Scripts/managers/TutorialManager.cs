using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;

    public GameObject arrow_to_shop;

    public GameObject arrow_to_collections;

    public GameObject game_pointer_tutorial;

    public bool is_collection_tutorial_and = false;

    public bool is_shop_tutorial_and = false;

    public bool is_game_tutorial_and = false;

    public bool is_tutorial_complete = false;

    public void ShowCollectionTutorial(){
        tutorialPanel.SetActive(true);
        arrow_to_collections.SetActive(true);
    }

    public void ShowShopTutorial(){
        tutorialPanel.SetActive(true);
        arrow_to_shop.SetActive(true);
    }

    public void ShowGameTutorial(){
        tutorialPanel.SetActive(true);
        game_pointer_tutorial.SetActive(true);
    }
}
