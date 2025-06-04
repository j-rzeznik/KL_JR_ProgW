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
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

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
                    double speed = 200.0;
                    Vector velocity = new(Math.Cos(angle) * speed, Math.Sin(angle) * speed);

                    Ball newBall = new(startingPosition, velocity);
                    upperLayerHandler(startingPosition, newBall);
                    BallsList.Add(newBall);
                    newBall.StartThread(this.Move);
                }
            }
            StartLoggingThread();
            timer = new System.Timers.Timer(60000); // 60 sekund
            timer.Elapsed += (_, _) =>
            {
                lock (BallsList)
                {
                    foreach (var ball in BallsList)
                    {
                        var pos = ball.GetPosition();
                        string logEntry = $"{DateTime.UtcNow:O}, {pos.x:F2}, {pos.y:F2}, {ball.Velocity.x:F2}, {ball.Velocity.y:F2}";
                        lock (logLock)
                        {
                            logQueue.Enqueue(logEntry);
                        }
                    }
                }
            };
            timer.AutoReset = true;
            timer.Start();
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

                    if (timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                        timer = null;
                    }
                    if (loggingThread != null)
                    {
                        loggingActive = false;
                        loggingThread.Join();
                        loggingThread = null;
                    }

                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private List<Ball> BallsList = [];

        private System.Timers.Timer? timer;
        private readonly double deltaTimeSeconds = 0.014;
        private readonly Queue<string> logQueue = new();
        private Thread? loggingThread;
        private bool loggingActive = true;
        private readonly object logLock = new();


        private void Move(Ball ball)
        {
            lock (BallsList)
            {
                Vector delta = new(ball.Velocity.x * deltaTimeSeconds, ball.Velocity.y * deltaTimeSeconds);
                ball.Move(delta);
            }
        }
        private void StartLoggingThread()
        {
            loggingThread = new Thread(() =>
            {
                using StreamWriter writer = new("diagnostics_log.txt", append: true, encoding: System.Text.Encoding.ASCII);
                while (loggingActive)
                {
                    try
                    {
                        string? entry = null;
                        lock (logLock)
                        {
                            if (logQueue.Count > 0)
                                entry = logQueue.Dequeue();
                        }
                        if (entry != null)
                        {
                            writer.WriteLine(entry);
                            writer.Flush();
                        }
                        else
                        {
                            Thread.Sleep(100); // Brak danych
                        }
                    }
                    catch (IOException)
                    {
                        // kanał może być chwilowo zablokowany — chwilowy brak przepustowości
                        Thread.Sleep(500);
                    }
                }
            });
            loggingThread.Start();
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
