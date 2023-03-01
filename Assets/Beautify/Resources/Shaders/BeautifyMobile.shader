Shader "Hidden/Kronnect/Beautify/BeautifyMobile" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_OverlayTex ("Overlay (RGB)", 2D) = "black" {}
		_DoFTex("DoF (RGBA)", any) = "black" {}
		_FXData("FX Additional Data", Vector) = (0.5, 0.5, 0)
		_Sharpen ("Sharpen Data", Vector) = (2.5, 0.035, 0.5)
		_ColorBoost ("Color Boost Data", Vector) = (1.1, 1.1, 0.08, 0)
		_Dither ("Dither Data", Vector) = (5, 0, 0, 1.0)
		_FXColor ("FXColor Color", Color) = (1,1,1,0)
		_TintColor ("Tint Color Color", Color) = (1,1,1,0)
		_Vignetting ("Vignetting", Color) = (0.3,0.3,0.3,0.05)
		_VignettingAspectRatio ("Vignetting Aspect Ratio", Float) = 1.0
		_VignettingMask ("Mask Texture (A)", 2D) = "white" {}
		_Frame("Frame Data", Vector) = (50,50,50,0)
		_FrameMask ("Mask Texture (A)", 2D) = "white" {}
        _FrameData("Frame Data 2", Vector) = (1,1,1,1)
		_Outline("Outline", Color) = (0,0,0,0.8)
		_Dirt("Dirt Data", Vector) = (0.5,0.5,0.5,0.5)
		_Bloom("Bloom Data", Vector) = (0.5,0,0)
		//_BloomTex("BloomTex (RGBA)", any) = "black" {}
		_BloomTex("BloomTex (RGBA)", any) = "black" {}
		_BloomWeights("Bloom Weights", Vector) = (0.35,0.55,0.7,0.8)
		_BloomWeights2("Bloom Weights 2", Vector) = (0.35,0.55,0.7,0.8)
		_BloomTint ("Bloom Tint", Color) = (1,1,1,0)
		//_ScreenLum("Luminance Tex (RGBA)", 2D) = "black" {}
		_ScreenLum("Luminance Tex (RGBA)", any) = "black" {}
		_CompareParams("Compare Params", Vector) = (0.785398175, 0.001, 0, 0)
		_AFTint ("Anamorphic Flares Tint", Color) = (1,1,1,0.5)
		_BokehData("Bokeh Data", Vector) = (10,1,0,1)		
		_DepthTexture("Depth (RGBA)", 2D) = "black" {}
		_DofExclusionTexture("DoF Exclusion (R)", 2D) = "white" {}
		_BokehData2("Bokeh Data 2", Vector) = (1000.0, 4, 0, 0)
        _BokehData3("Bokeh Data 3", Vector) = (1000.0, 100000.0, 0)
		_EyeAdaptation("Eye Adaptation Data", Vector) = (0.1, 2, 0.7, 1)
		_Purkinje ("Purkinje Data", Vector) = (1.0, 0.15, 0)
		_BloomDepthThreshold("Bloom Depth Threshold", Float) = 0.0
        _BloomNearThreshold("Bloom Near Threshold", Float) = 0.0
		_SunPos("SunFlares Sun Screen Position", Vector) = (0.5, 0.5, 0, 0)
        _SunPosRightEye("SF Screen Position Right Eye", Vector) = (0.5, 0.5, 0, 0)		
		_SunData("SunFlares Sun Data", Vector) = (0.1, 0.05, 3.5, 0.13)
		_SunCoronaRays1("SunFlares Corona Rays 1 Data", Vector) = (0.02, 12, 0.001, 0)
		_SunCoronaRays2("SunFlares Corona Rays 2 Data", Vector) = (0.05, 12, 0.1, 0)
		_SunGhosts1("SunFlares Ghosts 1 Data", Vector) = (0, 0.03, 0.6, 0.06)
		_SunGhosts2("SunFlares Ghosts 2 Data", Vector) = (0, 0.1, 0.2, 0.03)
		_SunHalo("SunFlares Halo Data", Vector) = (0.22, 15.1415, 1.0)
		_SunTint("Sun Flare Tint Color", Color) = (1,1,1)
//		_CompareTex ("Compare Image (RGB)", 2D) = "black" {}
		_CompareTex ("Compare Image (RGB)", any) = "black" {}
		_BlurScale ("Blur Scale", Float) = 1.0
		_LUTTex ("Lut Texture (RGB)", 2D) = "white" {}
        _LUT3DTex ("Lut 3D Texture (RGB)", 3D) = "" {}
        _LUT3DParams("Lut 3D Params", Vector) = (1,1,0,0)
        _HardLight("HardLight Data", Vector) = (0,0,0)
        _ChromaticAberrationData("ChromaticAberrationData", Vector) = (0,0,0)
	}

