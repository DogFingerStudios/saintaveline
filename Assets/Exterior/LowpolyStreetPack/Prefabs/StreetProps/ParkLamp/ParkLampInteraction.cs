using UnityEngine;

public class ParkLampInteraction : MonoBehaviour
{
    [SerializeField] private GameObject _objectToRotate;

    [SerializeField] private float _minTimeBetweenRotations = 5f;
    private float _timeSinceLookedAway;

    void Start()
    {
        _minTimeBetweenRotations = Random.Range(10f, 600f);
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
            _objectToRotate.transform.Rotate(0, 0, 90);
            _timeSinceLookedAway = _minTimeBetweenRotations;
            _minTimeBetweenRotations = Random.Range(10f, 600f);
        }
    }
}
