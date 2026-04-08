using UnityEngine;

public class TutorialPanelHide : MonoBehaviour
{
    public GameObject tutorialPanel;

    public GameObject arrow_to_shop;

    public GameObject arrow_to_collections;

    public GameObject game_tutorial_ui;

    public GameObject pointer;
    public GameObject pointer_arrow_1;
    public GameObject pointer_arrow_2;
 
    public void CloseShopTutorial(){
        tutorialPanel.SetActive(false);
        arrow_to_shop.SetActive(false);
        TutorialData.SetShopTutorialEnd();
    }

     public void CloseCollectionTutorial(){
        arrow_to_collections.SetActive(false);
        TutorialData.SetCollectionTutorialEnd();
    }

    public void SetGameTutorialEnd() {
        tutorialPanel.SetActive(false);
        game_tutorial_ui.SetActive(false);
        TutorialData.SetGameTutorialEnd();
    }

    public void HidePointerTutorial() {
        pointer.SetActive(false);
        pointer_arrow_1.SetActive(false);
        pointer_arrow_2.SetActive(false);
    }
}
