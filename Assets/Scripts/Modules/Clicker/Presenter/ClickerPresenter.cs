using System;
using UniRx;
using Modules.Clicker.Model;
using Modules.Clicker.VFX;
using Modules.Clicker.View;

namespace Modules.Clicker.Presenter
{
    public class ClickerPresenter : IDisposable
    {
        private readonly ClickerModel _model;
        private readonly IClickerView _view;
        private readonly ClickerConfig _config;
        private readonly ClickerVFXManager _vfxManager;
        
        private readonly CompositeDisposable _disposables = new();
        private IDisposable _autoCollectSubscription;
        
        public ClickerPresenter(ClickerModel model, IClickerView view, ClickerConfig config, ClickerVFXManager vfxManager)
        {
            _model = model;
            _view = view;
            _config = config;
            _vfxManager = vfxManager;
        }
        
        public void Initialize()
        {
            BindView();
            BindModel();
            BindVFX();
            StartAutoCollect();
            StartEnergyRestore();
        }

        private void BindVFX()
        {
            _vfxManager.OnCoinReachedTarget += HandleCoinReachedTarget;
        }

        private void BindView()
        {
            _view.OnClickButton
                .Where(_ => _model.CanClick.Value)
                .Subscribe(_ => HandleClick())
                .AddTo(_disposables);
            
            _view.OnClickButton
                .Where(_ => !_model.CanClick.Value)
                .Subscribe(_ => _view.ShowEnergyDepletedEffect())
                .AddTo(_disposables);
        }
        
        private void BindModel()
        {
            _model.Currency
                .Pairwise()
                .Where(pair => pair.Previous != pair.Current)
                .Subscribe(pair =>
                {
                    _view.SetCurrencyText($"{pair.Current}");
                })
                .AddTo(_disposables);
            
            _model.Energy
                .Pairwise()
                .Where(pair => pair.Previous != pair.Current)
                .Subscribe(pair =>
                {
                    _view.SetEnergyText($"{pair.Current}/{_config.MaxEnergy}");
                    
                    if (pair.Current > pair.Previous)
                    {
                        _view.PlayEnergyIconAnimation();
                    }
                    else if (pair.Current < pair.Previous && pair.Current < _config.MaxEnergy * 0.2f)
                    {
                        _view.PlayEnergyDepletedIconAnimation();
                    }
                })
                .AddTo(_disposables);
        }
        
        private void HandleClick()
        {
            if (!_model.TrySpendEnergy(_config.ClickEnergyCost))
            {
                _view.ShowEnergyDepletedEffect();
                _view.PlayEnergyDepletedIconAnimation();
                return;
            }
        
            _model.AddCurrency(_config.ClickReward);
        
            PlayVFX();
        }
        
        private void StartAutoCollect()
        {
            _autoCollectSubscription = Observable.Interval(TimeSpan.FromSeconds(_config.AutoCollectInterval))
                .Where(_ => _model.CanClick.Value)
                .Subscribe(_ => HandleAutoCollect())
                .AddTo(_disposables);
        }
        
        private void HandleAutoCollect()
        {
            if (!_model.TrySpendEnergy(_config.AutoCollectEnergyCost)) return;
            
            _model.AddCurrency(_config.AutoCollectReward);
            
            PlayVFX();
        }

        private void PlayVFX()
        {
            _view.PlayClickAnimation();
            _view.PlayClickSound();
    
            if (_view.ParticleSpawnPoint != null)
            {
                _vfxManager.PlayParticles(_view.ParticleSpawnPoint.position);
            }
    
            if (_view.ParticleSpawnPoint != null && _view.CoinTarget != null)
            {
                _vfxManager.PlayCoinFly(
                    _view.ParticleSpawnPoint.position,
                    _view.CoinTarget.position
                );
            }
        }
        
        private void StartEnergyRestore()
        {
            Observable.Interval(TimeSpan.FromSeconds(_config.EnergyRestoreInterval))
                .Subscribe(_ => _model.AddEnergy(_config.EnergyRestoreAmount))
                .AddTo(_disposables);
        }
        
        private void HandleCoinReachedTarget()
        {
            _view.PlayCurrencyIconAnimation();
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            _autoCollectSubscription?.Dispose();
            _model?.Dispose();
            
            if (_vfxManager != null)
            {
                _vfxManager.OnCoinReachedTarget -= HandleCoinReachedTarget;
            }
        }
    }
}