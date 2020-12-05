﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    private FieldScript field;
    private GameplayScript gameplay;
    private TimerScript timer;

    [SerializeField]
    private bool DisplayDebug;

    private void Start()
    {
        GetDependencies();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            string folderPath = Directory.GetCurrentDirectory() + "/Screenshots/";

            string fileName = $"Screenshot_{System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}.png";
            string path = System.IO.Path.Combine(folderPath, fileName);

            ScreenCapture.CaptureScreenshot(path, 2);
            Debug.Log($"Screenshot captured: {path}");
        }
    }

    private void GetDependencies()
    {
        field = GameObject.FindObjectOfType<FieldScript>();
        gameplay = GameObject.FindObjectOfType<GameplayScript>();
        timer = GameObject.FindObjectOfType<TimerScript>();
    }

    void OnGUI()
    {
        if (DisplayDebug)
        {
            float screenScale = Screen.width / 360.0f;
            Matrix4x4 scaledMatrix = Matrix4x4.identity * Matrix4x4.Scale(new Vector3(screenScale, screenScale, screenScale));
            GUI.matrix = scaledMatrix;

            GUI.Label(new Rect(10, 0, 1000, 90), $"Timer: {timer.DebugTimeLeftCurrentScanline()}");

            if (field.DebugCurrentPick()) GUI.Label(new Rect(10, 46, 1000, 90), "Current Pick: " + field.DebugCurrentPick().id);
            if (field.DebugCurrentPick()) GUI.Label(new Rect(10, 56, 1000, 90), "Current Type: " + field.DebugCurrentPick().ToString());

            if (field.DebugPossibleDropId() != -1) GUI.Label(new Rect(10, 66, 1000, 90), "Possible drop: " + field.DebugPossibleDropId());
            GUI.Label(new Rect(10, 77, 1000, 90), "Dots       : " + field.DebugCountDots() + "/" + (field.DebugColumns() * field.DebugRows()));
            GUI.Label(new Rect(10, 87, 1000, 90), "Dots(empty): " + field.DebugCountDots(true) + "/" + (field.DebugColumns() * field.DebugRows()));
            GUI.Label(new Rect(10, 97, 1000, 90), "Score: " + gameplay.DebugScore());

            if (field.DebugbMouseCoordsOnClick()) GUI.Label(new Rect(10, 106, 1000, 90), "Mouse onClick: " + field.DebugvMouseCoordsOnClick().ToString());
            if (field.DebugbMouseCoordsNow()) GUI.Label(new Rect(10, 116, 1000, 90), "Mouse now: " + field.DebugvMouseCoordsNow().ToString());

            if (gameplay.DebugIsGameOver()) GUI.Label(new Rect(10, 126, 1000, 90), "GAME OVER");

            if (GUI.Button(new Rect(10, 156, 100, 20), "Spawn Dots"))
            {
                field.DebugDropNewDots();
            }

            if (GUI.Button(new Rect(10, 176, 100, 20), "Pause/Resume timer"))
            {
                timer.DebugPauseResume();
            }

            if (GUI.Button(new Rect(10, 196, 100, 20), "Clear dots"))
            {
                field.DebugClearDots();
            }

            if (GUI.Button(new Rect(10, 216, 100, 20), "Set Pattern"))
            {
                field.DebugSetPattern();
            }

            if (GUI.Button(new Rect(10, 236, 100, 20), "Reset Game"))
            {
                gameplay.DebugResetGame();
            }
        }
    }
#endif
}
