Shader "Unlit/TransparentColor"
{
	Properties
	{
		_Color("Color", color) = (1, 1, 1, .5)
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Int) = 2
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull[_Cull]//Back
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4 _Color;

			struct input
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(input v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return _Color;
			}
			ENDCG
		}
	}
}
