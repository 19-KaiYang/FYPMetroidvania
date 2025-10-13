using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    private MeleeEnemy brawler;
    private Spearman spearman;

    private void Awake()
    {
        brawler = GetComponentInParent<MeleeEnemy>();
        spearman = GetComponentInParent<Spearman>();
    }

    private void BrawlerClawFinished()
    {
        brawler.isAttackFinished = true;
    }

    private void SpearManThrow()
    {
        spearman.ThrowSpear();
    }
    private void SpearManThrowFinished()
    {
        spearman.isThrowFinished = true;
    }
    private void SpearManThrustFinished()
    {
        spearman.isThrustFinished = true;
    }
}
