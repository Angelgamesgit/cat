using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>プレイヤーとの接触時に抽選した効果を発生させるアイテム／トラップ。</summary>
public class RandomDebuffItem : MonoBehaviour
{
    // 抽選で選択される効果の種類。
    public enum EffectType
    {
        DropCargoTrap,
        LightRecoveryItem,
        LightDispelledMovementTrap,
        IncreasedLightDrain,
        SpottedByThiefCat
    }


    [Header("Random Selection")]
    [SerializeField] private EffectType effectCandidates;
        [SerializeField] private bool triggerOnce = true;

    // 光回復アイテムの回復割合と光量管理コンポーネントを設定する。
    [Header("Light Recovery")]
    [SerializeField, Range(0f, 1f)] private float lightRecoveryAmount = 0.25f;

    // 行動停止トラップの効果時間と、停止対象を設定する。
    [Header("Movement Trap")]
    [SerializeField] private float immobilizedDuration = 3f;
    [SerializeField] private CatSystem catToDisable;

    // 既存のゲーム処理をInspectorから各イベントに割り当てる。
    [Header("Effect Events (Configure in Inspector)")]
    [SerializeField] private UnityEvent onCargoDropped;
    [SerializeField] private UnityEvent onLightRecovered;
    [SerializeField] private UnityEvent onMovementStopped;
    [SerializeField] private UnityEvent onLightDrainIncreased;
    [SerializeField] private UnityEvent onSpottedByThiefCat;
    [SerializeField] private UnityEvent onMovementTrapDispelled;

    private bool hasTriggered;
    private bool catWasEnabled;
    private Coroutine immobilizeCoroutine;

    // CatPlayerから呼び出され、候補から一つの効果を抽選して発動する。
    public void OnPlayerTouched(CatPlayer player)
    {
        if (hasTriggered) return;
        if (effectCandidates == null)
        {
            Debug.LogWarning($"{name}: No effect candidates are configured.", this);
            return;
        }

        if (catToDisable == null) catToDisable = player;
        ApplyEffect(effectCandidates, player);
        if (triggerOnce) hasTriggered = true;
    }

    // 抽選で選ばれた効果を実行する。
    public void ApplyEffect(EffectType effect, CatPlayer player)
    {
        switch (effect)
        {
            case EffectType.DropCargoTrap:
                player.DropCargo();
                break;

          
            case EffectType.LightDispelledMovementTrap:
                onMovementStopped?.Invoke();
                if (catToDisable == null) catToDisable = FindObjectOfType<CatPlayer>();
                if (catToDisable != null)
                {
                    catWasEnabled = catToDisable.enabled;
                    catToDisable.enabled = false;
                    if (immobilizeCoroutine != null) StopCoroutine(immobilizeCoroutine);
                    immobilizeCoroutine = StartCoroutine(ReleaseMovementAfterDelay());
                }
                break;

            case EffectType.IncreasedLightDrain:
                onLightDrainIncreased?.Invoke();
                break;

            case EffectType.SpottedByThiefCat:
                onSpottedByThiefCat?.Invoke();
                break;
        }
    }

    // 光が当たったとき、既存の光判定処理から呼び出してトラップを解除する。
    public void DispelWithLight()
    {
        if (immobilizeCoroutine != null)
        {
            StopCoroutine(immobilizeCoroutine);
            immobilizeCoroutine = null;
        }
        if (catToDisable != null) catToDisable.enabled = catWasEnabled;
        onMovementTrapDispelled?.Invoke();
        Destroy(gameObject);
    }

    // 設定時間が経過したら猫の有効状態を元に戻す。
    private IEnumerator ReleaseMovementAfterDelay()
    {
        yield return new WaitForSeconds(immobilizedDuration);
        if (catToDisable != null) catToDisable.enabled = catWasEnabled;
        immobilizeCoroutine = null;
    }
}
