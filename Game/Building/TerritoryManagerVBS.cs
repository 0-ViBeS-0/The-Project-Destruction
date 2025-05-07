using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryManagerVBS : MonoBehaviour
{
    public string owner;
    public List<GameObject> builds;

    private List<GameObject> _objectsToRemove = new();

    public void CheckUpdatesSupport()
    {
        if (_objectsToRemove != null) _objectsToRemove.Clear();

        foreach (GameObject buildObj in builds)
            if (!buildObj.GetComponent<BuildVBS>().CheckSupports())
                _objectsToRemove.Add(buildObj);

        if (_objectsToRemove.Count > 0)
        {
            foreach (GameObject buildObj in _objectsToRemove)
            {
                builds.Remove(buildObj);
                PhotonNetwork.Destroy(buildObj);
            }
            Invoke(nameof(CheckUpdatesSupport), 0.1f);
        }
        else if (builds.Count == 0)
            PhotonNetwork.Destroy(gameObject);
    }

    public void DestroyBuild(GameObject obj)
    {
        if (builds.Contains(obj))
        {
            builds.Remove(obj);
            PhotonNetwork.Destroy(obj);
            Invoke(nameof(CheckUpdatesSupport), 0.1f);
        }
    }
}