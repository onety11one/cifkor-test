using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Modules.Clicker.VFX
{
    public class ClickerVFXManager : IDisposable
    {
        private readonly ClickParticlesPool _particlePool;
        private readonly FlyingCoinPoolImpl _coinPool;
        private readonly ClickerConfig _config;
        private readonly CompositeDisposable _disposables = new();
        
        public event Action OnCoinReachedTarget;
        
        public ClickerVFXManager(
            ClickParticlesPool particlePool,
            FlyingCoinPoolImpl coinPool,
            ClickerConfig config)
        {
            _particlePool = particlePool;
            _coinPool = coinPool;
            _config = config;
        }
        
        public void PlayParticles(Vector3 position)
        {
            var particles = _particlePool.Spawn();
            particles.transform.position = position;
            
            Observable.Timer(TimeSpan.FromSeconds(_config.ParticleLifetime))
                .Subscribe(_ => _particlePool.Despawn(particles))
                .AddTo(_disposables);
        }
        
        public void PlayCoinFly(Vector3 from, Vector3 to, Action onComplete = null)
        {
            var coin = _coinPool.Spawn();
            coin.position = from;
            
            var sequence = DOTween.Sequence();
            sequence.Append(coin.DOMove(to, _config.CoinFlyDuration).SetEase(Ease.InBack));
            sequence.Join(coin.DOScale(0, _config.CoinFlyDuration).SetEase(Ease.InQuint));
            sequence.OnComplete(() =>
            {
                _coinPool.Despawn(coin);
                coin.localScale = Vector3.one;
                onComplete?.Invoke();
                OnCoinReachedTarget?.Invoke();
            });
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}