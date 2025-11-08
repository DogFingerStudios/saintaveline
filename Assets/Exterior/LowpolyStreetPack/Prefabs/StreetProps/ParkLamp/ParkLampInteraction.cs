using UnityEngine;

public class ParkLampInteraction : MonoBehaviour
{
    [SerializeField] private GameObject _objectToRotate;

    [SerializeField] private float _minCooldownTime = 10f;
    [SerializeField] private float _maxCooldownTime = 600f;

    private float _timeSinceLookedAway;

    void Start()
    {
        _timeSinceLookedAway = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeSinceLookedAway > 0)
        {
            _timeSinceLookedAway -= Time.deltaTime;
        }
    }

    public void LookedAway()
    {
        if (_timeSinceLookedAway <= 0f)
        {
            Debug.Log("ParkLampInteraction: LookedAway triggered, rotating object.");
            _objectToRotate.transform.Rotate(0, 0, 90);
            _timeSinceLookedAway = Random.Range(_minCooldownTime, _maxCooldownTime);
        }
    }
}
