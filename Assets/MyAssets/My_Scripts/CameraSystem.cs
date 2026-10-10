using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class CameraSystem : MonoBehaviour
{
    public Camera mainCamera;
    public Camera subCamera;
    public Transform LightTransform;
    [SerializeField] private Canvas canvas;
    [Header("Camera Position")]
    [SerializeField] private float cameraFrontDistance = 2f;
    [SerializeField] private float cameraFrontHeight = 1f;
    [Header("Obstruction Fade")]
    [SerializeField] private Material replacementMaterial;
    [SerializeField] private LayerMask obstructionMask = -1;
    private Transform targetObject;
    private Transform sphereTransform;
    private CatPlayer playerCat;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        CaptureInitialPose();
    }

    public void SetWorldReferences(Camera camera, Transform target, Transform sphere, CatPlayer cat, Material fadeMaterial)
    {
        if (camera != null) mainCamera = camera;
        targetObject = target;
        sphereTransform = sphere;
        playerCat = cat;
        if (fadeMaterial != null) replacementMaterial = fadeMaterial;
        CaptureInitialPose();
    }

    private void Update()
    {
        if (mainCamera == null) return;
        if (Input.GetKey(KeyCode.LeftArrow)) mainCamera.transform.Rotate(Vector3.up, -20f * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow)) mainCamera.transform.Rotate(Vector3.up, 20f * Time.deltaTime);
    }

    public void ChangeCameraPosition(bool useOverheadPosition, float duration, CatPlayer cat, Camera camera)
    {
        if (camera != null) mainCamera = camera;
        if (mainCamera == null) return;
        if (useOverheadPosition)
        {
            mainCamera.transform.DOMove(initialCameraPosition, duration);
            mainCamera.transform.DORotate(initialCameraRotation.eulerAngles, duration);
            return;
        }
        if (cat == null) return;
        Vector3 frontPosition = cat.transform.position - cat.transform.forward * cameraFrontDistance + cat.transform.up * cameraFrontHeight;
        Vector3 directionToCat = (cat.transform.position - frontPosition).normalized;
        Quaternion frontRotation = Quaternion.LookRotation(directionToCat, cat.transform.up);
        mainCamera.transform.DOMove(frontPosition, duration);
        mainCamera.transform.DORotate(frontRotation.eulerAngles, duration);
    }

    public void RotateSphereToFaceTarget(Transform sphere, Transform target, float sphereRadius)
    {
        if (sphere == null || target == null || playerCat == null) return;
        Vector3 currentDirection = (target.position - sphere.position).normalized;
        Vector3 goalDirection = (playerCat.transform.up * sphereRadius - sphere.position).normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(currentDirection, goalDirection) * sphere.rotation;
        sphere.DOKill();
        float duration = Mathf.Clamp(Quaternion.Angle(sphere.rotation, targetRotation) / 180f, 2f, 5f);
        sphere.DORotateQuaternion(targetRotation, duration).SetEase(Ease.OutCubic).OnComplete(() => sphere.rotation = targetRotation);
    }

    public void RefreshObstructions()
    {
        if (mainCamera == null || targetObject == null || replacementMaterial == null) return;
        var obstructed = new HashSet<Renderer>();
        Vector3 cameraPosition = mainCamera.transform.position;
        AddRayObstructions(cameraPosition, targetObject.position, obstructed);
        if (playerCat != null) AddRayObstructions(cameraPosition, playerCat.transform.position, obstructed);
        foreach (Collider hit in Physics.OverlapSphere(cameraPosition, 0.5f, obstructionMask)) AddRenderers(hit.gameObject, obstructed);
        var restore = new List<Renderer>();
        foreach (Renderer renderer in originalMaterials.Keys)
            if (renderer == null || !obstructed.Contains(renderer)) restore.Add(renderer);
        foreach (Renderer renderer in restore)
        {
            if (renderer != null) renderer.materials = originalMaterials[renderer];
            originalMaterials.Remove(renderer);
        }
        foreach (Renderer renderer in obstructed)
        {
            if (renderer == null || originalMaterials.ContainsKey(renderer)) continue;
            originalMaterials.Add(renderer, renderer.materials);
            Material[] faded = new Material[renderer.materials.Length];
            for (int i = 0; i < faded.Length; i++) faded[i] = replacementMaterial;
            renderer.materials = faded;
        }
    }

    private void AddRayObstructions(Vector3 origin, Vector3 destination, HashSet<Renderer> renderers)
    {
        Vector3 direction = destination - origin;
        foreach (RaycastHit hit in Physics.RaycastAll(origin, direction.normalized, direction.magnitude, obstructionMask))
            AddRenderers(hit.collider.gameObject, renderers);
    }

    private void AddRenderers(GameObject source, HashSet<Renderer> renderers)
    {
        if (source.transform.IsChildOf(mainCamera.transform) || source.transform == targetObject || (playerCat != null && source.transform == playerCat.transform)) return;
        foreach (Renderer renderer in source.GetComponentsInChildren<Renderer>()) renderers.Add(renderer);
    }

    private void CaptureInitialPose()
    {
        if (mainCamera == null) return;
        initialCameraPosition = mainCamera.transform.position;
        initialCameraRotation = mainCamera.transform.rotation;
    }

    public void SwitchCamera(bool enableSubCamera)
    {
        if (mainCamera == null || subCamera == null) return;
        mainCamera.enabled = !enableSubCamera;
        subCamera.enabled = enableSubCamera;
        mainCamera.tag = enableSubCamera ? subCamera.tag : mainCamera.tag;
        subCamera.tag = enableSubCamera ? mainCamera.tag : subCamera.tag;
        if (canvas != null) canvas.worldCamera = enableSubCamera ? subCamera : mainCamera;
    }
}
