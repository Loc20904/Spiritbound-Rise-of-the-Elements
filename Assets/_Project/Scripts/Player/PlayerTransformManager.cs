using System.Collections;
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
    [Header("Input Actions")]
    public InputAction transformToFireAction = new InputAction("TransformFire", binding: "<Keyboard>/q");
    public InputAction revertToNormalAction = new InputAction("RevertNormal", binding: "<Keyboard>/r");

    private GameObject playerRoot;
    private PlayerController baseController;
    private Rigidbody2D baseRb;
    private Animator baseAnim;
    private PlayerHealth health;
    private Vector3 normalScale;
    private Vector3 fireScale;
    private GameObject currentExtraForm;
    private bool isTransforming;
    private Renderer[] renderers;
    private Collider2D[] colliders;
    private Canvas[] canvases;
    private bool[] rendererStates, colliderStates, canvasStates;

    public bool IsTransformed => currentExtraForm != null;
    public Transform ActiveTarget => IsTransformed ? currentExtraForm.transform : playerRoot != null ? playerRoot.transform : transform;
    public Rigidbody2D ActiveBody => IsTransformed ? currentExtraForm.GetComponent<Rigidbody2D>() : baseRb;

    private void Awake()
    {
        playerRoot = normalTarget != null ? normalTarget.gameObject : gameObject;
        baseController = playerRoot.GetComponent<PlayerController>();
        baseRb = playerRoot.GetComponent<Rigidbody2D>();
        baseAnim = playerRoot.GetComponent<Animator>();
        health = PlayerHealth.Resolve(playerRoot);
        normalScale = playerRoot.transform.localScale;
        if (fireForm != null)
        {
            fireScale = fireForm.transform.localScale;
            var link = fireForm.GetComponent<PlayerHealthLink>() ?? fireForm.AddComponent<PlayerHealthLink>();
            link.Bind(health);
            // Keep the form scene-owned when detached; destroy it together with its owner.
            fireForm.transform.SetParent(null);
            fireForm.SetActive(false);
        }
        renderers = playerRoot.GetComponentsInChildren<Renderer>(true);
        colliders = playerRoot.GetComponentsInChildren<Collider2D>(true);
        canvases = playerRoot.GetComponentsInChildren<Canvas>(true);
    }

    private void Start() => UpdateCameraTarget(ActiveTarget);
    private void OnEnable() { transformToFireAction?.Enable(); revertToNormalAction?.Enable(); }
    private void OnDisable() { transformToFireAction?.Disable(); revertToNormalAction?.Disable(); }
    private void OnDestroy()
    {
        transformToFireAction?.Dispose();
        revertToNormalAction?.Dispose();
        if (fireForm != null) Destroy(fireForm);
    }

    private void Update()
    {
        if (Time.timeScale <= 0 || isTransforming || (health != null && (!health.CanAct || health.isKnockback))) return;
        if (transformToFireAction.WasPressedThisFrame()) SwitchToFire();
        else if (revertToNormalAction.WasPressedThisFrame()) RevertToNormal();
    }

    private void LateUpdate()
    {
        // Existing enemies track the normal root, including while the form is detached.
        if (IsTransformed) playerRoot.transform.position = currentExtraForm.transform.position;
    }

    public void SwitchToFire()
    {
        if (isTransforming || IsTransformed || fireForm == null || (health != null && !health.CanAct)) return;
        StartCoroutine(SwitchToForm());
    }

    public void RevertToNormal()
    {
        if (isTransforming || !IsTransformed || (health != null && !health.CanAct)) return;
        StartCoroutine(BackToNormal());
    }

    private IEnumerator SwitchToForm()
    {
        isTransforming = true;
        Vector3 position = ActiveTarget.position;
        float facing = Mathf.Sign(ActiveTarget.localScale.x);
        ToggleBasePlayer(false);
        currentExtraForm = fireForm;
        fireForm.transform.SetPositionAndRotation(position, Quaternion.identity);
        fireForm.transform.localScale = new Vector3(Mathf.Abs(fireScale.x) * facing, fireScale.y, fireScale.z);
        fireForm.SetActive(true);
        UpdateCameraTarget(ActiveTarget);
        var movement = fireForm.GetComponent<TransformController>();
        if (movement != null)
        {
            movement.PlayTransformAnimation();
            yield return new WaitForSeconds(Mathf.Clamp(movement.transformAnimTime, 0f, 1f));
        }
        isTransforming = false;
    }

    private IEnumerator BackToNormal()
    {
        isTransforming = true;
        var movement = currentExtraForm.GetComponent<TransformController>();
        if (movement != null)
        {
            movement.PlayTransformAnimation();
            yield return new WaitForSeconds(Mathf.Clamp(movement.transformAnimTime, 0f, 1f));
        }
        ReturnToNormalImmediately();
    }

    public void StopActiveMovement()
    {
        if (!IsTransformed) return;
        currentExtraForm.GetComponent<TransformController>()?.StopMove();
        currentExtraForm.GetComponent<AttackController>()?.StopAttack();
    }

    public void ReturnToNormalImmediately()
    {
        StopAllCoroutines();
        if (IsTransformed)
        {
            Vector3 position = currentExtraForm.transform.position;
            float facing = Mathf.Sign(currentExtraForm.transform.localScale.x);
            currentExtraForm.SetActive(false);
            currentExtraForm = null;
            playerRoot.transform.position = position;
            playerRoot.transform.localScale = new Vector3(Mathf.Abs(normalScale.x) * facing, normalScale.y, normalScale.z);
            ToggleBasePlayer(true);
        }
        isTransforming = false;
        UpdateCameraTarget(ActiveTarget);
    }

    private void ToggleBasePlayer(bool active)
    {
        if (!active)
        {
            baseController?.StopMove();
            rendererStates = System.Array.ConvertAll(renderers, r => r.enabled);
            colliderStates = System.Array.ConvertAll(colliders, c => c.enabled);
            canvasStates = System.Array.ConvertAll(canvases, c => c.enabled);
        }
        if (baseController != null) baseController.enabled = active && (health == null || !health.IsDead);
        if (baseAnim != null) baseAnim.enabled = active;
        if (baseRb != null) { baseRb.linearVelocity = Vector2.zero; baseRb.simulated = active; }
        for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = active && rendererStates[i];
        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = active && colliderStates[i];
        for (int i = 0; i < canvases.Length; i++) canvases[i].enabled = active && canvasStates[i];
    }

    private void UpdateCameraTarget(Transform target)
    {
        if (cineCam == null || target == null) return;
        cineCam.Follow = target;
        cineCam.LookAt = target;
    }
}
