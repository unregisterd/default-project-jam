using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EndingDirector : MonoBehaviour
{
    [Header("引用（留空自动查找）")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Transform playerRoot;
    [SerializeField] private FirstPersonMotor motor;
    [SerializeField] private PlayerLook look;

    [Header("天空切换")]
    [SerializeField] private Material endingSkybox;
    [SerializeField] private Color endingFogColor = new Color(0.02f, 0.02f, 0.05f, 1f);
    [SerializeField] private float endingFogEnd = 300f;

    [Header("相机路径")]
    [SerializeField] private float fallDistance = 24f;
    [SerializeField] private float fallDuration = 4f;
    [SerializeField] private float invertDegrees = 180f;
    [SerializeField] private float pullBackDistance = 35f;
    [SerializeField] private float riseDistance = 28f;
    [SerializeField] private float pullBackDuration = 6f;
    [SerializeField] private float fovEnd = 85f;

    [Header("调试")]
    [Tooltip("F10 播放，F9 还原")]
    [SerializeField] private bool enableDebugKeys = true;

    private Transform _cameraOriginalParent;
    private Vector3 _cameraOriginalLocalPosition;
    private Quaternion _cameraOriginalLocalRotation;
    private float _cameraOriginalFov;
    private Material _originalSkybox;
    private Color _originalFogColor;
    private float _originalFogEnd;
    private bool _originalFogEnabled;
    private Coroutine _routine;

    private void Awake()
    {
        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        if (motor == null)
        {
            motor = FindObjectOfType<FirstPersonMotor>();
        }

        if (look == null)
        {
            look = FindObjectOfType<PlayerLook>();
        }

        if (playerRoot == null && motor != null)
        {
            playerRoot = motor.transform;
        }
    }

    private void Update()
    {
        if (!enableDebugKeys || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            Play();
        }

        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Restore();
        }
    }

    public void Play()
    {
        if (viewCamera == null || _routine != null)
        {
            return;
        }

        _routine = StartCoroutine(Run());
    }

    public void Restore()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (viewCamera == null)
        {
            return;
        }

        viewCamera.transform.SetParent(_cameraOriginalParent, false);
        viewCamera.transform.localPosition = _cameraOriginalLocalPosition;
        viewCamera.transform.localRotation = _cameraOriginalLocalRotation;
        viewCamera.fieldOfView = _cameraOriginalFov;

        if (motor != null)
        {
            motor.enabled = true;
        }

        if (look != null)
        {
            look.enabled = true;
        }

        RenderSettings.skybox = _originalSkybox;
        RenderSettings.fogColor = _originalFogColor;
        RenderSettings.fogEndDistance = _originalFogEnd;
        RenderSettings.fog = _originalFogEnabled;
        DynamicGI.UpdateEnvironment();

        Debug.Log("[Ending] 已还原相机与天空");
    }

    private IEnumerator Run()
    {
        _cameraOriginalParent = viewCamera.transform.parent;
        _cameraOriginalLocalPosition = viewCamera.transform.localPosition;
        _cameraOriginalLocalRotation = viewCamera.transform.localRotation;
        _cameraOriginalFov = viewCamera.fieldOfView;
        _originalSkybox = RenderSettings.skybox;
        _originalFogColor = RenderSettings.fogColor;
        _originalFogEnd = RenderSettings.fogEndDistance;
        _originalFogEnabled = RenderSettings.fog;

        if (motor != null)
        {
            motor.enabled = false;
        }

        if (look != null)
        {
            look.enabled = false;
        }

        viewCamera.transform.SetParent(null, true);

        Vector3 startPosition = viewCamera.transform.position;
        Quaternion startRotation = viewCamera.transform.rotation;
        float startFov = viewCamera.fieldOfView;

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fallDuration);
            float eased = progress * progress * (3f - 2f * progress);

            viewCamera.transform.position = startPosition + Vector3.down * (fallDistance * eased);
            viewCamera.transform.rotation = startRotation * Quaternion.Euler(invertDegrees * eased, 0f, 0f);
            viewCamera.fieldOfView = Mathf.Lerp(startFov, fovEnd, eased);

            yield return null;
        }

        if (endingSkybox != null)
        {
            RenderSettings.skybox = endingSkybox;
        }

        RenderSettings.fog = true;
        RenderSettings.fogEndDistance = endingFogEnd;
        RenderSettings.fogColor = endingFogColor;
        DynamicGI.UpdateEnvironment();

        Vector3 pullStart = viewCamera.transform.position;
        Vector3 away = pullStart;
        away.y = 0f;
        away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.back;
        Vector3 pullEnd = pullStart + Vector3.up * riseDistance + away * pullBackDistance;
        Quaternion pullStartRotation = viewCamera.transform.rotation;
        Quaternion pullEndRotation = Quaternion.Euler(-55f, viewCamera.transform.eulerAngles.y, 0f);
        float pullStartFov = viewCamera.fieldOfView;

        elapsed = 0f;

        while (elapsed < pullBackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / pullBackDuration);
            float eased = progress * progress * (3f - 2f * progress);

            viewCamera.transform.position = Vector3.Lerp(pullStart, pullEnd, eased);
            viewCamera.transform.rotation = Quaternion.Slerp(pullStartRotation, pullEndRotation, eased);
            viewCamera.fieldOfView = Mathf.Lerp(pullStartFov, 60f, eased);

            yield return null;
        }

        _routine = null;
        Debug.Log("[Ending] 相机路径播放完成（占位版）。按 F9 还原。");
    }
}
