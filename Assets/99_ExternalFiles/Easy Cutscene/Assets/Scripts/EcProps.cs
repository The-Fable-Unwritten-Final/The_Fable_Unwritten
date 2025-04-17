using UnityEngine;

namespace HisaGames.Props
{
    [System.Serializable]
    public class EcProps : MonoBehaviour
    {
        [Tooltip("Time speed for the prop to fade in and become visible.")]
        float fadeSpeed;

        /// <summary>
        /// Sets the visibility timer and whether the prop uses base animation.
        /// </summary>
        /// <param name="timer">Duration for which the prop remains visible.</param>
        public void SetVisibility(float fadeSpeed)
        {
            this.fadeSpeed = fadeSpeed;
        }

        /// <summary>
        /// Updates the prop's visibility and scale based on the timer and animation state.
        /// </summary>
        public void PropUpdate()
        {
            var step = fadeSpeed * Time.deltaTime;

            if (step != 0)
            {
                if (transform.localScale != Vector3.one)
                    transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.one, step);
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
    }
}