//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {

        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            Random random = new Random();
            lock (BallsList)
            {
                for (int i = 0; i < numberOfBalls; i++)
                {
                    Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));

                    double angle = random.NextDouble() * 2 * Math.PI;
                    double speed = 2.0;
                    Vector velocity = new(Math.Cos(angle) * speed, Math.Sin(angle) * speed);

                    Ball newBall = new(startingPosition, velocity);
                    upperLayerHandler(startingPosition, newBall);
                    BallsList.Add(newBall);
                    newBall.StartThread(this.Move);
                }
            }
        }
        public override IVector MakeVector(double x, double y)
        {
            return new Vector(x, y);
        }
        public override void ModifyPosition(IBall iball, IVector vec)
        {
            Ball ball = (Ball)iball;
            ball.SetPosition((Vector)vec);
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    lock (BallsList)
                    {
                        foreach (Ball ball in BallsList)
                        {
                            ball.StopThread();
                        }
                        BallsList.Clear();
                    }
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        //private bool disposedValue;
        private bool Disposed = false;
        private List<Ball> BallsList = [];
        private void Move(Ball ball)
        {
            lock (BallsList)
            {
                ball.Move(new Vector(ball.Velocity.x, ball.Velocity.y));
            }
        }

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}