using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerMech : MonoBehaviour, IInteractable
{
    public Vector3 OpenPosition, ClosePosition;

    float _moveSpeed;
    float _lerpTimer;
    public bool _drawerBool;

    void Start()
    {
        _drawerBool = false;
    }
        
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == ("Player") && Input.GetKeyDown(KeyCode.Q))
        {
            _drawerBool = !_drawerBool;
        }
    }

    void Update()
    {
        if (_drawerBool)
        {
            _moveSpeed = +1f;
            _lerpTimer = Mathf.Clamp(_lerpTimer + Time.deltaTime * _moveSpeed, 0f, 1f);
            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, _lerpTimer);
        }
        else
        {
            _moveSpeed = -1f;
            _lerpTimer = Mathf.Clamp(_lerpTimer + Time.deltaTime * _moveSpeed, 0f, 1f);
            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, _lerpTimer);
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
        this._drawerBool = !this._drawerBool;
    }
}

