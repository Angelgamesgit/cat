using System.Collections;
using UnityEngine;

/// <summary>サーチライトで猫を見つけ、追跡やデバフを行う影猫の制御。</summary>
[RequireComponent(typeof(Collider))]
public class ShadowCatSystem : MonoBehaviour
{
    public enum ContactDebuff
    {
        StealAndHide,
        ImmobilizeCat
    }

    [Header("References")]
    [SerializeField] private GameSystem gameSystem;
    [SerializeField] private Light searchLight;

    [Header("Search Light")]
    [SerializeField, Range(1f, 179f)] private float startingSpotAngle = 60f;
    [SerializeField, Range(1f, 179f)] private float minimumSpotAngle = 10f;
    [SerializeField] private float angleNarrowDuration = 8f;
    [SerializeField] private float lightRange = 25f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseDuration = 5f;
    [SerializeField] private float fleeSpeed = 4f;
    [SerializeField] private float fleeDuration = 1.5f;

    [Header("Contact Debuff")]
    [SerializeField] private ContactDebuff contactDebuff = ContactDebuff.StealAndHide;
    [SerializeField] private float immobilizeDuration = 3f;
    [SerializeField] private float hideDuration = 6f;
    [SerializeField] private float contactRepeatDelay = 1f;

    [Header("Renderer and Collider References")]
    [SerializeField] private Renderer[] hiddenRenderers;
    [SerializeField] private Collider[] hiddenColliders;

    private Transform chaseTarget;
    private Transform sphereTransform;
    private float sphereRadius;
    private float lightEnabledAt;
    private float chaseUntil;
    private float searchLightUnavailableUntil;
    private float fleeUntil;
    private float nextContactTime;
    private float narrowStartAngle;
    private Coroutine immobilizeCoroutine;
    private Coroutine hideCoroutine;
    private bool isChasing;
    private bool isFleeing;
    private bool isHidden;
    private bool searchLightWasEnabledBeforeHide;
    private bool wasHitByPlayerLight;

