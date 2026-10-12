using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
public class TouchSystem : MonoBehaviour
{
    private GameSystem gameSystem;
    private CameraSystem cameraSystem;
    [SerializeField]
    int lightAngle;
    [SerializeField]
    float defaultLightAngle;

    public void Initialize(GameSystem owner, CameraSystem cameraController)
    {
        gameSystem = owner;
        cameraSystem = cameraController;
    }

    // 画面のタッチ位置を球面上の移動先へ変換し、必要な光判定を行う。
    public void ProcessTouch()
    {
        if (gameSystem == null) return;
        Camera camera = gameSystem.mainCamera;
        GameObject sphere = gameSystem.sphere;
        Transform target = gameSystem.targetObject != null ? gameSystem.targetObject.transform : null;
        if (camera == null || sphere == null || target == null) return;
        SphereCollider sphereCollider = sphere.GetComponent<SphereCollider>();
        if (sphereCollider == null) return;

        Vector2 screenPosition = Input.mousePosition;
        Transform lightTarget = gameSystem.LightTargetSystem != null ? gameSystem.LightTargetSystem.ProcessTouch(screenPosition) : null;
        Ray ray = camera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, sphere.transform.lossyScale.x * 5f).OrderBy(hit => hit.distance).ToArray();
        Vector3 direction = Vector3.zero;
        Vector3 surfacePosition = Vector3.zero;
        bool foundSurface = false;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Untagged")) continue;
            direction = (hit.point - sphere.transform.position).normalized;
            surfacePosition = sphere.transform.position + direction * sphereCollider.radius * GetMaxAbsScale(sphere.transform.lossyScale);
            foundSurface = true;
            break;
        }

        if (!foundSurface && Input.mousePosition.y > Screen.height * 0.7f)
        {
            direction = (camera.transform.position - sphere.transform.position).normalized;
            direction.z *= -1f;
            surfacePosition = sphere.transform.position + direction * sphereCollider.radius * GetMaxAbsScale(sphere.transform.lossyScale);
            foundSurface = true;
        }
        if (!foundSurface) return;

        CatPlayer playerCat = gameSystem.PlayerCat;
        if (playerCat != null && Vector3.Distance(surfacePosition, playerCat.transform.position) < playerCat.moveSpeed * 6f) return;
        if (lightTarget != null)
        {
            target.position = lightTarget.position;
            direction = (lightTarget.position - sphere.transform.position).normalized;
        }
        else target.position = surfacePosition;

        target.SetParent(sphere.transform);
        target.up = direction;
        cameraSystem?.RotateSphereToFaceTarget(sphere.transform, target, sphereCollider.radius, gameSystem.transform);

        StartCoroutine (LightTargetProcess());
    }

    private float GetMaxAbsScale(Vector3 scale)
    {
        return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
    }
    IEnumerator LightTargetProcess()
    {
        Debug.Log("lightAngle is " + lightAngle);
        for(int i = 0; i < lightAngle; i++)
        {
            Debug.Log("lightAngle is " + i);
            gameSystem.LightTargetSystem.sceneSpotLight.spotAngle = defaultLightAngle + (float)i;
            gameSystem.LightTargetSystem.sceneSpotLight.innerSpotAngle = (float)i / 2;
            
            yield return null;
        }
    }
}
