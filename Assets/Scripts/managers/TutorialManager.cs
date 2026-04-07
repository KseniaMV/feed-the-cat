using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;

    public GameObject arrow_to_shop;

    public GameObject arrow_to_collections;

    public GameObject game_pointer_tutorial;

    public GameObject goal_tutorial_arrow;

    public void ShowCollectionTutorial(){
        arrow_to_collections.SetActive(true);
    }

    public void ShowShopTutorial(){
        arrow_to_shop.SetActive(true);
    }

    public void ShowGoalTutorial(){
        goal_tutorial_arrow.SetActive(true);
    }

    public void ShowGameTutorial(){
        if(TutorialData.is_game_tutorial_and == false) {
            tutorialPanel.SetActive(true);
            game_pointer_tutorial.SetActive(true);
            Invoke("ShowGoalTutorial", 2f);
        }
    }

    public void ShowCollectionAndShopTutorial() {
        if(TutorialData.is_shop_and_collection_tutorial_and == false) {
            tutorialPanel.SetActive(true);
            if(TutorialData.is_collections_tutorial_and == false) {
                ShowCollectionTutorial();
            }
            if(TutorialData.is_shop_tutorial_and == false) {
                Invoke("ShowShopTutorial", 2.3f);
            }
        }
    }
}