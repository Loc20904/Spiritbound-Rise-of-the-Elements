using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTransformManager : MonoBehaviour
{
    [Header("Forms")]
    public Transform normalTarget;
    public GameObject fireForm;

    [Header("Camera")]
    public CinemachineCamera cineCam;

    private Transform currentTarget;
    private GameObject currentExtraForm;
    
    // Components off the base player to disable when transformed
    private GameObject playerRoot;
    private PlayerController baseController;
    private Rigidbody2D baseRb;
    private Animator baseAnim;

    void Awake()
    {
        playerRoot = normalTarget != null ? normalTarget.gameObject : gameObject;
        baseController = playerRoot.GetComponent<PlayerController>();
        baseRb = playerRoot.GetComponent<Rigidbody2D>();
        baseAnim = playerRoot.GetComponent<Animator>();
    }

    void Start()
    {
        // Unparent forms so disabling playerRoot's children later doesn't disable them
        if (fireForm != null) { fireForm.transform.SetParent(null); fireForm.SetActive(false); }

        currentTarget = playerRoot.transform;
        UpdateCameraTarget(currentTarget);
    }

    [Header("Input Actions")]
    public InputAction transformToFireAction = new InputAction("TransformFire", binding: "<Keyboard>/q");
    public InputAction revertToNormalAction = new InputAction("RevertNormal", binding: "<Keyboard>/r");

    private void OnEnable()
    {
        transformToFireAction.Enable();
        revertToNormalAction.Enable();
    }

    private void OnDisable()
    {
        transformToFireAction.Disable();
        revertToNormalAction.Disable();
    }

    private bool isTransforming = false;

    void Update()
    {
        if (isTransforming) return;

        if (transformToFireAction.WasPressedThisFrame()) StartCoroutine(SwitchToFormCoroutine(fireForm));
        if (revertToNormalAction.WasPressedThisFrame()) StartCoroutine(BackToNormalCoroutine());
    }

    private System.Collections.IEnumerator SwitchToFormCoroutine(GameObject newForm)
    {
        if (newForm == null || currentExtraForm == newForm) yield break;

        isTransforming = true;

        if (currentExtraForm != null)
        {
            var tc = currentExtraForm.GetComponent<TransformController>();
            if (tc != null)
            {
                tc.PlayTransformAnimation();
                yield return new WaitForSeconds(tc.transformAnimTime);
            }
            currentExtraForm.SetActive(false);
        }
        else 
        {
            ToggleBasePlayer(false);
        }

        Vector3 oldPos = currentTarget != null ? currentTarget.position : playerRoot.transform.position;
        float facingDir = (currentTarget != null ? currentTarget.localScale.x : playerRoot.transform.localScale.x) > 0 ? 1f : -1f;

        newForm.SetActive(true);
        newForm.transform.position = oldPos;
        
        if (newForm == fireForm)
            newForm.transform.localScale = new Vector3(1f * facingDir, 1f, 0f);
        else
            newForm.transform.localScale = new Vector3(1f * facingDir, 1f, 1f);

        currentExtraForm = newForm;
        currentTarget = newForm.transform;
        UpdateCameraTarget(currentTarget);

        var newTc = newForm.GetComponent<TransformController>();
        if (newTc != null)
        {
            newTc.enabled = true; // Phòng hờ script bị tắt nhầm ở inspector
            newTc.PlayTransformAnimation();
            yield return new WaitForSeconds(Mathf.Min(newTc.transformAnimTime, 1f));
        }

        isTransforming = false;
    }

    private System.Collections.IEnumerator BackToNormalCoroutine()
    {
        if (currentExtraForm == null) yield break;

        isTransforming = true;

        var tc = currentExtraForm.GetComponent<TransformController>();
        if (tc != null)
        {
            tc.enabled = true; // Đảm bảo gọi được
            tc.PlayTransformAnimation();
            yield return new WaitForSeconds(Mathf.Min(tc.transformAnimTime, 1f));
        }

        Vector3 oldPos = currentTarget != null ? currentTarget.position : playerRoot.transform.position;
        float facingDir = (currentTarget != null ? currentTarget.localScale.x : playerRoot.transform.localScale.x) > 0 ? 1f : -1f;

        currentExtraForm.SetActive(false);
        currentExtraForm = null;

        playerRoot.transform.position = oldPos;
        playerRoot.transform.localScale = new Vector3(5f * facingDir, 5f, 0f);
        currentTarget = playerRoot.transform;

        ToggleBasePlayer(true);
        UpdateCameraTarget(currentTarget);
        isTransforming = false;
    }

    void ToggleBasePlayer(bool isActive)
    {
        if (baseController != null) baseController.enabled = isActive;
        if (baseAnim != null) baseAnim.enabled = isActive;
        if (baseRb != null) 
        {
            baseRb.simulated = isActive;
            if (isActive) baseRb.linearVelocity = Vector2.zero;
        }

        Renderer[] allRenderers = playerRoot.GetComponentsInChildren<Renderer>(true);
        foreach (var r in allRenderers)
        {
            r.enabled = isActive;
        }

        Collider2D[] allColliders = playerRoot.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in allColliders)
        {
            c.enabled = isActive;
        }

        Canvas[] allCanvas = playerRoot.GetComponentsInChildren<Canvas>(true);
        foreach(var c in allCanvas) {
            c.enabled = isActive;
        }
    }

    void UpdateCameraTarget(Transform target)
    {
        if (cineCam == null || target == null) return;

        cineCam.Follow = target;
        cineCam.LookAt = target;
    }
}