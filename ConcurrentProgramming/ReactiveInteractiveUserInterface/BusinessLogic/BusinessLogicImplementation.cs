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
                    lastPositions[sourceBall] = lastPos;
                    return;
                }

                if (e.y - radiusBall <= 0 || e.y + radiusBall >= heightTable)
                {
                    sourceBall.Velocity = layerBellow.MakeVector(vel.x, -vel.y);
                    layerBellow.ModifyPosition(sourceBall, lastPos);
                    lastPositions[sourceBall] = lastPos;
                    return;
                }

                // kolizje między piłkami
                var pos1 = lastPositions[sourceBall];

                foreach (var ball in dataBalls)
                {
                    if (ball == sourceBall) continue;

                    var pos2 = lastPositions[ball];

                    // pozycje i prędkości
                    double p1x = pos1.x;
                    double p1y = pos1.y;
                    double p2x = pos2.x;
                    double p2y = pos2.y;

                    double dx = p1x - p2x;
                    double dy = p1y - p2y;
                    double distanceSquared = dx * dx + dy * dy;
                    double minDistance = 2 * radiusBall;

                    if (distanceSquared < minDistance * minDistance)
                    {
                        double v1x = sourceBall.Velocity.x;
                        double v1y = sourceBall.Velocity.y;
                        double v2x = ball.Velocity.x;
                        double v2y = ball.Velocity.y;

                        double dvx = v1x - v2x;
                        double dvy = v1y - v2y;

                        double dot = dvx * dx + dvy * dy;
                        if (dot >= 0) return;

                        double m1 = sourceBall.Mass;
                        double m2 = ball.Mass;

                        double coeff1 = (2 * m2) / (m1 + m2) * (dot / distanceSquared);
                        double coeff2 = (2 * m1) / (m1 + m2) * (dot / distanceSquared);

                        double newV1x = v1x - coeff1 * dx;
                        double newV1y = v1y - coeff1 * dy;
                        double newV2x = v2x + coeff2 * dx;
                        double newV2y = v2y + coeff2 * dy;

                        sourceBall.Velocity = layerBellow.MakeVector(newV1x, newV1y);
                        ball.Velocity = layerBellow.MakeVector(newV2x, newV2y);
                    }

                    //layerBellow.ModifyPosition(sourceBall, posSourceBall);
                    //layerBellow.ModifyPosition(ball, posBall);
                    //lastPositions[sourceBall] = posSourceBall;
                    //lastPositions[ball] = posBall;
                
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