﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    public bool pauseTimer;
    public float timeLeftCurrentScanline;
    public float timeBetweenScanLines = 1.0f;
    public Image timerImage;

    private void OnEnable()
    {
        EventManager.OnTouchStart += Pause;
        EventManager.OnTouchEnd   += Resume;
    }

    private void OnDisable()
    {
        EventManager.OnTouchStart -= Pause;
        EventManager.OnTouchEnd   -= Resume;
    }

    // Start is called before the first frame update
    void Start()
    {
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        if (!pauseTimer)
        {
            timeLeftCurrentScanline -= Time.smoothDeltaTime;
        }

        if (timeLeftCurrentScanline <= 0)
        {
            //MoveScanline();
            //ClearDots();
            //DropNewDots();
            EventManager.OnTimerEnd?.Invoke();
            Reset();
        }

        timerImage.fillAmount = timeLeftCurrentScanline;
    }

    private void Pause()
    {
        pauseTimer = true;
    }

    private void Resume()
    {
        pauseTimer = false;
    }

    private void Reset()
    {
        timeLeftCurrentScanline = timeBetweenScanLines;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 0, 1000, 90), $"Time: {timeLeftCurrentScanline}");
    }
}
