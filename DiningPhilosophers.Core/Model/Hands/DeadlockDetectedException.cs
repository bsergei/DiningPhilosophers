using System;

namespace DiningPhilosophers.Core.Model.Hands
{
    /// <summary>
    /// Exception that throws to exit thread when possible deadlock occurred.
    /// Used in ProblemHands to release resources because there is high probability
    /// of deadlock.
    /// </summary>
    public class DeadlockDetectedException : ApplicationException
    {
    }
}