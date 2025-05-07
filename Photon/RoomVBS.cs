using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomVBS : MonoBehaviour
{
    #region Переменные

    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;

    #endregion

    public void SetUp(RoomInfo roomInfo)
    {
        roomNameText.text = roomInfo.Name;
        playerCountText.text = roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;

        GetComponent<Button>().onClick.AddListener(JoinRoom);
    }

    public void JoinRoom()
    {
        LauncherVBS.instance.JoinToRoom(roomNameText.text);
    }
}
