using System;
using UniRx;
using UnityEngine;
using Core.Navigation;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace Modules.Weather.View
{
    public class WeatherView : MonoBehaviour, ITabView, IWeatherView
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _contentPanel;
        [SerializeField] private TMP_Text _weatherText;
        [SerializeField] private Image _weatherIcon; 
        [SerializeField] private Image _weatherIconHolder;
        
        [Header("Cloud Animation")]
        [SerializeField] private float _floatHeight = 10f;
        [SerializeField] private float _floatDuration = 1.5f;
        [SerializeField] private Ease _floatEase = Ease.InOutSine;
        
        private readonly Subject<Unit> _onTabSelected = new();
        private readonly Subject<Unit> _onTabDeselected = new();
        
        private Tweener _floatTweener;
        private Vector3 _originalPosition;
        private bool _isFloating = false;
        
        public IObservable<Unit> OnTabSelected => _onTabSelected;
        public IObservable<Unit> OnTabDeselected => _onTabDeselected;
        
        private void Awake()
        {
            if (_weatherIconHolder != null)
            {
                _originalPosition = _weatherIconHolder.transform.localPosition;
            }
        }
        
        private void OnDestroy()
        {
            _floatTweener?.Kill();
        }
        
        public void Show()
        {
            if (_contentPanel != null)
                _contentPanel.SetActive(true);
            else
                gameObject.SetActive(true);
            
            StartCloudFloat();
            _onTabSelected.OnNext(Unit.Default);
        }
        
        public void Hide()
        {
            if (_contentPanel != null)
                _contentPanel.SetActive(false);
            else
                gameObject.SetActive(false);
            
            StopCloudFloat();
            _onTabDeselected.OnNext(Unit.Default);
        }
        
        public void SetWeather(string temperature)
        {
            if (_weatherText != null)
            {
                _weatherText.text = $"Today - {temperature}";
            }
        }
        
        public void SetWeatherIcon(Sprite icon)
        {
            if (_weatherIcon != null && icon != null)
            {
                _weatherIcon.sprite = icon;
            }
        }
        
        private void StartCloudFloat()
        {
            if (_weatherIconHolder == null || _isFloating) return;
            
            _floatTweener?.Kill();
            
            _weatherIconHolder.transform.localPosition = _originalPosition;
            
            _floatTweener = _weatherIconHolder.transform
                .DOLocalMoveY(_originalPosition.y + _floatHeight, _floatDuration)
                .SetEase(_floatEase)
                .SetLoops(-1, LoopType.Yoyo) 
                .SetLink(gameObject);
            
            _isFloating = true;
        }
        
        private void StopCloudFloat()
        {
            if (!_isFloating) return;
            
            _floatTweener?.Kill();
            _isFloating = false;
            
            if (_weatherIconHolder != null)
            {
                _weatherIconHolder.transform.localPosition = _originalPosition;
            }
        }
    }
}