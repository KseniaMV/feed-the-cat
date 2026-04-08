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
        Invoke("ShowShopTutorial", 2.6f);
    }

    public void ShowShopTutorial(){
        arrow_to_shop.SetActive(true);
    }

    public void ShowGoalTutorial(){
        goal_tutorial_arrow.SetActive(true);
    }

    public void closeUIArrows(){
        arrow_to_shop.SetActive(false);
        arrow_to_collections.SetActive(false);
    }

    public void ShowGameTutorial(){
        closeUIArrows();
        if(TutorialData.is_game_tutorial_and == false) {
            tutorialPanel.SetActive(true);
            game_pointer_tutorial.SetActive(true);
            Invoke("ShowGoalTutorial", 3f);
        }
    }

    public void ShowCollectionAndShopTutorial() {
        Debug.Log("TutorialData.is_shop_tutorial_and" + TutorialData.is_shop_tutorial_and);
        Debug.Log("TutorialData.is_collections_tutorial_and" + TutorialData.is_collections_tutorial_and);
        if(TutorialData.is_shop_tutorial_and == true && TutorialData.is_collections_tutorial_and == true) return;

        tutorialPanel.SetActive(true);
        if(TutorialData.is_collections_tutorial_and == false) ShowCollectionTutorial();
    }
}