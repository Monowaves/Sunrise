using TMPro;
using UnityEngine;

public class KillsCounter : MonoBehaviour
{
    public TMP_Text counter;
    public int count;

    public void AddKill()
    {
        count++;
        counter.text = "Kills: " + count;
    }
}
