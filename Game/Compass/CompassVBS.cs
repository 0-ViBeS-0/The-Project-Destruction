using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CompassVBS : MonoBehaviour
{
    #region Переменные

    [Header("SETTINGS")]
    [SerializeField] private RawImage _compassImage;
    [SerializeField] private TMP_Text _compassDirectionText;

    [HideInInspector] public Transform player;

    private static readonly string[] _directions = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    private const int _angleStep = 45;

    [Header("QUEST MARKERS")]
    [SerializeField] private GameObject _markPrefab;
    [SerializeField] private float _maxDistance = 200f;
    [SerializeField] private QuestMarkVBS[] _questMarksTEST;

    private List<QuestMarkVBS> _activeQuestMarks = new();
    private List<GameObject> _activeMarks = new();
    private float _compassUnit;

    #endregion

    #region Start/Update

    private void Start()
    {
        _compassUnit = _compassImage.rectTransform.rect.width / 360f;

        foreach (QuestMarkVBS mark in _questMarksTEST) // TEST
        {
            AddQuestMark(mark);
        }
    }

    private void Update()
    {
        if (!player)
            return;

        UpdateCompassUI();
        UpdateQuestMarks();
    }

    #endregion

    #region События компаса

    private void UpdateCompassUI()
    {
        if (player)
        {
            if (_compassImage)
            {
                _compassImage.uvRect = new Rect(player.localEulerAngles.y / 360f, 0, 1, 1);
            }

            int displayAngle = Mathf.RoundToInt(player.eulerAngles.y / _angleStep) * _angleStep;
            _compassDirectionText.text = _directions[(displayAngle / _angleStep) % _directions.Length];
        }
    }

    #endregion

    #region События маркеров

    public void AddQuestMark(QuestMarkVBS mark)
    {
        GameObject newMark = Instantiate(_markPrefab, _compassImage.transform);
        mark.image = newMark.GetComponent<Image>();
        mark.image.sprite = mark.icon;

        TMP_Text distanceText = newMark.GetComponentInChildren<TMP_Text>();
        mark.distanceText = distanceText;

        _activeQuestMarks.Add(mark);
        mark.createdPrefab = newMark;
    }

    public void RemoveQuestMark(QuestMarkVBS mark)
    {
        if (_activeQuestMarks.Contains(mark))
        {
            Destroy(mark.createdPrefab);
            _activeQuestMarks.Remove(mark);
        }
    }

    private void UpdateQuestMarks()
    {
        Vector2 playerPos = new(player.transform.position.x, player.transform.position.z);

        foreach (QuestMarkVBS mark in _activeQuestMarks)
        {
            mark.image.rectTransform.anchoredPosition = GetPositionOnCompass(mark, playerPos);
            UpdateDistanceText(mark, playerPos);
        }
    }

    private Vector2 GetPositionOnCompass(QuestMarkVBS mark, Vector2 playerPos)
    {
        Vector2 playerFwd = new(player.transform.forward.x, player.transform.forward.z);
        float angle = Vector2.SignedAngle(mark.position - playerPos, playerFwd);
        return new Vector2(_compassUnit * angle, 0f);
    }

    private void UpdateDistanceText(QuestMarkVBS mark, Vector2 playerPos)
    {
        int distance = (int)Vector2.Distance(playerPos, mark.position);
        mark.distanceText.text = $"{distance} m";
        mark.distanceText.enabled = distance < _maxDistance;
    }

    #endregion
}