using System.Xml.Serialization;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    private MeleeEnemy brawler;
    private Spearman spearman;
    private TruckBoss boss;
    private Enemy enemy;

    private void Awake()
    {
        brawler = GetComponentInParent<MeleeEnemy>();
        spearman = GetComponentInParent<Spearman>();
        boss = GetComponentInParent<TruckBoss>();
        enemy = GetComponentInParent<Enemy>();
    }
    private void ResetSuperArmour()
    {
        enemy.health.spriteRenderer.color = Color.white;
        enemy.health.knockdownImmune = false;
        enemy.health.stunImmune = false;
    }
    private void BrawlerClawFinished()
    {
        brawler.isAttackFinished = true;
    }
    private void BrawlerClawSFX()
    {
        AudioManager.PlaySFX(SFXTYPE.BRAWLER_ATTACK, 0.35f, pitch: Random.Range(0.9f,1.1f));
    }
    private void GetUp()
    {
        enemy.getUp = true;
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

    private void TruckMoveTrue()
    {
        boss.canMove = true;
    }
    private void TruckMoveFalse()
    {
        boss.canMove = false;
    }

    private void BurstFinished()
    {
        boss.bFinished = true;
    }

    private void SlashFinished()
    {
        boss.slashFinished = true;
    }

    private void StartSlash()
    {
        boss.startSlash = true;
    }

    private void ChangePhase()
    {
        boss.changePhase = true;
    }

    private void StartRevving()
    {
        boss.startRevving = true;
    }
    private void PlayAttackFlash()
    {
        AudioManager.PlaySFX(SFXTYPE.ENEMY_ATTACKFLASH, 0.65f, pitch: 1.2f);
    }
    private void HawkAttackSFX()
    {
        AudioManager.PlaySFX(SFXTYPE.HAWK_ATTACK, 0.5f);
    }
}
