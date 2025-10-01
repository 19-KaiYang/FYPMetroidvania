using UnityEngine;
using static Debuff;

public class DebuffInstance
{
    public Debuff debuff;

    // For over time effects
    public int stacks;
    public float duration;
    public float lastTickTime;
    
    public void OnRemove(Health owner)
    {
        if (debuff.triggerType == debuffTriggerType.WhenRemoved)
            debuff.TriggerDebuff(owner, this);
        else if (debuff.triggerType == debuffTriggerType.WhenHit)
            owner.damageTaken -= TriggerDebuffEvent;
        
    }
    public void TriggerDebuffEvent(Health owner) // Handling debuffs triggered by external factors (eg. being hit)
    {
        debuff.TriggerDebuff(owner, this);
    }
    public void UpdateTime(Health owner, float currTime)
    {
        float timeElapsed = currTime - lastTickTime;
        if(debuff.triggerType == debuffTriggerType.OverTime)
        {
            if (timeElapsed >= debuff.tickRate)
            {
                debuff.TriggerDebuff(owner, this);
                lastTickTime = currTime;
            }
        }
        duration -= Time.deltaTime;
    }

    public DebuffInstance(Debuff debuff, int stacks, float duration, float lastTickTime)
    {
        this.debuff = debuff;
        this.stacks = Mathf.Clamp(stacks, 0, debuff.maxStacks);
        this.duration = duration;
        this.lastTickTime = lastTickTime;
    }
}
