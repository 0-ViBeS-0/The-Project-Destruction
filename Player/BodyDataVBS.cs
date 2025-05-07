using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BodyDataVBS : MonoBehaviour
{
    private QuickSlotInventoryVBS _quickSlotInventory;
    public Transform playerSkin;
    public Transform cameraHeadTargetTransform;
    public Transform headTargetTransform;
    public Transform itemInRightHand;
    [HideInInspector] public Animator animator;

    private void Start()
    {
        if (!animator)
            animator = GetComponent<Animator>();

        _quickSlotInventory = UIManagerVBS.instance.GetQuickSlotInventory();
    }

    public void ChangeLayerWeight(float weight)
    {
        StartCoroutine(SmoothLayerWeightChange(animator.GetLayerWeight(1), weight, 0.3f));
    }

    private IEnumerator SmoothLayerWeightChange(float oldWeight, float newWeight, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            animator.SetLayerWeight(1, Mathf.Lerp(oldWeight, newWeight, elapsed / time));
            elapsed += Time.deltaTime;
            yield return null;
        }
        animator.SetLayerWeight(1, newWeight);
    }

    public void Hit()
    {
        _quickSlotInventory.Hit();
    }
}