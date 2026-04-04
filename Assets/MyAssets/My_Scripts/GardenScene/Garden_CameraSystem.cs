using UnityEngine;
using DG.Tweening;
public class Garden_CameraSystem : MonoBehaviour
{
    public static Garden_CameraSystem Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Camera _mainCamera, _subCamera;
    Vector3 mainCameraPosition;
    Quaternion mainCameraRotation;
    bool pushed,mainbool;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        mainCameraPosition = _mainCamera.transform.position;
        mainCameraRotation = _mainCamera.transform.rotation;
    }

    // Update is called once per frame

    void Update()
    {
        if (pushed)
        {
            CameraPositionChange(mainbool);
        }
    }


    public void CameraPositionChange(bool main)
    {
        if (main)
        {
            _mainCamera.transform.DOMove(_subCamera.transform.position, 0.5f).SetEase(Ease.OutQuad);
            _mainCamera.transform.DORotate(_subCamera.transform.rotation.eulerAngles, 0.5f).SetEase(Ease.OutQuad);
            Debug.Log($"Camera position changed to sub camera position: {_subCamera.transform.position}");
        }
        else
        {
            _mainCamera.transform.DOMove(mainCameraPosition, 0.5f).SetEase(Ease.OutQuad);
            _mainCamera.transform.DORotate(mainCameraRotation.eulerAngles, 0.5f).SetEase(Ease.OutQuad);
            Debug.Log($"Camera position changed to main camera position: {mainCameraPosition}");
        }
    }

    public void Button_cameraPositionChange(bool main)
    {
        pushed = main;
        mainbool = main;
    }
}
