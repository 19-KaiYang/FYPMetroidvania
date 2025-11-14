using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Transform damageNumberTransform;
    [SerializeField] List<DamageNumber> damageNumberObjPool;
    [SerializeField] GameObject damageNumberPrefab;
    public Slider armourBar;

    private void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            GameObject number = Instantiate(damageNumberPrefab, damageNumberTransform);
            damageNumberObjPool.Add(number.GetComponent<DamageNumber>());
            number.SetActive(false);
        }
    }
    private void OnEnable()
    {
        health = GetComponentInParent<Health>();
        health.updateUI += UpdateHealthUI;
        foreach (var dmgNumber in damageNumberObjPool)
        {
            dmgNumber.gameObject.SetActive(false);
        }
        if(armourBar != null)
        {
            health.updateArmour += UpdateArmourUI;
            armourBar.gameObject.SetActive(false);
        }
    }
    private void OnDisable()
    {
        health.updateUI -= UpdateHealthUI;
        if (armourBar != null)
        {
            health.updateArmour -= UpdateArmourUI;
        }
    }
    private void Update()
    {
        if (health.transform.localScale.x < 0) transform.localScale = new Vector3(-1f, 1f, 1f);
        else transform.localScale = new Vector3(1f, 1f, 1f);
    }
    void UpdateHealthUI(Health health, float numberValue, Color numberColor, bool isCritical = false)
    {
        if (numberValue <= 0) return;
        healthBar.value = health.currentHealth / health.maxHealth;

        foreach(var dmgNumber in damageNumberObjPool)
        {
            if(!dmgNumber.gameObject.activeInHierarchy)
            {
                dmgNumber.gameObject.SetActive(true);
                dmgNumber.Initialize(Mathf.RoundToInt(numberValue), numberColor, isCritical);
                //StartCoroutine(DamageNumberCoroutine(dmgNumber));
                dmgNumber.transform.localScale = Vector3.one * 0.25f;
                dmgNumber.transform.localPosition = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(0f, 0.5f), 0f);
                Vector3 position = dmgNumber.transform.position;
                dmgNumber.transform.SetParent(null, false);
                dmgNumber.transform.position = position;
                dmgNumber.transform.rotation = Quaternion.identity;

                DG.Tweening.Sequence numberSequence = DOTween.Sequence();
                numberSequence.Append(dmgNumber.transform.DOScale(isCritical ? Vector3.one * 1.2f : Vector3.one, 0.2f));
                numberSequence.Join(dmgNumber.numberText.DOFade(1f, 0.2f));
                numberSequence.AppendInterval(0.5f);
                numberSequence.Append(dmgNumber.numberText.DOFade(0f, 0.75f));
                numberSequence.Join(dmgNumber.transform.DOScale(Vector3.one * 0.25f, 0.75f));
                TweenCallback endDamageNumber = new TweenCallback(() =>
                {
                    dmgNumber.transform.SetParent(damageNumberTransform);
                    dmgNumber.gameObject.SetActive(false);
                    Debug.Log("Damage number disabled");
                });
                numberSequence.AppendCallback(endDamageNumber);
                return;
            }
        }
    }
    void UpdateArmourUI(float currentArmour, float armourDamage, bool isCritical = false)
    {
        armourBar.value = currentArmour;
        foreach (var dmgNumber in damageNumberObjPool)
        {
            if (!dmgNumber.gameObject.activeInHierarchy)
            {
                dmgNumber.gameObject.SetActive(true);
                dmgNumber.Initialize(Mathf.RoundToInt(armourDamage), Color.orange, isCritical);
                //StartCoroutine(DamageNumberCoroutine(dmgNumber));
                dmgNumber.transform.localScale = Vector3.one * 0.25f;
                dmgNumber.transform.localPosition = new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(0f, 0.5f), 0f);
                Vector3 position = dmgNumber.transform.position;
                dmgNumber.transform.SetParent(null, false);
                dmgNumber.transform.position = position;
                dmgNumber.transform.rotation = Quaternion.identity;

                DG.Tweening.Sequence numberSequence = DOTween.Sequence();
                numberSequence.Append(dmgNumber.transform.DOScale(isCritical ? Vector3.one * 1.2f : Vector3.one, 0.2f));
                numberSequence.Join(dmgNumber.numberText.DOFade(1f, 0.2f));
                numberSequence.AppendInterval(0.5f);
                numberSequence.Append(dmgNumber.numberText.DOFade(0f, 0.75f));
                numberSequence.Join(dmgNumber.transform.DOScale(Vector3.one * 0.25f, 0.75f));
                TweenCallback endDamageNumber = new TweenCallback(() =>
                {
                    dmgNumber.transform.SetParent(damageNumberTransform);
                    dmgNumber.gameObject.SetActive(false);
                    Debug.Log("Damage number disabled");
                });
                numberSequence.AppendCallback(endDamageNumber);
                return;
            }
        }
    }
}
