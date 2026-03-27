using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [Header("Head Bob")]
    public Transform cameraHolder;
    public float bobFrequency = 2f;
    public float bobAmplitudeY = 0.05f;
    public float bobAmplitudeX = 0.02f;

    [Header("Наклон при повороте")]
    public float tiltAmount = 3f;
    public float tiltSpeed = 5f;

    private float bobTimer = 0f;
    private Vector3 defaultCamPos;
    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        if (cameraHolder != null)
            defaultCamPos = cameraHolder.localPosition;
    }

    // LateUpdate — чтобы bob применялся ПОСЛЕ того как
    // PlayerController уже переместил персонажа
    void LateUpdate()
    {
        if (cameraHolder == null) return;
        HandleHeadBob();
        HandleCameraTilt();
    }

    void HandleHeadBob()
    {
        float speed = cc.velocity.magnitude;
        bool isMoving = speed > 0.1f && cc.isGrounded;

        if (isMoving)
        {
            // Частота боба зависит от скорости (бег качается быстрее)
            float dynamicFreq = bobFrequency * (speed / 3f);
            bobTimer += Time.deltaTime * dynamicFreq;

            float newY = defaultCamPos.y + Mathf.Sin(bobTimer) * bobAmplitudeY;
            float newX = defaultCamPos.x + Mathf.Cos(bobTimer * 0.5f) * bobAmplitudeX;
            cameraHolder.localPosition = new Vector3(newX, newY, defaultCamPos.z);
        }
        else
        {
            // Плавное возвращение в исходное положение
            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition, defaultCamPos, Time.deltaTime * 8f);

            // bobTimer сбрасываем плавно до ближайшего 2π
            // чтобы не было рывка при следующем движении
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * 6f);
        }
    }

    void HandleCameraTilt()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float targetTilt = -mouseX * tiltAmount;

        Vector3 euler = cameraHolder.localEulerAngles;
        // Переводим из [0..360] в [-180..180], чтобы LerpAngle работал корректно
        float currentZ = euler.z > 180f ? euler.z - 360f : euler.z;
        float newZ = Mathf.LerpAngle(currentZ, targetTilt, Time.deltaTime * tiltSpeed);

        // Меняем только Z-наклон, X (pitch) не трогаем — им управляет PlayerController
        cameraHolder.localEulerAngles = new Vector3(euler.x, euler.y, newZ);
    }
}
