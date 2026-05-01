using System;

public static class Bag_EventBridge
{
    public static Action<Bag_GameController.BagState> OnBagClosed;

    public static void NotifyBagClosed(Bag_GameController.BagState state)
    {
        OnBagClosed?.Invoke(state);
    }
    public static void SubscribeBagClosed(Action<Bag_GameController.BagState> callback)
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