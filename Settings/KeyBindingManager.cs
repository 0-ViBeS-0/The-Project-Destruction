using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBindingManager : MonoBehaviour
{
    public KeyCode[] KeyCodes;
    public Button[] KeyButtons;

    private string _waitingForKey = "";
    private Button _currentButton;

    private void Start()
    {
        LoadKeyBindings();

        for(int i = 0; i < KeyButtons.Length; i++)
            KeyButtons[i].GetComponentInChildren<TMP_Text>().text = KeyCodes[i].ToString();
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(_waitingForKey))
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        AssignKey(keyCode);
                        break;
                    }
                }
            }
        }
    }

    public void StartKeyAssignmentButton(Button currentButton)
    {
        _waitingForKey = "key";
        _currentButton = currentButton;

        foreach (Button button in KeyButtons)
            if (button == currentButton)
                button.GetComponentInChildren<TMP_Text>().text = "Press any key...";
    }

    private void AssignKey(KeyCode keyCode)
    {
        for (int i = 0; i < KeyButtons.Length; i++)
            if (KeyButtons[i] == _currentButton)
            {
                KeyCodes[i] = keyCode;
                KeyButtons[i].GetComponentInChildren<TMP_Text>().text = KeyCodes[i].ToString();
                PlayerPrefs.SetString("KeyCode" + i, KeyCodes[i].ToString());
            }

        _waitingForKey = "";
    }

    public void LoadKeyBindings()
    {
        for (int i = 0; i < KeyCodes.Length; i++)
        {
            if (PlayerPrefs.HasKey("KeyCode" + i))
            {
                string savedKey = PlayerPrefs.GetString("KeyCode" + i);
                if (System.Enum.TryParse(savedKey, out KeyCode savedKeyCode))
                {
                    KeyCodes[i] = savedKeyCode;
                }
            }
        }
    }
}
