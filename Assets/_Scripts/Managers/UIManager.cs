using Assets.Scripts.Singleton;
using UnityEngine;

public class UIManager : Singleton<UIManager> {
    [SerializeField] private GameObject tutorialUI;
    private bool isTutorialActive = true;

    public void ToggleTutorialActive() {
        this.isTutorialActive = !this.isTutorialActive;
        tutorialUI.SetActive(this.isTutorialActive);
    }
}
