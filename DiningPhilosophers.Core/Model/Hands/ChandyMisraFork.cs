namespace DiningPhilosophers.Core.Model.Hands
{
    /// <summary>
    /// Smart fork that knows owner's hands, by whom it was requested
    /// and whether it is dirty.
    /// </summary>
    public class ChandyMisraFork : Fork
    {
        public ChandyMisraFork()
        {
            IsDirty = true;
        }

        public volatile bool IsDirty;

        public volatile ChandyMisraHands Owner;

        public volatile ChandyMisraHands RequestedOwner;
    }
}