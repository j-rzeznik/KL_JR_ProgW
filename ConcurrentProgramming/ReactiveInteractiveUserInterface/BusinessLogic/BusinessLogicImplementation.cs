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
            layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.x), new Ball(databall)));
        }

        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;
        private readonly List<Ball> LogicBalls = new();
        private readonly UnderneathLayerAPI layerBellow;

        private void DetectCollisions(Ball sourceBall)
        {
            double radius = 10;
            double width = 400;
            double height = 420;

            var pos1 = sourceBall.Position;

            foreach (var ball in LogicBalls)
            {
                if (ball == sourceBall) continue;

                var pos2 = ball.Position;

                double dx = pos2.X - pos1.X;
                double dy = pos2.Y - pos1.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                double minDist = 2 * radius;

                if (distance < minDist && distance > 0)
                {
                    sourceBall.Velocity = new Vector(-sourceBall.Velocity.x, -sourceBall.Velocity.y);
                    ball.Velocity = new Vector(-ball.Velocity.x, -ball.Velocity.y);
                }
            }

            // Odbicia od ścian
            var vel = sourceBall.Velocity;
            var pos = sourceBall.Position;

            if (pos.X - radius <= 0 || pos.X + radius >= width)
                vel = new Vector(-vel.x, vel.y);

            if (pos.Y - radius <= 0 || pos.Y + radius >= height)
                vel = new Vector(vel.x, -vel.y);

            sourceBall.Velocity = vel;
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