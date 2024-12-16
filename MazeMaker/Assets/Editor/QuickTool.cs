using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class QuickTool : EditorWindow
{
    private float lineHeight = 1.0f; // Default height

    [MenuItem("QuickTool/Open _%#T")]
    public static void ShowWindow()
    {
        var window = GetWindow<QuickTool>();
        window.titleContent = new GUIContent("QuickTool");
        window.minSize = new Vector2(280, 50);
    }

    private void CreateGUI()
    {
        var root = rootVisualElement;
        root.styleSheets.Add(Resources.Load<StyleSheet>("QuickTool_Style"));

        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("QuickTool_Main");
        quickToolVisualTree.CloneTree(root);

        // Create a text field for line height input
        TextField heightField = new TextField("Line Height");
        heightField.value = lineHeight.ToString();
        heightField.RegisterValueChangedCallback(evt =>
        {
            if (float.TryParse(evt.newValue, out float newHeight))
            {
                lineHeight = newHeight;
            }
            else
            {
                heightField.SetValueWithoutNotify(lineHeight.ToString());
            }
        });
        root.Add(heightField);

        var toolButtons = root.Query(className: "quicktool-button");
        toolButtons.ForEach(SetupButton);
    }

    private void SetupButton(VisualElement button)
    {
        var buttonIcon = button.Q(className: "quicktool-button-icon");
        var iconPath = "Icons/" + button.parent.name + "_icon";
        var iconAsset = Resources.Load<Texture2D>(iconPath);
        buttonIcon.style.backgroundImage = iconAsset;

        button.RegisterCallback<PointerUpEvent, string>(HandleButtonClick, button.parent.name);
        button.tooltip = button.parent.name;
    }

    private void HandleButtonClick(PointerUpEvent _, string buttonName)
    {
        Debug.Log("Button clicked: " + buttonName);
        if (buttonName == "Maze")
        {
            Debug.Log("Generating Maze...");
            GenerateMaze();
        }
        else
        {
            CreateObject(buttonName);
        }
    }

    private void CreateObject(string primitiveTypeName)
    {
        var pt = (PrimitiveType)Enum.Parse(typeof(PrimitiveType), primitiveTypeName, true);
        var go = ObjectFactory.CreatePrimitive(pt);
        go.transform.position = Vector3.zero;
    }

    private void GenerateMaze()
    {
        int layers = 5; // Number of squares
        float size = 10f; // Initial size of the largest square
        float decrement = 0.8f; // Size decrement factor

        for (int i = 0; i < layers; i++)
        {
            bool isFlipped = i % 2 == 1;
            Debug.Log("Creating square of size: " + size + (isFlipped ? " (flipped)" : ""));
            CreateSquare(size, isFlipped);
            size *= decrement;
        }
    }

    private void CreateSquare(float size, bool isFlipped)
    {
        float lineWidth = 0.1f; // Width of the line segments

        Vector3[] corners;
        if (isFlipped)
        {
            corners = new Vector3[]
            {
                new Vector3(size / 2, 0, -size / 2),
                new Vector3(-size / 2, 0, -size / 2),
                new Vector3(-size / 2, 0, size / 2),
                new Vector3(size / 2, 0, size / 2)
            };
        }
        else
        {
            corners = new Vector3[]
            {
                new Vector3(-size / 2, 0, size / 2),
                new Vector3(size / 2, 0, size / 2),
                new Vector3(size / 2, 0, -size / 2),
                new Vector3(-size / 2, 0, -size / 2)
            };
        }

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[(i + 1) % corners.Length];
            if (i == corners.Length - 1)
            {
                break; // Skip the last line to keep the bottom open
            }
            CreateLineSegment(start, end, lineWidth, lineHeight);
        }
    }

    private void CreateLineSegment(Vector3 start, Vector3 end, float width, float height)
    {
        GameObject lineSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // Calculate the position and scale
        Vector3 position = (start + end) / 2;
        Vector3 scale = new Vector3(width, height, Vector3.Distance(start, end));

        lineSegment.transform.position = position;
        lineSegment.transform.LookAt(end);
        lineSegment.transform.localScale = scale;
    }
}
