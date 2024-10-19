using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Yu5h1Lib.MoreExtension
{
	public static class BlendColor
	{
		//public static Color Burn(this Color from, Color to, float Opacity)
		//{
		//	float4 f = new float4() ;
		//          Color.Lerp(from, Color.white - (Color.white - to) / from, Opacity);
		//      }

		//    Darken

		//void Unity_Blend_Darken_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = min(Blend, Base);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Difference

		//void Unity_Blend_Difference_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = abs(Blend - Base);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Dodge

		//void Unity_Blend_Dodge_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Base / (1.0 - Blend);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Divide

		//void Unity_Blend_Divide_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Base / (Blend + 0.000000000001);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Exclusion

		//void Unity_Blend_Exclusion_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Blend + Base - (2.0 * Blend * Base);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    HardLight

		//void Unity_Blend_HardLight_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
		//        float4 result2 = 2.0 * Base * Blend;
		//        float4 zeroOrOne = step(Blend, 0.5);
		//        Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    HardMix

		//void Unity_Blend_HardMix_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = step(1 - Base, Blend);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Lighten

		//void Unity_Blend_Lighten_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = max(Blend, Base);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    LinearBurn

		//void Unity_Blend_LinearBurn_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Base + Blend - 1.0;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    LinearDodge

		//void Unity_Blend_LinearDodge_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Base + Blend;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    LinearLight

		//void Unity_Blend_LinearLight_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Blend < 0.5 ? max(Base + (2 * Blend) - 1, 0) : min(Base + 2 * (Blend - 0.5), 1);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    LinearLightAddSub

		//void Unity_Blend_LinearLightAddSub_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Blend + 2.0 * Base - 1.0;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Multiply

		public static Color Multiply(this Color target, Color other, float Opacity)
			=> Color.Lerp(target, target * other, Opacity);

		//Negation
		//void Unity_Blend_Negation_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = 1.0 - abs(1.0 - Blend - Base);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Overlay

		//void Unity_Blend_Overlay_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        float4 result1 = 1.0 - 2.0 * (1.0 - Base) * (1.0 - Blend);
		//        float4 result2 = 2.0 * Base * Blend;
		//        float4 zeroOrOne = step(Base, 0.5);
		//        Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    PinLight

		//void Unity_Blend_PinLight_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        float4 check = step(0.5, Blend);
		//        float4 result1 = check * max(2.0 * (Base - 0.5), Blend);
		//        Out = result1 + (1.0 - check) * min(2.0 * Base, Blend);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Screen

		//void Unity_Blend_Screen_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = 1.0 - (1.0 - Blend) * (1.0 - Base);
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    SoftLight

		//void Unity_Blend_SoftLight_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        float4 result1 = 2.0 * Base * Blend + Base * Base * (1.0 - 2.0 * Blend);
		//        float4 result2 = sqrt(Base) * (2.0 * Blend - 1.0) + 2.0 * Base * (1.0 - Blend);
		//        float4 zeroOrOne = step(0.5, Blend);
		//        Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Subtract

		//void Unity_Blend_Subtract_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = Base - Blend;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    VividLight

		//void Unity_Blend_VividLight_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        float4 result1 = 1.0 - (1.0 - Blend) / (2.0 * Base);
		//        float4 result2 = Blend / (2.0 * (1.0 - Base));
		//        float4 zeroOrOne = step(0.5, Base);
		//        Out = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
		//        Out = lerp(Base, Out, Opacity);
		//    }
		//    Overwrite

		//void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, float Opacity, out float4 Out)
		//    {
		//        Out = lerp(Base, Blend, Opacity);
		//    }
	} 
}
