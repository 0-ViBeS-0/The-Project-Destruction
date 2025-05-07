using UnityEngine;
using UnityEngine.Events;

public class EventInteraction : MonoBehaviour
{
    #region Variables

    [Header("|-# DESCRIPION FOR:")]
    [SerializeField] private string _descriptionFALSE;
    [SerializeField] private string _descriptionTRUE;

    [Header("|-# ACTIONS")]
    [SerializeField] private float _timeForActionsStart;
    [SerializeField] private UnityEvent[] _actionsStart;
    [SerializeField] private float _timeForActionsInverse;
    [SerializeField] private UnityEvent[] _actionsInverse;
    [SerializeField] private float _timeForActionsStop;
    [SerializeField] private UnityEvent[] _actionsStop;

    [Header("|-# DESTROYinStopActions")]
    [SerializeField] private GameObject[] _objectsForDelete;

    [Header("|-# SWITCH")]
    [SerializeField] private bool _isSwitch;
    [SerializeField] private bool _repeatSwitchingInStop;
    [Space(10)]
    public bool _isTrue;

    [Header("|-# ANIMATOR")]
    [SerializeField] private Animator _objectAnimator;
    [SerializeField] private string _nameForObjectAnimation;
    [SerializeField] private bool _repeatObjectAnimationInStop;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private string _nameForPlayerAnimation;
    [SerializeField] private bool _repeatPlayerAnimationInStop;

    [Header("|-# TIME")]
    [SerializeField] private float _time;

    #endregion

    public string GetDescription()
    {
        if (!_isTrue) return _descriptionFALSE;
        return _descriptionTRUE;
    }

    public void Interact()
    {
        // SWITCH -----------------------------------------------------------------------------
        _isTrue = _isSwitch && !_isTrue;
        // ANIMATOR -----------------------------------------------------------------------------
        if (_objectAnimator) _objectAnimator.SetBool(_nameForObjectAnimation, _isTrue);
        if (_playerAnimator) _playerAnimator.SetBool(_nameForPlayerAnimation, _isTrue);
        // EVENTS -----------------------------------------------------------------------------
        if (_actionsInverse != null)
        {
            if (_isTrue)
            {
                if (_actionsStart != null) Invoke(nameof(ActionStart), _timeForActionsStart);
            }
            else
            {
                if (_actionsInverse != null) Invoke(nameof(ActionInverse), _timeForActionsInverse);
            }
        }
        else
            if (_actionsStart != null) Invoke(nameof(ActionStart), _timeForActionsStart);
        // TIME -----------------------------------------------------------------------------
        Invoke(nameof(InteractStop), _time);
    }

    public void InteractStop()
    {
        // SWITCH -----------------------------------------------------------------------------
        if (_repeatSwitchingInStop) _isTrue = _isSwitch && !_isTrue;
        // ANIMATOR -----------------------------------------------------------------------------
        if (_objectAnimator && _repeatObjectAnimationInStop) _objectAnimator.SetBool(_nameForObjectAnimation, _isTrue);
        if (_playerAnimator && _repeatPlayerAnimationInStop) _playerAnimator.SetBool(_nameForPlayerAnimation, _isTrue);
        // EVENTS -----------------------------------------------------------------------------
        if (_actionsStop != null) Invoke(nameof(ActionStop), _timeForActionsStop);
        // DESTROY -----------------------------------------------------------------------------
        if (_objectsForDelete != null)
            foreach (GameObject obj in _objectsForDelete)
                Destroy(obj);
    }

    #region Actions: Start/Inverse/Stop

    private void ActionStart()
    {
        foreach (UnityEvent action in _actionsStart) action.Invoke();
    }

    private void ActionInverse()
    {
        foreach (UnityEvent action in _actionsInverse) action.Invoke();
    }

    private void ActionStop()
    {
        foreach (UnityEvent action in _actionsStop) action.Invoke();
    }

    #endregion
}
