Shader "Custom/UIGrayscaleBlend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Blend ("Grayscale Blend", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        Cull Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 버텍스 컬러 입력을 위한 appdata_t 수정 (틴트/알파 전달)
            struct appdata_t { 
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
                float4 color : COLOR; 
            };
            
            // 버텍스 컬러 출력을 위한 v2f 수정 (틴트/알파 전달)
            struct v2f { 
                float2 uv : TEXCOORD0; 
                float4 vertex : SV_POSITION; 
                float4 color : COLOR; 
            };

            sampler2D _MainTex;
            float _Blend;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // 버텍스 컬러 (Image.color) 전달
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // ⭐ 1. Image.color의 RGB 틴트 값을 텍스처에 적용합니다.
                col.rgb *= i.color.rgb; 
                
                // 2. 그레이스케일 값 계산
                float gray = dot(col.rgb, float3(0.299,0.587,0.114));
                
                // 3. RGB에 그레이스케일 블렌딩 적용
                col.rgb = lerp(col.rgb, float3(gray,gray,gray), _Blend);
                
                // 4. Image.color의 알파 값을 최종 알파에 곱하여 반투명값을 적용합니다.
                col.a *= i.color.a; 
                
                return col;
            }
            ENDCG
        }
    }
}