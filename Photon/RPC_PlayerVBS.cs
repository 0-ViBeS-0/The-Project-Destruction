using Photon.Pun;
using UnityEngine;

public class RPC_PlayerVBS : MonoBehaviour
{
    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public void SetParent(RpcTarget target, GameObject child, GameObject parent)
    {
        _photonView.RPC("RPC_SetParent", target, child.GetComponent<PhotonView>().ViewID, parent.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void RPC_SetParent(int childViewID, int parentViewID)
    {
        PhotonView childView = PhotonView.Find(childViewID);
        PhotonView parentView = PhotonView.Find(parentViewID);

        if (childView && parentView)
            childView.transform.SetParent(parentView.transform, true);
    }

    public void ApplyOriginalTransform(RpcTarget target, GameObject obj, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 spawnScale)
    {
        _photonView.RPC("RPC_ApplyOriginalTransform", target, obj.GetComponent<PhotonView>().ViewID, spawnPosition, spawnRotation, spawnScale);
    }

    [PunRPC]
    private void RPC_ApplyOriginalTransform(int viewID, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 spawnScale)
    {
        PhotonView viewObj = PhotonView.Find(viewID);
        if (viewObj)
        {
            GameObject obj = viewObj.gameObject;
            obj.transform.SetLocalPositionAndRotation(spawnPosition, spawnRotation);
            obj.transform.localScale = spawnScale;
        }
    }

    public void InitializeBuild(RpcTarget target, GameObject build, GameObject territory)
    {
        _photonView.RPC("RPC_InitializeBuild", target, build.GetComponent<PhotonView>().ViewID, territory.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void RPC_InitializeBuild(int buildView, int territoryView)
    {
        PhotonView viewObj = PhotonView.Find(buildView);
        PhotonView viewTerr = PhotonView.Find(territoryView);
        if (viewObj && viewTerr)
        {
            GameObject buildObj = viewObj.gameObject;
            TerritoryManagerVBS territoryManager = viewTerr.gameObject.GetComponent<TerritoryManagerVBS>();
            BuildVBS build = buildObj.GetComponent<BuildVBS>();

            build.SetBuildComponents(UIManagerVBS.instance.GetBuildData(build.ID));
            territoryManager.builds.Add(buildObj);
            build.territoryManager = territoryManager;
        }
    }

    public void InitializeTerritory(RpcTarget target, GameObject territory)
    {
        _photonView.RPC("RPC_InitializeTerritory", target, territory.GetComponent<PhotonView>().ViewID, GameData.MyName);
    }

    [PunRPC]
    private void RPC_InitializeTerritory(int territoryView, string name)
    {
        PhotonView viewTerr = PhotonView.Find(territoryView);
        if (viewTerr)
        {
            viewTerr.gameObject.name = "TerritoryBy:" + name;
            TerritoryManagerVBS territoryManager = viewTerr.gameObject.GetComponent<TerritoryManagerVBS>();
            territoryManager.owner = name;
        }
    }
}