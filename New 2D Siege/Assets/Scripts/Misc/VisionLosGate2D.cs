using System.Collections.Generic;
using System.Linq;
using PurrNet;
using UnityEngine;

public class VisionLosGate2D : MonoBehaviour
{
    [Tooltip("Layers that block vision. Tick: SolidWalls + Barricades + DestWalls.")]
    public LayerMask occluders;

    [Range(1, 5)] public int samples = 5;           // center + 4 corners
    [Range(1, 5)] public int minClearSamples = 1;   // how many points must be clear
    public float stopShortEpsilon = 0.02f;          // avoid hitting behind target

    [Header("Debug Vars")]
    public bool drawRays = false;
    public Color clearColor = new Color(0f, 1f, 0f, 0.95f);
    public Color blockedColor = new Color(1f, 0f, 0f, 0.95f);
    public bool CheckClear = false;
    public float __UseUnshadowedMaskID = 0;

    static readonly int UseUnshadowedMaskID = Shader.PropertyToID("_UseUnshadowedMask");

    [SerializeField] private SpriteRenderer sr;   // set in Inspector
    [SerializeField] private Collider2D col;      // set in Inspector

    MaterialPropertyBlock mpb;
    Collider2D[] selfCols;

    // --- debug buffer ---
    struct RaySeg { public Vector3 a, b; public Color c; }
    readonly List<RaySeg> _debugRays = new List<RaySeg>(32);

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        selfCols = GetComponentsInChildren<Collider2D>(true);
    }

    void LateUpdate()
    {
        if (!InstanceHandler.TryGetInstance(out GameController gameController)) return;
        if (!gameController.playerTransform) return;

        var eye = gameController.playerTransform;

        // Clear debug buffer for this frame
        if (drawRays) _debugRays.Clear();

        // Sample points on this collider's bounds
        Bounds b = col.bounds;
        Vector2[] pts = {
            b.center,
            new Vector2(b.min.x, b.min.y),
            new Vector2(b.min.x, b.max.y),
            new Vector2(b.max.x, b.min.y),
            new Vector2(b.max.x, b.max.y),
        };

        int max = Mathf.Min(samples, pts.Length);
        int clear = 0;

        for (int i = 0; i < max; i++)
        {
            Vector2 origin = eye.position;
            Vector2 to = pts[i];
            Vector2 dir = to - origin;
            float dist = dir.magnitude;

            if (dist < 0.0001f)
            {
                clear++;
                if (drawRays)
                {
                    _debugRays.Add(new RaySeg { a = origin, b = to, c = clearColor });
                    Debug.DrawLine(origin, to, clearColor, Time.deltaTime, false);
                }
                continue;
            }

            float castDist = Mathf.Max(dist - stopShortEpsilon, 0f);
            var hits = Physics2D.RaycastAll(origin, dir.normalized, castDist, occluders);

            bool blocked = false;
            Vector2 endPoint = to;

            for (int h = 0; h < hits.Length; h++)
            {
                var hit = hits[h];
                if (!hit.collider || hit.collider.isTrigger) continue;

                // Ignore any collider belonging to this object (self or children)
                if (selfCols.Contains(hit.collider)) continue;
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;

                blocked = true;
                endPoint = hit.point; // draw to first valid hit
                break;
            }

            if (!blocked) clear++;

            if (drawRays)
            {
                var color = blocked ? blockedColor : clearColor;
                _debugRays.Add(new RaySeg { a = origin, b = endPoint, c = color });
                Debug.DrawLine(origin, endPoint, color, Time.deltaTime, false);
            }
        }

        bool anyClear = clear >= Mathf.Clamp(minClearSamples, 1, max);
        CheckClear = anyClear;

        sr.GetPropertyBlock(mpb);
        mpb.SetFloat(UseUnshadowedMaskID, anyClear ? 1f : 0f);
        sr.SetPropertyBlock(mpb);
        __UseUnshadowedMaskID = anyClear ? 1f : 0f;
    }

    void OnDrawGizmos()
    {
        if (!drawRays || _debugRays == null || _debugRays.Count == 0) return;

        // Draw buffered rays as Gizmos so they’re visible even if Debug.DrawLine is missed.
        for (int i = 0; i < _debugRays.Count; i++)
        {
            Gizmos.color = _debugRays[i].c;
            Gizmos.DrawLine(_debugRays[i].a, _debugRays[i].b);
        }
    }
}
