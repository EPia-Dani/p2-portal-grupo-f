using UnityEngine;
namespace Player
{
    public class PitchController : MonoBehaviour
    {
        private float _basePitch;
        private bool _isInvertPitch;
        private float _leanPitch;
        private float _leanRoll;
        private GameObject _hand;
        private Quaternion _handBaseLocalRotation;

        private void Awake()
        {
            _hand = gameObject.GetChildRecursive("Hand");
            _handBaseLocalRotation = _hand.transform.localRotation;
        }

        public void SetPitch(float pitch, bool invertPitch = false)
        {
            _basePitch = pitch;
            _isInvertPitch = invertPitch;
            ApplyRotation();
        }

        public void SetLean(float leanPitchDegrees, float leanRollDegrees)
        {
            _leanPitch = leanPitchDegrees;
            _leanRoll = leanRollDegrees;
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            var appliedPitch = _isInvertPitch ? _basePitch : -_basePitch;
            transform.localRotation = Quaternion.Euler(appliedPitch, 0f, 0f);

            var leanRotation = Quaternion.Euler(_leanPitch, 0f, _leanRoll);
            _hand.transform.localRotation = _handBaseLocalRotation * leanRotation;
        }
    }
}
