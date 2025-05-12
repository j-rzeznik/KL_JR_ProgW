//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall
    {
        #region ctor

        internal Ball(Vector initialPosition, Vector initialVelocity)
        {
            Position = initialPosition;
            Velocity = initialVelocity;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;
        private Thread thread;

        private void RaiseNewPositionChangeNotification()
        {
            NewPositionNotification?.Invoke(this, Position);
        }

        internal IVector GetPosition() => Position;
        internal void SetPosition(Vector newPosition)
        {
            Position = newPosition;
            RaiseNewPositionChangeNotification();
        }

        internal void Move()
        {
            Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
            RaiseNewPositionChangeNotification();
        }

        public void StartThread(Action<Ball> moveAction)
        {
            if (thread != null)
            {
                throw new InvalidOperationException("Thread already started");
            }
            thread = new Thread(ThreadLife);
            thread.Start(moveAction);
        }

        private void ThreadLife(object moveAction)
        {
            if (moveAction is not Action<Ball> tmpMoveAction)
                throw new ArgumentException("Invalid thread argument.");

            try
            {
                while (true)
                {
                    Thread.Sleep(8);
                    tmpMoveAction(this);
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        public void StopThread()
        {
            if (thread == null)
            {
                throw new InvalidOperationException("Thread not running");
            }
            thread.Interrupt();
            thread.Join();
            thread = null;
        }

        #endregion private
    }
}