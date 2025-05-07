using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemSO itemSO;
    public int amount;
    [HideInInspector] public bool isPickUped;

    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public void PickUp()
    {
        isPickUped = true;
        _photonView.RPC("DestroyItem", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void DestroyItem()
    {
        isPickUped = true;
        PhotonNetwork.Destroy(gameObject);
    }
}