Shader "Unlit/Checkerboard"
{
    Properties {
        _CheckerColor1 ("Checker Color 1", Color) = (0.5, 0.5, 0.5, 1)
        _CheckerColor2 ("Checker Color 2", Color) = (0.75, 0.75, 0.75, 1)
        _PixelPerUnit ("Pixels Per Unit", Float) = 100.0
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "SpriteMode"="Default" }
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2_f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 world_pos : TEXCOORD1;
            };
            
            float4 _CheckerColor1;
            float4 _CheckerColor2;
            float _PixelPerUnit;
            sampler2D _MainTex;

            v2_f vert (appdata_t input)
            {
                v2_f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color;
                output.world_pos = mul(unity_ObjectToWorld, input.vertex);
                return output;
            }
            
            fixed4 frag (v2_f input) : SV_Target
            {
                fixed4 tex_color = tex2D(_MainTex, input.texcoord);
                
                if (tex_color.a < 0.01)
                    clip(-1);
                
                float2 pixel_coord = input.world_pos.xy * _PixelPerUnit;
                float2 checker_uv = pixel_coord / 16;
                
                float2 grid = floor(checker_uv);
                grid += float2(0.5, 0.5);
                
                float pattern = fmod(abs(grid.x + grid.y), 2.0);
                
                fixed4 checker_color = lerp(_CheckerColor1, _CheckerColor2, pattern);
                checker_color.a *= tex_color.a * input.color.a;
                
                return checker_color;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}