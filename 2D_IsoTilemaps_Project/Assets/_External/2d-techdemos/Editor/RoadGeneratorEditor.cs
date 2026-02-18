using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for RoadGenerator - adds a big "Generate Roads" button
/// Place this in an "Editor" folder
/// </summary>
[CustomEditor(typeof(RoadGenerator))]
public class RoadGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        RoadGenerator generator = (RoadGenerator)target;
        
        EditorGUILayout.Space(20);
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fixedHeight = 40;
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🛣️ Generate Road Network", buttonStyle))
        {
            generator.GenerateRoadNetwork();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Setup Steps:\n" +
            "1. Assign Loading Zone transform\n" +
            "2. Assign 3 Drop-off Point transforms\n" +
            "3. Assign Road Prefabs\n" +
            "4. Click Generate Road Network\n\n" +
            "The roads will connect all points randomly each time!",
            MessageType.Info
        );
    }
}
