using UnityEngine;

public class PlayerLandingParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem landingBurst;
    [SerializeField] private float heightOffset = 0.02f;

    private CharacterController _controller;
    private bool _wasGrounded = true;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

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

        if (grounded && !_wasGrounded && landingBurst != null)
        {
            landingBurst.transform.position = transform.position + Vector3.up * heightOffset;
            landingBurst.Play();
        }

        _wasGrounded = grounded;
    }
}
