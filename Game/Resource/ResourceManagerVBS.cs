using Photon.Pun;
using UnityEngine;

public class ResourceManagerVBS : MonoBehaviour
{
    private PhotonView _photonView;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public int GetChildIndex(GameObject child)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject == child)
            {
                return i;
            }
        }

        return -1;
    }

    public void ResourceDamage(GameObject obj, int damage)
    {
        int id = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject == obj)
            {
                id = i;
                break;
            }
        }

        _photonView.RPC("RPC_ResurceDamage", RpcTarget.AllBuffered, id, damage);
    }

    [PunRPC]
    private void RPC_ResurceDamage(int id, int damage)
    {
        transform.GetChild(id).GetComponent<ResourceVBS>().ResurceDamage(damage);
    }
}