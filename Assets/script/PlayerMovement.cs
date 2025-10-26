using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;

    public float maxSprintTime = 5f;
    public float sprintCooldown = 10f;
    private float sprintTimer;
    private bool isSprinting;
    private bool isSprintOnCooldown;

    public float crosshairSize = 8f;
    private Texture2D crosshairTexture;

    public TextMeshProUGUI heartbeatText;
    public float baseHeartbeat = 85f;
    public float sprintMaxHeartbeat = 140f;
    public float entityMinHeartbeat = 145f;
    public float entityMaxHeartbeat = 160f;
    public float heartbeatRiseSpeed = 40f;
    public float heartbeatFallSpeed = 25f;
    public bool entityNearby = false;

    private float currentHeartbeat;
    private float targetHeartbeat;
    private float sprintIntensity;
    private float jumpBoost = 0f;
    private float jumpDecaySpeed = 10f;

    public AudioSource heartbeatAudio;
    public float heartbeatVolume = 0.6f;
    public Volume postProcessingVolume;
    private DepthOfField depthOfField;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sprintTimer = maxSprintTime;

        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, Color.white);
        crosshairTexture.Apply();

        currentHeartbeat = baseHeartbeat;
        targetHeartbeat = baseHeartbeat;

        if (postProcessingVolume != null)
            postProcessingVolume.profile.TryGet(out depthOfField);
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && !isSprintOnCooldown && sprintTimer > 0 && Input.GetAxis("Vertical") > 0;
        isSprinting = wantsToRun;

        if (isSprinting)
        {
            sprintTimer -= Time.deltaTime;
            sprintIntensity = Mathf.Clamp01(sprintIntensity + Time.deltaTime * 0.6f);
            if (sprintTimer <= 0)
                StartCoroutine(SprintCooldown());
        }
        else
        {
            sprintIntensity = Mathf.MoveTowards(sprintIntensity, 0, Time.deltaTime * 0.4f);
            if (!isSprintOnCooldown && sprintTimer < maxSprintTime)
                sprintTimer += Time.deltaTime * 0.5f;
        }

        float curSpeedX = canMove ? (isSprinting ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isSprinting ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButtonDown("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
            jumpBoost += 12f;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (Input.GetKey(KeyCode.C) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

         if (currentHeartbeat > 150f)
        {
            CardiacArrest();
        }

        UpdateHeartbeat();
        UpdateHeartbeatAudio();
        UpdateVisionEffect();
    }

    void UpdateHeartbeat()
    {
        jumpBoost = Mathf.MoveTowards(jumpBoost, 0, Time.deltaTime * jumpDecaySpeed);

        if (entityNearby)
            targetHeartbeat = Mathf.Lerp(entityMinHeartbeat, entityMaxHeartbeat, 0.5f);
        else if (isSprinting || sprintIntensity > 0.1f)
            targetHeartbeat = Mathf.Lerp(baseHeartbeat, sprintMaxHeartbeat, sprintIntensity);
        else
            targetHeartbeat = baseHeartbeat;

        targetHeartbeat += jumpBoost;

        if (currentHeartbeat < targetHeartbeat)
            currentHeartbeat = Mathf.MoveTowards(currentHeartbeat, targetHeartbeat, heartbeatRiseSpeed * Time.deltaTime);
        else
            currentHeartbeat = Mathf.MoveTowards(currentHeartbeat, targetHeartbeat, heartbeatFallSpeed * Time.deltaTime);

        if (heartbeatText != null)
            heartbeatText.text = $"{Mathf.RoundToInt(currentHeartbeat)} BPM";
    }

    void UpdateHeartbeatAudio()
    {
        if (heartbeatAudio == null) return;
        if (currentHeartbeat > 130f)
        {
            if (!heartbeatAudio.isPlaying) heartbeatAudio.Play();
            heartbeatAudio.volume = Mathf.MoveTowards(heartbeatAudio.volume, heartbeatVolume, Time.deltaTime * 2f);
            heartbeatAudio.pitch = Mathf.Lerp(1f, 1.3f, (currentHeartbeat - 120f) / 40f);
        }
        else
        {
            heartbeatAudio.volume = Mathf.MoveTowards(heartbeatAudio.volume, 0f, Time.deltaTime * 2f);
            if (heartbeatAudio.volume <= 0.01f) heartbeatAudio.Stop();
        }
    }

    void UpdateVisionEffect()
    {
        if (depthOfField == null) return;
        if (currentHeartbeat > 140f)
        {
            depthOfField.active = true;
            depthOfField.gaussianStart.Override(Mathf.Lerp(0.3f, 0.1f, (currentHeartbeat - 140f) / 20f));
            depthOfField.gaussianEnd.Override(Mathf.Lerp(2f, 0.5f, (currentHeartbeat - 140f) / 20f));
        }
        else
        {
            depthOfField.active = false;
        }
    }

    void CardiacArrest()
    {
    canMove = false;
    moveDirection = Vector3.zero;
    if (currentHeartbeat < 200f)
        currentHeartbeat = 200f;
    if (heartbeatAudio != null)
        heartbeatAudio.Stop();
    if (depthOfField != null)
        depthOfField.active = false;
    }


    IEnumerator SprintCooldown()
    {
        isSprintOnCooldown = true;
        sprintTimer = 0;
        yield return new WaitForSeconds(sprintCooldown);
        sprintTimer = maxSprintTime;
        isSprintOnCooldown = false;
    }

    void OnGUI()
    {
        float xMin = (Screen.width / 2) - (crosshairSize / 2);
        float yMin = (Screen.height / 2) - (crosshairSize / 2);
        GUI.DrawTexture(new Rect(xMin, yMin, crosshairSize, crosshairSize), crosshairTexture);
    }
}
