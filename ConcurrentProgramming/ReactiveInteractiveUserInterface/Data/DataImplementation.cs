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
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000/60));
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
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));

                double angle = random.NextDouble() * 2 * Math.PI;
                double speed = 2.0;
                Vector velocity = new(Math.Cos(angle) * speed, Math.Sin(angle) * speed);

                Ball newBall = new(startingPosition, velocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
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
        private const double Radius = 10;
        private const double TableWidth = 400;
        private const double TableHeight = 420;
        private readonly Timer MoveTimer;
        private List<Ball> BallsList = [];

        private void Move(object? x)
        {
            Ball[] ballsSnapshot;

            lock (BallsList)
            {
                ballsSnapshot = BallsList.ToArray();
            }

            // Obsługa kolizji między piłkami
            for (int i = 0; i < ballsSnapshot.Length; i++)
            {
                for (int j = i + 1; j < ballsSnapshot.Length; j++)
                {
                    Ball ball1 = ballsSnapshot[i];
                    Ball ball2 = ballsSnapshot[j];

                    IVector pos1 = ball1.GetPosition();
                    IVector pos2 = ball2.GetPosition();

                    double dx = pos2.x - pos1.x;
                    double dy = pos2.y - pos1.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDistance = 2 * Radius;

                    if (distance < minDistance && distance > 0)
                    {
                        ball1.Velocity = new Vector(-ball1.Velocity.x, -ball1.Velocity.y);      //odbicie
                        ball2.Velocity = new Vector(-ball2.Velocity.x, -ball2.Velocity.y);

                        double overlap = 0.5 * (minDistance - distance);                        // Odsunięcie piłek, żeby się nie stykały
                        double nx = dx / distance;
                        double ny = dy / distance;

                        ball1.SetPosition(new Vector(pos1.x - nx * overlap, pos1.y - ny * overlap));
                        ball2.SetPosition(new Vector(pos2.x + nx * overlap, pos2.y + ny * overlap));
                    }
                }
            }

            // Ruch i odbicia od ścian
            foreach (Ball ball in ballsSnapshot)
            {
                IVector position = ball.GetPosition();
                double nextX = position.x + ball.Velocity.x;
                double nextY = position.y + ball.Velocity.y;

                if (nextX - Radius <= 0 || nextX + Radius >= TableWidth)                // Odbicie od lewej/prawej krawędzi
                {
                    ball.Velocity = new Vector(-ball.Velocity.x, ball.Velocity.y);
                }

                if (nextY - Radius <= 0 || nextY + Radius >= TableHeight)                // Odbicie od góry/dołu
                {
                    ball.Velocity = new Vector(ball.Velocity.x, -ball.Velocity.y);
                }

                ball.Move();
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