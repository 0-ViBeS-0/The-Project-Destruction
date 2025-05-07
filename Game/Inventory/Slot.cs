using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [HideInInspector] public ItemSO itemSO;
    [HideInInspector] public int amount;
    public Image icon;
    public TMP_Text amountText;

    public void SetIcon(Sprite _icon)
    {
        icon.color = new Color(1, 1, 1, 1);
        icon.sprite = _icon;
    }
}