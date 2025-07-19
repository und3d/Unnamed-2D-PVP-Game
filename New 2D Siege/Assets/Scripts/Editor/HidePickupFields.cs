using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ToolGadget), true)]
public class ToolGadgetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target object
        var so = serializedObject;
        so.Update();

        var property = so.GetIterator();

        var showNext = property.NextVisible(true); // Start with script field

        while (showNext)
        {
            // Skip inherited GadgetBase fields by name
            if (IsFieldFromGadgetBase(property.name))
            {
                showNext = property.NextVisible(false);
                continue;
            }

            EditorGUILayout.PropertyField(property, true);
            showNext = property.NextVisible(false);
        }

        so.ApplyModifiedProperties();
    }

    private bool IsFieldFromGadgetBase(string fieldName)
    {
        // Fields you want to hide ONLY in ToolGadget
        return fieldName is "pickupHoldTime" or "pickUpKey" or "playerPickupDistance" or "cursorPickupDistance";
    }
}