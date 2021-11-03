using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class PlayableAssetPRM : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<MeshTimelinePRM> meshTimelinePRM;
    public PlayableBehaviourPRM promePlayableBehaviour;
    public ClipCaps clipCaps => ClipCaps.None;
    // Factory method that generates a playable based on this asset
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        promePlayableBehaviour.meshTimelinePRM = meshTimelinePRM.Resolve(graph.GetResolver());
        var playable = ScriptPlayable<PlayableBehaviourPRM>.Create(graph, promePlayableBehaviour);
        return playable;
    }
}
