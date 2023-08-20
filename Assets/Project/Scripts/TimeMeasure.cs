using TMPro;
using UnityEngine;

public class TimeMeasure : MonoBehaviour
{
    [SerializeField] private TMP_Text _finishText;
    private float _time;
    private bool _blockTimer;

    private void Awake() => _blockTimer = true;

    public void ActivateTimer()
    { 
        _finishText.text = "";
        _blockTimer = false;
        _time = 0f;
    }

    private void Update() 
    {
        if (!_blockTimer)
            _time += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.TryGetComponent(out PlayerBase _))
        {
            _blockTimer = true;
            _finishText.text = $"You are completed the parkour in {_time} seconds!"; 
        }
    }
}
