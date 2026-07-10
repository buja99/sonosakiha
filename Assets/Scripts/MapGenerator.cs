using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Data")]
    public TextAsset csvFile; // Load the CSV file placed in the Resources folder

    [Header("Tile Prefabs")]
    public GameObject wellTilePrefab;  // 2 = Wall
    public GameObject pathTilePrefab;  // 1 = Path
    public GameObject trapTilePrefab;  // 3 = Trap
    public GameObject goalTilePrefab;  // 4 = Goal
    //

    [Header("Player Settings")]
    public GameObject player;

    [Header("Settings")]
    public int faceSize = 6; // Grid size of a single face
    public float tileSize = 1f; // Size of a single tile

    [Header("Goal Settings")]
    public float goalScale = 0.5f;
    public float goalInsideOffset = 0.25f;

    [Header("Title Preview Settings")]
    public bool rotatePreview = false;
    public float previewRotationSpeed = 8f;
    public Vector3 previewPosition = Vector3.zero;
    public Vector3 previewScale = Vector3.one;
    void Start()
    {
        GenerateCubeMap();
    }

    void GenerateCubeMap()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file is not assigned!");
            return;
        }

        // 1. Split CSV data into rows
        string[] lines = csvFile.text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // 2. Create the central pillar parent object
        GameObject cubeMapCenter = new GameObject("CubeMap_Center");
        cubeMapCenter.transform.position = previewPosition;
        cubeMapCenter.transform.localScale = previewScale;

        if (rotatePreview)
        {
            TitleMapRotator rotator = cubeMapCenter.AddComponent<TitleMapRotator>();
            rotator.rotationSpeed = previewRotationSpeed;
        }

        float half = faceSize * tileSize / 2f;

        // 3. Create and assemble six faces
        CreateFace("Top", 0, faceSize, lines, cubeMapCenter, new Vector3(0, half, 0), Quaternion.Euler(0, 0, 0));
        CreateFace("Left", faceSize, 0, lines, cubeMapCenter, new Vector3(-half, 0, 0), Quaternion.Euler(-90, -90, 0));
        CreateFace("Front", faceSize, faceSize, lines, cubeMapCenter, new Vector3(0, 0, -half), Quaternion.Euler(-90, 0, 0));
        CreateFace("Right", faceSize, faceSize * 2, lines, cubeMapCenter, new Vector3(half, 0, 0), Quaternion.Euler(-90, 90, 0));
        CreateFace("Back", faceSize, faceSize * 3, lines, cubeMapCenter, new Vector3(0, 0, half), Quaternion.Euler(-90, 180, 0));
        CreateFace("Bottom", faceSize * 2, faceSize, lines, cubeMapCenter, new Vector3(0, -half, 0), Quaternion.Euler(180, 0, 0));
    }

    // Create a specific face, place tiles, and rotate it
    void CreateFace(string faceName, int startRow, int startCol, string[] csvLines, GameObject parent, Vector3 offsetPosition, Quaternion faceRotation)
    {
        // Create an empty object for the face
        GameObject faceParent = new GameObject("Face_" + faceName);
        faceParent.transform.SetParent(parent.transform);
        faceParent.transform.localPosition = offsetPosition; // Move it to the center of each face
        faceParent.transform.localRotation = faceRotation;   // Rotate it to the correct orientation

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
                else if (cellData == "1" || cellData == "5") tilePrefab = pathTilePrefab;
                else if (cellData == "3") tilePrefab = trapTilePrefab;
                else if (cellData == "4") tilePrefab = goalTilePrefab;
                // Skip creation if the value is '0' (empty space)

                if (tilePrefab != null)
                {
                    GameObject tile = Instantiate(tilePrefab, faceParent.transform);
                    tile.transform.localPosition = tilePos; // Set the local position

                    if (cellData == "4")
                    {
                        tile.transform.localScale = Vector3.one * goalScale;

                        tile.transform.localPosition += Vector3.up * goalInsideOffset;
                    }

                    if (cellData == "5" && player != null)
                    {
                        Vector3 insideUp = tile.transform.up;
                        // // Move the player to the tile position and slightly raise it to prevent clipping into the ground
                        player.transform.position = tile.transform.position + insideUp * 1f;
                        // Adjust the player's rotation to match the face orientation
                        Quaternion startRotation = Quaternion.LookRotation(tile.transform.forward, insideUp);
                        player.transform.rotation = Quaternion.AngleAxis(90f, insideUp) * startRotation;
                    }
                }
            }
        }
    }
}