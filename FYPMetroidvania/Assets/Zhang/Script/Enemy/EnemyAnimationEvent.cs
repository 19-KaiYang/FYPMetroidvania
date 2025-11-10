using System.Xml.Serialization;
using UnityEngine;

public class EnemyAnimationEvent : MonoBehaviour
{
    private MeleeEnemy brawler;
    private Spearman spearman;
    private TruckBoss boss;

    private void Awake()
    {
        brawler = GetComponentInParent<MeleeEnemy>();
        spearman = GetComponentInParent<Spearman>();
        boss = GetComponentInParent<TruckBoss>();
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
}
