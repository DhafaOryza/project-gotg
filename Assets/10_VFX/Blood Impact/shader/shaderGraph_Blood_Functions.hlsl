void WaterDrop_float(
    float4 UV,
    float Time,
    float4 DeepColor,
    float4 EdgeColor,
    float TrailStrength,
    float TrailLength,
    float RippleSpeed,
    float Opacity,
    float TextureAlpha,
    out float3 OutColor,
    out float OutAlpha)
{
    float2 p = UV.xy - 0.5;

    // Badan tetesan darah mengikuti silhouette texture waterdrop.
    // Trail tetap dibuat prosedural supaya tidak terpotong oleh texture.
    float r = length(p);
    float body = saturate(TextureAlpha);

    // Ekor trail mengarah ke -V saat TrailLength positif.
    // Makin besar |TrailLength|, makin jauh ekor dari badan.
    float signY = TrailLength < 0.0 ? 1.0 : -1.0;
    float trailOffset = max(0.05, abs(TrailLength));
    float2 tp = p - float2(0.0, signY * trailOffset);
    tp.x *= 2.3;
    tp.y *= 0.8;

    float trail = 1.0 - smoothstep(0.07, 0.20, length(tp));
    trail *= saturate(0.35 + 0.85 * TrailStrength);

    float mask = saturate(max(body, trail));
    float trailOnly = saturate(trail * (1.0 - body));

    float ripple = sin(r * 45.0 - Time * RippleSpeed) * 0.5 + 0.5;
    float edge = smoothstep(0.05, 0.25, r);
    float3 color = lerp(DeepColor.rgb, EdgeColor.rgb, saturate(edge * 0.75 + ripple * 0.18));

    // Kilau basah bernuansa merah, bukan putih kebiruan.
    float highlight = 1.0 - smoothstep(0.0, 0.11, length(p - float2(-0.06, 0.07)));
    color += float3(1.0, 0.18, 0.12) * highlight * 0.18;

    // Ekor dibuat lebih terang dan tetap merah darah.
    color = lerp(color, EdgeColor.rgb, trailOnly * 0.85);
    color += trailOnly * float3(0.20, 0.015, 0.01);

    OutColor = saturate(color);
    OutAlpha = mask * Opacity;
}
