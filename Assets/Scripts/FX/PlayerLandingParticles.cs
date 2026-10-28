using UnityEngine;

public class PlayerLandingParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem landingBurst;
    [SerializeField] private float heightOffset = 0.02f;

    private CharacterController _controller;
    private RippleEmitter _ripple;
    private bool _wasGrounded = true;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        _ripple = GetComponent<RippleEmitter>();

        if (_ripple == null)
        {
            _ripple = gameObject.AddComponent<RippleEmitter>();
        }

        if (landingBurst == null)
        {
            GameObject burstObject = GameObject.Find("FX_LandingBurst");

            if (burstObject != null)
            {
                landingBurst = burstObject.GetComponent<ParticleSystem>();
            }
        }
    }

    private void Update()
    {
        if (_controller == null)
        {
            return;
        }

        bool grounded = _controller.isGrounded;

        if (grounded && !_wasGrounded)
        {
            if (landingBurst != null)
            {
                landingBurst.transform.position = transform.position + Vector3.up * heightOffset;
                landingBurst.Play();
            }

            if (_ripple != null)
            {
                _ripple.PlayOnce();
            }
        }

        _wasGrounded = grounded;
    }
}
