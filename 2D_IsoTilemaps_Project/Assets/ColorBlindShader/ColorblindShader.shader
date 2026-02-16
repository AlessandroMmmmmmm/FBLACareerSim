Shader "Custom/ColorblindShaderWithPatterns"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorblindType ("Colorblind Type", Int) = 0
        _PatternIntensity ("Pattern Intensity", Range(0, 1)) = 0.3
        _PatternScale ("Pattern Scale", Float) = 20.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            int _ColorblindType;
            float _PatternIntensity;
            float _PatternScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            // Pattern generators
            float DiagonalStripes(float2 uv, float scale)
            {
                float pattern = frac((uv.x + uv.y) * scale);
                return step(0.5, pattern);
            }

            float Dots(float2 uv, float scale)
            {
                float2 gridUV = frac(uv * scale);
                float2 center = float2(0.5, 0.5);
                float dist = distance(gridUV, center);
                return step(dist, 0.3);
            }

            float Checkerboard(float2 uv, float scale)
            {
                float2 c = floor(uv * scale);
                return fmod(c.x + c.y, 2.0);
            }

            float HorizontalStripes(float2 uv, float scale)
            {
                float pattern = frac(uv.y * scale);
                return step(0.5, pattern);
            }

            // Detect if a color is in a problematic range
            bool IsReddish(float3 color)
            {
                // Detect red colors (red > green AND red > blue by significant margin)
                return color.r > 0.3 && color.r > color.g * 1.3 && color.r > color.b * 1.3;
            }

            bool IsGreenish(float3 color)
            {
                // Detect green colors
                return color.g > 0.3 && color.g > color.r * 1.3 && color.g > color.b * 1.3;
            }

            bool IsBluish(float3 color)
            {
                // Detect blue colors
                return color.b > 0.3 && color.b > color.r * 1.3 && color.b > color.g * 1.3;
            }

            bool IsYellowish(float3 color)
            {
                // Detect yellow (high red and green, low blue)
                return color.r > 0.4 && color.g > 0.4 && color.b < 0.3 && abs(color.r - color.g) < 0.2;
            }

            bool IsOrangish(float3 color)
            {
                // Detect orange (red > green, both high, low blue)
                return color.r > 0.4 && color.g > 0.2 && color.g < color.r && color.b < 0.3;
            }

            // Apply pattern overlay based on original color
            float3 ApplyPatternOverlay(float3 originalColor, float3 transformedColor, float2 screenUV, int cbType)
            {
                float pattern = 0;
                bool shouldApplyPattern = false;

                // For Protanopia and Protanomaly (red-blind/weak)
                if (cbType == 1 || cbType == 4)
                {
                    if (IsReddish(originalColor))
                    {
                        pattern = DiagonalStripes(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                    else if (IsOrangish(originalColor))
                    {
                        pattern = Dots(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                }
                // For Deuteranopia and Deuteranomaly (green-blind/weak)
                else if (cbType == 2 || cbType == 5)
                {
                    if (IsGreenish(originalColor))
                    {
                        pattern = HorizontalStripes(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                    else if (IsYellowish(originalColor))
                    {
                        pattern = Checkerboard(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                }
                // For Tritanopia and Tritanomaly (blue-blind/weak)
                else if (cbType == 3 || cbType == 6)
                {
                    if (IsBluish(originalColor))
                    {
                        pattern = Dots(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                    else if (IsYellowish(originalColor))
                    {
                        pattern = DiagonalStripes(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                }
                // For Achromatopsia and Achromatomaly (total/partial color blindness)
                else if (cbType == 7 || cbType == 8)
                {
                    // Apply different patterns to different color ranges
                    if (IsReddish(originalColor))
                    {
                        pattern = DiagonalStripes(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                    else if (IsGreenish(originalColor))
                    {
                        pattern = HorizontalStripes(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                    else if (IsBluish(originalColor))
                    {
                        pattern = Dots(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                    else if (IsYellowish(originalColor))
                    {
                        pattern = Checkerboard(screenUV, _PatternScale);
                        shouldApplyPattern = true;
                    }
                }

                if (shouldApplyPattern)
                {
                    // Blend the pattern with the transformed color
                    float patternValue = pattern * _PatternIntensity;
                    return lerp(transformedColor, transformedColor * 0.7, patternValue);
                }

                return transformedColor;
            }

            float3 ApplyColorblindness(float3 color, int type)
            {
                float3 result = color;
                
                if (type == 1) // Protanopia
                {
                    result.r = 0.567 * color.r + 0.433 * color.g;
                    result.g = 0.558 * color.r + 0.442 * color.g;
                    result.b = 0.242 * color.g + 0.758 * color.b;
                }
                else if (type == 2) // Deuteranopia
                {
                    result.r = 0.625 * color.r + 0.375 * color.g;
                    result.g = 0.700 * color.r + 0.300 * color.g;
                    result.b = 0.300 * color.g + 0.700 * color.b;
                }
                else if (type == 3) // Tritanopia
                {
                    result.r = 0.950 * color.r + 0.050 * color.g;
                    result.g = 0.433 * color.g + 0.567 * color.b;
                    result.b = 0.475 * color.g + 0.525 * color.b;
                }
                else if (type == 4) // Protanomaly
                {
                    result.r = 0.817 * color.r + 0.183 * color.g;
                    result.g = 0.333 * color.r + 0.667 * color.g;
                    result.b = 0.125 * color.g + 0.875 * color.b;
                }
                else if (type == 5) // Deuteranomaly
                {
                    result.r = 0.800 * color.r + 0.200 * color.g;
                    result.g = 0.258 * color.r + 0.742 * color.g;
                    result.b = 0.142 * color.g + 0.858 * color.b;
                }
                else if (type == 6) // Tritanomaly
                {
                    result.r = 0.967 * color.r + 0.033 * color.g;
                    result.g = 0.733 * color.g + 0.267 * color.b;
                    result.b = 0.183 * color.g + 0.817 * color.b;
                }
                else if (type == 7) // Achromatopsia (monochrome)
                {
                    float gray = dot(color, float3(0.299, 0.587, 0.114));
                    result = float3(gray, gray, gray);
                }
                else if (type == 8) // Achromatomaly
                {
                    float gray = dot(color, float3(0.299, 0.587, 0.114));
                    result = lerp(float3(gray, gray, gray), color, 0.3);
                }
                
                return result;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 originalColor = col.rgb;
                
                // Apply colorblind transformation
                float3 transformedColor = ApplyColorblindness(originalColor, _ColorblindType);
                
                // Calculate screen-space UV for patterns
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                screenUV *= _ScreenParams.xy / _ScreenParams.y; // Maintain aspect ratio
                
                // Apply pattern overlay if needed
                col.rgb = ApplyPatternOverlay(originalColor, transformedColor, screenUV, _ColorblindType);
                
                return col;
            }
            ENDCG
        }
    }
}
