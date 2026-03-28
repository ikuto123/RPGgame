using UnityEditor;
using UnityEngine;

public class AutoDataLinkWindow : EditorWindow
{
    [MenuItem("Tools/Auto Link Character Data")]
    public static void ShowWindow()
    {
        var window = GetWindow<AutoDataLinkWindow>("UIElements");
        window.titleContent = new GUIContent("Auto Link Character Data");
        window.Show();
    }

    private DataAutoLinker autoDataLink = new DataAutoLinker();
    
    private void OnGUI()
    {
        if (GUILayout.Button("Link Character Data to Character Status"))
        {
            autoDataLink.LinkData();
        }
    }
    
}
