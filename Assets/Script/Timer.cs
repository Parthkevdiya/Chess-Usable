using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private float timerRemaining = 100;

    [SerializeField] private float TotalTime;
    private void Start()
    {
        timerRemaining = TotalTime;
    }
    private void Update()
    {
        if (timerRemaining > 0)
        {
            timerRemaining -= Time.deltaTime;
            ConvertSecondsIntoMinutes(timerRemaining, out int minutes, out int seconds);
            Debug.Log(minutes + " : " + seconds);
        }
    }

    public void ConvertSecondsIntoMinutes(float totalSeconds , out int minutes , out int seconds)
    {
        minutes = 0;
        seconds = 0;

        minutes = (int) totalSeconds / 60;
        seconds = (int) totalSeconds - (minutes*60);
    }
}
