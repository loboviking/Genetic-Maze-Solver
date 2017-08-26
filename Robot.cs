// Author: Pehr Collins
// Genetic Algorithms Robot Maze Solver
// 10/14/2010


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticMazeSolver
{
    public class Robot
    {
        #region Control Constants

        const int genoTypeMinSize = 10;
        const int genoTypleMaxSize = 64;
        const int idealNumberOfMoves = 26;
        const double fitnessDistanceDecrement = 0.04;
        const double fitnessMovesDecrement = 0.005;
        const int goalPositionX = 10;
        const int goalPositionY = 11;

        #endregion

        #region Private Variables

        // Note that each genotype is one byte, but will only use the first two bits
        // Those two bits of each genoType correspond to one phenoType:
        //      00 = 0 = F = Move Forward
        //      01 = 1 = R = Turn 90 degrees to the right
        //      10 = 2 = L = Turn 90 degrees to the left
        //      11 = 3 = S = Stop
        private string genoType = "";
        private char[] phenoType;
        private int phenoTypeLength;
        private double fitness = 0.0;

        #endregion

        #region Properties

        public double Fitness
        {
            get 
            {
                return fitness;
            }
        }

        public string GenoType
        {
            get
            {
                return genoType;
            }
        }

        public char[] PhenoType
        {
            get
            {
                return phenoType;
            }
        }

        #endregion

        #region Public Constructors

        public Robot(Random rand)
        {
            Initialize(rand);
        }

        public Robot(string genoTypeValue, double fitnessValue)
        {
            genoType = genoTypeValue;
            GeneratePhenoType();
            fitness = fitnessValue;
        }

        #endregion

        #region Private Methods

        // Set genoType to random length array of random binary characters.
        // Decode genoType to phenoType
        private void Initialize(Random rand)
        {
            // Length of the phenotype and genotype varies between constants, genotype is twice as long because of 2 bit instructions
            phenoTypeLength = rand.Next(genoTypeMinSize, genoTypleMaxSize);
            phenoType = new char[phenoTypeLength];

            for (int i = 0; i < phenoTypeLength; i++)
            {
                int robotInstruction = (byte)rand.Next(4);

                switch (robotInstruction)
                {
                    // Insert index is multiplied by 2 to account for 2 bits of each robot instruction
                    case 0:
                        phenoType[i] = 'F';
                        genoType = genoType.Insert(i*2, "00");
                        break;
                    case 1:
                        phenoType[i] = 'R';
                        genoType = genoType.Insert(i*2, "01");
                        break;
                    case 2:
                        phenoType[i] = 'L';
                        genoType = genoType.Insert(i*2, "10");
                        break;
                    case 3:
                        phenoType[i] = 'S';
                        genoType = genoType.Insert(i*2, "11");
                        break;
                }
            }

        }

        // Decodes the genoType to phenoType and assigns phenoTypeLength
        private void GeneratePhenoType()
        {
            phenoTypeLength = genoType.Length / 2;
            phenoType = new char[phenoTypeLength];

            for (int i = 0; i < phenoTypeLength; i++)
            {
                string chromosome = genoType.Substring(i * 2, 2);

                switch (chromosome)
                {
                    case "00":
                        phenoType[i] = 'F';
                        break;
                    case "01":
                        phenoType[i] = 'R';
                        break;
                    case "10":
                        phenoType[i] = 'L';
                        break;
                    case "11":
                        phenoType[i] = 'S';
                        break;
                }
            }
        }

        // Fitness will be combination of moves made and (distance to goal, and reached goal)
        private double CalculateFitness(int movesMade, int[] position, Maze maze, bool reachedGoal)
        {
            double fitness = 0.0;

            if (reachedGoal)
            {
                fitness = 1.0;
            }
            else
            {
                // Each unit of distance from the goal reduces the fitness by 5%
                // and we start at 5% less because we haven't stopped on the goal
                fitness = 0.95 - fitnessDistanceDecrement * (DistanceToGoal(position, maze));
            }

            // Each move over the ideal solution of 26 reduces the fitness by .5%
            if (movesMade > idealNumberOfMoves)
            {
                fitness -= (movesMade - idealNumberOfMoves) * fitnessMovesDecrement;
            }

            if (fitness < 0)
            {
                fitness = 0.0;
            }

            return fitness;
        }

        private int DistanceToGoal(int[] position, Maze maze)
        {
            // We know that the goal '$' is at cell 10,11
            // Could easily make a generic goal search if we wanted to use different mazes
            return Math.Abs(position[0] - goalPositionX) + Math.Abs(position[1] - goalPositionY);
        }

        // This flips the bit at a random point in the genoType
        public void Mutate(Random rand)
        {
            int mutationIndex = rand.Next(genoType.Length - 1);

            string mutationBit = genoType.Substring(mutationIndex, 1);
            genoType = genoType.Remove(mutationIndex, 1);
            genoType = genoType.Insert(mutationIndex, OppositeBit(mutationBit));

            //Up
            GeneratePhenoType();
        }

        // Gets the opposite bit as a string
        private string OppositeBit(string bit)
        {
            if (0 == String.Compare(bit, "0"))
            {
                return "1";
            }
            return "0";
        }

        #endregion

        #region Public Methods

        // Processes the robot's instructions on the passed in maze and returns a fitness value
        // If a robot hits a wall it will immediately return its fitness value of based on its current location
        // and the fact that it hit a wall.
        // Note that the fitness value of 0.0 was previously used to effectively kill robots that hit walls.
        // Assigning fitness values of 0.0 to robots that hit walls was done to meet the requirement 
		// that stated that these robots die.  This decreased the effectiveness of the
        // genetic algorithm since robots that were close to solving the maze that hit walls were 
        // eliminated from the gene pool, even though they could have potentially help generate a solution
        // with their offspring.
        public double ProcessRobot(Maze maze)
        {
            // heading:
            // 0 = North
            // 1 = East
            // 2 = South
            // 3 = West
            // Provides a clockwise rotation by incrementing and counter-clockwise by decrementing
            // Robots start heading south (at position 1,1).
            int heading = 2;
            int[] position = { 1, 1 };
            int movesMade = 0;
            bool reachedGoal = false;

            // Walk the robot through the maze to see if it reached the end
            for (int i = 0; i < phenoTypeLength && !reachedGoal; i++)
            {
                movesMade++;

                switch (phenoType[i])
                {
                    case 'F':
                        switch (heading)
                        {
                            case 0: // North
                                position[0] = position[0] - 1;
                                break;
                            case 1: // East
                                position[1] = position[1] + 1;
                                break;
                            case 2: // South
                                position[0] = position[0] + 1;
                                break;
                            case 3: // West
                                position[1] = position[1] - 1;
                                break;
                        }
                        if ('*' == maze.grid[position[0], position[1]])
                        {
                            // We have hit a wall so return a fitness value now
                            fitness = CalculateFitness(movesMade, position, maze, reachedGoal);
                            return fitness;
                        }
                        break;
                    case 'R':
                        // When we are going West wrap heading around to North
                        if (3 == heading)
                        {
                            heading = 0;
                        }
                        // Otherwise just increment normally (turning clockwise)
                        else
                        {
                            heading++;
                        }
                        break;
                    case 'L':
                        // When we are going North wrap heading around to West
                        if (0 == heading)
                        {
                            heading = 3;
                        }
                        // Otherwise just decrement normally (turning counter-clockwise)
                        else
                        {
                            heading--;
                        }
                        break;
                    case 'S':
                        if ('$' == maze.grid[position[0],position[1]])
                        {
                            reachedGoal = true; 
                        }
                        break;
                }
            }
            fitness = CalculateFitness(movesMade, position, maze, reachedGoal);
            return fitness;
        }

        // Creates 2 new robots in the population (putting them in the passed in robot objects)
        // using genetic crossover
        public static void Crossover(ref Robot parentOne, ref Robot parentTwo, Random rand)
        {
            // Use the smaller genoType length to determine crossover point to make sure crossover point is in both genoTypes
            int crossoverLength = Math.Min(parentOne.genoType.Length, parentTwo.genoType.Length);
            int crossoverPoint = rand.Next(1, crossoverLength - 1);

            // If crossoverPoint is odd, subtract 1 so that it is even since our chromosomes (directions) are 2 bits
            int remainder = 0;
            Math.DivRem(crossoverLength, 2, out remainder);

            if (0 != remainder)
            {
                crossoverLength--;
            }

            String parentOneSubstring = parentOne.genoType.Substring(crossoverPoint);
            String parentTwoSubstring = parentTwo.genoType.Substring(crossoverPoint);

            String robotOneString = parentOne.genoType.Remove(crossoverPoint);
            robotOneString = robotOneString.Insert(robotOneString.Length - 1, parentTwoSubstring);
            String robotTwoString = parentTwo.genoType.Remove(crossoverPoint);
            robotTwoString = robotTwoString.Insert(robotTwoString.Length - 1, parentOneSubstring);

            // Our parents are now reborn as the children
            parentOne.genoType = robotOneString;
            parentTwo.genoType = robotTwoString;
            parentOne.GeneratePhenoType();
            parentTwo.GeneratePhenoType();

        }

        #endregion
    }
}
