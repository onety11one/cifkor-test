using System;
using UniRx;
using UnityEngine;

namespace Modules.Clicker.View
{
    public interface IClickerView
    {
        IObservable<Unit> OnClickButton { get; }
        RectTransform ButtonTransform { get; }
        Transform CoinTarget { get; }
        Transform ParticleSpawnPoint { get; }
        
        void SetCurrencyText(string text);
        void SetEnergyText(string text);
        void PlayClickAnimation();
        void PlayClickSound();
        void ShowEnergyDepletedEffect();
        void PlayCurrencyIconAnimation();
        void PlayEnergyIconAnimation();
        void PlayEnergyDepletedIconAnimation();
    }
}