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
    private void BrawlerClawSFX()
    {
        AudioManager.PlaySFX(SFXTYPE.BRAWLER_ATTACK, 0.35f, pitch: Random.Range(0.9f,1.1f));
    }
    private void SpearmanThrustSFX()
    {
        AudioManager.PlaySFX(SFXTYPE.SPEARMAN_ATTACK, 0.35f, pitch: Random.Range(0.9f, 1.1f));
        spearman.ThrustVFX();
    }
    private void SpearManThrow()
    {
        spearman.ThrowSpear();
        AudioManager.PlaySFX(SFXTYPE.SPEARMAN_THROW, 0.5f, pitch: 1.1f);
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
