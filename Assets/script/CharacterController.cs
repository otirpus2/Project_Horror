using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Sprint Settings")]
    public float sprintDuration = 5f;
    public float sprintCooldown = 5f;
    private float sprintTimer;
    private bool canSprint = true;

    [Header("Camera Settings")]
    public Transform cam;
    public float mouseSensitivity = 100f;
    float xRotation = 0f;

    CharacterController controller;
    Vector3 velocity;
    bool isCrouching = false;

    [Header("Crosshair")]
    private GameObject crosshair;
    
    [Header("Inspect Mode")]
    private bool isInspectMode = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // Create simple white dot crosshair
        crosshair = new GameObject("Crosshair");
        var canvas = new GameObject("CrosshairCanvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        crosshair.transform.SetParent(canvas.transform);
        var img = crosshair.AddComponent<Image>();
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.zero);
        img.rectTransform.sizeDelta = new Vector2(6, 6);
        img.rectTransform.anchoredPosition = Vector2.zero;
    }

    void Update()
{
    if (isInspectMode)
    {
        velocity = Vector3.zero; // freeze all movement
        return;
    }

    HandleCamera();
    HandleMovement();
    HandleJump();
    HandleSprint();
    HandleCrouch();
    ApplyGravity();
}


    void HandleCamera()
    {
        if (isInspectMode) return;
        
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (isInspectMode) return;
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = walkSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && canSprint)
            speed = sprintSpeed;
        if (isCrouching)
            speed = crouchSpeed;

        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleJump()
    {
        if (isInspectMode) return;
        
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    void HandleSprint()
    {
        if (isInspectMode) return;
        
        if (Input.GetKey(KeyCode.LeftShift) && canSprint && controller.velocity.magnitude > 0.1f)
        {
            sprintTimer += Time.deltaTime;
            if (sprintTimer >= sprintDuration)
            {
                canSprint = false;
                Invoke(nameof(ResetSprint), sprintCooldown);
            }
        }
    }

    void ResetSprint()
    {
        sprintTimer = 0f;
        canSprint = true;
    }

    void HandleCrouch()
    {
        if (isInspectMode) return;
        
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? 1.0f : 2.0f;
        }
    }

    void ApplyGravity()
{
    if (isInspectMode) return;

    velocity.y += gravity * Time.deltaTime;
    controller.Move(velocity * Time.deltaTime);
}

    
    // Public method to enable/disable inspect mode
   public void SetInspectMode(bool inspecting)
{
    isInspectMode = inspecting;

    // Freeze velocity if entering inspect mode
    if (isInspectMode)
        velocity = Vector3.zero;

    // Hide/show crosshair
    if (crosshair != null)
        crosshair.SetActive(!inspecting);
}

}