using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Core.Navigation
{
    public class TabButton : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _activeIndicator;
        [SerializeField] private TMP_Text _text;
        
        [Header("Colors")]
        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _inactiveColor;

        public IObservable<Unit> OnClick => _button.OnClickAsObservable();

        public void SetActive(bool isActive)
        {
            if (_activeIndicator != null)
                _activeIndicator.DOFade((isActive ? 1f : 0f), 0.25f).SetEase(Ease.OutQuad);
            
            if (_text != null)
                _text.color = isActive ? _activeColor : _inactiveColor;
        }
    }
}