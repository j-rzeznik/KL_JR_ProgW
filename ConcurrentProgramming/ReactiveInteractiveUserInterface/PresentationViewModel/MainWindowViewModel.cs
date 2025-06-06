﻿//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
    #region ctor

    public MainWindowViewModel() : this(null)
    { }

    internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
    {
      ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
      Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
      StartCommand = new RelayCommand(Start);               //wywołuje metodę Start
        }

    #endregion ctor

    #region public API

    public int BallCount
    {
        get => ballCount;
        set => Set(ref ballCount, value);
    }

        public ICommand StartCommand { get; }

    public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

    private void Start()
    {
        if (Disposed)
            throw new ObjectDisposedException(nameof(MainWindowViewModel));

        if (BallCount > 0)
        {
            ModelLayer.Start(BallCount);
            Observer.Dispose(); // opcjonalnie, tylko jeśli trzeba zakończyć stary observer
        }
    }

    #endregion public API

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
        if (disposing)
        {
            Balls.Clear();
            Observer.Dispose();
            ModelLayer.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        Disposed = true;
        }
    }

    public void Dispose()
    {
        if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    private IDisposable Observer = null;
    private ModelAbstractApi ModelLayer;
    private bool Disposed = false;
    private int ballCount = 5;

        #endregion private

    public void Start(int numberOfBalls)
    {
        BallCount = numberOfBalls;
        Start();
    }
  }

}