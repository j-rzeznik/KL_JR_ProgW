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
            mass = 1.0;
        }

        #endregion ctor

        #region IBall

        public event EventHandler<IVector>? NewPositionNotification;

        public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        private Vector Position;
        private Thread thread;
        private double mass;

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

        internal void Move(Vector delta)
        {
            Position = new Vector(Position.x + delta.x, Position.y + delta.y);
            RaiseNewPositionChangeNotification();
        }


        private void ThreadLife(object tmpMoveAction)
        {
            if (tmpMoveAction is not Action<Ball> moveAction)           // rzutujemy z powrotem na Action<Ball>
                throw new ArgumentException("Invalid thread argument.");

            try
            {
                while (true)
                {                           // co 14ms wykonuje się przekazana metoda (u nas jest to Move())
                    Thread.Sleep(14);
                    moveAction(this);
                }
            }
            catch (ThreadInterruptedException)
            {

            }
        }

        #endregion private

        #region public

        public double Mass => mass;     // pole tylko do odczytu

        public void StopThread()
        {
            if (thread == null)         // sprawdzamy czy StartThread na pewno było poprawnie wywołane
            {
                throw new InvalidOperationException("The thread is not running");
            }
            thread.Interrupt();         // przerwanie wątku - przerwanie sleep i wejście do catch
            thread.Join();              // metoda synchronizacji, która blokuje wątek wywołujący StopThread do momentu zakończenia thread
            thread = null;
        }

        public void StartThread(Action<Ball> moveAction)
        {
            if (thread != null)
            {
                throw new ThreadStateException("The thread has already been started.");
            }
            thread = new Thread(ThreadLife);        // argumentem jest funkcja startowa
            thread.Start(moveAction);               // wywołanie funkcji startowej, ta funkcja klasy Thread w .NET przyjmuje argument jako object
        }

        internal void Move()
        {
            throw new NotImplementedException();
        }

        #endregion public
    }
}