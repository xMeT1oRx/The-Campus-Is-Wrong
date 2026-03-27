using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 3f;
    public float sprintSpeed = 5f;
    public float crouchSpeed = 1.5f;

    [Header("Физика")]
    public float gravity = -9.81f;

    [Header("Аниматор (перетащить вручную)")]
    public Animator characterAnimator;

    // ── приватные поля ──────────────────────────────────────────
    private CharacterController controller;
    private Vector3 velocity;

    private float animSpeedCurrent = 0f;
    private const float AnimDamp = 0.1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>(true);

        if (characterAnimator == null)
            Debug.LogError("Animator не найден!");
        else
            characterAnimator.applyRootMotion = false;
    }

    void Update()
    {
        HandleMovement();
        HandleAnimator();
    }

    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.C);

        float currentSpeed = moveSpeed;
        if (isSprinting) currentSpeed = sprintSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (!controller.isGrounded) velocity.y += gravity * Time.deltaTime;
        else velocity.y = -2f;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleAnimator()
    {
        if (characterAnimator == null) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float inputMagnitude = Mathf.Clamp01(new Vector2(x, z).magnitude);

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        bool isCrouching = Input.GetKey(KeyCode.C);

        float targetSpeed = 0f;
        if (inputMagnitude > 0.05f)
        {
            if (isSprinting) targetSpeed = 1.0f;
            else if (isCrouching) targetSpeed = 0.25f;
            else targetSpeed = 0.5f;
        }

        animSpeedCurrent = Mathf.Lerp(animSpeedCurrent, targetSpeed, Time.deltaTime / AnimDamp);
        characterAnimator.SetFloat("Speed", animSpeedCurrent);
    }
}
