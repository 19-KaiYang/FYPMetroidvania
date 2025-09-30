using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    public Trigger trigger;
    public abstract void DoEffect(ActionContext context);
}

