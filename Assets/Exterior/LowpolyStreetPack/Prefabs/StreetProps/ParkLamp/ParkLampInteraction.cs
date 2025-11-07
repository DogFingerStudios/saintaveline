using UnityEngine;

public class ParkLampInteraction : MonoBehaviour
{
    [SerializeField] private GameObject _objectToRotate;

    [SerializeField] private float _minTimeBetweenRotations = 5f;
    private float _timeSinceLookedAway;

    void Start()
    {
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
            BottomTypewriter.Instance.Enqueue("The lamp post seems to have a loose fixture.");
            _timeSinceLookedAway = _minTimeBetweenRotations;
        }
    }
}
