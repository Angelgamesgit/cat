// === File: Garden_EditorManager.cs ===
// (プロジェクトフォルダ/Scripts/Managers/ などに作成してください)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System.Collections;
using UnityEngine.Playables;

public class Garden_EditorManager : MonoBehaviour
{
    public enum EditorState { Viewing, Editing_Moving, Editing_Rotating }

    [Header("現在の状態")]
    public EditorState currentState = EditorState.Viewing;

    [Header("コア設定")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask placeableObjectLayer;
    [SerializeField] private float gardenRadius = 10f;
    [SerializeField] private Transform gardenCenter;
    public Transform objectParent;
    [SerializeField] private float itemYOffsetFromGround = 0.05f;

    [Header("回転設定")]
    [SerializeField] private float rotationSensitivity = 0.5f;
    [SerializeField] private GameObject rotationIndicatorPrefab;
    private GameObject _currentRotationIndicator;

    [Header("アイテム管理")]
    [SerializeField] private List<Garden_ItemSO> allAvailableItems;

    [Header("UI要素 (Inspectorで設定)")]
    [SerializeField] private GameObject viewingModePanel;
    [SerializeField] private GameObject editingModePanel;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform itemButtonParentPanel;
    [SerializeField] private float uiPanelFadeDuration = 0.3f;
     Camera _mainCamera;
   
    private Garden_SaveLoadManager _saveLoadManager;
    private GameObject _selectedObject;
    private Vector3 _objectToDragStartOffset;
    private bool _isDraggingObject = false;
    private bool _isRotatingObjectCurrentFrame = false; // 現在のフレームで回転操作が行われたか

    private List<Garden_UndoAction> _undoHistory = new List<Garden_UndoAction>();
    private List<Garden_UndoAction> _redoHistory = new List<Garden_UndoAction>();
    private const int MAX_HISTORY_STEPS = 10;
    private Vector3 _preOperationPosition;
    private Quaternion _preOperationRotation;

    private Dictionary<Garden_ItemSO, int> _itemTotalPossession; // Key: Garden_ItemSO
    private Dictionary<Garden_ItemSO, int> _itemPlacedCount;     // Key: Garden_ItemSO

    private Vector2 _lastTouchPositionForRotation;

    #region  通知機能 変数
    public enum NotificationType
    {
        ItemRemoved,
        ItemAllRemoved,
        ItemUndo,
        Save,
        Error
    }
      [Header("通知プレハブ")]
    [Tooltip("通知UIのプレハブを指定")]
    public GameObject notificationPrefab;

    [Header("通知の親オブジェクト")]
    [Tooltip("通知UIを生成する階層の親（Panelなど）を指定")]
    public Transform notificationParent;

  
    // 表示中の通知を順番に管理するリスト
    private static List<NotificationInstance> activeNotifications = new List<NotificationInstance>();

    /// <summary>
    /// 通知オブジェクトにアタッチして、自身のタイプを保持させるためのインナークラス
    /// </summary>
    public class NotificationInstance : MonoBehaviour
    {
        public NotificationType Type;
    }

    

    #endregion

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null) { Debug.LogError("メインカメラが見つかりません。"); enabled = false; return; }
       
        _saveLoadManager = GetComponent<Garden_SaveLoadManager>();
        if (_saveLoadManager == null) { Debug.LogError("Garden_SaveLoadManagerが見つかりません。"); enabled = false; return; }

        if (objectParent == null) objectParent = this.transform;
        _saveLoadManager.objectParent = objectParent;
        _saveLoadManager.availableGardenItems = allAvailableItems;

