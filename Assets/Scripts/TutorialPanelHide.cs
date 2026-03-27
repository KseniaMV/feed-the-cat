using UnityEngine;

public class TutorialPanelHide : MonoBehaviour
{
    public GameObject tutorialPanel;
 
    public void CloseCollectionTutorial(){
        tutorialPanel.SetActive(false);
        TutorialData.SetCollectionTutorialEnd();
    }

    public void SetShopTutorialEnd() {
        tutorialPanel.SetActive(false);
        TutorialData.SetShopTutorialEnd();
    }

    public void SetGameTutorialEnd() {
        tutorialPanel.SetActive(false);
        TutorialData.SetGameTutorialEnd();
    }
}
