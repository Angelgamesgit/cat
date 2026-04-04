// ファイル名: Garden_CatSystem.cs
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class Garden_CatSystem : CatSystem
{
    [SerializeField]
    private Garden_EditorManager editorManager;

   
    private Transform objectParent;
    private Camera _mainCamera;

    public override void Start()
    {
        _mainCamera = Camera.main;
        otherHeight = new Dictionary<Collider, float>();
        jumpDifference = 0f;
        distance = 0f;
        mindistance = 0.05f;
        rundistance = mindistance * 5f;
        moveSpeed = 1f; // Time.deltaTimeを乗算するため、値を調整
        if (targetObject != null)
        {
            targetObject.position = transform.position;
        }
        if (editorManager != null)
        {
            objectParent = editorManager.objectParent;
        }
        obstacleTag = "Obstacle";
    }

    public override void AssignVariables()
    {
        // このクラスでは独自の初期化を行うため、親のメソッドは呼び出さない
    }

    public override void Update()
    {
        if (editorManager == null) return;

        catObject.gameObject.SetActive(ViewModeCheck());
        if (!ViewModeCheck()) return;
       
        MoveCat();
        Touch_TargetMove();
    }

    bool ViewModeCheck()
    {
        return editorManager.currentState == Garden_EditorManager.EditorState.Viewing;
    }

    public void Touch_TargetMove()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // UI上をタッチしている場合は何もしない
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            Ray ray = _mainCamera.ScreenPointToRay(touch.position);
            
            // RaycastAllで全てのヒットを取得し、objectParentを優先する
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) // 近い順にソート
            {
                if (hit.collider.gameObject == objectParent.gameObject)
                {
                    targetObject.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                    return; // ターゲットを更新したらループを抜ける
                }
            }
        }
    }

    // ▼▼▼ [全面的な修正] MoveCatメソッドを新しい親クラスの仕様に対応 ▼▼▼
    private void MoveCat()
    {
        // --- 1. 変数の初期化 ---
        float currentSpeed = 0f;
        AnimState nextState = AnimState.idle;
        bool isJumpingOrGettingOff = false;

        // --- 2. ジャンプ/着地状態の判定（最優先） ---
        float jumpMoveAmount = moveSpeed * 3f * Time.deltaTime; // 高さが変わる速度
        float targetHeight = (otherHeight != null && otherHeight.Count > 0) ? otherHeight.Values.Max() : 0f;
        
        if (Mathf.Abs(jumpDifference - targetHeight) > 0.01f)
        {
            jumpDifference = Mathf.MoveTowards(jumpDifference, targetHeight, jumpMoveAmount);
            nextState = (jumpDifference < targetHeight) ? AnimState.jump : AnimState.getoff;
            isJumpingOrGettingOff = true;
        }
        
        // --- 3. 距離の計算と移動/アイドル状態の判定 ---
        Vector3 flatTargetPosition = new Vector3(targetObject.position.x, transform.position.y, targetObject.position.z);
        distance = Vector3.Distance(transform.position, flatTargetPosition);

        if (distance < mindistance)
        {
            // ジャンプ中でなければアイドル状態
            if (!isJumpingOrGettingOff) nextState = AnimState.idle;
            currentSpeed = 0f;
        }
        else if (!isJumpingOrGettingOff)
        {
            // ▼▼▼【修正②】▼▼▼
            // 親クラスのメソッド名が CalculateSpeedRatio に変更されたため、呼び出しを修正
            float speedRatio = CalculateSpeedRatio(distance);
            currentSpeed = moveSpeed * speedRatio;
            nextState = (speedRatio > 0.9f) ? AnimState.run : AnimState.walk;
        }

        // --- 4. 猫の向きと位置を更新 ---
        transform.position = new Vector3(transform.position.x, objectParent.position.y + jumpDifference, transform.position.z);

        if (nextState != AnimState.idle)
        {
            Quaternion targetRotation = Quaternion.LookRotation(flatTargetPosition - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            
            // ▼▼▼【修正③】▼▼▼
            // Time.deltaTimeを乗算し、フレームレートに依存しない移動速度に修正
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
        }

        // ▼▼▼【修正④】▼▼▼
        // 親クラスのUpdateAnimationStateは互換性がなくなったため、このクラス内でアニメーションを直接制御します。
        float finalSpeedRatio = (nextState == AnimState.idle || isJumpingOrGettingOff) ? 0f : currentSpeed / moveSpeed;
        animator.SetFloat(speedHash, finalSpeedRatio);
        
        if (currentState != nextState)
        {
            if (nextState == AnimState.jump) animator.SetTrigger(jumpHash);
            else if (nextState == AnimState.getoff) animator.SetTrigger(getoffHash);
        }
        currentState = nextState;
    }
}