using System;
using DiningPhilosophers.Core.Model;

namespace DiningPhilosophers.Sim.Model
{
    public class StateDto
    {
        public Guid TableId { get; set; }

        public TableType TableType { get; set; }

        public int PhilosopherId { get; set; }

        public bool IsDeadlockDetected { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
        
        public TotalStats TotalStats { get; set; }
    }
}