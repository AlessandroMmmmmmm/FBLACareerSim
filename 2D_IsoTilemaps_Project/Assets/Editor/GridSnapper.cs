using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool to help snap objects to a grid while building your city manually
/// Place this in an "Editor" folder in your Unity project
/// </summary>
[ExecuteInEditMode]
public class GridSnapper : EditorWindow
{
    private float gridSize = 5f;
    private bool snapEnabled = true;
    private bool showGrid = true;
    
    [MenuItem("Tools/Grid Snapper")]
    public static void ShowWindow()
    {
        GetWindow<GridSnapper>("Grid Snapper");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);
        
        gridSize = EditorGUILayout.FloatField("Grid Size", gridSize);
        snapEnabled = EditorGUILayout.Toggle("Snap Enabled", snapEnabled);
        showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Snap Selected Objects"))
        {
            SnapSelectedObjects();
        }
        
        if (GUILayout.Button("Align Selected to Ground"))
        {
            AlignToGround();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Select objects in the scene and click 'Snap Selected Objects' to align them to the grid.\n\n" +
            "Or enable 'Snap Enabled' and objects will snap as you move them in the scene view.",
            MessageType.Info);
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        // Draw grid
        if (showGrid)
        {
            DrawGrid();
        }
        
        // Auto-snap while dragging
        if (snapEnabled && Selection.transforms.Length > 0)
        {
            foreach (Transform t in Selection.transforms)
            {
                Vector3 snappedPos = SnapToGrid(t.position);
                if (t.position != snappedPos)
                {
                    Undo.RecordObject(t, "Snap to Grid");
                    t.position = snappedPos;
                }
            }
        }
    }
    
    private void DrawGrid()
    {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        int gridCount = 50;
        float totalSize = gridSize * gridCount;
        
        // Draw grid lines
        for (int i = -gridCount; i <= gridCount; i++)
        {
            // Lines along X axis
            Handles.DrawLine(
                new Vector3(i * gridSize, 0, -totalSize),
                new Vector3(i * gridSize, 0, totalSize)
            );
            
            // Lines along Z axis
            Handles.DrawLine(
                new Vector3(-totalSize, 0, i * gridSize),
                new Vector3(totalSize, 0, i * gridSize)
            );
        }
    }
    
    private void SnapSelectedObjects()
    {
        foreach (Transform t in Selection.transforms)
        {
            Undo.RecordObject(t, "Snap to Grid");
            t.position = SnapToGrid(t.position);
        }
    }
    
    private Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            position.y,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }
    
    private void AlignToGround()
    {
        foreach (Transform t in Selection.transforms)
        {
            RaycastHit hit;
            if (Physics.Raycast(t.position + Vector3.up * 100, Vector3.down, out hit, 200f))
            {
                Undo.RecordObject(t, "Align to Ground");
                t.position = new Vector3(t.position.x, hit.point.y, t.position.z);
            }
        }
    }
}
