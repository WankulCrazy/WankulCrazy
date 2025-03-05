using UnityEngine;
using System.Collections;

namespace WankulCrazyPlugin.patch
{
    public class AnimationOpeningDisplay : MonoBehaviour
    {
        private IEnumerator AnimateChild(Transform child)
        {
            // Début de l'animation
            Vector3 startPosition = new Vector3(-0.001f, 0.1107f - 0.08f, 0);
            Quaternion startRotation = Quaternion.Euler(90, 180, 0);

            // Fin de l'animation
            Vector3 endPosition = new Vector3(-0.001f, 0.195f - 0.08f, -0.084f);
            Quaternion endRotation = Quaternion.Euler(180, 180, 0);

            float duration = 1.5f; // Durée de l'animation en secondes
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                child.localPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                child.localRotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            child.localPosition = endPosition;
            child.localRotation = endRotation;
        }

        public void StartAnimation(Transform child)
        {
            StartCoroutine(AnimateChild(child));
        }
    }
}