Subshader {	

  ZTest Always Cull Off ZWrite Off
  Fog { Mode Off }

  Pass { // 0
      CGPROGRAM
      #pragma vertex vertCompare
      #pragma fragment fragCompareFast
      #pragma target 3.0
	  #pragma fragmentoption ARB_precision_hint_fastest      
      #include "BeautifyMobile.cginc"
      ENDCG
  }
 
  Pass { // 1
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBeautifyFast
      #pragma target 3.0
	  #pragma fragmentoption ARB_precision_hint_fastest      
// Edited by Shader Control: #pragma multi_compile_local __ BEAUTIFY_DALTONIZE BEAUTIFY_LUT BEAUTIFY_LUT3D BEAUTIFY_NIGHT_VISION BEAUTIFY_THERMAL_VISION
#pragma multi_compile_local __ BEAUTIFY_DALTONIZE 
// Edited by Shader Control: #pragma multi_compile_local __ BEAUTIFY_DEPTH_OF_FIELD BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
#pragma multi_compile_local __ BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
#pragma multi_compile_local __ BEAUTIFY_OUTLINE
// Disabled by Shader Control: #pragma multi_compile_local __ BEAUTIFY_DIRT
#pragma multi_compile_local __ BEAUTIFY_BLOOM
// Disabled by Shader Control: #pragma multi_compile_local __ BEAUTIFY_EYE_ADAPTATION
// Disabled by Shader Control: #pragma multi_compile_local __ BEAUTIFY_TONEMAP_ACES
// Disabled by Shader Control: #pragma multi_compile_local __ BEAUTIFY_PURKINJE
// Edited by Shader Control: #pragma multi_compile_local __ BEAUTIFY_VIGNETTING BEAUTIFY_VIGNETTING_MASK
#pragma multi_compile_local __ BEAUTIFY_VIGNETTING 
// Disabled by Shader Control: #pragma multi_compile_local __ BEAUTIFY_FRAME BEAUTIFY_FRAME_MASK
// Disabled by Shader Control: #pragma multi_compile_local __ BEAUTIFY_CHROMATIC_ABERRATION
      #include "BeautifyMobile.cginc"
      ENDCG
  }

  Pass { // 2
      CGPROGRAM
      #pragma vertex vertLum
      #pragma fragment fragLum
      #pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_local __ BEAUTIFY_BLOOM_USE_DEPTH
#pragma multi_compile_local __ BEAUTIFY_BLOOM_USE_LAYER
      #include "BeautifyLum.cginc"
      ENDCG
  }    
  
  Pass { // 3
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragDebugBloom
      #pragma fragmentoption ARB_precision_hint_fastest
      #include "BeautifyLum.cginc"
      ENDCG
  }    
      
  
  Pass { // 4
      CGPROGRAM
      #pragma vertex vertBlurH
      #pragma fragment fragBlur
      #pragma target 3.0
      #pragma fragmentoption ARB_precision_hint_fastest
      #include "BeautifyLum.cginc"
      ENDCG
  }    
  
      
  Pass { // 5
      CGPROGRAM
      #pragma vertex vertBlurV
      #pragma fragment fragBlur
      #pragma target 3.0
      #pragma fragmentoption ARB_precision_hint_fastest
      #include "BeautifyLum.cginc"
      ENDCG
  }    
  

  Pass { // 6 DoF CoC
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragCoC
	  #pragma target 3.0
      #pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_local __ BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
      #include "BeautifyDoF.cginc"
      ENDCG
  } 
 
  Pass { // 7 DoF CoC Debug
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragCoCDebug
      #pragma target 3.0
      #pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_local __ BEAUTIFY_DEPTH_OF_FIELD_TRANSPARENT
      #include "BeautifyDoF.cginc"
      ENDCG
  } 
 
  Pass { // 8 DoF Blur
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBlur
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyDoF.cginc"
      ENDCG
  }    
  
 Pass { // 9 Compute Screen Lum
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragScreenLum
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyEA.cginc"
      ENDCG
  }      
  
  Pass { // 10 Reduce Screen Lum
      CGPROGRAM
      #pragma vertex vertCross
      #pragma fragment fragReduceScreenLum
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyEA.cginc"
      ENDCG
  }  
  
  Pass { // 11 Blend Screen Lum
	  Blend SrcAlpha OneMinusSrcAlpha
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBlendScreenLum
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyEA.cginc"
      ENDCG
  }    

    
  Pass { // 12 Simple copy
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBlend
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyEA.cginc"
      ENDCG
  }  
          
  Pass { // 13 Bloom premul compose
	  Blend One One
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragCopy
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #define APPLY_TINT_COLOR
      #include "BeautifyLum.cginc"
      ENDCG
  } 

  Pass { // 14 Anamorphic Resample Fast
	  Blend One One
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragResampleFastAF
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyLum.cginc"
      ENDCG
  } 
 
  Pass { // 15 DoF Blur wo/Bokeh
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragBlurNoBokeh
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyDoF.cginc"
      ENDCG
  }    
  
  Pass { // 16 Sun Flares
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragSFFast
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifySF.cginc"
      ENDCG
  }
  
    Pass { // 17 Sun Flares Additive
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragSFFastAdditive
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifySF.cginc"
      ENDCG
  }
  
  Pass { // 18  Raw copy used in single blits with Single Pass Instanced
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment fragCopy
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #include "BeautifyLum.cginc"
      ENDCG
  }
    
  Pass { // 19 Chromatic Aberration Custom Pass
      CGPROGRAM
      #pragma vertex vertSimple
      #pragma fragment fragChromaticAberrationFast
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma target 3.0
      #define BEAUTIFY_CHROMATIC_ABERRATION 1
      #include "BeautifyCAberration.cginc"
      ENDCG
  }  

}
FallBack Off
}
