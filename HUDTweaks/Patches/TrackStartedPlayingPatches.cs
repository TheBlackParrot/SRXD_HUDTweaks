using HarmonyLib;

namespace HUDTweaks.Patches;

[HarmonyPatch]
internal static class TrackStartedPlayingPatches
{
    [HarmonyPatch(typeof(Track), nameof(Track.TrackStartedPlaying))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    internal static void TrackStartedPlayingPatch(Track __instance)
    {
        //metadataHandle.GetMaxPossibleScoreForDifficulty(trackData.Difficulty);
        MetadataHandle metadata = PlayState.Active.TrackDataSetup.TrackDataSegmentForSingleTrackDataSetup.metadata;
        DomeHudContainer.MaximumPossibleScore = metadata.GetMaxPossibleScoreForDifficulty(PlayState.Active.Handle.Data.Difficulty);
    }
}