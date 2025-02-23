using UnityEngine;
using WankulCrazyPlugin;

public class AnimationCopier : MonoBehaviour
{
    public static void CopyAnimation(GameObject source, GameObject target)
    {
        Animation sourceAnimation = source.GetComponent<Animation>();
        if (sourceAnimation == null)
        {
            Plugin.Logger.LogError("Source object does not have an Animation component.");
            return;
        }

        Animation targetAnimation = target.GetComponent<Animation>();
        if (targetAnimation == null)
        {
            targetAnimation = target.AddComponent<Animation>();
        }

        foreach (AnimationState animState in sourceAnimation)
        {
            targetAnimation.AddClip(animState.clip, animState.name);
        }
    }

    public static void CopyAnimation(GameObject source, GameObject target, string clipName)
    {
        Animation sourceAnimation = source.GetComponent<Animation>();
        if (sourceAnimation == null)
        {
            Plugin.Logger.LogError("Source object does not have an Animation component.");
            return;
        }

        AnimationClip clip = sourceAnimation.GetClip(clipName);
        if (clip == null)
        {
            Plugin.Logger.LogError($"Animation clip '{clipName}' not found in source object.");
            return;
        }

        Animation targetAnimation = target.GetComponent<Animation>();
        if (targetAnimation == null)
        {
            targetAnimation = target.AddComponent<Animation>();
        }

        if (targetAnimation.GetClip(clipName) == null)
        {
            targetAnimation.AddClip(clip, clipName);
            //Plugin.Logger.LogInfo($"Animation clip '{clipName}' copied from source object to target object.");
        }
    }
}
