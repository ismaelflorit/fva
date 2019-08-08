/*
 * Ismael Florit - August 2019
 * Simple volumetric audio solution.
 * 
 * Taking advantage of Collider.ClosestPoint(), the volume of a 2D audio source is adjusted
 * in an either Inverse Square fashion or adopting a logarithmic model. 
 */

using UnityEngine;

namespace FVA
{
    public enum CurveModel
    {
        Inverse_Square,
        Logarithmic
    }

    public class FreeVolumetricAudio : MonoBehaviour
    {
        public GameObject listenerTarget;
        [Tooltip("A reference to the audio file which you want to be volumetric.")] public AudioSource audioSource;
        private Collider thisCollider;
        public bool stereoMode;
        [Tooltip("How strong the audio source is. (-1 to 1) ")] [Range(-1f, 1f)] public float audioSourceStrength = 0;
        public CurveModel curveModel;

        private void Start()
        {
            thisCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (stereoMode)
            {
                StereoPanning();
            }
            PickCurveModel(curveModel);
        }

        private void ClampAudioStrength() {
            audioSourceStrength = Mathf.Clamp(audioSourceStrength, -1, 1);
        }

        private void PickCurveModel(CurveModel model)
        {
            switch (model)
            {
                case CurveModel.Inverse_Square:
                    MapDistanceInverseSquareLaw();
                    break;
                case CurveModel.Logarithmic:
                    MapDistanceLogarithmically();
                    break;
            }
        }

        private void MapDistanceInverseSquareLaw()
        {
            Vector3 closestPoint = thisCollider.ClosestPoint(listenerTarget.transform.position);
            float distance = Vector3.Distance(closestPoint, listenerTarget.transform.position);
            float inverse = 1 / Mathf.Sqrt(distance);
            audioSource.volume = inverse + audioSourceStrength;
        }

        private void MapDistanceLogarithmically()
        {
            Vector3 closestPoint = thisCollider.ClosestPoint(listenerTarget.transform.position);
            float distance = Vector3.Distance(closestPoint, listenerTarget.transform.position);
            float logarithmicDistance = Mathf.Log(distance, 2);
            float volumeDivider = 2 + audioSourceStrength;
            float volume = 1 - logarithmicDistance / volumeDivider;
            volume = Mathf.Clamp(volume, 0, 1);
            audioSource.volume = volume;
        }

        private void StereoPanning()
        {
            Vector3 closestPoint = thisCollider.ClosestPoint(listenerTarget.transform.position);
            Vector3 heading = Vector3.Normalize(closestPoint - listenerTarget.transform.position);
            float distance = Vector3.Distance(closestPoint, listenerTarget.transform.position);
            float dot = Vector3.Dot(heading, listenerTarget.transform.forward);
            float direction = Vector3.Dot(heading, listenerTarget.transform.right);
            float pan = direction;
            if (Mathf.Abs(pan) < 0.001)
            {
                pan = 0;
            }
            if (distance < 0.01)
            {
                float tParam = Time.deltaTime * .6f;
                pan = Mathf.Lerp(distance, 0, tParam);
            }
            audioSource.panStereo = pan;
        }
    }
}