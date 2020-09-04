﻿using System;
using System.Collections;
using System.Globalization;
using DG.Tweening;
using Rhodos.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Rhodos.UI
{
    /// <summary>
    /// TODO In progress (refactor).
    /// </summary>
    public class Chest : MonoBehaviour
    {
        [SerializeField] private Image chestImage;
        [SerializeField] private RectTransform chestTransform;
        [SerializeField] private RectTransform background;
        private Tween _scaleAnimation;
        [SerializeField] private Text fillAmountIndicator;

        private void Awake()
        {
            //Start "idle" animation
            background.DORotate(new Vector3(0, 0, 5f), 1f).SetEase(Ease.Linear).SetRelative(true)
                      .SetLoops(-1, LoopType.Incremental);
            _scaleAnimation = chestTransform.DOScale(Vector3.one * 1.05f, 0.6f).SetLoops(-1, LoopType.Yoyo)
                                            .SetEase(Ease.Linear);
        }

        public void Init(float startingFillAmount)
        {
            chestImage.fillAmount = startingFillAmount;
            fillAmountIndicator.text = "%" + Math.Round(startingFillAmount * 100f);
        }

        #if UNITY_EDITOR
        private void Update() //For debugging easily
        {
            if (Input.GetMouseButton(0)) Fill(0.2f * Time.deltaTime);
            else if (Input.GetMouseButton(1)) Fill(-0.2f * Time.deltaTime);
        }
        #endif

        public Coroutine Fill(float addition)
        {
            return StartCoroutine(CoFill());

            IEnumerator CoFill()
            {
                bool isEqual = Mathf.RoundToInt(100f * (chestImage.fillAmount + addition)) == 100;
                
                if (chestImage.fillAmount + addition < 1 && !isEqual)
                {
                    yield return chestImage.DOFillAmount(chestImage.fillAmount + addition, Math.Abs(addition))
                                           .OnUpdate(() => fillAmountIndicator.text = "%" + Mathf.RoundToInt(chestImage.fillAmount * 100f))
                                           .OnStart(()=>SaveLoadManager.IncreaseChestProgress(addition));
                }
                else if (isEqual)
                {
                    yield return chestImage.DOFillAmount(1f, Math.Abs(addition))
                                           .OnUpdate(() => fillAmountIndicator.text = "%" + Mathf.RoundToInt(chestImage.fillAmount * 100f))
                                           .OnStart(()=>SaveLoadManager.IncreaseChestProgress(addition));
                    _scaleAnimation.Pause();

                    yield return StartCoroutine(OpenChest());

                    fillAmountIndicator.text = "%0";
                    chestImage.fillAmount = 0f;
                    
                    _scaleAnimation.Play();
                }
                else
                {
                    float firstStep = 1f - chestImage.fillAmount;
                    float secondStep = addition - firstStep;

                    yield return chestImage.DOFillAmount(firstStep, firstStep)
                                           .OnUpdate(() => fillAmountIndicator.text = "%" + Mathf.RoundToInt(chestImage.fillAmount * 100f))
                                           .OnStart(()=>SaveLoadManager.IncreaseChestProgress(addition));
                    _scaleAnimation.Pause();

                    yield return StartCoroutine(OpenChest());

                    chestImage.fillAmount = 0f;
                    fillAmountIndicator.text = "%0";
                    yield return chestImage.DOFillAmount(secondStep, secondStep)
                                           .OnUpdate(() => fillAmountIndicator.text = "%" + Mathf.RoundToInt(chestImage.fillAmount * 100f))
                                           .OnStart(() => SaveLoadManager.IncreaseChestProgress(addition));
                    _scaleAnimation.Play();
                }
            }
        }

        private IEnumerator OpenChest() //Just change OpenChest method for applying another animation.
        {
            yield return chestTransform.DOShakeRotation(1.5f, Vector3.forward * 10f, fadeOut: false)
                                       .OnComplete(() => chestTransform.DORotate(Vector3.zero, 0.02f));
            
            //TODO chest opening anim
            Debug.Log("Chest Opened");
        }
    }
}