using System.Collections.Generic;
using UnityEngine;

public class FieldScript : MonoBehaviour
{
    [SerializeField]
    public GameObject DotPrefab;

    [SerializeField]
    public Transform dotsContainer;

    [SerializeField]
    private int rows;

    [SerializeField]
    private int columns;

    [SerializeField]
    private int maxDrop;

    [SerializeField]
    private int noConnectionRequired;

    private DotScript[]   dots;
    private Lane          currentPick;
    private Lane[]        possibleCurrentPick;
    private Lane.LaneType currentPickType;

    private int  possibleDropId;
    private Lane currentDrop;

    private bool    bMouseCoordsOnClick;
    private Vector2 vMouseCoordsOnClick;
    private bool    bMouseCoordsNow;
    private Vector2 vMouseCoordsNow;
    private bool    bCurrentPickTypeLocked;

    //Dependencies
    private GameplayScript    gameplayScript;
    private TimerScript       timerScript;
    private FieldLabelsScript fieldLabelsScript;

    [SerializeField]
    private GameObject touchPoint;

    private void OnEnable()
    {
        EventManager.OnTimerEnd += ClearDots;
        EventManager.OnTimerEnd += DropNewDots;

        EventManager.OnTouchStart += OnTouchStart;
        EventManager.OnTouchMove  += OnTouchMove;
        EventManager.OnTouchEnd   += OnTouchEnd;
    }

    private void OnDisable()
    {
        EventManager.OnTimerEnd -= ClearDots;
        EventManager.OnTimerEnd -= DropNewDots;

        EventManager.OnTouchStart -= OnTouchStart;
        EventManager.OnTouchMove  -= OnTouchMove;
        EventManager.OnTouchEnd   -= OnTouchEnd;
    }

    void Start()
    {
        Application.targetFrameRate = 60; // TODO: test

        GetDependencies();
        SetupDots();
    }

    private void GetDependencies()
    {
        gameplayScript    = GameObject.FindObjectOfType<GameplayScript>();
        timerScript       = GameObject.FindObjectOfType<TimerScript>();
        fieldLabelsScript = GameObject.FindObjectOfType<FieldLabelsScript>();
    }

    private void SetupDots()
    {
        dots = new DotScript[rows * columns];
        possibleCurrentPick = new Lane[2];

        for (int i = 0; i < rows * columns; i++)
        {
            int x = i % rows;
            int y = i / columns;

            GameObject child = Instantiate(DotPrefab, Vector3.zero, Quaternion.identity, dotsContainer) as GameObject;
            DotScript dot = child.GetComponent<DotScript>();
            dot.Init(x, y);
            dots[i] = dot;
        }

        possibleDropId = -1;
    }

    void OnTouchStart()
    {
        if (gameplayScript.IsGameOver)
        {
            return;
        }

        Vector2 pos = InputScript.GetTouchPosition();
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);

        for (int i = 0; i < hits.Length; i++)
        {
            possibleCurrentPick[i] = hits[i].collider.GetComponent<Lane>();
        }

        bMouseCoordsOnClick = true;
        vMouseCoordsOnClick = pos;

        bMouseCoordsNow = true;
        vMouseCoordsNow = pos;

        currentPick = null;