        if (gardenCenter == null)
        {
            var go = new GameObject("DefaultGardenCenter_AutoGen");
            go.transform.position = Vector3.zero;
            gardenCenter = go.transform;
        }
        InitializeItemCountsAndTryLoad();
        SetEditorState(EditorState.Viewing, true);
    }

   

    void Update()
    {
        if (currentState == EditorState.Viewing) return;

        // UI操作中は3Dインタラクションをブロック (ドラッグ解除はしない)
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // if (_isDragging) { /* ドラッグ中にUIに触れたらどうするか？何もしないのが一般的 */ }
            return;
        }

        bool pointerOverUI = false;
        if (Input.touchCount > 0)
            pointerOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        else pointerOverUI = EventSystem.current.IsPointerOverGameObject();
        Debug.Log($"Pointer over UI: {pointerOverUI}");
        if (pointerOverUI)
        {
            // UI操作中は、進行中の3D操作を中断することが望ましい場合がある
            // if (_isDraggingObject || _isRotatingObjectCurrentFrame) HandleTouchEnded(true); // 操作を強制終了
            return;
        }

        bool undoPressed = Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand));
        bool redoPressed = Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand)) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        if (redoPressed) ExecuteRedo();
        else if (undoPressed) ExecuteUndo();

        if (Input.GetKeyDown(KeyCode.Delete) && _selectedObject != null &&
            (currentState == EditorState.Editing_Moving || currentState == EditorState.Editing_Rotating))
        {
            RemoveSelectedObject();
        }
        HandleTouchInput();
    }

    #region アイテム管理とUI
    void InitializeItemCountsAndTryLoad()
    {
        _itemTotalPossession = new Dictionary<Garden_ItemSO, int>();
        _itemPlacedCount = new Dictionary<Garden_ItemSO, int>();

        foreach (Garden_ItemSO itemSO in allAvailableItems)
        {
            if (itemSO == null || string.IsNullOrEmpty(itemSO.name)) // itemSO.name を使用
            {
                Debug.LogWarning("allAvailableItemsリストに無効なGarden_ItemSO（名前が空など）が含まれています。");
                continue;
            }
            itemSO.Load();
            _itemTotalPossession.Add(itemSO, itemSO.initialTotalPossession);
            _itemPlacedCount[itemSO] = 0;
        }

        List<Garden_ObjectSaveData> loadedData = _saveLoadManager.Load();
        if (loadedData != null)
        {
            ApplyLoadedData(loadedData);
        }
        SetupItemUIButtons();
    }

    void ApplyLoadedData( List<Garden_ObjectSaveData>  dataToApply)
    {
      
        // 配置数をリセットしてから、ロードされたオブジェクトに基づいて再カウント
        foreach (var key in _itemPlacedCount.Keys.ToList()) _itemPlacedCount[key] = 0;

        if (dataToApply != null)
        {
            foreach (Garden_ObjectSaveData objData in dataToApply)
            {
                try
                {
                    if (string.IsNullOrEmpty(objData.ItemSO.name)) continue;
                    RestoreSingleObject(objData, false); // オブジェクト復元
                    if (_itemPlacedCount.ContainsKey(objData.ItemSO))
                    {
                        _itemPlacedCount[objData.ItemSO]++;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"オブジェクトの復元中にエラーが発生しました: {ex.Message}");
                    StartNotification(NotificationType.Error);
                }
            }
        }
    }

    void SetupItemUIButtons()
    {
        if (itemButtonParentPanel == null || itemButtonPrefab == null || allAvailableItems == null)
        { 
            Debug.LogWarning("UIボタンの設定に必要な要素が不足しています。");
            return;
        }
        
        for (int i = itemButtonParentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(itemButtonParentPanel.GetChild(i).gameObject);
        }

        foreach (Garden_ItemSO itemSO in allAvailableItems)
        {
            if (itemSO == null || string.IsNullOrEmpty(itemSO.name))
            {
                Debug.LogWarning("allAvailableItemsリストに無効なGarden_ItemSO（名前が空など）が含まれています。");
                continue;
            }
            

            int currentTotal = _itemTotalPossession.ContainsKey(itemSO) ? _itemTotalPossession[itemSO] : 0;
            int placed = _itemPlacedCount.ContainsKey(itemSO) ? _itemPlacedCount[itemSO] : 0;

            if (currentTotal <= 0)
            {
                Debug.LogWarning($"アイテム '{itemSO.displayName}' の総所持数が0です。UIボタンは作成されません。" + "itemSO.initialTotalPossession" + itemSO.initialTotalPossession);
                continue; // 総所持数が0ならボタンは作らない
            }
            GameObject btnGo = Instantiate(itemButtonPrefab, itemButtonParentPanel);
            Garden_ItemButton itemButtonScript = btnGo.GetComponent<Garden_ItemButton>();

            if (itemButtonScript != null)
            {
                // remainingToPlace は (currentTotal - placed)
                // isPlacedInGarden は (placed > 0)
                itemButtonScript.Setup(itemSO, currentTotal - placed, placed > 0);
                itemButtonScript.SetClickAction(GenerateAndSelectObject);
            }
            else { Destroy(btnGo); }
        }
    }

    void UpdateItemCountsOnPlacement(Garden_ItemSO item, bool isPlacement) // itemName を使用
    {
        if (!_itemTotalPossession.ContainsKey(item)) return;

        if (!_itemPlacedCount.ContainsKey(item)) _itemPlacedCount[item] = 0; // 配置数がない場合は初期化

        if (isPlacement)
        {
            _itemPlacedCount[item]++;
        }
        else
        {
            if (_itemPlacedCount[item] > 0)
            {
                _itemPlacedCount[item]--;
            }
        }
        SetupItemUIButtons();
    }
    #endregion

    #region モード管理
    public void SetEditorState(EditorState newState, bool forceUpdate = false)
    {
        if (!forceUpdate && currentState == newState) return;

        EditorState previousState = currentState;
        currentState = newState;
        Debug.Log($"Editor State changed from: {previousState} to: {currentState}");

        if (previousState == EditorState.Editing_Rotating) ShowRotationIndicator(false);

        //モード変更時にカメラの位置、角度を変更する
     
            // 鑑賞モードへ移行時はカメラを元の位置に戻す
         Garden_CameraSystem.Instance.CameraPositionChange(newState != EditorState.Viewing);
       
        // モード遷移時に、進行中の操作を適切に完了させるか、選択を解除する
        if (_selectedObject != null)
        {
            if (newState == EditorState.Viewing)
            { // 鑑賞モードへ移行時は必ず配置完了
                PlaceAndDeselectCurrentObject(true); // アンドゥ記録も行う
            }
            else if (previousState == EditorState.Editing_Rotating && newState == EditorState.Editing_Moving)
            {
                // 回転完了からの移動モード。選択は維持。回転インジケータは上で非表示済み。
                // 回転操作のアンドゥはTouchEndedで記録済みのはず。
            }
            else if (previousState == EditorState.Editing_Moving && newState == EditorState.Editing_Moving)
            {
                // アイテム選択などで同じ移動モードが再度セットされた場合。
                // この場合は、GenerateAndSelectObject内で PlaceAndDeselectCurrentObject が呼ばれるので、ここでは何もしない。
            }
            else if (previousState != EditorState.Viewing && currentState != EditorState.Viewing && _selectedObject != null && !_isDraggingObject && !_isRotatingObjectCurrentFrame)
            {
                // 編集系モード間の遷移で、現在ドラッグや回転中でないなら、選択を維持。
                // ただし、GenerateAndSelectObjectなど操作を伴う場合はそちらで制御。
            }
        }


        if (viewingModePanel != null) AnimatePanel(viewingModePanel, newState == EditorState.Viewing);
        if (editingModePanel != null) AnimatePanel(editingModePanel, newState != EditorState.Viewing);

        if (newState == EditorState.Editing_Moving || newState == EditorState.Editing_Rotating)
        {
            SetupItemUIButtons();
        }
        if (newState == EditorState.Editing_Rotating && _selectedObject != null)
        {
            ShowRotationIndicator(true, _selectedObject);
            // _isRotatingObjectCurrentFrame = false; // 回転操作はスワイプが始まってからtrue
        }
        else
        {
           
        }
    }

    private void AnimatePanel(GameObject panel, bool show)
    {
        CanvasGroup panelGroup = panel.GetComponent<CanvasGroup>();
        if (panelGroup == null)
        {
            panel.AddComponent<CanvasGroup>();
        }
        panelGroup.DOKill();
        if (show)
        {
            panelGroup.gameObject.SetActive(true);
            panelGroup.DOFade(1f, uiPanelFadeDuration).SetEase(Ease.OutQuad);
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }
        else
        {
            panelGroup.DOFade(0f, uiPanelFadeDuration).SetEase(Ease.InQuad)
                .OnComplete(() => { if (panelGroup != null) panelGroup.gameObject.SetActive(false); });
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }
    }

    public void ToggleEditMode()
    {
        SetEditorState(currentState == EditorState.Viewing ? EditorState.Editing_Moving : EditorState.Viewing);
    }
    #endregion

    #region オブジェクト生成とインタラクション
    public void GenerateAndSelectObject(Garden_ItemSO itemSO)
    {
        if (itemSO == null || string.IsNullOrEmpty(itemSO.name)) return; // itemSO.name

        if (currentState == EditorState.Viewing) SetEditorState(EditorState.Editing_Moving, true);
        else if (currentState == EditorState.Editing_Rotating) SetEditorState(EditorState.Editing_Moving, true);
        if (currentState != EditorState.Editing_Moving) return;

        int currentTotal = _itemTotalPossession.ContainsKey(itemSO) ? _itemTotalPossession[itemSO] : 0;
        int placed = _itemPlacedCount.ContainsKey(itemSO) ? _itemPlacedCount[itemSO] : 0;
        if ((currentTotal - placed) <= 0)
        {
            Debug.LogWarning($"アイテム '{itemSO.displayName}' はこれ以上配置できません。");
            return;
        }

        if (_selectedObject != null) PlaceAndDeselectCurrentObject(true); // 既存の選択物を配置完了

        Vector3 cameraCenterScreenPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray rayToGround = _mainCamera.ScreenPointToRay(cameraCenterScreenPos);
        RaycastHit hit;

        if (Physics.Raycast(rayToGround, out hit, 200f, groundLayer) ||
            Physics.Raycast(_mainCamera.transform.position + _mainCamera.transform.forward * 5f, Vector3.down, out hit, 200f, groundLayer))
        {
            Vector3 spawnPos = hit.point + hit.normal * itemYOffsetFromGround;
            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

            Vector3 currentGardenCenter = gardenCenter.position;
            Vector3 spawnPosXZ = new Vector3(spawnPos.x, 0, spawnPos.z);
            Vector3 gardenCenterXZ = new Vector3(currentGardenCenter.x, 0, currentGardenCenter.z);

            if (Vector3.Distance(spawnPosXZ, gardenCenterXZ) > gardenRadius)
            {
                Vector3 dirFromCenter = (spawnPosXZ - gardenCenterXZ).normalized;
                spawnPosXZ = gardenCenterXZ + dirFromCenter * gardenRadius;
                spawnPos.x = spawnPosXZ.x;
                spawnPos.z = spawnPosXZ.z;
                RaycastHit groundSnapHit;
                if (Physics.Raycast(new Ray(new Vector3(spawnPos.x, spawnPos.y + 5f, spawnPos.z), Vector3.down), out groundSnapHit, 10f, groundLayer))
                {
                    spawnPos = groundSnapHit.point + groundSnapHit.normal * itemYOffsetFromGround;
                    spawnRot = Quaternion.FromToRotation(Vector3.up, groundSnapHit.normal);
                }
            }

            GameObject newObj = Instantiate(itemSO.prefab, spawnPos, spawnRot, objectParent);
            newObj.layer = LayerMask.NameToLayer("PlaceableObject");
            Garden_PlaceableObjectData data = newObj.AddComponent<Garden_PlaceableObjectData>();
            data.itemSO = itemSO; // itemSO.name を使用

            _selectedObject = newObj;
            HighlightObject(_selectedObject);
            _preOperationPosition = newObj.transform.position;
            _preOperationRotation = newObj.transform.rotation;

            UpdateItemCountsOnPlacement(itemSO, true);
            AddToActionHistory(new Garden_UndoAction(newObj, itemSO));
            Debug.Log($"アイテム '{itemSO.displayName}' をカメラ正面に生成し選択しました。");
        }
        else { Debug.LogWarning("カメラ正面にオブジェクトを生成できる地面が見つかりません。"); }
    }

    void HandleTouchInput()
    {
        // Reset frame-specific flags
        _isRotatingObjectCurrentFrame = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = _mainCamera.ScreenPointToRay(touch.position);
            switch (touch.phase)
            {
                case TouchPhase.Began: HandleTouchBegan(ray, touch.position); break;
                case TouchPhase.Moved: HandleTouchMoved(ray, touch.position); break;
                case TouchPhase.Ended: HandleTouchEnded(); break;
                case TouchPhase.Canceled: HandleTouchEnded(true); break;
            }
        }
        else
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0)) HandleTouchBegan(ray, Input.mousePosition);
            else if (Input.GetMouseButton(0)) HandleTouchMoved(ray, Input.mousePosition);
            else if (Input.GetMouseButtonUp(0)) HandleTouchEnded();
        }
    }

    void HandleTouchBegan(Ray ray, Vector2 touchPosition)
    {
        _isDraggingObject = false;
        // _isRotatingObjectCurrentFrame は Moved で設定
        _lastTouchPositionForRotation = touchPosition;

        RaycastHit hit;
        if (currentState == EditorState.Editing_Moving)
        {
            if (Physics.Raycast(ray, out hit, 200f, placeableObjectLayer))
            {
                if (_selectedObject != hit.collider.gameObject)
                {
                    if (_selectedObject != null) PlaceAndDeselectCurrentObject(true);
                    _selectedObject = hit.collider.gameObject;
                    HighlightObject(_selectedObject);
                }
                _preOperationPosition = _selectedObject.transform.position;
                _preOperationRotation = _selectedObject.transform.rotation;
                // オフセット計算はドラッグ開始時に行うため、ここでは不要かも
            }
            else
            {
                if (_selectedObject != null)
                {
                    _preOperationPosition = _selectedObject.transform.position; // 回転前の状態を保存
                    _preOperationRotation = _selectedObject.transform.rotation;
                    SetEditorState(EditorState.Editing_Rotating); // 回転モードに移行
                }
            }

        }
        else if (currentState == EditorState.Editing_Rotating)
        {
            // 回転モード中に再度タップした場合の処理 (回転を確定して移動モードに戻るなど)
            // 通常、回転はドラッグで行うので、タップ開始では特別な処理は不要かもしれない
            // もしタップで回転終了としたいなら、HandleTouchEnded で処理する
        }
    }

    void HandleTouchMoved(Ray ray, Vector2 currentTouchPosition)
    {
        if (currentState == EditorState.Editing_Moving && _selectedObject != null)
        {
            if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
            { // ボタン/タッチが押され続けているか確認
                if (!_isDraggingObject)
                { // 初めてのMovedでドラッグ開始と判定
                    // オフセット計算はここで行う
                    RaycastHit groundHit;
                    if (Physics.Raycast(ray, out groundHit, 200f, groundLayer))
                    {
                        Vector3 _dragStartMouseWorldPosOnGround = groundHit.point;
                        _objectToDragStartOffset = _selectedObject.transform.position - _dragStartMouseWorldPosOnGround;
                    }
                    else
                    { // オブジェクトの真下の地面でオフセット計算を試みる
                        if (Physics.Raycast(new Ray(_selectedObject.transform.position + Vector3.up * 0.5f, Vector3.down), out groundHit, 100f, groundLayer))
                        {
                            _objectToDragStartOffset = _selectedObject.transform.position - groundHit.point;
                        }
                        else { _objectToDragStartOffset = Vector3.zero; } // フォールバック
                    }
                    _objectToDragStartOffset.y = itemYOffsetFromGround; // Yオフセットは固定
                    _isDraggingObject = true;
                }
            }

            if (_isDraggingObject)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 200f, groundLayer))
                {
                    MoveObjectToGroundPoint(hit.point, hit.normal);
                }
            }
        }
        else if (currentState == EditorState.Editing_Rotating && _selectedObject != null)
        {
            if (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved))
            {
                _isRotatingObjectCurrentFrame = true; // このフレームで回転操作があった
                float deltaX = (currentTouchPosition.x - _lastTouchPositionForRotation.x);
                _selectedObject.transform.Rotate(Vector3.up, deltaX * rotationSensitivity, Space.World);
            }
        }
        _lastTouchPositionForRotation = currentTouchPosition;
    }

    void HandleTouchEnded(bool canceled = false)
    {
        if (currentState == EditorState.Editing_Moving && _isDraggingObject && _selectedObject != null)
        {
            // 移動完了
            if (_selectedObject.transform.position != _preOperationPosition ||
                _selectedObject.transform.rotation != _preOperationRotation)
            {
                AddToActionHistory(new Garden_UndoAction(_selectedObject, _preOperationPosition, _preOperationRotation));
            }
            _isDraggingObject = false; // ドラッグ終了
        }
        else if (currentState == EditorState.Editing_Rotating && _isRotatingObjectCurrentFrame && _selectedObject != null)
        {
            // 回転完了
            if (_selectedObject.transform.rotation != _preOperationRotation)
            {
                AddToActionHistory(new Garden_UndoAction(_selectedObject, _preOperationPosition, _preOperationRotation));
            }
            // 回転モード終了後、オブジェクトは選択されたまま移動モードに戻る
            // SetEditorState(EditorState.Editing_Moving); // ここでモード変更すると、UIボタンクリックなど他の操作と競合する可能性
            // 回転操作が終了したことを示すフラグだけリセットし、モードは維持
            _isRotatingObjectCurrentFrame = false;
            ShowRotationIndicator(false); // インジケータは必ず非表示
            // 再度地面タップで回転モードを抜けるか、別オブジェクト選択で抜ける
        }
        else if (currentState == EditorState.Editing_Rotating && !Input.GetMouseButton(0) && !_isRotatingObjectCurrentFrame)
        {
            // 回転モード中にタップアップしたが、スワイプ（回転操作）がなかった場合
            SetEditorState(EditorState.Editing_Moving); // 移動モード（選択状態）に戻る
        }

        // _isDraggingObject = false; // ここでリセットすると、回転直後の移動ができなくなる可能性がある
        // _isRotatingObjectCurrentFrame は各フレームのUpdateの最初でリセット
    }

    void MoveObjectToGroundPoint(Vector3 groundHitPoint, Vector3 groundNormal)
    {
        if (_selectedObject == null) return;

        // 保持したいY軸回転を取得 (preOperationRotationはオブジェクト選択時または回転完了時のもの)
        float targetGlobalYRotation = _preOperationRotation.eulerAngles.y;

        // 新しい位置を計算
        Vector3 targetPosition = groundHitPoint + _objectToDragStartOffset;
        targetPosition.y = groundHitPoint.y + itemYOffsetFromGround;

        // 範囲制限
        Vector3 currentGardenCenter = gardenCenter.position;
        Vector3 targetPosXZ = new Vector3(targetPosition.x, 0, targetPosition.z);
        Vector3 gardenCenterXZ = new Vector3(currentGardenCenter.x, 0, currentGardenCenter.z);
        if (Vector3.Distance(targetPosXZ, gardenCenterXZ) > gardenRadius)
        {
            Vector3 dirFromCenter = (targetPosXZ - gardenCenterXZ).normalized;
            targetPosXZ = gardenCenterXZ + dirFromCenter * gardenRadius;
            targetPosition.x = targetPosXZ.x;
            targetPosition.z = targetPosXZ.z;
        }

        // DOTweenでスムーズな移動
        _selectedObject.transform.DOMove(targetPosition, 0.05f).SetEase(Ease.OutSine);

        // よりロバストな方法：
        // 保持したい前方ベクトルを計算（Y回転のみを考慮）
        Vector3 originalForward = Quaternion.Euler(0, _preOperationRotation.eulerAngles.y, 0) * Vector3.forward;
        // 新しい地面の法線に対して、この前方ベクトルを維持するように回転を計算
        Quaternion finalRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(originalForward, groundNormal).normalized, groundNormal);
        _selectedObject.transform.DORotateQuaternion(finalRotation, 0.05f).SetEase(Ease.OutSine);
    }

    void PlaceAndDeselectCurrentObject(bool addToUndo)
    {
        if (_selectedObject != null)
        {
            if (addToUndo)
            {
                if (_selectedObject.transform.position != _preOperationPosition ||
                    _selectedObject.transform.rotation != _preOperationRotation)
                {
                    AddToActionHistory(new Garden_UndoAction(_selectedObject, _preOperationPosition, _preOperationRotation));
                }
            }
            Debug.Log($"オブジェクト '{_selectedObject.name}' を配置完了（選択解除）。");
            UnhighlightObject(_selectedObject);
            ShowRotationIndicator(false); // 回転インジケータも確実に非表示
            _selectedObject = null;
        }
        _isDraggingObject = false;
        _isRotatingObjectCurrentFrame = false;
    }
    #endregion

    #region オブジェクト撤去機能
    public void RemoveSelectedObject()
    {
        if (currentState == EditorState.Viewing || _selectedObject == null) return;

        Garden_PlaceableObjectData data = _selectedObject.GetComponent<Garden_PlaceableObjectData>();

        Garden_ObjectSaveData removedObjectData = new Garden_ObjectSaveData
        {
            ItemSO = data.itemSO, // itemName
            position = _selectedObject.transform.position,
            rotation = _selectedObject.transform.rotation,
            scale = _selectedObject.transform.localScale
        };
        AddToActionHistory(new Garden_UndoAction(removedObjectData));

        if (data.itemSO != null) UpdateItemCountsOnPlacement(data.itemSO, false); // itemName

        GameObject objToDestroy = _selectedObject;
        _selectedObject = null;
        _isDraggingObject = false;
        _isRotatingObjectCurrentFrame = false;
        ShowRotationIndicator(false);
        UnhighlightObject(objToDestroy);
        Destroy(objToDestroy);

        SetEditorState(EditorState.Editing_Moving);
        Debug.Log("選択されたオブジェクトを撤去しました。");
    }

    public void RemoveAllPlacedObjects()
    {
        if (objectParent == null || objectParent.childCount == 0) return;
        if (_selectedObject != null) PlaceAndDeselectCurrentObject(true);
        // SetEditorState(EditorState.Editing_Moving); // モード変更は不要な場合も

        List<Garden_ObjectSaveData> removedListForUndo = new List<Garden_ObjectSaveData>();
        List<Transform> childrenToRemove = new List<Transform>(objectParent.Cast<Transform>());

        foreach (Transform child in childrenToRemove)
        {
            Garden_PlaceableObjectData data = child.GetComponent<Garden_PlaceableObjectData>();
            if (data != null)
            {
                removedListForUndo.Add(new Garden_ObjectSaveData
                {
                    ItemSO = data.itemSO, // itemName
                    position = child.position,
                    rotation = child.rotation,
                    scale = child.localScale
                });
                UpdateItemCountsOnPlacement(data.itemSO, false); // itemName
            }
            Destroy(child.gameObject);
        }

        if (removedListForUndo.Count > 0) AddToActionHistory(new Garden_UndoAction(removedListForUndo));
        SetupItemUIButtons(); // 全削除後、UI更新は必須
        Debug.Log("全ての配置オブジェクトを撤去しました。");
        StartNotification(NotificationType.ItemAllRemoved);
    }
    #endregion

    #region アンドゥ/リドゥ機能
    void AddToActionHistory(Garden_UndoAction action)
    {
        if (_undoHistory.Count >= MAX_HISTORY_STEPS) _undoHistory.RemoveAt(0);
        _undoHistory.Add(action);
        _redoHistory.Clear();
        Debug.Log($"アンドゥ履歴に追加: {action.actionType}");
    }

    public void ExecuteUndo()
    {
        if (_undoHistory.Count == 0) return;
        Garden_UndoAction lastAction = _undoHistory[_undoHistory.Count - 1];
        _undoHistory.RemoveAt(_undoHistory.Count - 1);
        _redoHistory.Add(lastAction);

        if (_selectedObject != null) PlaceAndDeselectCurrentObject(false); // 現在の操作はアンドゥに含めない
        SetEditorState(EditorState.Editing_Moving);

        Debug.Log($"アンドゥ実行: {lastAction.actionType}");
        PerformUndoRedoAction(lastAction, true);
        SetupItemUIButtons();
        StartNotification(NotificationType.ItemUndo);
    }

    public void ExecuteRedo()
    {
        if (_redoHistory.Count == 0) return;
        Garden_UndoAction nextAction = _redoHistory[_redoHistory.Count - 1];
        _redoHistory.RemoveAt(_redoHistory.Count - 1);
        _undoHistory.Add(nextAction);

        if (_selectedObject != null) PlaceAndDeselectCurrentObject(false);
        SetEditorState(EditorState.Editing_Moving);

        Debug.Log($"リドゥ実行: {nextAction.actionType}");
        PerformUndoRedoAction(nextAction, false);
        SetupItemUIButtons();
    }

    void PerformUndoRedoAction(Garden_UndoAction action, bool isUndo)
    {
        switch (action.actionType)
        {
            case UndoActionType.ObjectPlaced:
                if (isUndo)
                {
                    GameObject objToUnplace = FindObjectByInstanceIDFromUndo(action.targetInstanceID);
                    if (objToUnplace != null)
                    {
                        UpdateItemCountsOnPlacement(action.targetItem, false); // itemName
                        Destroy(objToUnplace);
                    }
                }
                else
                { // リドゥ: 配置
                    // ObjectPlaced の UndoAction には objectData がないため、targetItemNameから再生成を試みる
                    // より正確には、UndoAction<ObjectPlaced> にも配置時の Garden_ObjectSaveData を含めるべき
                    Garden_ItemSO itemSO = allAvailableItems.FirstOrDefault(x => x == action.targetItem);
                    if (itemSO != null)
                    {
                        // GenerateAndSelectObject(itemSO); // これは新しい操作としてアンドゥに積まれるので不適切
                        // ここでは、保存された位置情報がないため、カメラ正面に再配置するのは難しい
                        // RestoreSingleObject(action.objectData, true); の objectData が必要
                        Debug.LogWarning("リドゥ ObjectPlaced: 正確な復元には配置時の完全なデータが必要です。");
                        // 簡単のため、アイテムカウントだけ戻す
                        UpdateItemCountsOnPlacement(action.targetItem, true);
                    }
                }
                break;

            case UndoActionType.ObjectMovedRotated:
                GameObject objToMove = FindObjectByInstanceIDFromUndo(action.targetInstanceID);
                if (objToMove != null)
                {
                    if (isUndo)
                    {
                        objToMove.transform.DOMove(action.previousPosition, 0.2f).SetEase(Ease.OutQuad);
                        objToMove.transform.DORotateQuaternion(action.previousRotation, 0.2f).SetEase(Ease.OutQuad);
                        _preOperationPosition = action.previousPosition; // アンドゥ後の状態をpreOpに
                        _preOperationRotation = action.previousRotation;
                    }
                    else
                    { // リドゥ: 移動/回転後の状態に戻す
                        // これもUndoActionに「操作後の状態」を保存していないと正確なリドゥは難しい
                        // 仮に、次のアンドゥ履歴（もしあれば）のpreviousを使うか、あるいは
                        // 操作前の状態に戻すだけ（実質的には何もしないか、再度Undoと同じ動き）
                        Debug.LogWarning("リドゥ ObjectMovedRotated: 正確な復元には操作後の状態保存が必要です。");
                        // リドゥは「アンドゥのアンドゥ」なので、previousに戻すのはおかしい。
                        // この場合、リドゥ対象のactionは「操作前の状態」を持っているので、
                        // リドゥで「その操作を再度実行した後の状態」に戻すのは複雑。
                        // 今回は、リドゥでこのタイプの操作が来たら、何もしないか、
                        // アンドゥスタックのさらに一つ前を見る、などの高度な処理が必要。
                        // 簡単には、リドゥスタックに積む前に「操作後の状態」も記録する。
                    }
                }
                break;

            case UndoActionType.ObjectRemoved:
                if (isUndo && action.objectData != null)
                {
                    RestoreSingleObject(action.objectData, true);
                    UpdateItemCountsOnPlacement(action.objectData.ItemSO, true); // itemName
                }
                else if (!isUndo && action.objectData != null)
                { // リドゥ: 削除
                    GameObject objToRedoRemove = FindObjectByObjectData(action.objectData);
                    if (objToRedoRemove != null)
                    {
                        UpdateItemCountsOnPlacement(action.objectData.ItemSO, false); // itemName
                        Destroy(objToRedoRemove);
                    }
                }
                StartNotification(NotificationType.ItemRemoved);
                break;

            case UndoActionType.AllObjectsRemoved:
                if (isUndo && action.multipleObjectsData != null)
                {
                    foreach (Garden_ObjectSaveData data in action.multipleObjectsData)
                    {
                        RestoreSingleObject(data, true);
                        UpdateItemCountsOnPlacement(data.ItemSO, true); // itemName
                    }
                }
                else if (!isUndo && action.multipleObjectsData != null)
                { // リドゥ: 全削除
                    if (objectParent != null)
                    {
                        List<Transform> childrenToRedoRemove = new List<Transform>(objectParent.Cast<Transform>());
                        foreach (Transform child in childrenToRedoRemove)
                        {
                            Garden_PlaceableObjectData pData = child.GetComponent<Garden_PlaceableObjectData>();
                            if (pData != null) UpdateItemCountsOnPlacement(pData.itemSO, false); // prefabId (itemName)
                            Destroy(child.gameObject);
                        }
                    }
                }
                break;
        }
    }

    GameObject FindObjectByObjectData(Garden_ObjectSaveData dataToFind)
    {
        if (objectParent == null || dataToFind == null) return null;
        foreach (Transform child in objectParent)
        {
            Garden_PlaceableObjectData pData = child.GetComponent<Garden_PlaceableObjectData>();
            if (pData != null && pData.itemSO == dataToFind.ItemSO)
            { // itemNameで比較
              // 位置や回転も比較してより正確に
                if (Vector3.Distance(child.position, dataToFind.position) < 0.01f &&
                    Quaternion.Angle(child.rotation, dataToFind.rotation) < 0.1f)
                {
                    return child.gameObject;
                }
            }
        }
        return null;
    }

    void ClearUndoRedoHistory()
    {
        _undoHistory.Clear();
        _redoHistory.Clear();
        Debug.Log("アンドゥ・リドゥ履歴をクリアしました。");
    }

    GameObject FindObjectByInstanceIDFromUndo(int instanceID)
    {
        if (objectParent == null) return null;
        foreach (Garden_PlaceableObjectData objData in objectParent.GetComponentsInChildren<Garden_PlaceableObjectData>(true))
        {
            if (objData.gameObject.GetInstanceID() == instanceID)
            {
                return objData.gameObject;
            }
        }
        return null;
    }

    void RestoreSingleObject(Garden_ObjectSaveData saveData, bool fromUndoOrRedo)
    {

        if (saveData == null) return; // itemName

        GameObject prefabToLoad = _saveLoadManager.FindPrefabByItem(saveData.ItemSO); // itemName
        if (prefabToLoad != null)
        {
            GameObject restoredObj = Instantiate(prefabToLoad, objectParent);
            restoredObj.transform.position = saveData.position;
            restoredObj.transform.rotation = saveData.rotation;
            restoredObj.transform.localScale = saveData.scale;
            Garden_PlaceableObjectData dataComp = restoredObj.AddComponent<Garden_PlaceableObjectData>();
            dataComp.itemSO = saveData.ItemSO; // itemName
            restoredObj.layer = LayerMask.NameToLayer("PlaceableObject");

            if (fromUndoOrRedo) Debug.Log($"アンドゥ/リドゥ: オブジェクト '{saveData.ItemSO.name}' を復元/再実行しました。");
        }
        else
        {
            Debug.LogError($"復元するプレハブが見つかりません: {saveData.ItemSO.name}");
        }
    }
    #endregion

    #region 回転インジケータ
    void ShowRotationIndicator(bool show, GameObject target = null)
    {
        if (rotationIndicatorPrefab == null) return;
        if (show && target != null)
        {
            if (_currentRotationIndicator == null) _currentRotationIndicator = Instantiate(rotationIndicatorPrefab);
            _currentRotationIndicator.SetActive(true);
            _currentRotationIndicator.transform.SetParent(target.transform, false);
            _currentRotationIndicator.transform.localPosition = Vector3.zero;
            _currentRotationIndicator.transform.localRotation = Quaternion.identity;
        }
        else if (_currentRotationIndicator != null)
        {
            _currentRotationIndicator.SetActive(false);
            if (_currentRotationIndicator.transform.parent != null) _currentRotationIndicator.transform.SetParent(null);
        }
    }
    #endregion

    #region ハイライト処理
    void HighlightObject(GameObject obj) { obj?.GetComponent<Garden_HighlightController>()?.Highlight(); }
    void UnhighlightObject(GameObject obj) { obj?.GetComponent<Garden_HighlightController>()?.Unhighlight(); }
    #endregion

    #region セーブ・ロード
    public void SaveGarden()
    {
        if (_selectedObject != null) PlaceAndDeselectCurrentObject(true);

        List<Garden_ObjectSaveData> placedObjectsToSave = new List<Garden_ObjectSaveData>();
        if (objectParent != null)
        {
            foreach (Transform child in objectParent)
            {
                Garden_PlaceableObjectData objData = child.GetComponent<Garden_PlaceableObjectData>();
                if (objData != null)
                {
                    placedObjectsToSave.Add(new Garden_ObjectSaveData
                    {
                        ItemSO = objData.itemSO, // itemName
                        position = child.position,
                        rotation = child.rotation,
                        scale = child.localScale
                    }
                    );
                }
            }
        }
        _saveLoadManager.Save(placedObjectsToSave);
        ClearUndoRedoHistory();
        SetEditorState(EditorState.Viewing, true);
        StartNotification(NotificationType.Save);
    }

    public void LoadGarden()
    {
        if (_selectedObject != null) PlaceAndDeselectCurrentObject(false);
        if (objectParent != null)
        {
            for (int i = objectParent.childCount - 1; i >= 0; i--)
            {
                Destroy(objectParent.GetChild(i).gameObject);
            }
        }

        InitializeItemCountsAndTryLoad();
        ClearUndoRedoHistory();
        SetEditorState(EditorState.Viewing, true);
    }
    #endregion

    #region ゲーム内通知機能
    private void StartNotification(NotificationType type)
    {
         // --- 同じ種類の通知が既に存在するかチェック ---
        NotificationInstance existingItem = activeNotifications.FirstOrDefault(item => item.Type == type);
        if (existingItem != null)
        {
            // 既存のオブジェクトをリストから削除し、フェードアウトさせてから破棄
            activeNotifications.Remove(existingItem);
            existingItem.transform.DOKill();
            existingItem.GetComponent<CanvasGroup>().DOFade(0, 0.2f).OnComplete(() =>
            {
                Destroy(existingItem.gameObject);
            });
            
            // 古い通知が消えた分のスペースを詰める
            RearrangeNotifications(true);
        }
        
        // 新しい通知の表示コルーチンを開始
        StartCoroutine(NotificationSequence(type));
    }


    /// <summary>
    /// 通知の表示から消去までの一連の流れを管理するコルーチン
    /// </summary>
    /// <param name="type">表示する通知の種類</param>
    private IEnumerator NotificationSequence(NotificationType type)
    {
        // --- 通知オブジェクトの生成と設定 ---
        GameObject notificationGO = Instantiate(notificationPrefab, notificationParent);
        // NotificationInstanceコンポーネントを追加してタイプを保持させる
        NotificationInstance newNotification = notificationGO.AddComponent<NotificationInstance>();
        newNotification.Type = type;

        // --- リストの先頭に追加し、既存の通知を下にスライドさせる ---
        activeNotifications.Insert(0, newNotification);
        RearrangeNotifications(false); // 新しい通知が追加されたので全通知を再配置

        // --- テキストの設定 ---
        TextMeshProUGUI tmpText = notificationGO.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            switch (type)
            {
                // (テキスト設定部分は前回と同じ)
                case NotificationType.ItemAllRemoved: tmpText.text = "全てのアイテムを消去しました"; break;
                case NotificationType.ItemRemoved: tmpText.text = "アイテムを消去しました"; break;
                case NotificationType.ItemUndo: tmpText.text = "配置を戻しました"; break;
                case NotificationType.Save: tmpText.text = "保存しました"; break;
                case NotificationType.Error: tmpText.text = "エラーが発生してしまいました";  break;
            }
        }

        // --- DOTweenによる入場演出 ---
        CanvasGroup canvasGroup = notificationGO.GetComponent<CanvasGroup>();
        RectTransform rectTransform = notificationGO.GetComponent<RectTransform>();

        // 初期状態を設定（Y座標はRearrangeで設定済み、X座標を画面右外に）
        canvasGroup.alpha = 0f;
        Vector2 finalPosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector2(finalPosition.x + rectTransform.rect.width, finalPosition.y);

        Sequence enterSequence = DOTween.Sequence();
        enterSequence.Append(rectTransform.DOAnchorPos(finalPosition, 0.6f).SetEase(Ease.OutQuint));
        enterSequence.Join(canvasGroup.DOFade(1f, 0.4f));

        Transform[] children = notificationGO.GetComponentsInChildren<Transform>();
        for (int i = 1; i < children.Length; i++)
        {
            children[i].localScale = Vector3.zero;
            enterSequence.Insert(0.2f + (i * 0.05f), children[i].DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        }

        // --- 表示時間待機 ---
        yield return new WaitForSeconds(2.0f);

        // --- オブジェクトを消去（フェードアウト演出付き） ---
        // この時点でオブジェクトがまだリストに存在する場合のみ処理
        if (activeNotifications.Contains(newNotification))
        {
            canvasGroup.DOFade(0f, 0.5f)
                .SetEase(Ease.InQuint)
                .OnComplete(() =>
                {
                    if (activeNotifications.Contains(newNotification))
                    {
                        activeNotifications.Remove(newNotification);
                        // この通知が消えた分のスペースを詰める
                        RearrangeNotifications(true);
                    }
                    Destroy(notificationGO);
                });
        }
    }
    
     private void RearrangeNotifications(bool useAnimation)
    {
        float notificationHeight = notificationPrefab.GetComponent<RectTransform>().rect.height;
        
        for (int i = 0; i < activeNotifications.Count; i++)
        {
            if (activeNotifications[i] != null)
            {
                RectTransform rect = activeNotifications[i].GetComponent<RectTransform>();
                // Y座標を計算 (i=0が一番上、Y=0の位置)
                float spacing = 10f; // 通知間のスペース
                float targetY = -(notificationHeight + spacing) * i;

                if (useAnimation)
                {
                    // アニメーションでスムーズに移動
                    rect.DOAnchorPosY(targetY, 0.3f).SetEase(Ease.OutCubic);
                }
                else
                {
                    // 即座に位置を反映（新しい通知の初期位置設定用）
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, targetY);
                }
            }
        }
    }
#endregion
}