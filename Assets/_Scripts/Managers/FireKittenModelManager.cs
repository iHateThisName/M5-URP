using Assets.Scripts.Singleton;
using UnityEngine;

public class FireKittenModelController : Singleton<FireKittenModelController> {

    [SerializeField] private Renderer ladderRenderer;
    [SerializeField] private Renderer hoseStraitRenderer;
    [SerializeField] private Renderer hoseNosalRenderer;
    [SerializeField] private Renderer hoseNosalBRenderer;
    [SerializeField] private Renderer hoseCoilRenderer;
    [SerializeField] private Renderer axeRenderer;

    protected override void Awake() {
        base.Awake();
        HideTools();
    }

    public void HideTools() {
        this.ladderRenderer.enabled = false;
        this.hoseStraitRenderer.enabled = false;
        this.hoseNosalRenderer.enabled = false;
        this.hoseNosalBRenderer.enabled = false;
        this.hoseCoilRenderer.enabled = false;
        this.axeRenderer.enabled = false;
    }

    public void ShowAxe() => this.axeRenderer.enabled = true;
    public void HideAxe() => this.axeRenderer.enabled = false;
    public void ShowLadder() => this.ladderRenderer.enabled = true;
    public void ShowWaterHoseNosal() => this.hoseNosalBRenderer.enabled = true;
    public void HideWaterHoseNosal() => this.hoseNosalBRenderer.enabled = false;
}