        Vector3 lollo = Camera.main.ScreenToWorldPoint(pos);
        touchPoint.transform.position = new Vector3(lollo.x, lollo.y, 0.0f);
        touchPoint.SetActive(true);
    }

    void OnTouchMove()
    {
        if (gameplayScript.IsGameOver)
        {
            return;
        }

        Vector2 pos = InputScript.GetTouchPosition();
        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);

        bMouseCoordsNow = true;
        vMouseCoordsNow = pos;

        float dist = Vector3.Distance(vMouseCoordsNow, vMouseCoordsOnClick);
        float dotProduct = Vector3.Dot(Vector3.right, (vMouseCoordsNow - vMouseCoordsOnClick).normalized);

        if (!bCurrentPickTypeLocked && dist >= 5.0f)
        {
            float absDotProduct = Mathf.Abs(dotProduct);
            if (absDotProduct >= 0.5)
            {
                currentPickType = Lane.LaneType.Columns;
            }
            else
            {
                currentPickType = Lane.LaneType.Row;
            }

            //find current pick
            for (int i = 0; i < possibleCurrentPick.Length; i++)
            {
                if (possibleCurrentPick[i].laneType == currentPickType)
                {
                    currentPick = possibleCurrentPick[i];
                }
            }

            if (currentPickType == Lane.LaneType.Row)
            {
                DotScript dot = GetDotAtXY(0, currentPick.id);
                dot.ToggleSelectVisibility(currentPickType, true);
            }
            else
            {
                DotScript dot = GetDotAtXY(currentPick.id, 0);
                dot.ToggleSelectVisibility(currentPickType, true);
            }

            bCurrentPickTypeLocked = true;
        }

        if (!currentPick)
        {
            return;
        }

        // find possible drop
        for (int i = 0; i < hits.Length; i++)
        {
            Lane possibleDrop = hits[i].collider.GetComponent<Lane>();
            if (possibleDrop.laneType == currentPickType)
            {
                currentDrop = possibleDrop;
            }
        }

        if (possibleDropId != currentDrop.id)
        {
            possibleDropId = currentDrop.id;

            if (currentPick.id < currentDrop.id)
            {
                for (int j = currentDrop.id; currentPick.id < j; j--)
                {
                    if (currentPickType == Lane.LaneType.Columns)
                    {
                        MoveColumn(j, j - 1);
                    }
                    else
                    {
                        MoveRow(j, j - 1);
                    }
                }
            }
            else if (currentPick.id > currentDrop.id)
            {
                for (int j = currentDrop.id; j < currentPick.id; j++)
                {
                    if (currentPickType == Lane.LaneType.Columns)
                    {
                        MoveColumn(j, j + 1);
                    }
                    else
                    {
                        MoveRow(j, j + 1);
                    }
                }
            }

            if (currentPickType == Lane.LaneType.Columns)
            {
                MoveColumn(currentPick.id, currentDrop.id);
            }
            else
            {
                MoveRow(currentPick.id, currentDrop.id);
            }

            foreach (DotScript dot in dots)
            {
                dot.SwapName();
            }

            UpdateConnections();
        }

        // update current pick
        for (int i = 0; i < hits.Length; i++)
        {
            possibleCurrentPick[i] = hits[i].collider.GetComponent<Lane>();
        }

        // find current pick
        for (int i = 0; i < possibleCurrentPick.Length; i++)
        {
            if (possibleCurrentPick[i].laneType == currentPickType)
            {
                currentPick = possibleCurrentPick[i];
            }
        }

        Vector3 lollo = Camera.main.ScreenToWorldPoint(pos);
        touchPoint.transform.position = new Vector3(lollo.x, lollo.y, 0.0f);
    }

    void OnTouchEnd()
    {
        if (gameplayScript.IsGameOver)
        {
            return;
        }

        currentPick = null;
        currentDrop = null;
        possibleCurrentPick[0] = null;
        possibleCurrentPick[1] = null;
        possibleDropId = -1;

        bMouseCoordsNow = bMouseCoordsOnClick = bCurrentPickTypeLocked = false;

        foreach (DotScript dot in dots)
        {
            dot.ToggleSelectVisibility(0, false);
        }

        touchPoint.SetActive(false);
    }

    private void MoveColumn(int j, int v)
    {
        for (int i = 0; i < columns; i++)
        {
            MoveDot(GetDotAtXY(j, i), v, i);
        }
    }

    private void MoveRow(int j, int v)
    {
        for (int i = 0; i < columns; i++)
        {
            MoveDot(GetDotAtXY(i, j), i, v);
        }
    }

    private void MoveDot(DotScript dotScript, int x, int y)
    {
        dotScript.SetNewName($"{x}_{y}");
    }

    private void ClearDots()
    {
        List<List<DotScript>> dotsToClear = new List<List<DotScript>>();

        //Rows
        HashSet<DotScript> rowsSet = new HashSet<DotScript>();
        for (int j = 0; j < columns; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                List<DotScript> dots = new List<DotScript>();

                var dot = GetDotAtXY(j, i);
                if (dot && dot.color != DotScript.Type.Empty && !rowsSet.Contains(dot))
                {
                    while (true)
                    {
                        dots.Add(dot);
                        rowsSet.Add(dot);
                        if (dot.leftConnectedTo)
                        {
                            dot = dot.leftConnectedTo;
                        }
                        else
                        {
                            if (dots.Count > 1)
                            {
                                dotsToClear.Add(dots);
                            }
                            break;
                        }
                    }
                }
            }
        }

        //Columns
        HashSet<DotScript> columnsSet = new HashSet<DotScript>();
        for (int j = 0; j < columns; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                List<DotScript> dots = new List<DotScript>();

                var dot = GetDotAtXY(j, i);
                if (dot && dot.color != DotScript.Type.Empty && !columnsSet.Contains(dot))
                {
                    while (true)
                    {
                        dots.Add(dot);
                        columnsSet.Add(dot);
                        if (dot.upConnectedTo)
                        {
                            dot = dot.upConnectedTo;
                        }
                        else
                        {
                            if (dots.Count > 1)
                            {
                                dotsToClear.Add(dots);
                            }
                            break;
                        }
                    }
                }
            }
        }

        foreach (var l in dotsToClear)
        {
            Color listColor = Color.black;
            int listCount = l.Count;

            foreach (var dot in l)
            {
                listColor = DotScript.GetColorFromType(dot.color);
                var ps    = dot.GetComponentInChildren<ParticleSystem>();
                var col   = ps.colorOverLifetime;
                var grad  = new Gradient();

                grad.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(listColor, 0.0f),
                        new GradientColorKey(Color.white, 1.0f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.0f, 0.0f),
                        new GradientAlphaKey(1.0f, 0.5f),
                        new GradientAlphaKey(0.0f, 1.0f)
                    }
                );
                col.color = new ParticleSystem.MinMaxGradient(grad);
                dot.GetComponentInChildren<ParticleSystem>().Play();
                dot.SetType(DotScript.Type.Empty);
                dot.highlight.gameObject.SetActive(false);
            }

            int score = CalcScore(listCount);

            EventManager.OnIncreaseScore(score);
            EventManager.OnClearDots(l, listColor, score);
        }

    }

    private int CalcScore(int c)    
    {
        int res = 0;

        switch (c)
        {
            case 2:
                res = 20;
                break;
            case 3:
                res = 60;
                break;
            case 4:
                res = 180;
                break;
            case 5:
                res = 540;
                break;
            case 6:
                res = 1620;
                break;
        }

        return res;
    }

    void UpdateConnections()
    {
        // Reset all connections
        foreach (var dot in dots)
        {
            dot.leftConnectedTo = dot.upConnectedTo = null;
            dot.highlight.gameObject.SetActive(false);
            if (dot.leftConnectedTo) dot.leftConnectedTo.highlight.gameObject.SetActive(false);
            if (dot.upConnectedTo) dot.upConnectedTo.highlight.gameObject.SetActive(false);
        }

        // Check for new connections
        for (int j = 0; j < columns; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                DotScript dot = GetDotAtXY(j, i);

                if (dot && dot.color != DotScript.Type.Empty)
                {
                    // Left neighbour
                    DotScript leftNeighbour = GetDotAtXY(j + 1, i);
                    if (leftNeighbour && dot.color == leftNeighbour.color)
                    {
                        dot.highlight.gameObject.SetActive(true);
                        dot.leftConnectedTo = leftNeighbour;
                        dot.leftConnectedTo.highlight.gameObject.SetActive(true);
                    }

                    // Upper neighbour
                    DotScript upNeighbour = GetDotAtXY(j, i + 1);
                    if (upNeighbour && dot.color == upNeighbour.color)
                    {
                        dot.highlight.gameObject.SetActive(true);
                        dot.upConnectedTo = upNeighbour;
                        dot.upConnectedTo.highlight.gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    List<DotScript.Type> DropDotCheckRules(int currX, int currY, List<DotScript.Type> colors)
    {
        List<DotScript.Type> possibleColors = new List<DotScript.Type>(colors);

        // left
        DotScript leftDot = GetDotAtXY(currX - 1, currY);
        if (leftDot && leftDot.color != DotScript.Type.Empty)
        {
            possibleColors.Remove(leftDot.color);
        }

        // right
        DotScript rightDot = GetDotAtXY(currX + 1, currY);
        if (rightDot && rightDot.color != DotScript.Type.Empty)
        {
            possibleColors.Remove(rightDot.color);
        }

        // up
        DotScript upDot = GetDotAtXY(currX, currY + 1);
        if (upDot && upDot.color != DotScript.Type.Empty)
        {
           possibleColors.Remove(upDot.color);
        }

        // down
        DotScript downDot = GetDotAtXY(currX, currY - 1);
        if (downDot && downDot.color != DotScript.Type.Empty)
        {
           possibleColors.Remove(downDot.color);
        }

        return possibleColors;
    }

    void DropNewDots()
    {
        if (CountDots() >= columns * rows)
        {
            EventManager.OnGameOver();
        }

        int leftToDropCount = maxDrop;

        var candidates = new List<DotScript>();
        var randomDots = new List<DotScript>(dots);
        randomDots.Shuffle<DotScript>();

        foreach (DotScript dot in randomDots)
        {
            if (leftToDropCount <= 0)
            {
                break;
            }

            if (dot && dot.color == DotScript.Type.Empty)
            {
                var possibleColors = new List<DotScript.Type>();

                if (ShouldDropQueen())
                {
                    possibleColors.Add(DotScript.Type.Queen);
                    leftToDropCount = 0;
                }
                else
                {
                    possibleColors.AddRange(
                        new List<DotScript.Type> {
                            DotScript.Type.Circle,
                            DotScript.Type.Square,
                            DotScript.Type.Diamond,
                            DotScript.Type.Star
                        }
                    );
                }

                var remainingColors = DropDotCheckRules(dot.currentX, dot.currentY, possibleColors);

                DotScript.Type color = DotScript.GetRandomColor(remainingColors);
                dot.SetType(color);
                leftToDropCount -= 1;
            }
        }
        UpdateConnections();
    }

    private bool ShouldDropQueen()
    {
        return timerScript.Turn % 10 == 0;
    }

    private DotScript GetDotAtXY(int x, int y)
    {
        foreach(var dot in dots)
        {
            if (dot.currentX == x && dot.currentY == y)
            {
                return dot;
            }
        }

        return null;
    }

    private int CountDots(bool onlyEmpty = false)
    {
        var res = 0;
        foreach (var dot in dots)
        {
            if (onlyEmpty)
            {
                if (dot.color == DotScript.Type.Empty) res += 1;
            }
            else
            {
                if(dot.color != DotScript.Type.Empty)  res += 1;
            }
        }

        return res;
    }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public Lane DebugCurrentPick()
    {
        return currentPick;
    }

    public int DebugPossibleDropId()
    {
        return possibleDropId;
    }

    public int DebugCountDots(bool b = false)
    {
        return CountDots(b);
    }

    public int DebugColumns()
    {
        return columns;
    }

    public int DebugRows()
    {
        return rows;
    }

    public bool DebugbMouseCoordsOnClick()
    {
        return bMouseCoordsOnClick;
    }

    public Vector2 DebugvMouseCoordsOnClick()
    {
        return vMouseCoordsOnClick;
    }

    public bool DebugbMouseCoordsNow()
    {
        return bMouseCoordsNow;
    }

    public Vector2 DebugvMouseCoordsNow()
    {
        return vMouseCoordsNow;
    }

    public void DebugDropNewDots()
    {
        DropNewDots();
    }

    public void DebugClearDots()
    {
        ClearDots();
    }

    public void DebugSetPattern()
    {
        foreach (var d in dots)
        {
            d.SetType(DotScript.Type.Empty);
        }

        GetDotAtXY(0, 0).SetType(DotScript.Type.Star);
        GetDotAtXY(2, 0).SetType(DotScript.Type.Star);
        GetDotAtXY(4, 0).SetType(DotScript.Type.Star);
        GetDotAtXY(0, 2).SetType(DotScript.Type.Star);
        GetDotAtXY(0, 4).SetType(DotScript.Type.Star);
    }
#endif
}
