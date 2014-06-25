﻿Shader "Custom/Toon_nooutline" {
		Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Ramp ("Ramp Texture", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
 
        Pass {
 
            Cull Back 
            Lighting On
            Tags { "LightMode"="Vertex" }
 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
 
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            sampler2D _Ramp; 
 
            struct a2v
            {
                fixed4 vertex : POSITION;
                fixed3 normal : NORMAL;
                fixed4 texcoord : TEXCOORD0;
 
            }; 
 
            struct v2f
            {
                fixed4 pos : POSITION;
                fixed2 uv : TEXCOORD0;
                fixed3 normal : TEXCOORD1;
 
            };
 
            v2f vert (a2v v)
            {
                v2f o;
                //Transform the vertex to projection space
                o.pos = mul( UNITY_MATRIX_MVP, v.vertex); 
                //Get the UV coordinates
                o.uv = v.texcoord;  
                o.normal = v.normal;
                return o;
            }
 
            fixed4 frag(v2f i) : COLOR  
            { 
                //Get the color of the pixel from the texture
                fixed4 c = tex2D (_MainTex, i.uv);                  
 
                //Get the normal from the bump map
                fixed3 n =  i.normal; 
 
                //Angle to the light
                fixed diff = saturate (dot (n, fixed3(1.0, 1.0, -1.0)));  
                //Perform our toon light mapping 
                diff = tex2D(_Ramp, fixed2(diff, 0.5));
                //Update the colour
                fixed3 lightColor = fixed3(0.5, 0.5, 0.5) + fixed3(0.25, 0.25, 0.25) * (diff); 
                //Product the final color
                c.rgb = lightColor * c.rgb * 3;
                return c; 
 
            } 
 
            ENDCG
        }
 
    }
    FallBack "Diffuse"
}
