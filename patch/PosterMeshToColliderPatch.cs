using HarmonyLib;
using UnityEngine;
using System;

namespace WankulCrazyPlugin.patch
{
    public class PosterPosterMeshToColliderPatch
    {
        public static void Postfix(InteractableObject __instance)
        {
            // Vérifie qu'on traite bien un InteractableDecoration de type Poster
            if (__instance is not InteractableDecoration deco || !PosterPlacementFix.IsPoster(deco.m_DecoObjectType))
                return;

            // Récupère le MeshFilter
            MeshFilter meshFilter = deco.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Plugin.Logger.LogWarning("Aucun MeshFilter trouvé sur l’objet poster.");
                return;
            }

            Mesh mesh = meshFilter.sharedMesh;
            Vector3 meshSize = mesh.bounds.size;
            Vector3 meshCenter = mesh.bounds.center;
            Vector3 lossyScale = meshFilter.transform.lossyScale;

            // Applique l’échelle réelle
            Vector3 realSize = Vector3.Scale(meshSize, lossyScale);
            Vector3 realCenter = Vector3.Scale(meshCenter, lossyScale);

            Transform moveArea = deco.transform.Find("MoveStateValidAreaGrp/MoveStateValidArea");
            if (moveArea == null)
            {
                Plugin.Logger.LogWarning("MoveStateValidArea introuvable.");
                return;
            }

            // Mets à jour ou ajoute un BoxCollider à MoveStateValidArea
            BoxCollider box = moveArea.GetComponent<BoxCollider>();
            if (box == null)
                box = moveArea.gameObject.AddComponent<BoxCollider>();

            box.size = realSize;
            box.center = realCenter;

            Plugin.Logger.LogDebug($"Zone valide ajustée pour {deco.name} avec mesh: {realSize}");
        }

        public static class PosterPlacementFix
        {
            public static bool IsPoster(EDecoObject decoObject)
            {
                string name = Enum.GetName(typeof(EDecoObject), decoObject);
                return name != null && name.StartsWith("Poster", StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public class PosterDashedLineHiderPatch
    {
        public static void Postfix(InteractableObject __instance)
        {
            if (__instance is not InteractableDecoration deco || !PosterPosterMeshToColliderPatch.PosterPlacementFix.IsPoster(deco.m_DecoObjectType))
                return;

            Transform dashedLineGrp = deco.transform.Find("MoveStateValidAreaGrp/DashedLineGrp");
            if (dashedLineGrp != null && dashedLineGrp.gameObject.activeSelf)
            {
                dashedLineGrp.gameObject.SetActive(false);
            }
        }
    }
}
