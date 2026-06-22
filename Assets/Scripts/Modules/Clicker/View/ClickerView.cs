using System;
using Core.Navigation;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Modules.Clicker.View
{
    public class ClickerView : MonoBehaviour, IClickerView, ITabView
    {
        [Header("UI Elements")] [SerializeField]
        private Button _clickButton;

        [SerializeField] private Shadow _clickButtonShadow;
        [SerializeField] private TMP_Text _currencyText;
        [SerializeField] private Image _currencyIcon;
        [SerializeField] private TMP_Text _energyText;
        [SerializeField] private Image _energyIcon;

        [Header("Tab Settings")] [SerializeField]
        private GameObject _contentPanel;

        [Header("Animation")] [SerializeField] private float _pressScale = 0.9f;
        [SerializeField] private float _pressDuration = 0.1f;

        [Header("VFX Points")] [SerializeField]
        private Transform _particleSpawnPoint;

        [SerializeField] private Transform _coinTarget;

        [Header("Audio")] [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clickSound;
        
        private Vector3 _originalPosition;

        public IObservable<Unit> OnClickButton => _clickButton.OnClickAsObservable();
        public RectTransform ButtonTransform => _clickButton.transform as RectTransform;
        public Transform CoinTarget => _coinTarget;
        public Transform ParticleSpawnPoint => _particleSpawnPoint;

        public void SetCurrencyText(string text) => _currencyText.text = text;
        public void SetEnergyText(string text) => _energyText.text = text;

        public void Show()
        {
            if (_contentPanel != null)
                _contentPanel.SetActive(true);
            else
                gameObject.SetActive(true);
            
            _originalPosition = _currencyIcon.transform.localPosition;
        }

        public void Hide()
        {
            if (_contentPanel != null)
                _contentPanel.SetActive(false);
            else
                gameObject.SetActive(false);
        }

        public void PlayClickAnimation()
        {
            if (_clickButton == null) return;

            _clickButton.transform
                .DOScale(_pressScale, _pressDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    _clickButton.transform
                        .DOScale(1f, _pressDuration)
                        .SetEase(Ease.InOutQuad);
                });

            DOTween.Sequence()
                .Append(DOTween.To(
                    () => _clickButtonShadow.effectDistance,
                    x => _clickButtonShadow.effectDistance = x,
                    new Vector2(0f, 0f),
                    0.1f
                ))
                .Append(DOTween.To(
                    () => _clickButtonShadow.effectDistance,
                    x => _clickButtonShadow.effectDistance = x,
                    new Vector2(4f, -4f),
                    0.1f
                ));

        }

        public void PlayClickSound()
        {
            if (_audioSource != null && _clickSound != null && _audioSource.isActiveAndEnabled)
            {
                _audioSource.PlayOneShot(_clickSound);
            }
        }

        public void ShowEnergyDepletedEffect()
        {
            _clickButton.transform.DOShakePosition(0.3f, 10f, 20);
        }

        public void PlayCurrencyIconAnimation()
        {
            if (_currencyIcon == null) return;
            
            var sequence = DOTween.Sequence();
            
            sequence.Append(_currencyIcon.transform
                .DOLocalJump(_originalPosition + new Vector3(0, 10f, 0), 1.2f, 1, 0.1f)
                .SetEase(Ease.OutQuad));

            sequence.Append(_currencyIcon.transform
                .DOScale(1.1f, 0.1f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _currencyIcon.transform
                        .DOScale(1f, 0.1f)
                        .SetEase(Ease.InBack);
                }));
            
            sequence.Append(_currencyIcon.transform
                .DOLocalJump(_originalPosition, 1f, 1, 0.2f)
                .SetEase(Ease.InQuad));
        }

        public void PlayEnergyIconAnimation()
        {
            if (_energyIcon == null) return;

            _energyIcon.transform
                .DOScale(1.2f, 0.12f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    _energyIcon.transform
                        .DOScale(1f, 0.12f)
                        .SetEase(Ease.InCubic);
                });
        }

        public void PlayEnergyDepletedIconAnimation()
        {
            if (_energyIcon == null) return;

            var originalColor = Color.white;
            _energyIcon.DOColor(Color.red, 0.1f)
                .OnComplete(() => { _energyIcon.DOColor(originalColor, 0.2f); });
        }
    }
}