// Author: Pehr Collins
// Genetic Algorithms Robot Maze Solver
// 10/14/2010

using System;
using System.IO;
using System.Collections.Generic;

namespace GeneticMazeSolver
{
    delegate bool FitnessComparisonDelegate(Robot a, Robot b);

    static class Program
    {
        const int startingPopulationSize = 500;
        const int saveKillCount = 10; // Amount of fittest robots to save and least fit robots to kill at each generation
        const int crossoverChance = 7; // 7 means 70% chance of crossover and 30% chance of mutation
        const double fitnessThreshold = 1.0; // This is the fitness required for a successful solution
                                              // If it is 1.0 then the solution is solved with a minimum path
                                              // If it is ~9.0 then the goal is reached, but the path isn't as short as possible

        #region Comparer

        public class FitnessComparer : IComparer<Robot>
        {
            int IComparer<Robot>.Compare(Robot x, Robot y)
            {
                return ((new System.Collections.Comparer(System.Globalization.CultureInfo.CurrentCulture)).Compare(y.Fitness, x.Fitness));
            }

        }


        #endregion

        static void Main(string[] args)
        {
            Maze maze = new Maze();
            Random rand = new Random(System.DateTime.Now.Millisecond);
            List<Robot> robots = new List<Robot>();
            int generation = 1;

            //Create comma delimted output file and open stream to it
            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(@"c:\Users\Pehr\Documents\robotDataFile.csv");
            streamWriter.WriteLine("Initial Fitness:");

            // Create initial population of robots
            for (int i = 0; i < startingPopulationSize; i++)
            {
                robots.Add(new Robot(rand));

                robots[i].ProcessRobot(maze);

                System.Console.Write(robots[i].Fitness);
                System.Console.Write("  ");

                //Write intial data to output file
                streamWriter.WriteLine(robots[i].Fitness);
            }

            // Sort our robots by fitness
            FitnessComparer fitnessComparer = new FitnessComparer();
            robots.Sort(fitnessComparer);

            // While fitness[highest sorted] < 0.9 or maybe 1.0
            while (robots[0].Fitness < fitnessThreshold)
            {
                // Save our fittest (first) 10 robots for the next generation and kill off the least fit 10 (last) robots
                List<Robot> fittestRobots = new List<Robot>();

                for (int i = 0; i < saveKillCount; i++)
                {
                    robots.RemoveAt(robots.Count - 1);
                    fittestRobots.Add(new Robot(robots[i].GenoType, robots[i].Fitness));
                }

                // Randomly sort our robot list to create random pairs for crossover
                robots.Sort(delegate(Robot x, Robot y) { return ((x.Fitness == y.Fitness) ? 0 : rand.Next(0, 1)); });

                // Iterate through our list pairing robots and performing crossover or mutation
                for (int i = 0; i < robots.Count; i++)
                {
                    int crossoverMutationChoser = rand.Next(0, 9);

                    // 70% chance of crossover (and must be at least 2 robots left)
                    if (i < robots.Count - 1 && crossoverMutationChoser < crossoverChance)
                    {
                        Robot parentOne = robots[i];
                        Robot parentTwo = robots[i + 1];
                        Robot.Crossover(ref parentOne, ref parentTwo, rand);
                        // ProcessRobot will get a new fitness value
                        robots[i].ProcessRobot(maze);
                        robots[i+1].ProcessRobot(maze);
                    }
                    else
                    // 30% chance of mutation
                    {
                        robots[i].Mutate(rand);
                        // ProcessRobot will get a new fitness value
                        robots[i].ProcessRobot(maze);
                    }
                }
                // Re-add our fittest robots
                for (int i = 0; i < saveKillCount; i++)
                {
                    robots.Add(fittestRobots[i]);
                }
                generation++;
                System.Console.Write("New generation: ");

                // Put our robots in fitness sort order
                robots.Sort(fitnessComparer);

                streamWriter.WriteLine(String.Format("Fitness Generation[{0}]:", generation));
                String phenoType = new String(robots[0].PhenoType);
                streamWriter.WriteLine(String.Format("Fittest Phenotype: {0}", phenoType));

                // Print out our current fitness data
                for (int i = 0; i < robots.Count; i++)
                {
                    System.Console.Write(robots[i].Fitness);
                    System.Console.Write("  ");

                    //Write current fitness data to output file
                    streamWriter.WriteLine(robots[i].Fitness);
                }
            }
            System.Console.Write("Final fitness values: ");
            streamWriter.WriteLine(String.Format("Final Fitness Generation[{0}]:", generation));
            String pheno = new String(robots[0].PhenoType);
            streamWriter.WriteLine(String.Format("Fittest Phenotype: {0}", pheno));

            StreamWriter fitPhenoWriter = new StreamWriter(@"c:\Users\Pehr\Documents\robotSolution.txt");
            fitPhenoWriter.Write(pheno);
            fitPhenoWriter.Close();

            for (int i = 0; i < robots.Count; i++)
            {
                System.Console.Write(robots[i].Fitness);
                System.Console.Write("  ");

                //Write current fitness data to output file
                streamWriter.WriteLine(robots[i].Fitness);
            }
            streamWriter.Close();
            // Wait for input so data can be viewed on console
            System.Console.ReadKey();
        } 
    }
}

