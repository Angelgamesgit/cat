// ファイル名: CatSystem.cs
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using Unity.VisualScripting;
using DG.Tweening.Core;
using System.Collections;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody))]
public class CatSystem : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Transform catObject;
    public GameSystem system;
    public Transform targetObject;
    public Transform sphereObject;
    [SerializeField] public Animator animator;
    [SerializeField] GameObject seaBoat;

    [Header("移動パラメータ")]
    [Tooltip("1フレームあたりの基本移動量")]
    public float moveSpeed = 0.03f;
    [Tooltip("この距離まで近づくと停止する")]
    public float mindistance = 1.0f;
    [Tooltip("この距離以上離れると最高速で走る")]
    public float rundistance = 3.0f;

    [Header("スムーズ化パラメータ")]
    [Tooltip("高さの変化を滑らかにする係数（0〜1）")]
    [Range(0, 1)] public float heightLerpFactor = 0.1f;
    [Tooltip("向きの変化を滑らかにする係数（0〜1）")]
    [Range(0, 1)] public float rotationSlerpFactor = 0.15f;

    [Header("状態確認（デバッグ用）")]
    public float distance;
    public float jumpDifference;
    public AnimState currentState;

    // --- 内部変数 ---
    private float sphereRadius;
    private bool touchCat;
    private SphereCollider seaCollider;
    private bool isInSea;
    private float seaHeight = 0f;
    public Dictionary<Collider, float> otherHeight;
    public string obstacleTag = "Obstacle";

    // 足音関連
    private AnimState soundState;
    private float stepTimer;
    private float walkStepInterval = 0.4f;
    private float runStepInterval = 0.35f;

    // アニメーターハッシュ
    protected readonly int speedHash = Animator.StringToHash("Speed");
    protected readonly int jumpHash = Animator.StringToHash("jump");
    protected readonly int getoffHash = Animator.StringToHash("getoff");

    

    //猫のアニメーション状態
    public enum AnimState
    {
        idle,
        walk,
        run,
        jump,
        getoff
    }

    public virtual void Start()
    {
        // Startメソッドは初期化の呼び出しに専念
        AssignVariables();
    }

    //ゲームの開始時と海などのスフィアが変わった時に呼ばれる
    public virtual void AssignVariables()
    {
        sphereRadius = sphereObject.lossyScale.x / 2f;
        jumpDifference = 0;
        otherHeight = new Dictionary<Collider, float>();
        currentState = AnimState.idle;
        isInSea = false;
        if (seaBoat != null) seaBoat.SetActive(false);

        otherHeight = new Dictionary<Collider, float>();
        jumpDifference = 0f;
        distance = 0f;
        mindistance = 0.2f;
        rundistance = mindistance * 5f;
        moveSpeed = 0.2f;
        obstacleTag = "Obstacle"; 
    }

    public virtual void Update()
    {
        if (!system.GamePlaying) return;

        // メインの更新処理を呼び出し
        CatBehaviorUpdate();
        // 足音のタイミングを管理
        CatFootStepTime();
        //キッチンの近くかどうか取得する
        isNearKitchen();
    }

    /// <summary>
    /// 猫の全ての挙動（移動、回転、高さ、アニメーション）を管理するメイン関数
    /// </summary>
    private void CatBehaviorUpdate()
    {
        if (targetObject == null || sphereObject == null) return;

        // ▼ STEP 1: 現状と目標値を計算する ▼
        distance = Vector3.Distance(transform.position, targetObject.position);
        float targetHeight = CalculateTargetHeight();
        float speedRatio = CalculateSpeedRatio(distance);
        float currentTargetSpeed = moveSpeed * speedRatio;

        // ▼ STEP 2: 高さや向きを滑らかに補間する ▼
        jumpDifference = Mathf.Lerp(jumpDifference, targetHeight, heightLerpFactor);

        if (speedRatio > 0.01f) // ほぼ停止している時は回転しない
        {
            Vector3 selfToCenter = transform.position - sphereObject.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetObject.position - transform.position, selfToCenter.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSlerpFactor);
        }

        // ▼ STEP 3: 計算された値に基づいて最終的な位置を決定・適用する ▼
        // 1. 1フレーム分の移動量を計算
        Vector3 movement = transform.forward * currentTargetSpeed;

        // 2. 移動後の「仮の目標地点」を計算
        Vector3 desiredPosition = transform.position + movement;

        // 3. 仮の目標地点を球面に投影し、「地面上の正しい目標地点」を決定
        Vector3 positionOnSphere = sphereObject.position + (desiredPosition - sphereObject.position).normalized * sphereRadius;

        // 4. 現在の位置から地面上の正しい目標地点へ移動
        transform.position = positionOnSphere;

        // 5. 高さ（ジャンプなど）を最終的な位置に反映
        Vector3 finalUpVector = (transform.position - sphereObject.position).normalized;
        catObject.position = transform.position + finalUpVector * jumpDifference;

        // ▼ STEP 4: 最終的な状態に基づいてアニメーションを更新する ▼
        UpdateAnimationState(targetHeight, speedRatio);
    }

    /// <summary>
    /// 障害物と海の高さを考慮して、現在のフレームで目指すべき高さを計算する
    /// </summary>
    private float CalculateTargetHeight()
    {
        // 障害物による高さを計算
        float obstacleHeight = 0f;
        if (otherHeight != null && otherHeight.Count > 0)
        {
            obstacleHeight = otherHeight.Values.Max();
        }

        // 海による高さを計算
        UpdateSeaState();

        // 障害物と海の高さを比較し、より高い方を目標値にする
        return Mathf.Max(obstacleHeight, seaHeight);
    }

    /// <summary>
    /// ターゲットとの距離に基づいて、移動速度の比率（0.0～1.0）を計算する
    /// </summary>
    public float CalculateSpeedRatio(float dist)
    {
       // 走行距離より遠ければ最高速
        if (dist >= rundistance) return 1.0f;

        // mindistanceとrundistanceの間を線形補間
        return Mathf.InverseLerp(mindistance, rundistance, dist);
    }

    /// <summary>
    /// 現在の状態に基づいて正しいアニメーションを再生する
    /// </summary>
    private void UpdateAnimationState(float targetHeight, float speedRatio)
    {
        AnimState nextState;

        // 高さに大きな変化があるか（ジャンプ/着地）
        bool isJumping = targetHeight > jumpDifference && (targetHeight - jumpDifference) > 0.05f;
        bool isGettingOff = jumpDifference > targetHeight && (jumpDifference - targetHeight) > 0.05f;

        // 状態の優先順位を決定
        if (isInSea) // 最優先：海の上にいる場合
        {
            nextState = AnimState.idle;
            speedRatio = 0f; // ボートに乗っているのでアニメーション上の速度は0
        }
        else if (isJumping)
        {
            nextState = AnimState.jump;
        }
        else if (isGettingOff)
        {
            nextState = AnimState.getoff;
        }
        else if (speedRatio > 0.9f) // 速度で判定
        {
            nextState = AnimState.run;
        }
        else if (speedRatio > 0.01f)
        {
            nextState = AnimState.walk;
        }
        else
        {
            nextState = AnimState.idle;
        }

        // Animatorのパラメータを更新
        animator.SetFloat(speedHash, speedRatio);

        // Stateが切り替わった瞬間にTriggerを発火
        if (currentState != nextState)
        {
            if (nextState == AnimState.jump)
            {
                animator.SetTrigger(jumpHash);
            }
            else if (nextState == AnimState.getoff)
            {
                animator.SetTrigger(getoffHash);
            }
        }
        currentState = nextState;
    }

    /// <summary>
    /// 海コライダーの内部にいるか判定し、海面の高さを更新する
    /// </summary>
    void UpdateSeaState()
    {
        if (seaCollider == null)
        {
            seaHeight = 0f; // 海のトリガーから抜けている場合は高さを0に
            return;
        }

        Transform seaTransform = seaCollider.transform;
        float distToSeaCenter = Vector3.Distance(transform.position, seaTransform.position);
        float seaRadius = seaCollider.radius * Mathf.Max(seaTransform.lossyScale.x, seaTransform.lossyScale.y, seaTransform.lossyScale.z);

        if (distToSeaCenter < seaRadius)
        {
            // 球体である海面までの高さを計算
            seaHeight = seaRadius - distToSeaCenter - (transform.lossyScale.y / 5f);
        }
        else
        {
            seaHeight = 0f;
        }
    }

    // --- 以下、既存のロジック（FindforFriends, 足音, Trigger関連） ---
    // これらのロジックは移動のスムーズさに直接影響しないため、大きな変更は加えていません。

    public bool FindforFriends()
    {
        if (system.findCatObject == null) return false;
        Vector3 targetDir = system.findCatObject.transform.position - transform.position;
        float targetDistance = targetDir.magnitude;
        float sightAngle = 60f;
        float cosHalf = Mathf.Cos(sightAngle / 2 * Mathf.Deg2Rad);
        float innerProduct = Vector3.Dot(transform.forward, targetDir.normalized);
        if (touchCat) return touchCat;
        return innerProduct > cosHalf && targetDistance < mindistance * 1.5f;
    }

    #region 足音の再生
    IEnumerator PlayFootstepSound(AnimState state)
    {
        float groundCheckDistance = 5f + jumpDifference;
        Vector3 rayDown = (sphereObject.position - transform.position).normalized;

        if (Physics.Raycast(transform.position + (Vector3.up * 0.5f), rayDown, out RaycastHit hit, groundCheckDistance))
        {
            if (hit.collider.TryGetComponent<GroundsoundType>(out GroundsoundType groundsound))
            {
                SoundManager.Instance.PlayFootstep(transform.position, groundsound.type, state);
                yield return null;
                if (state != AnimState.run) yield break;
                SoundManager.Instance.PlayFootstep(transform.position, groundsound.type, state);
            }
            else
            {
                SoundManager.Instance.PlayFootstep(transform.position, SurfaceType.Default, state);
                yield return null;
                if (state != AnimState.run) yield break;
                SoundManager.Instance.PlayFootstep(transform.position, SurfaceType.Default, state);
            }
        }
    }
    #endregion

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sea"))
        {
            isInSea = true;
            if (seaBoat != null) seaBoat.SetActive(true);
            seaCollider = other.GetComponent<SphereCollider>();
        }

        if (other.CompareTag(obstacleTag))
        {
            if (other is BoxCollider boxColl)
            {
                float jumpHeight = boxColl.size.y * 0.7f * other.transform.lossyScale.y;
                if (!otherHeight.ContainsKey(other))
                {
                    otherHeight.Add(other, jumpHeight);
                }
            }
        }
        else if (other.CompareTag("Cat"))
        {
            touchCat = true;
        }
    }

   

    public void OnTriggerExit(Collider other)
    {
        if (otherHeight.ContainsKey(other))
        {
            otherHeight.Remove(other);
        }

        if (other.CompareTag("sea"))
        {
            if (seaBoat != null) seaBoat.SetActive(false);
            isInSea = false;
            seaCollider = null;
            seaHeight = 0f; // トリガーから出たら必ず高さをリセット
        }
    }

    void CatFootStepTime()
    {
        float currentInterval = 1;

        if (soundState != currentState)
        {
            soundState = currentState;
            stepTimer = 0;
        }

        stepTimer += Time.deltaTime; // Note: Sound timing can remain framerate-independent

        switch (soundState)
        {
            case AnimState.run:
                currentInterval = runStepInterval;
                break;
            case AnimState.walk:
                currentInterval = walkStepInterval;
                break;
            default:
                stepTimer = 0;
                break;
        }

        if (stepTimer > currentInterval)
        {
            StartCoroutine(PlayFootstepSound(currentState));
            stepTimer = 0f;
        }
    }

    public void CatAnimationMove(AnimState state)
    {
        if (currentState == state) { return; }
        currentState = state;
        animator.SetTrigger(state.ToString());
    }
    
    void isNearKitchen()
    {
        if (system.kitchenObject == null) return;
        float kitchenDistance = Vector3.Distance(transform.position, system.kitchenObject.transform.position);
        if (kitchenDistance <  10f)
        {
          FindObjectsByType<UISystem>(FindObjectsSortMode.None).First().KitichenButton.SetActive(true);
        }
        //
        else
        {
           FindObjectsByType<UISystem>(FindObjectsSortMode.None).First().KitichenButton.SetActive(false);
        }
    }
}