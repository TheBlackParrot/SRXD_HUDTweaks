using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HUDTweaks.Patches;

[HarmonyPatch]
internal static class DomeHudPatches
{
    private static Transform? _copiedScoreText;
    private static CustomTextMeshPro? _copiedScoreTextTMP;

    /*private static Transform? _copiedScoreLeftSide;
    private static Transform? _copiedScoreRightSide;

    private static TextNumber? _accLeft;
    private static TextNumber? _accRight;*/
    
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Init))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void InitPatch(DomeHud __instance)
    {
        /*
        if (_copiedScoreLeftSide == null)
        {
            Transform scoreTextOriginal = __instance.number.transform;
            
            _copiedScoreLeftSide = Object.Instantiate(scoreTextOriginal.gameObject, __instance.number.transform.parent).transform;
            _copiedScoreLeftSide.name = "AccValueLeft";
            _accLeft = _copiedScoreLeftSide.GetComponent<TextNumber>();
            
            _copiedScoreRightSide = Object.Instantiate(scoreTextOriginal.gameObject, __instance.number.transform.parent).transform;
            _copiedScoreRightSide.name = "AccValueRight";
            _accRight = _copiedScoreRightSide.GetComponent<TextNumber>();
        }
        */
        
        __instance.number.gameObject.SetActive(false);
        
        _copiedScoreText = __instance.number.gameObject.transform.parent.Find("AccPercentageText");

        if (_copiedScoreText == null)
        {
            Transform scoreTextOriginal = __instance.number.gameObject.transform.parent.parent.Find("ScoreText");
            
            _copiedScoreText = Object.Instantiate(scoreTextOriginal.gameObject, __instance.number.gameObject.transform.parent).transform;
            _copiedScoreText.name = "AccPercentageText";
            
            if (_copiedScoreText.TryGetComponent(out TranslatedTextMeshPro translatedTextMeshPro))
            {
                Object.DestroyImmediate(translatedTextMeshPro);
            }
            
            Transform percentageSymbol = Object.Instantiate(_copiedScoreText.gameObject, __instance.number.gameObject.transform.parent).transform;
            percentageSymbol.name = "AccPercentageSymbol";

            CustomTextMeshPro percentageSymbolTMP = percentageSymbol.GetComponent<CustomTextMeshPro>();
            percentageSymbolTMP.text = "%";
            percentageSymbolTMP.rectTransform.offsetMax = percentageSymbolTMP.rectTransform.offsetMax with { y = 57.5f };
            percentageSymbolTMP.enableAutoSizing = false;
            percentageSymbolTMP.fontSizeMax = 400;
            percentageSymbolTMP.fontSize = 400;
        }

        _copiedScoreTextTMP = _copiedScoreText.GetComponent<CustomTextMeshPro>();

        _copiedScoreTextTMP.rectTransform.offsetMax = _copiedScoreTextTMP.rectTransform.offsetMax with { y = 57.5f };
        _copiedScoreTextTMP.enableAutoSizing = false;
        _copiedScoreTextTMP.fontSizeMax = 600;
        _copiedScoreTextTMP.fontSize = 600;
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Update))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UpdatePatch(DomeHud __instance)
    {
        if (_copiedScoreTextTMP == null)
        {
            return;
        }

        ScoreState scoreState = PlayState.Active.scoreState;
        float accuracy = (scoreState.TotalScore / (float)((scoreState.CurrentTotals.baseScore + scoreState.CurrentTotals.baseScoreLost) * 4)) * 100;

        _copiedScoreTextTMP.text = $"{(float.IsNaN(accuracy) ? 100 : accuracy):0.00}";

        /*
        if (_accLeft == null || _accRight == null)
        {
            return;
        }
        
        ScoreState scoreState = PlayState.Active.scoreState;
        float accuracy = (scoreState.TotalScore / (float)((scoreState.CurrentTotals.baseScore + scoreState.CurrentTotals.baseScoreLost) * 4)) * 100;
        
        _accLeft.desiredNumber = (int)accuracy;
        _accRight.desiredNumber = (int)((accuracy % 1f) * 100f);
        */
    }
}