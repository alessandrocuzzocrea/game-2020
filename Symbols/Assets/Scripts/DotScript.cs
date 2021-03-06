﻿using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;

public class DotScript : MonoBehaviour
{
    public enum Type { Empty, Circle, Square, Diamond, Star, Queen } // #TODO: Gree, lmao
    private static readonly Color[] colors = new Color[] { Color.black, Color.white, Color.white, Color.white, Color.white, Color.white };

    [FormerlySerializedAs("spriteLoller")]
    public Sprite[] sprites;
    public Sprite[] spritesConnected;

    public int currentX;
    public int currentY;

    public int oldX;
    public int oldY;

    public Type color;
    public DotScript leftConnectedTo;
    public DotScript upConnectedTo;
    public GameObject sprite;
    public GameObject highlight;
    public string newName;

    public int newCurrentRow;
    public int newCurrentColumn;
    public bool isMoving;

    public float speed = 0.05f;
    public Animator animator;

    [SerializeField]
    private GameObject rowSelect;

    [SerializeField]
    private GameObject columnSelect;

    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<SpriteRenderer>().color = loller[Random.Range(0, 3)];
        isMoving = false;
        name = $"{currentX}_{currentY}";
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            float step = speed * Time.deltaTime;

            // move sprite towards the target location
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, new Vector3(currentX, currentY), .2f);

            if (transform.localPosition == new Vector3(currentX, currentY))
            {
                isMoving = false;
                //currentX    = newCurrentRow;
                //currentY    = newCurrentColumn;
            }
        }

    }

    public void SetType(Type c)
    {
        color = c;

        if (color == Type.Empty)
        {
            animator.SetBool("IsClearing", true);
        }
        else
        {
            sprite.GetComponent<SpriteRenderer>().color     = colors[(int)color];
            sprite.GetComponent<SpriteRenderer>().sprite    = sprites[(int)color];
            highlight.GetComponent<SpriteRenderer>().sprite = spritesConnected[(int)color];

            animator.SetBool("IsClearing", false);
        }
    }

    public static DotScript.Type GetRandomColor()
    {
        return (DotScript.Type) UnityEngine.Random.Range(0, 4);
    }

    public void Init(int x, int y)
    {
        transform.localPosition = new Vector2(x, y);
        currentX = x;
        currentY = y;
        SetType(DotScript.Type.Empty);
    }

    public static Type GetRandomColor(List<Type> possibleColors)
    {
        if (possibleColors.Count == 0)
        {
            //possibleColors.Add(Type.Circle);
            //possibleColors.Add(Type.Square);
            //possibleColors.Add(Type.Diamond);
            //possibleColors.Add(Type.Star);

            possibleColors.Add(Type.Queen);
        }

        return possibleColors[UnityEngine.Random.Range(0, possibleColors.Count)];
    }

    public void SetNewName(string s)
    {
        newName = s;
    }

    public void SwapName()
    {
        SwapName(null);
    }

    public void SwapName(string neewName)
    {
        if (string.IsNullOrEmpty(neewName) == false)
        {
            newName = neewName;
        }

        if (string.IsNullOrEmpty(newName))
        {
            return;
        }

        name = newName;
        newName = null;

        int newX = System.Convert.ToInt32(name[0].ToString());
        int newY = System.Convert.ToInt32(name[2].ToString());

        oldX = currentX;
        oldY = currentY;

        currentX = newX;
        currentY = newY;

        isMoving = true;

        name = $"{newCurrentRow}_{newCurrentColumn}";


        //transform.localPosition = new Vector3(newX, newY, 0);
    }

    public void ToggleSelectVisibility(Lane.LaneType lt, bool b)
    {
        if (b)
        {
            if (Lane.LaneType.Row == lt)
            {
                rowSelect.SetActive(b);

            }
            else
            {
                columnSelect.SetActive(b);
            }
        }
        else
        {
            rowSelect.SetActive(b);
            columnSelect.SetActive(b);
        }
    }

    public static Color GetColorFromType(Type t)
    {
        Color res;

        switch (t)
        {
            case Type.Empty:
                res = Color.black;
                break;

            case Type.Circle:
                res = new Color(46 / 255.0f, 204 / 255.0f, 113 / 255.0f);
                break;

            case Type.Square:
                res = new Color(231 / 255.0f, 76 / 255.0f, 60 / 255.0f);
                break;

            case Type.Diamond:
                res = new Color(52 / 255.0f, 152 / 255.0f, 219 / 255.0f);
                break;

            case Type.Star:
                res = new Color(241 / 255.0f, 196 / 255.0f, 15 / 255.0f);
                break;

            case Type.Queen:
                res = new Color(155 / 255.0f, 89 / 255.0f, 182 / 255.0f);
                break;

            default:
                res = Color.black;
                break;
        }

        return res;
    }

    public void ResetConnections()
    {
        leftConnectedTo = upConnectedTo = null;
        highlight.gameObject.SetActive(false);
    }
}
