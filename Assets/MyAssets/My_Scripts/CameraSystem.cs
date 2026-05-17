using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class CameraSystem : MonoBehaviour
{
    public Camera mainCamera;
    public Camera subCamera;

[SerializeField]
    Canvas canvas;
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Example: Rotate the camera around the Y-axis
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            mainCamera.transform.Rotate(Vector3.up, -20 * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            mainCamera.transform.Rotate(Vector3.up, 20 * Time.deltaTime);
        }
    }
    public void SwitchCamera(bool enableSubCamera)
    {
        mainCamera.enabled = !enableSubCamera;
        subCamera.enabled = enableSubCamera;

        mainCamera.tag = enableSubCamera ? subCamera.tag : mainCamera.tag;
        subCamera.tag = enableSubCamera ? mainCamera.tag : subCamera.tag;
        canvas.worldCamera = enableSubCamera ? subCamera : mainCamera;
    }
}
