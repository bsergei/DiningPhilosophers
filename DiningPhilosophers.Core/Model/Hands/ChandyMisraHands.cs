using System;
using System.Diagnostics;
using System.Threading;

namespace DiningPhilosophers.Core.Model.Hands
{
    /// <summary>
    /// "ChandyMisra" Philosopher's hands implementation.
    /// </summary>
    public class ChandyMisraHands : IPhilosopherHands
    {
        private ChandyMisraFork[] forks_ = new ChandyMisraFork[0];

        public void AssignFork(ChandyMisraFork fork)
        {
            var ownedForks = new ChandyMisraFork[forks_.Length + 1];
            Array.Copy(forks_, ownedForks, forks_.Length);
            ownedForks[ownedForks.Length - 1] = fork;
            forks_ = ownedForks;
        }

        public void PickUpForks()
        {
            bool result;
            do
            {
                result = true;
                for (int i = 0; i < forks_.Length; i++)
                {
                    ChandyMisraFork fork = forks_[i];
                    if (!TryPickUpFork(fork))
                    {
                        result = false;
                        break;
                    }
                }
            } while (!result);
        }

        public void PutDownForks()
        {
            bool anyForkMoved = false;
            for (int i = 0; i < forks_.Length; i++)
            {
                ChandyMisraFork fork = forks_[i];
                if (PutDownFork(fork))
                {
                    anyForkMoved = true;
                }
            }

            if (anyForkMoved)
            {
                Thread.Yield();
            }
        }

        private bool TryPickUpFork(ChandyMisraFork fork)
        {
            lock (fork)
            {
                if (fork.Owner != this)
                {
                    if (fork.IsDirty)
                    {
                        // Other Philosopher just put down fork. Get it immediately.
                        fork.IsDirty = false;
                        fork.Owner = this;
                        fork.RequestedOwner = null;
                    }
                    else
                    {
                        // Fork is clean. Just ask to give this fork to me.
                        fork.RequestedOwner = this;
                        return false;
                    }
                }
                else
                {
                    // I already own this fork. Just clean it.
                    fork.IsDirty = false;
                }
            }

            return true;
        }

        private bool PutDownFork(ChandyMisraFork fork)
        {
            lock (fork)
            {
                // Assert I am still in a good condition.
                Debug.Assert(fork.Owner == this);
                Debug.Assert(fork.IsDirty == false);

                if (fork.RequestedOwner != null)
                {
                    // Someone requested this fork. Clean and give it to requester.
                    fork.IsDirty = false;
                    fork.Owner = fork.RequestedOwner;
                    fork.RequestedOwner = null;
                    return true;
                }
                else
                {
                    // Just mark fork dirty and do not touch.
                    fork.IsDirty = true;
                    return false;
                }
            }
        }
    }
}