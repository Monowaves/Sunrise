using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRestart : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other) 
    {
        FindObjectOfType<TimeMeasure>().ActivateTimer();
    }
}
