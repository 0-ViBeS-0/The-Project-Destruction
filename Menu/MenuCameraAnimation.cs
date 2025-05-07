using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class MenuCameraAnimation : MonoBehaviour
{
    #region Variables

    public static MenuCameraAnimation instance;

    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Transform[] _points;
    [SerializeField] private string[] _nameMenu;
    [SerializeField] private float _lerpSpeed;

    [SerializeField] private Transform currentPointTransform;

    #endregion

    #region Awake/Update

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (currentPointTransform != null)
        {
            _cameraTransform.SetPositionAndRotation(Vector3.Lerp(_cameraTransform.position, currentPointTransform.position, _lerpSpeed * Time.deltaTime),
                Quaternion.Lerp(_cameraTransform.rotation, currentPointTransform.rotation, _lerpSpeed * Time.deltaTime));
        }
    }

    #endregion

    #region Animation

    public void SetCameraPos(string Name)
    {
        for (int i = 0; i < _nameMenu.Length; i++)
            if (_nameMenu[i] == Name)
                currentPointTransform = _points[i];
    }

    #endregion
}
