using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Data")]
    public TextAsset csvFile; // Load the CSV file placed in the Resources folder

    [Header("Difficulty CSV")]
    public TextAsset easyCsv;
    public TextAsset normalCsv;
    public TextAsset hardCsv;
    public TextAsset randomHellCsv;

    [Header("Easy CSV")]
    public TextAsset[] easyCsvStages = new TextAsset[4];

    [Header("Normal CSV")]
    public TextAsset[] normalCsvStages = new TextAsset[4];

    [Header("Hard CSV")]
    public TextAsset[] hardCsvStages = new TextAsset[4];

    [Header("Tile Prefabs")]
    public GameObject wellTilePrefab;  // 2 = Wall
    public GameObject pathTilePrefab;  // 1 = Path
    public GameObject trapTilePrefab;  // 3 = Trap
    public GameObject goalTilePrefab;  // 4 = Goal

    [Header("Player Settings")]
    public GameObject player;

    [Header("Settings")]
    public int faceSize = 6; // Grid size of a single face
    public float tileSize = 1f; // Size of a single tile

    public int mapWidth = 6;
    public int mapHeight = 6;
    public int mapDepth = 6;

    [Header("Goal Settings")]
    public float goalScale = 0.5f;
    public float goalInsideOffset = 0.25f;

    [Header("Title Preview Settings")]
    public bool rotatePreview = false;
    public float previewRotationSpeed = 8f;
    public Vector3 previewPosition = Vector3.zero;
    public Vector3 previewScale = Vector3.one;

    [Header("Difficulty Setting")]
    public bool useDifficultySettings = true;

    [Header("Minimum Wall Count")]
    public int easyMinWallCount = 70;
    public int normalMinWallCount = 120;
    public int hardMinWallCount = 180;
    public int hellMinWallCount = 320;

    private int currentMinWallCount = 0;
    private enum CubeFace
    {
        Top,
        Left,
        Front,
        Right,
        Back,
        Bottom
    }

    [Header("Random Base CSV")]
    public TextAsset randomEasyBaseCsv;    // rendom1
    public TextAsset randomNormalBaseCsv;  // rendom2
    public TextAsset randomHardBaseCsv;    // rendom3
    public TextAsset randomHellBaseCsv;    // rendom4

    [Header("Hard Random Settings")]
    public int seamSafeMargin = 1;
    public int startGoalSafeMargin = 2;
    public int startRightClearLength = 1;
    public int startGoalClearRadius = 1;

    [Range(0, 100)]
    public int extraOpenChance = 8;

    [Range(0, 100)]
    public int deadEndBranchChance = 35;

    public int maxDeadEndLength = 3;

    private bool hasStartTile = false;
    private Vector3 startInsideUp;
    private Vector3 startForward;

    [Header("Minimum Open Cells Per Face")]
    public int easyMinOpenPerFace = 4;
    public int normalMinOpenPerFace = 5;
    public int hardMinOpenPerFace = 6;
    public int hellMinOpenPerFace = 8;

    private int currentMinOpenPerFace = 4;

    [Header("Maze Route Settings")]
    public int easyMinGoalDistance = 25;
    public int normalMinGoalDistance = 40;
    public int hardMinGoalDistance = 60;
    public int hellMinGoalDistance = 90;

    public int currentMinGoalDistance = 25;

    [Range(0, 100)]
    public int loopChance = 8;

    public int maxLoopCount = 4;
    void Start()
    {
        if (useDifficultySettings && !rotatePreview)
        {
            ApplyDifficulty();
        }

        if (csvFile != null)
        {
            GenerateCubeMap();
        }
        else
        {
            GenerateRandomCubeMap();
        }
    }

    void ApplyDifficulty()
    {
        switch (GameData.SelectedDifficulty)
        {
            case DifficultyType.Easy:
                faceSize = 6;
                ApplyStageCsv(easyCsvStages);
                break;

            case DifficultyType.Normal:
                faceSize = 7;
                ApplyStageCsv(normalCsvStages);
                break;

            case DifficultyType.Hard:
                faceSize = 8;
                ApplyStageCsv(hardCsvStages);
                break;

            case DifficultyType.RandomHell:
                faceSize = 10;
                csvFile = null;
                break;
        }
    }

    void ApplyStageCsv(TextAsset[] stageCsvs)
    {
        int stage = GameData.SelectedStage;


        if (stage >= 1 && stage <= 4)
        {
            csvFile = stageCsvs[stage - 1];
        }

        else if (stage == 5)
        {
            csvFile = null;
        }
    }

    void GenerateRandomCubeMap()
    {
        TextAsset baseCsv = GetRandomBaseCsv();

        if (baseCsv == null)
        {
            Debug.LogWarning("Random base CSV is not assigned.");
            return;
        }

        ApplyRandomDifficultySettings();

        string[,] template = LoadBaseCsv(baseCsv);
        string[,] result = CreateWallFilledMap(template);

        Vector2Int startCell = PickStartCell(template);

        HashSet<Vector2Int> protectedCells = new HashSet<Vector2Int>();

        BuildDifficultPath(template, result, startCell, out Vector2Int goalCell, protectedCells);

        ClearAround(result, template, startCell, startGoalClearRadius);
        ClearAround(result, template, goalCell, startGoalClearRadius);

        ProtectAround(template, protectedCells, startCell, startGoalClearRadius);
        ProtectAround(template, protectedCells, goalCell, startGoalClearRadius);

        ClearStartRight(result, template, startCell);
        ProtectStartRight(template, protectedCells, startCell);

        protectedCells.Add(startCell);
        protectedCells.Add(goalCell);

      
        EnsureMinimumWallCount(template, result, protectedCells);

        EnsureEachFaceHasOpenCells(template, result, protectedCells);

        ApplyFoldEdgeConsistency(template, result);

        result[startCell.y, startCell.x] = "5";
        result[goalCell.y, goalCell.x] = "4";

        csvFile = new TextAsset(ConvertMapToCsv(result));

        Debug.Log("Hard random map generated. Face Size: " + faceSize);

        GenerateCubeMap();
    }
    void EnsureEachFaceHasOpenCells(
    string[,] template,
    string[,] result,
    HashSet<Vector2Int> protectedCells)
    {
        CubeFace[] faces =
        {
        CubeFace.Top,
        CubeFace.Left,
        CubeFace.Front,
        CubeFace.Right,
        CubeFace.Back,
        CubeFace.Bottom
    };

        for (int i = 0; i < faces.Length; i++)
        {
            CubeFace face = faces[i];

            int openCount = CountOpenCellsOnFace(template, result, face);

            if (openCount >= currentMinOpenPerFace)
            {
                continue;
            }

            int needCount = currentMinOpenPerFace - openCount;

            List<Vector2Int> candidates = GetWallCellsOnFace(template, result, face);

            ShuffleCells(candidates);

            int opened = 0;

            for (int j = 0; j < candidates.Count; j++)
            {
                if (opened >= needCount)
                {
                    break;
                }

                Vector2Int cell = candidates[j];

                result[cell.y, cell.x] = "1";
                protectedCells.Add(cell);

                opened++;
            }
        }
    }
    int CountOpenCellsOnFace(string[,] template, string[,] result, CubeFace face)
    {
        int count = 0;

        for (int row = 0; row < faceSize; row++)
        {
            for (int col = 0; col < faceSize; col++)
            {
                Vector2Int cell = GetFaceCell(face, row, col);

                if (!IsUsable(template, cell)) continue;

                if (result[cell.y, cell.x] != "2")
                {
                    count++;
                }
            }
        }

        return count;
    }
    List<Vector2Int> GetWallCellsOnFace(
    string[,] template,
    string[,] result,
    CubeFace face)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int row = 0; row < faceSize; row++)
        {
            for (int col = 0; col < faceSize; col++)
            {
                Vector2Int cell = GetFaceCell(face, row, col);

                if (!IsUsable(template, cell)) continue;

                if (result[cell.y, cell.x] == "2")
                {
                    cells.Add(cell);
                }
            }
        }

        return cells;
    }

    string[,] LoadBaseCsv(TextAsset baseCsv)
    {
        int height = faceSize * 3;
        int width = faceSize * 4;

        string[,] map = new string[height, width];

        string[] lines = baseCsv.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int r = 0; r < height; r++)
        {
            string[] cells = new string[0];

            if (r < lines.Length)
            {
                cells = lines[r].Split(',');
            }

            for (int c = 0; c < width; c++)
            {
                if (c < cells.Length)
                {
                    map[r, c] = cells[c].Trim();
                }
                else
                {
                    map[r, c] = "0";
                }
            }
        }

        return map;
    }

    string[,] CreateWallFilledMap(string[,] template)
    {
        int height = faceSize * 3;
        int width = faceSize * 4;

        string[,] result = new string[height, width];

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (template[r, c] == "0")
                {
                    result[r, c] = "0";
                }
                else
                {
                    
                    result[r, c] = "2";
                }
            }
        }

        return result;
    }

    Vector2Int PickStartCell(string[,] template)
    {
        List<Vector2Int> candidates = GetInnerCells(template, CubeFace.Front);

        if (candidates.Count == 0)
        {
            Debug.LogWarning("Start candidate not found.");
            return GetFaceCell(CubeFace.Front, faceSize / 2, faceSize / 2);
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
    void BuildDifficultPath(
     string[,] template,
     string[,] result,
     Vector2Int startCell,
     out Vector2Int goalCell,
     HashSet<Vector2Int> protectedCells = null)
    {
        if (protectedCells == null)
        {
            protectedCells = new HashSet<Vector2Int>();
        }

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();

        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        result[startCell.y, startCell.x] = "1";
        visited.Add(startCell);
        distance[startCell] = 0;
        stack.Push(startCell);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();

            List<Vector2Int> neighbors = GetUsableNeighbors(template, current);
            ShuffleCells(neighbors);

            neighbors.RemoveAll(n =>
                visited.Contains(n) ||
                !CanOpenCorridorCell(template, result, n)
            );

            if (neighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }

            Vector2Int next = neighbors[0];

            result[next.y, next.x] = "1";

            visited.Add(next);
            parent[next] = current;
            distance[next] = distance[current] + 1;
            stack.Push(next);
        }

        goalCell = PickFarGoalCellWithMinimumDistance(
            template,
            visited,
            distance,
            startCell,
            currentMinGoalDistance
        );

        List<Vector2Int> mainPath = ReconstructPath(parent, startCell, goalCell);

        for (int i = 0; i < mainPath.Count; i++)
        {
            Vector2Int p = mainPath[i];
            result[p.y, p.x] = "1";
            protectedCells.Add(p);
        }

        AddMazeBranches(template, result, mainPath, protectedCells);

        AddLoopConnections(template, result, protectedCells);
    }
    Vector2Int PickFarGoalCellWithMinimumDistance(
    string[,] template,
    HashSet<Vector2Int> visited,
    Dictionary<Vector2Int, int> distance,
    Vector2Int startCell,
    int minDistance)
    {
        Vector2Int best = startCell;
        int bestDistance = -1;

        Vector2Int fallback = startCell;
        int fallbackDistance = -1;

        CubeFace startFace;
        int sr;
        int sc;
        TryGetFaceInfo(startCell, out startFace, out sr, out sc);

        foreach (Vector2Int cell in visited)
        {
            if (!IsInnerSafeCell(cell)) continue;

            CubeFace face;
            int row;
            int col;

            if (!TryGetFaceInfo(cell, out face, out row, out col)) continue;

            if (face == startFace) continue;

            int d = distance[cell];

            if (d > fallbackDistance)
            {
                fallbackDistance = d;
                fallback = cell;
            }

            if (d >= minDistance && d > bestDistance)
            {
                bestDistance = d;
                best = cell;
            }
        }

        if (bestDistance >= 0)
        {
            return best;
        }

        return fallback;
    }
    bool CanOpenCorridorCell(string[,] template, string[,] result, Vector2Int cell)
    {
        if (!IsUsable(template, cell)) return false;
        if (result[cell.y, cell.x] != "2") return false;

        int openNeighborCount = CountOpenNeighbors(template, result, cell);

       
        if (openNeighborCount > 1)
        {
            return false;
        }

        if (WouldCreateWideArea(template, result, cell))
        {
            return false;
        }

        return true;
    }

    bool WouldCreateWideArea(string[,] template, string[,] result, Vector2Int cell)
    {
        for (int y = -1; y <= 0; y++)
        {
            for (int x = -1; x <= 0; x++)
            {
                int openCount = 0;

                for (int yy = 0; yy <= 1; yy++)
                {
                    for (int xx = 0; xx <= 1; xx++)
                    {
                        Vector2Int p = new Vector2Int(
                            cell.x + x + xx,
                            cell.y + y + yy
                        );

                        if (!IsUsable(template, p)) continue;

                        if (p == cell)
                        {
                            openCount++;
                        }
                        else if (result[p.y, p.x] != "2")
                        {
                            openCount++;
                        }
                    }
                }

                if (openCount >= 4)
                {
                    return true;
                }
            }
        }

        return false;
    }

    int CountOpenNeighbors(string[,] template, string[,] result, Vector2Int cell)
    {
        int count = 0;

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int next = cell + directions[i];

            if (!IsUsable(template, next)) continue;

            if (result[next.y, next.x] != "2")
            {
                count++;
            }
        }

        return count;
    }

    void AddMazeBranches(
    string[,] template,
    string[,] result,
    List<Vector2Int> mainPath,
    HashSet<Vector2Int> protectedCells)
    {
        for (int i = 0; i < mainPath.Count; i++)
        {
            if (UnityEngine.Random.Range(0, 100) > deadEndBranchChance)
            {
                continue;
            }

            Vector2Int current = mainPath[i];

            int length = UnityEngine.Random.Range(1, maxDeadEndLength + 1);

            for (int step = 0; step < length; step++)
            {
                List<Vector2Int> neighbors = GetUsableNeighbors(template, current);
                ShuffleCells(neighbors);

                neighbors.RemoveAll(n => !CanOpenCorridorCell(template, result, n));

                if (neighbors.Count == 0)
                {
                    break;
                }

                Vector2Int next = neighbors[0];

                result[next.y, next.x] = "1";
                protectedCells.Add(next);

                current = next;
            }
        }
    }

    void AddLoopConnections(
    string[,] template,
    string[,] result,
    HashSet<Vector2Int> protectedCells)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        int height = faceSize * 3;
        int width = faceSize * 4;

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                Vector2Int cell = new Vector2Int(c, r);

                if (!IsUsable(template, cell)) continue;
                if (result[r, c] != "2") continue;
                if (WouldCreateWideArea(template, result, cell)) continue;

                int openNeighborCount = CountOpenNeighbors(template, result, cell);

               
                if (openNeighborCount == 2)
                {
                    candidates.Add(cell);
                }
            }
        }

        ShuffleCells(candidates);

        int createdLoopCount = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            if (createdLoopCount >= maxLoopCount)
            {
                break;
            }

            if (UnityEngine.Random.Range(0, 100) > loopChance)
            {
                continue;
            }

            Vector2Int cell = candidates[i];

            if (WouldCreateWideArea(template, result, cell))
            {
                continue;
            }

            result[cell.y, cell.x] = "1";
            protectedCells.Add(cell);

            createdLoopCount++;
        }
    }



    Vector2Int PickFarGoalCell(
    string[,] template,
    HashSet<Vector2Int> visited,
    Dictionary<Vector2Int, int> distance,
    Vector2Int startCell)
    {
        Vector2Int best = startCell;
        int bestDistance = -1;

        CubeFace startFace;
        int sr;
        int sc;
        TryGetFaceInfo(startCell, out startFace, out sr, out sc);

        foreach (Vector2Int cell in visited)
        {
            if (!IsInnerSafeCell(cell)) continue;

            CubeFace face;
            int row;
            int col;

            if (!TryGetFaceInfo(cell, out face, out row, out col)) continue;

            if (face == startFace) continue;

            int d = distance[cell];

            if (d > bestDistance)
            {
                bestDistance = d;
                best = cell;
            }
        }

        return best;
    }
    void EnsureMinimumWallCount(
    string[,] template,
    string[,] result,
    HashSet<Vector2Int> protectedCells)
    {
        int currentWallCount = CountWalls(template, result);

        if (currentWallCount >= currentMinWallCount)
        {
            return;
        }

        List<Vector2Int> candidates = new List<Vector2Int>();

        int height = faceSize * 3;
        int width = faceSize * 4;

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                Vector2Int cell = new Vector2Int(c, r);

                if (!IsUsable(template, cell)) continue;
                if (protectedCells.Contains(cell)) continue;

                
                if (result[r, c] == "1")
                {
                    candidates.Add(cell);
                }
            }
        }

        ShuffleCells(candidates);

        int index = 0;

        while (currentWallCount < currentMinWallCount && index < candidates.Count)
        {
            Vector2Int cell = candidates[index];

            result[cell.y, cell.x] = "2";
            currentWallCount++;
            index++;
        }

        if (currentWallCount < currentMinWallCount)
        {
            Debug.LogWarning(
                "Minimum wall count could not be fully reached. " +
                "Current: " + currentWallCount +
                " / Target: " + currentMinWallCount
            );
        }
    }

    int CountWalls(string[,] template, string[,] result)
    {
        int count = 0;

        int height = faceSize * 3;
        int width = faceSize * 4;

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (template[r, c] == "0") continue;

                if (result[r, c] == "2")
                {
                    count++;
                }
            }
        }

        return count;
    }
    void ShuffleCells(List<Vector2Int> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, cells.Count);

            Vector2Int temp = cells[i];
            cells[i] = cells[randomIndex];
            cells[randomIndex] = temp;
        }
    }
    void ProtectAround(
    string[,] template,
    HashSet<Vector2Int> protectedCells,
    Vector2Int center,
    int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2Int p = new Vector2Int(center.x + x, center.y + y);

                if (!IsUsable(template, p)) continue;

                protectedCells.Add(p);
            }
        }
    }
    void ProtectStartRight(
    string[,] template,
    HashSet<Vector2Int> protectedCells,
    Vector2Int startCell)
    {
        CubeFace face;
        int row;
        int col;

        if (!TryGetFaceInfo(startCell, out face, out row, out col)) return;

        for (int i = 1; i <= startRightClearLength; i++)
        {
            int nextCol = col + i;

            if (nextCol >= faceSize) continue;

            Vector2Int rightCell = GetFaceCell(face, row, nextCol);

            if (IsUsable(template, rightCell))
            {
                protectedCells.Add(rightCell);
            }
        }
    }

    List<Vector2Int> ReconstructPath(
    Dictionary<Vector2Int, Vector2Int> parent,
    Vector2Int start,
    Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        Vector2Int current = goal;
        path.Add(current);

        while (current != start)
        {
            if (!parent.ContainsKey(current))
            {
                break;
            }

            current = parent[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
    void AddDeadEndBranches(string[,] template, string[,] result, List<Vector2Int> mainPath)
    {
        for (int i = 0; i < mainPath.Count; i++)
        {
            if (UnityEngine.Random.Range(0, 100) > deadEndBranchChance) continue;

            Vector2Int current = mainPath[i];

            int length = UnityEngine.Random.Range(1, maxDeadEndLength + 1);

            for (int step = 0; step < length; step++)
            {
                List<Vector2Int> neighbors = GetUsableNeighbors(template, current);

                if (neighbors.Count == 0) break;

                Vector2Int next = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];

                if (result[next.y, next.x] == "1") break;
                if (IsFaceEdge(next)) break;

                result[next.y, next.x] = "1";
                current = next;
            }
        }
    }

 

    void ClearStartRight(string[,] result, string[,] template, Vector2Int startCell)
    {
        CubeFace face;
        int row;
        int col;

        if (!TryGetFaceInfo(startCell, out face, out row, out col)) return;

        for (int i = 1; i <= startRightClearLength; i++)
        {
            int nextCol = col + i;

            if (nextCol >= faceSize - seamSafeMargin) continue;

            Vector2Int rightCell = GetFaceCell(face, row, nextCol);

            if (IsUsable(template, rightCell))
            {
                result[rightCell.y, rightCell.x] = "1";
            }
        }
    }

    void ClearAround(string[,] result, string[,] template, Vector2Int center, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2Int p = new Vector2Int(center.x + x, center.y + y);

                if (!IsUsable(template, p)) continue;

                result[p.y, p.x] = "1";
            }
        }
    }
    List<Vector2Int> GetInnerCells(string[,] template, CubeFace face)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int row = startGoalSafeMargin; row < faceSize - startGoalSafeMargin; row++)
        {
            for (int col = startGoalSafeMargin; col < faceSize - startGoalSafeMargin; col++)
            {
                Vector2Int p = GetFaceCell(face, row, col);

                if (IsUsable(template, p))
                {
                    cells.Add(p);
                }
            }
        }

        return cells;
    }

    Vector2Int GetFaceOrigin(CubeFace face)
    {
        switch (face)
        {
            case CubeFace.Top:
                return new Vector2Int(faceSize, 0);

            case CubeFace.Left:
                return new Vector2Int(0, faceSize);

            case CubeFace.Front:
                return new Vector2Int(faceSize, faceSize);

            case CubeFace.Right:
                return new Vector2Int(faceSize * 2, faceSize);

            case CubeFace.Back:
                return new Vector2Int(faceSize * 3, faceSize);

            case CubeFace.Bottom:
                return new Vector2Int(faceSize, faceSize * 2);

            default:
                return new Vector2Int(faceSize, faceSize);
        }
    }

    Vector2Int GetFaceCell(CubeFace face, int row, int col)
    {
        Vector2Int origin = GetFaceOrigin(face);
        return new Vector2Int(origin.x + col, origin.y + row);
    }

    bool TryGetFaceInfo(Vector2Int cell, out CubeFace face, out int row, out int col)
    {
        CubeFace[] faces =
        {
        CubeFace.Top,
        CubeFace.Left,
        CubeFace.Front,
        CubeFace.Right,
        CubeFace.Back,
        CubeFace.Bottom
    };

        for (int i = 0; i < faces.Length; i++)
        {
            Vector2Int origin = GetFaceOrigin(faces[i]);

            int localCol = cell.x - origin.x;
            int localRow = cell.y - origin.y;

            if (localRow >= 0 && localRow < faceSize &&
                localCol >= 0 && localCol < faceSize)
            {
                face = faces[i];
                row = localRow;
                col = localCol;
                return true;
            }
        }

        face = CubeFace.Front;
        row = 0;
        col = 0;
        return false;
    }

    bool IsFaceEdge(Vector2Int cell)
    {
        CubeFace face;
        int row;
        int col;

        if (!TryGetFaceInfo(cell, out face, out row, out col)) return false;

        return
            row < seamSafeMargin ||
            col < seamSafeMargin ||
            row >= faceSize - seamSafeMargin ||
            col >= faceSize - seamSafeMargin;
    }

    bool IsInnerSafeCell(Vector2Int cell)
    {
        CubeFace face;
        int row;
        int col;

        if (!TryGetFaceInfo(cell, out face, out row, out col)) return false;

        return
            row >= startGoalSafeMargin &&
            col >= startGoalSafeMargin &&
            row < faceSize - startGoalSafeMargin &&
            col < faceSize - startGoalSafeMargin;
    }

    List<Vector2Int> GetUsableNeighbors(string[,] template, Vector2Int cell)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int next = cell + directions[i];

            if (IsUsable(template, next))
            {
                result.Add(next);
            }
        }

        return result;
    }

    List<Vector2Int> GetShuffledUsableNeighbors(
        string[,] template,
        Vector2Int cell,
        HashSet<Vector2Int> visited)
    {
        List<Vector2Int> neighbors = GetUsableNeighbors(template, cell);

        for (int i = 0; i < neighbors.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, neighbors.Count);
            Vector2Int temp = neighbors[i];
            neighbors[i] = neighbors[randomIndex];
            neighbors[randomIndex] = temp;
        }

        neighbors.RemoveAll(p => visited.Contains(p));

        return neighbors;
    }

    bool IsUsable(string[,] template, Vector2Int cell)
    {
        int height = faceSize * 3;
        int width = faceSize * 4;

        if (cell.y < 0 || cell.y >= height) return false;
        if (cell.x < 0 || cell.x >= width) return false;

        return template[cell.y, cell.x] != "0";
    }

    string ConvertMapToCsv(string[,] map)
    {
        System.Text.StringBuilder builder = new System.Text.StringBuilder();

        int height = faceSize * 3;
        int width = faceSize * 4;

        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                builder.Append(map[r, c]);

                if (c < width - 1)
                {
                    builder.Append(",");
                }
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
    struct EdgeCellRef
    {
        public Vector2Int cell;

        public EdgeCellRef(Vector2Int cell)
        {
            this.cell = cell;
        }
    }

    void ApplyFoldEdgeConsistency(string[,] template, string[,] result)
    {
        Dictionary<Vector3Int, List<EdgeCellRef>> edgeGroups =
            new Dictionary<Vector3Int, List<EdgeCellRef>>();

        CubeFace[] faces =
        {
        CubeFace.Top,
        CubeFace.Left,
        CubeFace.Front,
        CubeFace.Right,
        CubeFace.Back,
        CubeFace.Bottom
    };

        for (int i = 0; i < faces.Length; i++)
        {
            AddFaceEdgeCells(edgeGroups, faces[i]);
        }

        foreach (KeyValuePair<Vector3Int, List<EdgeCellRef>> pair in edgeGroups)
        {
            List<EdgeCellRef> refs = pair.Value;

            if (refs.Count < 2)
            {
                continue;
            }

            bool hasOpen = false;

            for (int i = 0; i < refs.Count; i++)
            {
                Vector2Int cell = refs[i].cell;

                if (!IsUsable(template, cell)) continue;

                if (result[cell.y, cell.x] != "2")
                {
                    hasOpen = true;
                    break;
                }
            }

            string syncedValue = hasOpen ? "1" : "2";

            for (int i = 0; i < refs.Count; i++)
            {
                Vector2Int cell = refs[i].cell;

                if (!IsUsable(template, cell)) continue;

                result[cell.y, cell.x] = syncedValue;
            }
        }
    }
    void AddFaceEdgeCells(
    Dictionary<Vector3Int, List<EdgeCellRef>> edgeGroups,
    CubeFace face)
    {
        for (int i = 0; i < faceSize; i++)
        {
            AddEdgeCell(edgeGroups, face, 0, i, new Vector3(
                GetLocalX(i),
                0f,
                GetLocalZ(0) + tileSize * 0.5f
            ));

            AddEdgeCell(edgeGroups, face, faceSize - 1, i, new Vector3(
                GetLocalX(i),
                0f,
                GetLocalZ(faceSize - 1) - tileSize * 0.5f
            ));

            AddEdgeCell(edgeGroups, face, i, 0, new Vector3(
                GetLocalX(0) - tileSize * 0.5f,
                0f,
                GetLocalZ(i)
            ));

            AddEdgeCell(edgeGroups, face, i, faceSize - 1, new Vector3(
                GetLocalX(faceSize - 1) + tileSize * 0.5f,
                0f,
                GetLocalZ(i)
            ));
        }
    }

    void AddEdgeCell(
        Dictionary<Vector3Int, List<EdgeCellRef>> edgeGroups,
        CubeFace face,
        int row,
        int col,
        Vector3 localEdgePoint)
    {
        Vector2Int cell = GetFaceCell(face, row, col);

        Matrix4x4 matrix = GetFaceMatrix(face);
        Vector3 worldPoint = matrix.MultiplyPoint3x4(localEdgePoint);

        Vector3Int key = new Vector3Int(
            Mathf.RoundToInt(worldPoint.x * 1000f),
            Mathf.RoundToInt(worldPoint.y * 1000f),
            Mathf.RoundToInt(worldPoint.z * 1000f)
        );

        if (!edgeGroups.ContainsKey(key))
        {
            edgeGroups[key] = new List<EdgeCellRef>();
        }

        edgeGroups[key].Add(new EdgeCellRef(cell));
    }
    float GetLocalX(int col)
    {
        return (col - (faceSize / 2f) + 0.5f) * tileSize;
    }

    float GetLocalZ(int row)
    {
        return ((faceSize / 2f) - row - 0.5f) * tileSize;
    }
    Matrix4x4 GetFaceMatrix(CubeFace face)
    {
        float half = faceSize * tileSize / 2f;

        switch (face)
        {
            case CubeFace.Top:
                return Matrix4x4.TRS(
                    new Vector3(0, half, 0),
                    Quaternion.Euler(0, 0, 0),
                    Vector3.one
                );

            case CubeFace.Left:
                return Matrix4x4.TRS(
                    new Vector3(-half, 0, 0),
                    Quaternion.Euler(-90, 0, 90),
                    Vector3.one
                );

            case CubeFace.Front:
                return Matrix4x4.TRS(
                    new Vector3(0, 0, -half),
                    Quaternion.Euler(-90, 0, 0),
                    Vector3.one
                );

            case CubeFace.Right:
                return Matrix4x4.TRS(
                    new Vector3(half, 0, 0),
                    Quaternion.Euler(-90, 0, -90),
                    Vector3.one
                );

            case CubeFace.Back:
                return Matrix4x4.TRS(
                    new Vector3(0, 0, half),
                    Quaternion.Euler(-90, 180, 0),
                    Vector3.one
                );

            case CubeFace.Bottom:
                return Matrix4x4.TRS(
                    new Vector3(0, -half, 0),
                    Quaternion.Euler(180, 0, 0),
                    Vector3.one
                );

            default:
                return Matrix4x4.identity;
        }
    }
    TextAsset GetRandomBaseCsv()
    {
        switch (GameData.SelectedDifficulty)
        {
            case DifficultyType.Easy:
                faceSize = 6;
                return randomEasyBaseCsv;

            case DifficultyType.Normal:
                faceSize = 7;
                return randomNormalBaseCsv;

            case DifficultyType.Hard:
                faceSize = 8;
                return randomHardBaseCsv;

            case DifficultyType.RandomHell:
                faceSize = 10;
                return randomHellBaseCsv;

            default:
                faceSize = 6;
                return randomEasyBaseCsv;
        }
    }

    void ApplyRandomDifficultySettings()
    {
        switch (GameData.SelectedDifficulty)
        {
            case DifficultyType.Easy:
                extraOpenChance = 0;
                deadEndBranchChance = 45;
                maxDeadEndLength = 3;
                currentMinGoalDistance = easyMinGoalDistance;
                loopChance = 5;
                maxLoopCount = 2;
                break;

            case DifficultyType.Normal:
                extraOpenChance = 0;
                deadEndBranchChance = 60;
                maxDeadEndLength = 4;
                currentMinGoalDistance = normalMinGoalDistance;
                loopChance = 8;
                maxLoopCount = 3;
                break;

            case DifficultyType.Hard:
                extraOpenChance = 0;
                deadEndBranchChance = 75;
                maxDeadEndLength = 5;
                currentMinGoalDistance = hardMinGoalDistance;
                loopChance = 10;
                maxLoopCount = 4;
                break;

            case DifficultyType.RandomHell:
                extraOpenChance = 0;
                deadEndBranchChance = 85;
                maxDeadEndLength = 6;
                currentMinGoalDistance = hellMinGoalDistance;
                loopChance = 12;
                maxLoopCount = 6;
                break;
        }
    }



    void GenerateCubeMap()
    {
        hasStartTile = false;

        Transform oldChildCube = transform.Find("CubeMap_Center");

        if (oldChildCube != null)
        {
            Destroy(oldChildCube.gameObject);
        }

        GameObject oldWorldCube = GameObject.Find("CubeMap_Center");

        if (oldWorldCube != null)
        {
            Destroy(oldWorldCube);
        }

        // 1. Split CSV data into rows
        string[] lines = csvFile.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // 2. Create the central pillar parent object
        GameObject cubeMapCenter = new GameObject("CubeMap_Center");
        if (rotatePreview)
        {
            cubeMapCenter.transform.SetParent(transform);
            cubeMapCenter.transform.localPosition = previewPosition;
            cubeMapCenter.transform.localRotation = Quaternion.identity;
            cubeMapCenter.transform.localScale = previewScale;

            TitleMapRotator rotator = cubeMapCenter.AddComponent<TitleMapRotator>();
            rotator.rotationSpeed = previewRotationSpeed;
        }
        else
        {
            cubeMapCenter.transform.position = previewPosition;
            cubeMapCenter.transform.rotation = Quaternion.identity;
            cubeMapCenter.transform.localScale = Vector3.one;
        }
        //cubeMapCenter.transform.position = previewPosition;
        //cubeMapCenter.transform.localScale = previewScale;

        //if (rotatePreview)
        //{
        //    TitleMapRotator rotator = cubeMapCenter.AddComponent<TitleMapRotator>();
        //    rotator.rotationSpeed = previewRotationSpeed;
        //}

        float half = faceSize * tileSize / 2f;

        // 3. Create and assemble six faces
        CreateFace("Top", 0, faceSize, lines, cubeMapCenter,
            new Vector3(0, half, 0),
        Quaternion.Euler(0, 0, 0));

        CreateFace("Left", faceSize, 0, lines, cubeMapCenter,
            new Vector3(-half, 0, 0),
            Quaternion.Euler(-90, 0, 90));

        CreateFace("Front", faceSize, faceSize, lines, cubeMapCenter,
            new Vector3(0, 0, -half),
            Quaternion.Euler(-90, 0, 0));

        CreateFace("Right", faceSize, faceSize * 2, lines, cubeMapCenter,
            new Vector3(half, 0, 0),
            Quaternion.Euler(-90, 0, -90));

        CreateFace("Back", faceSize, faceSize * 3, lines, cubeMapCenter,
            new Vector3(0, 0, half),
            Quaternion.Euler(-90, 180, 0));

        CreateFace("Bottom", faceSize * 2, faceSize, lines, cubeMapCenter,
            new Vector3(0, -half, 0),
            Quaternion.Euler(180, 0, 0));

        if (player != null && hasStartTile)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.cubeCenter = cubeMapCenter.transform;
                playerController.InitializeStartDirection(startInsideUp, startForward);
            }
        }
    }

    public void GeneratePreviewMap(TextAsset previewCsv, int previewFaceSize)
    {
        csvFile = previewCsv;
        faceSize = previewFaceSize;

        if (csvFile == null)
        {
            return;
        }

        GenerateCubeMap();
    }

    // Create a specific face, place tiles, and rotate it
    void CreateFace(string faceName, int startRow, int startCol, string[] csvLines, GameObject parent, Vector3 offsetPosition, Quaternion faceRotation)
    {
        // Create an empty object for the face
        GameObject faceParent = new GameObject("Face_" + faceName);
        faceParent.transform.SetParent(parent.transform, false);
        faceParent.transform.localPosition = offsetPosition;
        faceParent.transform.localRotation = faceRotation;
        faceParent.transform.localScale = Vector3.one;

        // Create tiles from the specified CSV range
        for (int row = 0; row < faceSize; row++)
        {
            if (startRow + row >= csvLines.Length) continue;

            string[] cells = csvLines[startRow + row].Split(',');

            for (int col = 0; col < faceSize; col++)
            {
                if (startCol + col >= cells.Length) continue;

                string cellData = cells[startCol + col].Trim();

                // Calculate tile positions based on the face center
                Vector3 tilePos = new Vector3(
                    (col - (faceSize / 2f) + 0.5f) * tileSize,
                    0,
                    ((faceSize / 2f) - row - 0.5f) * tileSize
                );

                GameObject tilePrefab = null;

                // Assign a prefab based on the value
                if (cellData == "2") tilePrefab = wellTilePrefab;
                else if (cellData == "1" || cellData == "5" || cellData == "4") tilePrefab = pathTilePrefab;
                else if (cellData == "3") tilePrefab = trapTilePrefab;
                else if (cellData == "4") tilePrefab = goalTilePrefab;
                // Skip creation if the value is '0' (empty space)

                if (tilePrefab != null)
                {
                    GameObject tile = Instantiate(tilePrefab, faceParent.transform);
                    tile.transform.localPosition = tilePos; // Set the local position

                    if (cellData == "4" && goalTilePrefab != null)
                    {
                        GameObject goal = Instantiate(goalTilePrefab, faceParent.transform);

                        
                        goal.transform.localPosition = tilePos - Vector3.up * goalInsideOffset;

                        
                        goal.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);

                        goal.transform.localScale = goal.transform.localScale * goalScale;
                    }

                    if (cellData == "5" && player != null)
                    {
                        Vector3 insideUp = tile.transform.up;

                        player.transform.position = tile.transform.position + insideUp * 1f;

                        Quaternion startRotation = Quaternion.LookRotation(tile.transform.forward, insideUp);
                        player.transform.rotation = Quaternion.AngleAxis(90f, insideUp) * startRotation;

                        hasStartTile = true;
                        startInsideUp = insideUp;
                        startForward = player.transform.forward;
                    }
                }
            }
        }
    }
}