Shader "Custom/PiksolVertexColor"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        sampler2D _MainTex;
		float _Alpha;

        struct Input
        {
			float4 color : COLOR;
			float2 metal;
			float4 emission;
		};

		void vert(inout appdata_full v, out Input o)
		{
			o.color = v.color;
			o.metal = v.texcoord;
			o.emission = float4(v.texcoord1.x, v.texcoord1.y, v.texcoord2.x, v.texcoord2.y);
		}

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = IN.color;
            o.Metallic = IN.metal.x;
            o.Smoothness = IN.metal.y;
            o.Alpha = IN.color.a * _Alpha;
			o.Emission = IN.emission.rgb;// *IN.emission.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
