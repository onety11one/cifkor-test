using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Core.Navigation;
using DG.Tweening;
using Modules.Dogs.Model;
using TMPro;

namespace Modules.Dogs.View
{
    public class DogsView : MonoBehaviour, ITabView, IDogsView
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _contentPanel;
        [SerializeField] private Transform _breedsContainer;
        [SerializeField] private BreedButton _breedButtonPrefab;
        [SerializeField] private GameObject _loadingIndicator;
        
        [Header("Fact Popup")]
        [SerializeField] private GameObject _factPopup;
        [SerializeField] private TMP_Text _factBreedName;
        [SerializeField] private TMP_Text _factDescription;
        [SerializeField] private Button _factCloseButton;
        
        [Header("Popup Animation")]
        [SerializeField] private RectTransform _popupContent;
        [SerializeField] private float _popupShowDuration = 0.4f;
        [SerializeField] private float _popupHideDuration = 0.3f;
        [SerializeField] private float _popupScaleFrom = 0.5f;
        [SerializeField] private float _popupScaleTo = 1f;
        [SerializeField] private Ease _popupShowEase = Ease.OutBack;
        [SerializeField] private Ease _popupHideEase = Ease.InBack;
        
        private readonly Subject<Unit> _onTabSelected = new();
        private readonly Subject<Unit> _onTabDeselected = new();
        private readonly Subject<string> _onBreedClicked = new();
        private readonly List<BreedButton> _breedButtons = new();
        private readonly CompositeDisposable _breedDisposables = new();
        
        private Tweener _popupTweener;
        private Tweener _overlayTweener;
        private bool _isPopupShowing = false;
        
        public IObservable<Unit> OnTabSelected => _onTabSelected;
        public IObservable<Unit> OnTabDeselected => _onTabDeselected;
        public IObservable<string> OnBreedClicked => _onBreedClicked;
        
        private void Awake()
        {
            if (_factPopup != null)
            {
                _factPopup.SetActive(false);
                _popupContent.localScale = Vector3.zero;
            }
        }
        
        private void OnDestroy()
        {
            _popupTweener?.Kill();
            _overlayTweener?.Kill();
        }
        
        public void Show()
        {
            if (_contentPanel != null)
                _contentPanel.SetActive(true);
            else
                gameObject.SetActive(true);
            
            _onTabSelected.OnNext(Unit.Default);
        }
        
        public void Hide()
        {
            if (_contentPanel != null)
                _contentPanel.SetActive(false);
            else
                gameObject.SetActive(false);
            
            ClearBreedsList();
            HideFactPopup();
            _onTabDeselected.OnNext(Unit.Default);
        }
        
        public void SetBreedsList(DogBreed[] breeds)
        {
            ClearBreedsList();
            
            _breedsContainer.localScale = Vector3.zero;
            _breedsContainer.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack);
            
            for (int i = 0; i < breeds.Length; i++)
            {
                var button = Instantiate(_breedButtonPrefab, _breedsContainer);
                
                var breedId = breeds[i].id;
                var breedName = breeds[i].name;
                var index = i + 1;
                
                button.Setup(breedId, breedName, index);
                
                button.transform.localScale = Vector3.zero;
                button.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.05f);
                
                button.Button.onClick.AsObservable()
                    .Subscribe(_ => _onBreedClicked.OnNext(breedId))
                    .AddTo(_breedDisposables);
                
                _breedButtons.Add(button);
            }
        }
        
        private void ClearBreedsList()
        {
            _breedDisposables.Clear();
            
            foreach (var button in _breedButtons)
            {
                button.Clear();
                Destroy(button.gameObject);
            }
            _breedButtons.Clear();
        }
        
        public void SetLoading(bool isLoading)
        {
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(isLoading);
                
                if (isLoading)
                {
                    _loadingIndicator.transform
                        .DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear)
                        .SetLoops(-1, LoopType.Restart);
                }
                else
                {
                    _loadingIndicator.transform.DOKill();
                    _loadingIndicator.transform.rotation = Quaternion.identity;
                }
            }
        }
        
        public void ShowFactPopup(string breedName, string description)
        {
            if (_factPopup == null || _isPopupShowing) return;
            
            _isPopupShowing = true;
            
            _factPopup.SetActive(true);
            _popupContent.localScale = Vector3.one * _popupScaleFrom;
            _popupContent.rotation = Quaternion.identity;
            
            _factBreedName.text = breedName;
            _factDescription.text = description;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_popupContent);
            
            _popupTweener?.Kill();
            _popupTweener = _popupContent
                .DOScale(_popupScaleTo, _popupShowDuration)
                .SetEase(_popupShowEase)
                .OnComplete(() =>
                {
                    _isPopupShowing = false;
                });
            
            _popupContent
                .DORotate(new Vector3(0, 0, 5f), _popupShowDuration * 0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _popupContent
                        .DORotate(Vector3.zero, _popupShowDuration * 0.3f)
                        .SetEase(Ease.InQuad);
                });
        }
        
        public void HideFactPopup()
        {
            if (_factPopup == null || !_factPopup.activeSelf) return;
            
            _isPopupShowing = true;
            
            _popupTweener?.Kill();
            _popupTweener = _popupContent
                .DOScale(_popupScaleFrom, _popupHideDuration)
                .SetEase(_popupHideEase)
                .OnComplete(() =>
                {
                    _factPopup.SetActive(false);
                    _isPopupShowing = false;
                });
        }
        
        void Start()
        {
            if (_factCloseButton != null)
            {
                _factCloseButton.onClick.AddListener(HideFactPopup);
            }
        }
    }
}