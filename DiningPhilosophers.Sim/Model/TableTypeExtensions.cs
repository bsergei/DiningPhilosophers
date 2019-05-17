using System;
using DiningPhilosophers.Core.Factories;

namespace DiningPhilosophers.Sim.Model
{
    public static class TableTypeExtensions
    {
        public static IDomainFactory CreateDomainFactory(this TableType tableType)
        {
            switch (tableType)
            {
                case TableType.Problem:
                    return new ProblemFactory();

                case TableType.Dijkstra:
                    return new DijkstraFactory();

                case TableType.Arbitrary:
                    return new ArbitraryFactory();

                case TableType.Agile:
                    return new AgileFactory();

                case TableType.ChandyMisra:
                    return new ChandyMisraFactory();

                default:
                    throw new ArgumentOutOfRangeException(nameof(tableType));
            }
        }
    }
}