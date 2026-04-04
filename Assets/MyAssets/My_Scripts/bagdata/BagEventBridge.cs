using System;

public static class BagEventBridge
{
    public static Action<BagGameController.BagResult> OnBagClosed;

    public static void NotifyBagClosed(BagGameController.BagResult result)
    {
        OnBagClosed?.Invoke(result);
    }
}