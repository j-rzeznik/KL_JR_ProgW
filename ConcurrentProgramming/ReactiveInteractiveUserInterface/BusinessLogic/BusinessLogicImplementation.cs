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
using TP.ConcurrentProgramming.Data;
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
            layerBellow.Start(numberOfBalls, (initialPosition, dataBall) =>
            {
                lock (dataBalls)
                {
                    dataBalls.Add(dataBall);
                    lastPositions[dataBall] = initialPosition;
                    dataBall.NewPositionNotification += DetectCollisions;
                }
                var logicBall = new Ball(dataBall);
                upperLayerHandler(new Position(initialPosition.x, initialPosition.y), logicBall);
            });
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;
        private readonly UnderneathLayerAPI layerBellow;
        private readonly List<Data.IBall> dataBalls = [];
        private readonly Dictionary<Data.IBall, Data.IVector> lastPositions = new();

        private void DetectCollisions(object? sender, Data.IVector e)
        {
            const double radiusBall = 10;
            const double widthTable = 400;
            const double heightTable = 420;
            var sourceBall = (Data.IBall)sender!;
            var lastPos = lastPositions[sourceBall];

            lock (dataBalls)
            {
                lastPositions[sourceBall] = e;

                // Odbicia od ścian
                var vel = sourceBall.Velocity;

                if (e.x - radiusBall <= 0 || e.x + radiusBall >= widthTable)
                {
                    sourceBall.Velocity = layerBellow.MakeVector(-vel.x, vel.y);
                    layerBellow.ModifyPosition(sourceBall, lastPos);
                    return;
                }

                if (e.y - radiusBall <= 0 || e.y + radiusBall >= heightTable)
                {
                    sourceBall.Velocity = layerBellow.MakeVector(vel.x, -vel.y);
                    layerBellow.ModifyPosition(sourceBall, lastPos);
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
                    double minDistance = 2 * radiusBall;

                    if (distance < minDistance && distance > 0)
                    {
                        double mass1 = sourceBall.Mass;
                        double mass2 = ball.Mass;
                        //double nextVx1 = (vel.x * (mass1 - mass2) + 2 * mass2 * );

                        sourceBall.Velocity = layerBellow.MakeVector(-vel.x, -vel.y);                   //odbicie
                        ball.Velocity = layerBellow.MakeVector(-ball.Velocity.x, -ball.Velocity.y);


                        double overlap = 0.5 * (minDistance - distance);                        // Odsunięcie piłek, żeby się nie stykały
                        double nx = dx / distance;
                        double ny = dy / distance;


                        var posSourceBall = layerBellow.MakeVector(pos1.x - nx * overlap, pos1.y - ny * overlap);
                        var posBall = layerBellow.MakeVector(pos2.x + nx * overlap, pos2.y + ny * overlap);

                        layerBellow.ModifyPosition(sourceBall, posSourceBall);
                        layerBellow.ModifyPosition(ball, posBall);
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