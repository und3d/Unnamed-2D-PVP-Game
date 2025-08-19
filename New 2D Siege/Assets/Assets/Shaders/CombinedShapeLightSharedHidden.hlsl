#if !defined(COMBINED_SHAPE_LIGHT_PASS)
#define COMBINED_SHAPE_LIGHT_PASS

// These are provided by URP 2D / Sprite-Lit pipeline:
half _HDREmulationScale;
half _UseSceneLighting;
half4 _RendererColor;

// NEW uniform coming from the material (Property in the .shader above)
half _UseUnshadowedMask;

inline half Luma(half3 c) { return dot(c, half3(0.30h, 0.59h, 0.11h)); }

half4 CombinedShapeLightShared(half4 color, half4 mask, half2 lightingUV)
{
    if (color.a == 0.0h) discard;

    // Keep your original sprite tint behavior
    color *= _RendererColor;
    color.rgb = max(color.rgb, half3(0.05h, 0.05h, 0.05h));

    // ---------------- Style 0 (shadowed cone) ----------------
    #if USE_SHAPE_LIGHT_TYPE_0
        half4 shapeLight0 = SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, lightingUV);
        if (any(_ShapeLightMaskFilter0))
        {
            half4 processedMask0 = (1 - _ShapeLightInvertedFilter0) * mask + _ShapeLightInvertedFilter0 * (1 - mask);
            shapeLight0 *= dot(processedMask0, _ShapeLightMaskFilter0);
        }
        half3 s0m = shapeLight0.rgb * _ShapeLightBlendFactors0.x;
        half3 s0a = shapeLight0.rgb * _ShapeLightBlendFactors0.y;
    #else
        half3 s0m = 0, s0a = 0;
    #endif

    // ---------------- Style 1 (unshadowed cone) ----------------
    #if USE_SHAPE_LIGHT_TYPE_1
        half4 shapeLight1 = SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, lightingUV);
        if (any(_ShapeLightMaskFilter1))
        {
            half4 processedMask1 = (1 - _ShapeLightInvertedFilter1) * mask + _ShapeLightInvertedFilter1 * (1 - mask);
            shapeLight1 *= dot(processedMask1, _ShapeLightMaskFilter1);
        }
        half3 s1m = shapeLight1.rgb * _ShapeLightBlendFactors1.x;
        half3 s1a = shapeLight1.rgb * _ShapeLightBlendFactors1.y;
    #else
        half3 s1m = 0, s1a = 0;
    #endif

    // ------- Visibility gates: ONLY Style0 and Style1 control visibility -------
    half3 visShadowedRGB   = s0m + s0a; // light reaching this pixel when shadows apply
    half3 visUnshadowedRGB = s1m + s1a; // pure cone mask without shadows

    half lumShadowed = Luma(visShadowedRGB);
    half lumNoShadow = Luma(visUnshadowedRGB);

    // Choose which to gate by (0 = shadowed cone, 1 = unshadowed cone)
    half gateLum = lerp(lumShadowed, lumNoShadow, _UseUnshadowedMask);

    // Hide when not visible by the chosen mask
    if (gateLum < 0.01h)
        discard;

    // ---------------- Final color ----------------
    // If using unshadowed mask: show the sprite color (ignore cross-shadows)
    // If using shadowed mask:   shade only by Style0 so other lights can't reveal it
    half3 rgbShadowedOnly = _HDREmulationScale * (color.rgb * s0m + s0a);
    half3 outRGB          = lerp(rgbShadowedOnly, color.rgb, _UseUnshadowedMask);

    // Alpha respects chosen visibility and sprite alpha
    half4 finalOutput;
    finalOutput.rgb = outRGB;
    finalOutput.a   = min(gateLum * 2.0h, color.a);

    // Keep your original "Use Scene Lighting" blend
    finalOutput = finalOutput * _UseSceneLighting + (1 - _UseSceneLighting) * color;

    return finalOutput;
}
#endif
