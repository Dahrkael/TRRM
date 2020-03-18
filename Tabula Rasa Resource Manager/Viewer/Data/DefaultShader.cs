using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TRRM.Viewer.Data
{
    class Shader
    {
        public const string Basic = @"
            struct Vertex_IN
            {
	            float3 pos : POSITION0;
	            float4 col : COLOR0;
            };

            struct Pixel_IN
            {
	            float4 pos : POSITION0;
	            float4 col : COLOR0;
            };

            uniform extern float4x4 gWorldViewProj;

            Pixel_IN ShadeVertex(Vertex_IN input)
            {
	            Pixel_IN output = (Pixel_IN)0;

	            output.pos = mul(float4(input.pos, 1.0f), gWorldViewProj);
	            output.col = input.col;

	            return output;
            }

            float4 ShadePixel(Pixel_IN input) : COLOR
            {
	            return input.col;
            }


            technique Main {
	            pass P0 {
		            VertexShader = compile vs_2_0 ShadeVertex();
		            PixelShader = compile ps_2_0 ShadePixel();
	            }
            }
        ";

        public const string Light = @"
            struct Vertex_IN
            {
	            float3 pos    : POSITION0;
                float3 normal : NORMAL0;
	            float4 col    : COLOR0;
            };

            struct Pixel_IN
            {
	            float4 pos : POSITION0;
	            float4 col : COLOR0;
            };

            uniform extern float4x4 gWorldViewProj;
            uniform extern float4x4 gWorldInvTrans;

            uniform extern float4 gAmbientLight;
            uniform extern float4 gDiffuseLight;
            uniform extern float3 gDiffuseVecW;

            Pixel_IN ShadeVertex(Vertex_IN input)
            {
	            Pixel_IN output = (Pixel_IN)0;

                float3 normalW = mul(float4(input.normal, 0.0f), gWorldInvTrans).xyz;
                normalW = normalize(normalW);

                float s = max(dot(gDiffuseVecW, normalW), 0.0f);
                float3 diffuse = s * (input.col * gDiffuseLight).rgb;

                float3 ambient = input.col * gAmbientLight;
                output.col.rgb = ambient + diffuse;
                output.col.a   = input.col.a;

                output.pos = mul(float4(input.pos, 1.0f), gWorldViewProj);

	            return output;
            }

            float4 ShadePixel(Pixel_IN input) : COLOR
            {
	            return input.col;
            }

            technique Main {
	            pass P0 {
		            VertexShader = compile vs_2_0 ShadeVertex();
		            PixelShader = compile ps_2_0 ShadePixel();
	            }
            }
        ";

        public const string Phong = @"
            struct Vertex_IN
            {
	            float3 pos    : POSITION0;
                float3 normal : NORMAL0;
	            float4 col    : COLOR0;
            };

            struct Vertex_OUT
            {
	            float4 pos     : POSITION0;
                float3 normalW : TEXCOORD0;
	            float4 col     : COLOR0;
            };

            struct Pixel_IN
            {
                float3 normalW : TEXCOORD0;
	            float4 col     : COLOR0;
            };

            uniform extern float4x4 gWorldViewProj;
            uniform extern float4x4 gWorldInvTrans;

            uniform extern float4 gAmbientLight;
            uniform extern float4 gDiffuseLight;
            uniform extern float3 gDiffuseVecW;

            Vertex_OUT ShadeVertex(Vertex_IN input)
            {
	            Vertex_OUT output = (Vertex_OUT)0;

                output.pos = mul(float4(input.pos, 1.0f), gWorldViewProj);
                output.normalW = normalize(mul(float4(input.normal, 0.0f), gWorldInvTrans).xyz);
                output.col = input.col;

	            return output;
            }

            float4 ShadePixel(Pixel_IN input) : COLOR
            {
                float3 normalW = normalize(input.normalW);
	            
                float s = max(dot(gDiffuseVecW, normalW), 0.0f);
                float3 diffuse = s * (input.col * gDiffuseLight).rgb;

                float3 ambient = input.col * gAmbientLight;

                return float4(ambient + diffuse , input.col.a);
            }

            technique Main {
	            pass P0 {
		            VertexShader = compile vs_2_0 ShadeVertex();
		            PixelShader = compile ps_2_0 ShadePixel();
	            }
            }
        ";

        public const string TexturePhong = @"
            uniform extern float4x4 gWorldViewProj;
            uniform extern float4x4 gWorldInvTrans;

            uniform extern float4 gAmbientLight;
            uniform extern float4 gDiffuseLight;
            uniform extern float3 gDiffuseVecW;

            uniform extern texture gDiffuseTexture;

            sampler TexS = sampler_state
            {
                  Texture = <gDiffuseTexture>;
                  MinFilter = LINEAR;
                  MagFilter = LINEAR;
                  MipFilter = LINEAR;
                  //MaxAnisotropy = 16;
            };

            struct Vertex_IN
            {
	            float3 pos     : POSITION0;
                float3 normal  : NORMAL0;
	            float4 col     : COLOR0;
                float2 texDiff : TEXCOORD0;
            };

            struct Vertex_OUT
            {
	            float4 pos     : POSITION0;
                float3 normalW : TEXCOORD0;                
	            float4 col     : COLOR0;
                float2 texDiff : TEXCOORD1;

            };

            struct Pixel_IN
            {
                float3 normalW : TEXCOORD0;
	            float4 col     : COLOR0;
                float2 texDiff : TEXCOORD1;
            };

            Vertex_OUT ShadeVertex(Vertex_IN input)
            {
	            Vertex_OUT output = (Vertex_OUT)0;

                output.pos = mul(float4(input.pos, 1.0f), gWorldViewProj);
                output.normalW = normalize(mul(float4(input.normal, 0.0f), gWorldInvTrans).xyz);
                output.col = input.col;
                output.texDiff = input.texDiff;

	            return output;
            }

            float4 ShadePixel(Pixel_IN input) : COLOR
            {
                float3 normalW = normalize(input.normalW);
	            
                float s = max(dot(gDiffuseVecW, normalW), 0.0f);
                float3 diffuse = s * (input.col * gDiffuseLight).rgb;

                float3 ambient = input.col * gAmbientLight;

                float3 texColor = tex2D(TexS, input.texDiff).rgb;

                float3 combined = (ambient + diffuse) * texColor;

                return float4(combined, input.col.a);
            }

            technique Main {
	            pass P0 {
		            VertexShader = compile vs_2_0 ShadeVertex();
		            PixelShader = compile ps_2_0 ShadePixel();
	            }
            }
        ";
    }
}
