//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, double initialSpeed, double initialAngle)
        {
            Position = initialPosition;
            Speed = initialSpeed;
            Angle = initialAngle;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;
        private double Speed;
        private double Angle;       //kąt w radianach

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal void Move()
        {
            // obliczenie prędkości dla osi X i Y
            double vx = Math.Cos(Angle) * Speed;
            double vy = Math.Sin(Angle) * Speed;
            // nowe pozycje kulki
            double newX = Position.x + vx;
            double newY = Position.y + vy;

            double radius = 10;

            // Odbicia
            if (newX < radius || newX > 400 - radius)
            {
                Angle = Math.PI - Angle; // odbicie w poziomie
            }
            if (newY < radius || newY > 420 - radius)
            {
                Angle = -Angle; // odbicie w pionie
            }

            Position = new Vector(Position.x + Math.Cos(Angle) * Speed, Position.y + Math.Sin(Angle) * Speed);
            RaiseNewPositionChangeNotification();
        }
        #endregion private
    }
}