using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GadgetController))]
public class GadgetControllerEditor : Editor
{
    private SerializedProperty playerController;
    
    private SerializedProperty primaryGadget;
    private SerializedProperty secondaryGadget;

    private SerializedProperty gadgetKey;
    private SerializedProperty primaryGadgetCount;
    private SerializedProperty secondaryGadgetCount;

    private SerializedProperty hasDurability;
    private SerializedProperty hasDuration;
    private SerializedProperty canRecharge;
    private SerializedProperty toolDurability;
    private SerializedProperty primaryGadgetDuration;
    private SerializedProperty primaryGadgetDelay;

    private SerializedProperty gadgetPlaceablePreview;
    private SerializedProperty gadgetTool;
    private SerializedProperty gadgetToggle;
    private SerializedProperty gadgetVisual;
    private SerializedProperty gadgetPrefab;
    
    // Throwable
    private SerializedProperty throwOrigin;
    private SerializedProperty throwForce;
    
    private void OnEnable()
    {
        playerController = serializedObject.FindProperty("playerController");
        
        primaryGadget = serializedObject.FindProperty("primaryGadget");
        secondaryGadget = serializedObject.FindProperty("secondaryGadget");

        gadgetKey = serializedObject.FindProperty("gadgetKey");
        primaryGadgetCount = serializedObject.FindProperty("primaryGadgetCount");
        secondaryGadgetCount = serializedObject.FindProperty("secondaryGadgetCount");
        
        hasDurability = serializedObject.FindProperty("hasDurability");
        hasDuration = serializedObject.FindProperty("hasDuration");
        canRecharge = serializedObject.FindProperty("canRecharge");
        toolDurability = serializedObject.FindProperty("toolDurability");
        primaryGadgetDuration = serializedObject.FindProperty("primaryGadgetDuration");
        primaryGadgetDelay = serializedObject.FindProperty("primaryGadgetDelay");
        
        gadgetPlaceablePreview = serializedObject.FindProperty("gadgetPlaceablePreview");
        gadgetTool = serializedObject.FindProperty("gadgetTool");
        gadgetToggle = serializedObject.FindProperty("gadgetToggle");
        
        gadgetPrefab = serializedObject.FindProperty("gadgetPrefab");
        gadgetVisual = serializedObject.FindProperty("gadgetVisual");
        
        
        // Throwable
        throwOrigin = serializedObject.FindProperty("throwOrigin");
        throwForce = serializedObject.FindProperty("throwForce");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(playerController);
        
        EditorGUILayout.PropertyField(gadgetKey);

        // Only show gadget counts for non-Tool/Toggle types
        var gadgetType = (GadgetController.GadgetType)primaryGadget.enumValueIndex;
        if (gadgetType != GadgetController.GadgetType.Tool &&
            gadgetType != GadgetController.GadgetType.Toggle)
        {
            EditorGUILayout.PropertyField(primaryGadgetCount);
            EditorGUILayout.PropertyField(secondaryGadgetCount);
        }
        
        else if (gadgetType is GadgetController.GadgetType.Tool or GadgetController.GadgetType.Toggle)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tool Settings", EditorStyles.boldLabel);

            // Only show hasDurability if this is a Tool gadget
            if (gadgetType == GadgetController.GadgetType.Tool)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(hasDurability);
                if (EditorGUI.EndChangeCheck())
                {
                    if (hasDurability.boolValue)
                        hasDuration.boolValue = false;
                }

                if (hasDurability.boolValue)
                {
                    EditorGUILayout.PropertyField(toolDurability);
                }
            }

            // If not durable, allow duration settings (applies to both Tool and Toggle)
            if (!hasDurability.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(hasDuration);
                if (EditorGUI.EndChangeCheck())
                {
                    if (hasDuration.boolValue)
                        hasDurability.boolValue = false;
                }

                if (hasDuration.boolValue)
                {
                    EditorGUILayout.PropertyField(primaryGadgetDuration);
                    EditorGUILayout.PropertyField(primaryGadgetDelay);

                    EditorGUILayout.PropertyField(canRecharge);
                }
            }
        }
        
        EditorGUILayout.PropertyField(primaryGadget);
        EditorGUILayout.PropertyField(secondaryGadget);
        if (gadgetType != GadgetController.GadgetType.Toggle)
        {
            EditorGUILayout.PropertyField(gadgetVisual);
            if (gadgetType != GadgetController.GadgetType.Placeable)
                EditorGUILayout.PropertyField(gadgetPrefab);
        }

        DrawGadgetPreviewFor(primaryGadget, "Primary Gadget References");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGadgetPreviewFor(SerializedProperty gadgetProp, string label)
    {
        var gadgetType = (GadgetController.GadgetType)gadgetProp.enumValueIndex;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        switch (gadgetType)
        {
            case GadgetController.GadgetType.Placeable:
                EditorGUILayout.PropertyField(gadgetPlaceablePreview);
                break;
            case GadgetController.GadgetType.Throwable:
                EditorGUILayout.PropertyField(throwOrigin);
                EditorGUILayout.PropertyField(throwForce);
                break;
            case GadgetController.GadgetType.Drone:
                EditorGUILayout.PropertyField(throwOrigin);
                EditorGUILayout.PropertyField(throwForce);
                break;
            case GadgetController.GadgetType.Tool:
                EditorGUILayout.PropertyField(gadgetTool);
                break;
            case GadgetController.GadgetType.Toggle:
                EditorGUILayout.PropertyField(gadgetToggle);
                break;
        }
    }
}
