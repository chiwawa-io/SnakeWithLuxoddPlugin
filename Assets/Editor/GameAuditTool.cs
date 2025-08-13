// This script MUST be in a folder named "Editor" in your Assets folder.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GameAuditTool : EditorWindow
{
    private SpawnPointMap selectedMap;
    private string itemNameToDraw = "Food"; // The item type to visualize
    private Vector2 scrollPosition;

    // --- Grid Settings ---
    private const int VISIBLE_GRID_DIM = 22; // 11x11 visual grid (-5 to 5)
    private const int CELL_SIZE = 25;       // Size of each square in pixels
    private Color gridLineColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    private Color spawnPointColor = new Color(0, 0.8f, 0.8f, 1f); // Bright cyan
    private Color outOfBoundsColor = new Color(1f, 0.3f, 0.3f, 1f); // Bright red
    private GUIStyle labelStyle;

    [MenuItem("Window/Game Audit Tool")]
    public static void ShowWindow()
    {
        GetWindow<GameAuditTool>("Game Audit Tool");
    }

    void OnEnable()
    {
        // Initialize GUIStyle here to avoid creating it every frame in OnGUI
        labelStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 9
        };
    }

    void OnGUI()
    {
        GUILayout.Label("Spawn Point Map Auditor", EditorStyles.boldLabel);
        
        selectedMap = (SpawnPointMap)EditorGUILayout.ObjectField("Map to Audit", selectedMap, typeof(SpawnPointMap), false);
        
        // Let user choose which item list to draw
        if (selectedMap != null)
        {
            List<string> itemNames = new List<string>();
            foreach (var list in selectedMap.spawnPoints)
            {
                itemNames.Add(list.itemName);
            }
            
            int selectedIndex = itemNames.IndexOf(itemNameToDraw);
            if(selectedIndex < 0) selectedIndex = 0; // Default to first item if current one not found

            if(itemNames.Count > 0)
            {
                selectedIndex = EditorGUILayout.Popup("Visualize Item", selectedIndex, itemNames.ToArray());
                itemNameToDraw = itemNames[selectedIndex];
            }
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (selectedMap != null && !string.IsNullOrEmpty(itemNameToDraw))
        {
            DrawVisualGrid();
        }

        EditorGUILayout.EndScrollView();
    }
    
    void DrawVisualGrid()
    {
        // Define the area for our grid + labels
        float padding = CELL_SIZE * 1.5f;
        Rect areaRect = GUILayoutUtility.GetRect(VISIBLE_GRID_DIM * CELL_SIZE + padding, 
                                                VISIBLE_GRID_DIM * CELL_SIZE + padding);
        Rect gridRect = new Rect(areaRect.x + padding/2, areaRect.y, 
                                 VISIBLE_GRID_DIM * CELL_SIZE, VISIBLE_GRID_DIM * CELL_SIZE);


        // --- Draw the Grid Background and Lines ---
        EditorGUI.DrawRect(gridRect, new Color(0.1f, 0.1f, 0.1f)); // Dark background
        
        Handles.BeginGUI();
        Handles.color = gridLineColor;
        for (int i = 0; i <= VISIBLE_GRID_DIM; i++)
        {
            Handles.DrawLine(new Vector3(gridRect.x + i * CELL_SIZE, gridRect.y), 
                             new Vector3(gridRect.x + i * CELL_SIZE, gridRect.y + gridRect.height));
            Handles.DrawLine(new Vector3(gridRect.x, gridRect.y + i * CELL_SIZE),
                             new Vector3(gridRect.x + gridRect.width, gridRect.y + i * CELL_SIZE));
        }
        Handles.EndGUI();


        // --- Draw the Spawn Points for the selected item type ---
        List<Vector2Int> spawnPointsToDraw = selectedMap.GetPointsFor(itemNameToDraw);
        if(spawnPointsToDraw == null) return;
        
        foreach (Vector2Int point in spawnPointsToDraw)
        {
            // Center of our visual grid
            const int centerOffset = VISIBLE_GRID_DIM / 2;

            int gridX = point.x + centerOffset;
            int gridY = point.y + centerOffset;
            
            // Invert Y axis for GUI drawing (top-left is 0,0)
            gridY = (VISIBLE_GRID_DIM - 1) - gridY;
            
            // Check if the point is INSIDE the visible grid area
            if (gridX >= 0 && gridX < VISIBLE_GRID_DIM && gridY >= 0 && gridY < VISIBLE_GRID_DIM)
            {
                Rect cellRect = new Rect(gridRect.x + gridX * CELL_SIZE + 1, 
                                       gridRect.y + gridY * CELL_SIZE + 1, 
                                       CELL_SIZE - 2, CELL_SIZE - 2); // Inset by 1px
                
                EditorGUI.DrawRect(cellRect, spawnPointColor);
            }
            else
            {
                // If it's outside, draw a warning label for it
                EditorGUILayout.HelpBox($"Point '{itemNameToDraw}' at ({point.x}, {point.y}) is OUTSIDE the visual grid bounds.", MessageType.Warning);
            }
        }
        
        // --- Draw Coordinate Labels ---
        for (int i = 0; i < VISIBLE_GRID_DIM; i++)
        {
            int coordValue = i - (VISIBLE_GRID_DIM / 2);
            
            // Y-axis labels (Left side)
            Rect yLabelRect = new Rect(gridRect.x - CELL_SIZE, gridRect.y + i * CELL_SIZE, CELL_SIZE, CELL_SIZE);
            GUI.Label(yLabelRect, ((VISIBLE_GRID_DIM / 2) - i).ToString(), labelStyle);
            
            // X-axis labels (Bottom)
            Rect xLabelRect = new Rect(gridRect.x + i * CELL_SIZE, gridRect.y + gridRect.height, CELL_SIZE, CELL_SIZE);
            GUI.Label(xLabelRect, coordValue.ToString(), labelStyle);
        }
    }
}