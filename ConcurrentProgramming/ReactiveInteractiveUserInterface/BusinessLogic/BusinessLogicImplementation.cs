//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using System.Numerics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor

        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));
            layerBellow.Start(numberOfBalls, (startingPosition, dataBall) =>
            {
                lock (dataBalls)
                {
                    dataBalls.Add(dataBall);
                    lastPositions[dataBall] = startingPosition;
                    dataBall.NewPositionNotification += DetectCollisions;
                }
                var businessBall = new Ball(dataBall);
                upperLayerHandler(
                    new Position(startingPosition.x, startingPosition.y),
                    businessBall);
            });
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;
        private readonly List<Ball> LogicBalls = new();
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Data.IBall> dataBalls = [];
        private readonly Dictionary<Data.IBall, Data.IVector> lastPositions = new();

        private void DetectCollisions(object? sender, Data.IVector e)
        {
            double radius = 10;
            double width = 400;
            double height = 420;
            var sourceBall = (Data.IBall)sender!;
            var lastPos = lastPositions[sourceBall];

            lock (dataBalls)
            {
                lastPositions[sourceBall] = e;

                // Odbicia od ścian
                var vel = sourceBall.Velocity;

                if (e.x - radius <= 0 || e.x + radius >= width)
                {
                    sourceBall.Velocity = layerBellow.makeVector(-vel.x, vel.y);
                    layerBellow.modifyPosition(sourceBall, lastPos);
                    return;
                }

                if (e.y - radius <= 0 || e.y + radius >= height)
                {
                    sourceBall.Velocity = layerBellow.makeVector(vel.x, -vel.y);
                    layerBellow.modifyPosition(sourceBall, lastPos);
                    return;
                }


                // kolizje między piłkami
                var pos1 = lastPositions[sourceBall];

                foreach (var ball in dataBalls)
                {
                    if (ball == sourceBall) continue;

                    var pos2 = lastPositions[ball];

                    double dx = pos2.x - pos1.x;
                    double dy = pos2.y - pos1.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDist = 2 * radius;

                    if (distance < minDist && distance > 0)
                    {
                        sourceBall.Velocity = layerBellow.makeVector(-vel.x, -vel.y);
                        ball.Velocity = layerBellow.makeVector(-ball.Velocity.x, -ball.Velocity.y);
                    }
                }

                return;
            }
        }


        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}