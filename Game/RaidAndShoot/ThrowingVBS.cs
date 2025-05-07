using Photon.Pun;
using System.Collections;
using UnityEngine;

public class ThrowingVBS : MonoBehaviour
{
    #region Переменные

    [Header("|-# COMPONENTS")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _rayDistance;

    private bool _isThrowing;

    [Header("|-# THROW DATA")]
    public GameObject currentPrefab;
    public float force;
    public float upForce;
    public float reloadTime;

    #endregion

    public void Throw()
    {
        if (!_isThrowing)
        {
            _isThrowing = true;
            GameObject obj = PhotonNetwork.Instantiate(currentPrefab.name, _spawnPoint.position, _cameraTransform.rotation);
            Rigidbody rb = obj.GetComponent<Rigidbody>(); 

            Vector3 forceDirection = _cameraTransform.forward;
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _rayDistance))
                forceDirection = (hit.point - _spawnPoint.position).normalized;

            Vector3 addForce = forceDirection * force + transform.up * upForce;
            rb.AddForce(addForce, ForceMode.Impulse);

            StartCoroutine(Reset());
        }
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(reloadTime);
        _isThrowing = false;
    }
}