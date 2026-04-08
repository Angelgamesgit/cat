using System;

public static class Bag_EventBridge
{
    public static Action<Bag_GameController.BagResult> OnBagClosed;

    public static void NotifyBagClosed(Bag_GameController.BagResult result)
    {
        OnBagClosed?.Invoke(result);
    }
    public static void SubscribeBagClosed(Action<Bag_GameController.BagResult> callback)
    {
        OnBagClosed += callback;
    }
    //スイカ側の動きが止まったことをバッグ側に通知する関数
    public static Action OnItemStopped;
    public static void NotifyItemStopped()
    {
        OnItemStopped?.Invoke();
    }
}