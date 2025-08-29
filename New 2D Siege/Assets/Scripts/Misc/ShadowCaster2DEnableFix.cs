using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DefaultExecutionOrder(+500)] // after most enable-time work
public class ShadowCaster2DEnableFix : MonoBehaviour
{
    [Tooltip("Extra frames to wait before rebuilding (useful if parents change scale/rotation).")]
    public int extraDelayFrames = 0;

    [Tooltip("Nudges the SpriteRenderer sprite to force silhouette refresh when needed.")]
    public bool alsoToggleSprite = true;

    void OnEnable()
    {
        StartCoroutine(FixNextFrame());
    }

    IEnumerator FixNextFrame()
    {
        // Wait a frame (plus optional extra) so parenting/scales settle.
        yield return null;
        for (int i = 0; i < extraDelayFrames; i++) yield return null;

        var c = GetComponent<ShadowCaster2D>();
        if (!c) yield break;

        // Trim Edge only affects renderer silhouettes.
        if (!c.useRendererSilhouette)
            Debug.LogWarning($"'{name}' has Use Renderer Silhouette = OFF; Trim Edge won't apply.", this);

        // Force a runtime rebuild (works in builds, not just editor).
        c.enabled = false; c.enabled = true;
        bool useSil = c.useRendererSilhouette;
        c.useRendererSilhouette = !useSil;
        c.useRendererSilhouette = useSil;

        if (alsoToggleSprite)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr && sr.sprite)
            {
                var s = sr.sprite; sr.sprite = null; sr.sprite = s;
            }
        }

        // If using a CompositeShadowCaster2D above, re-toggle it to rebuild its combined mesh.
        var composite = GetComponentInParent<CompositeShadowCaster2D>();
        if (composite) { composite.enabled = false; composite.enabled = true; }
    }
}