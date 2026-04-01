using HarmonyLib;

namespace HUDTweaks.Patches;

[HarmonyPatch]
public static class TrackStartedPlayingPatch
{
    [HarmonyPatch(typeof(Track), nameof(Track.TrackStartedPlaying))]
    [HarmonyPostfix]
    public static void TrackStartedPlaying_Patch()
    {
        PlayableTrackData trackData = Track.PlayHandle.Data;
        MetadataHandle metadataHandle = trackData.Setup.TrackDataSegmentForSingleTrackDataSetup.metadata;
        
        DomeHudContainer.MaximumPossibleScore = metadataHandle.GetMaxPossibleScoreForDifficulty(trackData.Difficulty);
    }
}