﻿using UnityEngine;

public class MusicScript : MonoBehaviour
{
    private static bool initialized = false;
    private string animatorKey = "ShiftPitch";

    private Animator animator;

    private void OnEnable()
    {
        if (initialized)
        {
            Destroy(this.gameObject);
        }
        else
        {
            animator = GetComponent<Animator>();
            DontDestroyOnLoad(this.gameObject);
            initialized = true;
        }

        EventManager.OnGameOver += ShiftPitchUp;
        EventManager.OnReset    += ShiftPitchDown;

        //PlayMusic();
    }

    private void ShiftPitchUp()
    {
        animator.SetBool(animatorKey, true);
    }

    private void ShiftPitchDown()
    {
        animator.SetBool(animatorKey, false);
    }
}