    private void Awake()
    {
        if (gameSystem == null) gameSystem = GameSystem.Instance;
        if (searchLight == null) searchLight = GetComponentInChildren<Light>();
        if (searchLight == null)
        {
            Debug.LogError($"{name}: サーチライトのSpot Lightが設定されていません。", this);
            enabled = false;
            return;
        }

        searchLight.type = LightType.Spot;
        searchLight.range = lightRange;
        searchLight.spotAngle = startingSpotAngle;
        searchLight.enabled = false;

        if (hiddenRenderers == null || hiddenRenderers.Length == 0)
            hiddenRenderers = GetComponentsInChildren<Renderer>(true);
        if (hiddenColliders == null || hiddenColliders.Length == 0)
            hiddenColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Start()
    {
        RefreshWorldReferences();
    }

    private void Update()
    {
        if (gameSystem == null) gameSystem = GameSystem.Instance;
        if (gameSystem == null || !gameSystem.isPlaying) return;

        RefreshWorldReferences();
        UpdateSearchLightAngle();

        bool playerLightHits = IsPlayerLightOnThis();
        if (playerLightHits)
        {
            if (!wasHitByPlayerLight) Repel();
            wasHitByPlayerLight = true;
            return;
        }
        wasHitByPlayerLight = false;

        if (isFleeing)
        {
            MoveAwayFromTarget();
            if (Time.time >= fleeUntil) isFleeing = false;
            return;
        }

        if (searchLight.enabled && !isChasing && IsTargetInsideSearchLight())
        {
            isChasing = true;
            chaseUntil = Time.time + chaseDuration;
        }

        if (isChasing)
        {
            if (Time.time >= chaseUntil)
            {
                isChasing = false;
                return;
            }
            MoveTowardTarget(chaseSpeed);
        }
    }

    /// <summary>プレイヤーのタッチ操作で照準が動いたときにサーチライトを点灯する。</summary>
    public static void NotifyPlayerLightMoved(GameSystem owner)
    {
        ShadowCatSystem[] shadows = FindObjectsByType<ShadowCatSystem>(FindObjectsSortMode.None);
        foreach (ShadowCatSystem shadow in shadows)
        {
            if (shadow.gameSystem == null) shadow.gameSystem = owner;
            if (shadow.gameSystem == owner) shadow.EnableSearchLight();
        }
    }

    /// <summary>猫との接触時に設定されたデバフを発動する。</summary>
    public void OnPlayerCatTouched(CatPlayer player)
    {
        if (player == null || Time.time < nextContactTime) return;
        nextContactTime = Time.time + contactRepeatDelay;

        switch (contactDebuff)
        {
            case ContactDebuff.StealAndHide:
                StealAndHide(player);
                break;
            case ContactDebuff.ImmobilizeCat:
                ImmobilizeCat(player);
                break;
        }
    }

    private void EnableSearchLight()
    {
        if (searchLight == null || isHidden || Time.time < searchLightUnavailableUntil) return;
        if (!searchLight.enabled)
        {
            searchLight.enabled = true;
            lightEnabledAt = Time.time;
            narrowStartAngle = Mathf.Max(searchLight.spotAngle, startingSpotAngle);
        }
    }

    private void UpdateSearchLightAngle()
    {
        if (!searchLight.enabled) return;
        float progress = angleNarrowDuration <= 0f
            ? 1f
            : Mathf.Clamp01((Time.time - lightEnabledAt) / angleNarrowDuration);
        searchLight.spotAngle = Mathf.Lerp(narrowStartAngle, minimumSpotAngle, progress);
        searchLight.innerSpotAngle = Mathf.Min(searchLight.spotAngle, searchLight.spotAngle * 0.8f);
    }

    private void RefreshWorldReferences()
    {
        if (gameSystem == null) return;
        if (chaseTarget == null && gameSystem.PlayerCat != null)
            chaseTarget = gameSystem.PlayerCat.transform;
        if (gameSystem.sphere == null) return;

        sphereTransform = gameSystem.sphere.transform;
        SphereCollider sphereCollider = gameSystem.sphere.GetComponent<SphereCollider>();
        if (sphereCollider != null)
            sphereRadius = sphereCollider.radius * Mathf.Max(
                Mathf.Abs(sphereTransform.lossyScale.x),
                Mathf.Abs(sphereTransform.lossyScale.y),
                Mathf.Abs(sphereTransform.lossyScale.z));
    }

    private bool IsTargetInsideSearchLight()
    {
        if (chaseTarget == null || searchLight == null) return false;
        Vector3 toTarget = chaseTarget.position - searchLight.transform.position;
        if (toTarget.magnitude > searchLight.range) return false;
        return Vector3.Angle(searchLight.transform.forward, toTarget) <= searchLight.spotAngle * 0.5f;
    }

    private bool IsPlayerLightOnThis()
    {
        if (gameSystem == null || gameSystem.LightTargetSystem == null) return false;
        Light playerLight = gameSystem.LightTargetSystem.GetSpotLight();
        if (playerLight == null || !playerLight.enabled || !playerLight.gameObject.activeInHierarchy) return false;

        Vector3 toShadow = transform.position - playerLight.transform.position;
        float distance = toShadow.magnitude;
        if (distance > playerLight.range || distance <= 0.001f) return false;
        return Vector3.Angle(playerLight.transform.forward, toShadow) <= playerLight.spotAngle * 0.5f;
    }

    private void Repel()
    {
        searchLight.enabled = false;
        searchLightUnavailableUntil = Time.time + 30f;
        isChasing = false;
        isFleeing = true;
        fleeUntil = Time.time + fleeDuration;
    }

    private void MoveTowardTarget(float speed)
    {
        MoveAlongSphere(chaseTarget != null ? chaseTarget.position : transform.position + transform.forward, speed);
    }

    private void MoveAwayFromTarget()
    {
        Vector3 away = chaseTarget != null
            ? transform.position - chaseTarget.position
            : transform.forward;
        if (sphereTransform != null)
        {
            Vector3 up = (transform.position - sphereTransform.position).normalized;
            away = Vector3.ProjectOnPlane(away, up);
        }
        if (away.sqrMagnitude > 0.001f) MoveAlongSphere(transform.position + away.normalized, fleeSpeed);
    }

    private void MoveAlongSphere(Vector3 destination, float speed)
    {
        Vector3 movementDirection = destination - transform.position;
        if (sphereTransform != null)
        {
            Vector3 up = (transform.position - sphereTransform.position).normalized;
            movementDirection = Vector3.ProjectOnPlane(movementDirection, up);
        }
        if (movementDirection.sqrMagnitude <= 0.001f) return;

        Vector3 direction = movementDirection.normalized;
        Vector3 nextPosition = transform.position + direction * speed * Time.deltaTime;
        if (sphereTransform != null && sphereRadius > 0f)
            nextPosition = sphereTransform.position + (nextPosition - sphereTransform.position).normalized * sphereRadius;
        transform.position = nextPosition;

        Vector3 surfaceUp = sphereTransform != null
            ? (transform.position - sphereTransform.position).normalized
            : Vector3.up;
        Vector3 forward = Vector3.ProjectOnPlane(direction, surfaceUp).normalized;
        if (forward.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(forward, surfaceUp);
    }

    private void StealAndHide(CatPlayer player)
    {
        player.DropCargo();
        if (gameSystem != null)
        {
            gameSystem.find_LostItem = false;
            if (gameSystem.findCatObject != null) gameSystem.findCatObject.SetActive(false);
        }

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideTemporarily());
    }

    private void ImmobilizeCat(CatPlayer player)
    {
        if (immobilizeCoroutine != null) StopCoroutine(immobilizeCoroutine);
        immobilizeCoroutine = StartCoroutine(ImmobilizeTemporarily(player));
    }

    private IEnumerator ImmobilizeTemporarily(CatPlayer player)
    {
        bool wasEnabled = player.enabled;
        player.enabled = false;
        yield return new WaitForSeconds(immobilizeDuration);
        if (player != null) player.enabled = wasEnabled;
        immobilizeCoroutine = null;
    }

    private IEnumerator HideTemporarily()
    {
        isHidden = true;
        searchLightWasEnabledBeforeHide = searchLight.enabled;
        searchLight.enabled = false;
        SetShadowVisible(false);
        yield return new WaitForSeconds(hideDuration);
        SetShadowVisible(true);
        isHidden = false;
        searchLight.enabled = searchLightWasEnabledBeforeHide && Time.time >= searchLightUnavailableUntil;
        hideCoroutine = null;
    }

    private void SetShadowVisible(bool visible)
    {
        foreach (Renderer item in hiddenRenderers)
            if (item != null) item.enabled = visible;
        foreach (Collider item in hiddenColliders)
            if (item != null) item.enabled = visible;
    }
}
