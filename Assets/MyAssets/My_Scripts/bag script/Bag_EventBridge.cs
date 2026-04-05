using System;

public static class Bag_EventBridge
{
    public static Action<Bag_GameController.BagResult> OnBagClosed;

    public static void NotifyBagClosed(Bag_GameController.BagResult result)
    {
        OnBagClosed?.Invoke(result);
    }
}