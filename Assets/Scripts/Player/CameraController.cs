using UnityEngine;

/// <summary>
/// Весь контроль камеры: mouse look, привязка к кости Head (world-space),
/// стабилизация позиции, head bob (аддитивный), camera tilt, near clip.
/// Вешается на тот же объект, что и PlayerController (корень игрока).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class CameraController : MonoBehaviour
{
    [Header("Обзор")]
    public float mouseSensitivity = 100f;
    [Tooltip("Camera — дочерний объект CameraHolder")]
    public Transform cameraTransform;

    [Header("Ограничения pitch")]
    [Tooltip("Максимум вверх — не уходит за спину модели")]
    public float pitchMin = -60f;
    [Tooltip("Максимум вниз — видно тело при взгляде под ноги")]
    public float pitchMax = 80f;

    [Header("Привязка к глазам")]
    [Tooltip("Родитель камеры — каждый кадр позиционируется по кости Head")]
    public Transform cameraHolder;
    [Tooltip("Смещение от центра кости Head до уровня глаз (локальные оси кости). " +
             "Y — высота, Z — вперёд к глазам")]
    public Vector3 eyeOffset = new Vector3(0f, 0.08f, 0.08f);

    [Header("Стабилизация")]
    [Tooltip("Время сглаживания позиции (сек). 0.03–0.06 — убирает дрожание анимации, " +
             "не создавая заметного отставания")]
    [Range(0f, 0.15f)]
    public float stabilizationTime = 0.04f;

    [Header("Head Bob (аддитивный, поверх анимации)")]
    public float bobFrequency = 2f;
    public float bobAmplitudeY = 0.05f;
    public float bobAmplitudeX = 0.02f;

    [Header("Наклон при повороте")]
    public float tiltAmount = 3f;
    public float tiltSpeed = 5f;

    [Header("Near Clip (анти-клипинг головы)")]
    [Tooltip("0.15 — голова за near clip, тело при взгляде вниз видно")]
    [Range(0.05f, 0.4f)]
    public float nearClipPlane = 0.15f;

    // ── приватные ───────────────────────────────────────────────
    private float   xRotation   = 0f;
    private float   currentTilt = 0f;   // хранится отдельно — не читаем из Euler
    private float   bobTimer    = 0f;
    private Vector3 bobOffset;

    // стабилизатор позиции
    private Vector3 smoothPos;
    private Vector3 smoothVelocity;

    private CharacterController cc;
    private Transform headBone;

    // ── инициализация ────────────────────────────────────────────
    void Start()
    {
        cc = GetComponent<CharacterController>();

        var anim = GetComponentInChildren<Animator>(true);
        if (anim != null && anim.isHuman)
            headBone = anim.GetBoneTransform(HumanBodyBones.Head);

        if (headBone == null)
            Debug.LogWarning("[CameraController] Кость Head не найдена — " +
                             "убедись что rig задан как Humanoid.");

        // Инициализируем стабилизатор сразу в позиции кости,
        // чтобы в первый кадр не было рывка из (0,0,0)
        if (headBone != null && cameraHolder != null)
            smoothPos = headBone.TransformPoint(eyeOffset);
        else if (cameraHolder != null)
            smoothPos = cameraHolder.position;

        // Применяем чувствительность из настроек главного меню
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);

        if (cameraTransform != null)
        {
            var cam = cameraTransform.GetComponent<Camera>();
            if (cam != null) cam.nearClipPlane = nearClipPlane;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    // LateUpdate — после того как Animator обновил скелет.
    void LateUpdate()
    {
        HandleMouseLook();

        if (cameraHolder != null)
        {
            HandleHeadBob();
            TrackHeadBone();
            HandleCameraTilt();
        }
    }

    // ── mouse look ───────────────────────────────────────────────
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation  = Mathf.Clamp(xRotation, pitchMin, pitchMax);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ── привязка к кости + стабилизация ─────────────────────────
    void TrackHeadBone()
    {
        if (headBone == null) return;

        // Целевая позиция: центр кости Head → смещение в её локальных осях
        // (headBone.TransformPoint корректно учитывает поворот кости при любом pitch)
        Vector3 target = headBone.TransformPoint(eyeOffset)
                       + transform.TransformDirection(bobOffset);

        // SmoothDamp сглаживает мелкое дрожание от анимации,
        // не создавая заметного отставания при резких движениях.
        smoothPos = Vector3.SmoothDamp(
            smoothPos, target, ref smoothVelocity, stabilizationTime);

        cameraHolder.position = smoothPos;
    }

    // ── head bob (аддитивный офсет) ──────────────────────────────
    void HandleHeadBob()
    {
        float speed    = cc.velocity.magnitude;
        bool  isMoving = speed > 0.1f && cc.isGrounded;

        if (isMoving)
        {
            float dynamicFreq = bobFrequency * (speed / 3f);
            bobTimer += Time.deltaTime * dynamicFreq;

            bobOffset = new Vector3(
                Mathf.Cos(bobTimer * 0.5f) * bobAmplitudeX,
                Mathf.Sin(bobTimer)         * bobAmplitudeY,
                0f);
        }
        else
        {
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * 8f);
            bobTimer  = Mathf.Lerp(bobTimer,    0f,           Time.deltaTime * 6f);
        }
    }

    // ── camera tilt ──────────────────────────────────────────────
    void HandleCameraTilt()
    {
        float targetTilt = -Input.GetAxis("Mouse X") * tiltAmount;

        // Lerp хранится в отдельной переменной — не читаем Euler из transform,
        // чтобы world-space смещение позиции не влияло на чтение углов.
        currentTilt = Mathf.LerpAngle(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Применяем: pitch (X) не трогаем — им управляет cameraTransform выше.
        cameraHolder.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}
