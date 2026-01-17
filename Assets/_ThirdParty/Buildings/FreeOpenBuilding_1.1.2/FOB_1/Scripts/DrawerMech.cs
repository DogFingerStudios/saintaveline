using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class DrawerMech : MonoBehaviour, IInteractable
{
    private Transform _transform;

    public Vector3 OpenPosition, ClosePosition;

    float _moveSpeed;
    float _lerpTimer;
    public bool _drawerBool;

    private readonly WaitForSeconds _drawerAnimationDelay = new(0.01f);

    void Start()
    {
        _transform = GetComponent<Transform>();
        _drawerBool = false;
    }

    private void StartOpenDrawer()
    {
        StopCoroutine(nameof(OpenDrawer));
        StartCoroutine(nameof(OpenDrawer));
    }

    private void StartCloseDrawer()
    {
        StopCoroutine(nameof(CloseDrawer));
        StartCoroutine(nameof(CloseDrawer));
    }

    private IEnumerator OpenDrawer()
    {
        while (_transform.localPosition != OpenPosition)
        {
            _moveSpeed = +1f;
            _lerpTimer = Mathf.Clamp(_lerpTimer + Time.deltaTime * _moveSpeed, 0f, 1f);
            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, _lerpTimer);
            yield return _drawerAnimationDelay;
        }
    }

    private IEnumerator CloseDrawer()
    {
        while (_transform.localPosition != ClosePosition)
        {
            _moveSpeed = -1f;
            _lerpTimer = Mathf.Clamp(_lerpTimer + Time.deltaTime * _moveSpeed, 0f, 1f);
            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, _lerpTimer);
            yield return _drawerAnimationDelay;
        }
    }

    string IInteractable.HoverText
    {
        get
        {
            if (_drawerBool)
            {
                return "Press [Q] to close";
            }

            return "Press [Q] to open";
        }
    }
    List<InteractionData> IInteractable.Interactions => new List<InteractionData>();

    void IInteractable.OnFocus()
    {
    }

    void IInteractable.OnDefocus()
    {
    }

    void IInteractable.Interact(GameEntity interactor)
    {
        _drawerBool = !_drawerBool;
        if (_drawerBool)
        {
            Debug.Log("Open this mthfkn drawer!");
            StartOpenDrawer();
        }
        else
        {
            Debug.Log("Close this mthfkn drawer!");
            StartCloseDrawer();
        }
    }
}

