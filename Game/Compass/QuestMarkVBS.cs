using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestMarkVBS : MonoBehaviour
{
    public Sprite icon;
    [HideInInspector] public Image image;
    [HideInInspector] public TMP_Text distanceText;
    [HideInInspector] public GameObject createdPrefab;

    public Vector2 position
    {
        get { return new Vector2(transform.position.x, transform.position.z);}
    }
}