using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Net.WebRequestMethods;

namespace GAMultidimKnapsack
{
    //this whole file processes 1st file  with fixed settings.
    class Program
    {
        private static List<long>[] averageTime=new List<long>[7];
        private static List<long>[] averageIterations = new List<long>[7];

        static void Algorithm(int itemsAmount, int dimensions, double maxCost, double[] restrictions, double[] costs, double[,] itemsSet, int testNumber)
        {
            int ConfigsAmount = 8;
            double mutationPercent = 0.75;
            GeneticalAlgorithm ga = new GeneticalAlgorithm(itemsAmount, dimensions, restrictions, costs, itemsSet, ConfigsAmount, GeneticalAlgorithm.BitByBitCrossover, GeneticalAlgorithm.MutateHalf, mutationPercent);
            int iterationNumber = 1;
            var controlWatch = new Stopwatch();
            controlWatch.Start();  
            while (ga.GetAbsoluteMaximalKnapsackCost() != maxCost)
            {
                ga.MakeIteration();
                iterationNumber++;
            }
            controlWatch.Stop();
            using (StreamWriter file1 = new StreamWriter(@"C:\Users\black_000\Documents\visual studio 2015\Projects\ConsoleKnapsack\ConsoleKnapsack\out.txt", true))
                file1.WriteLine(iterationNumber + " iterations, " + controlWatch.ElapsedMilliseconds + " ms");
            //averageTime[testNumber].Add(controlWatch.ElapsedMilliseconds);
            //averageIterations[testNumber].Add(iterationNumber);
        }

       

        static void ProcessTestSet(string file)
        {
            using (StreamReader sr = new StreamReader(file))
            {
                int experimentsAmount = Convert.ToInt32(sr.ReadLine());

                for (int experiment = 0; experiment < experimentsAmount; experiment++)
                {
                    string[] initializationSequence;
                    string firstString = sr.ReadLine();
                    if (firstString.Trim() == "")
                        initializationSequence = sr.ReadLine().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    else initializationSequence = firstString.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); ;
                    int itemsAmount = Convert.ToInt32(initializationSequence[0]),
                    dimensions = Convert.ToInt32(initializationSequence[1]);
                    double maxCost = Convert.ToDouble(initializationSequence[2]);

                    List<double> tempCosts = new List<double>();
                    while (tempCosts.Count() != itemsAmount)
                        tempCosts.AddRange(sr
                            .ReadLine()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => Convert.ToDouble(x))
                            .ToList());
                    double[] costs = tempCosts.ToArray();

                    double[,] itemsSet = new double[itemsAmount, dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        int itemsReaden = 0;
                        while (itemsReaden != itemsAmount)
                        {
                            double[] currentString = sr.ReadLine()
                                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).
                                Select(x => Convert.ToDouble(x)).
                                ToArray();
                            for (int j = itemsReaden, k = 0; j < currentString.Count() + itemsReaden; j++, k++)
                                itemsSet[j, i] = currentString[k];
                            itemsReaden += currentString.Count();
                        }
                    }
                    List<double> tempRestrictions = new List<double>();
                    while (tempRestrictions.Count() != dimensions)
                        tempRestrictions.AddRange(sr
                            .ReadLine()
                            .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => Convert.ToDouble(x))
                            .ToList());
                    double[] restrictions = tempRestrictions.ToArray();
                    //using (StreamWriter file1 = new StreamWriter(@"C:\Users\black_000\Documents\visual studio 2015\Projects\ConsoleKnapsack\ConsoleKnapsack\out.txt", true))
                    //{
                    //    file1.Write(experiment + 1 + ") ");
                    //}
                    Algorithm(itemsAmount, dimensions, maxCost, restrictions, costs, itemsSet, experiment);

                    // Thread.Sleep(3000);
                }
            }
        }

        static void Main2(string[] args)
        {
                ProcessTestSet(@"C:\Users\black_000\Source\Repos\GeneticKnapsack\GAMultidimKnapsack\1.txt");
            
            //for (var i = 0; i < 7; i++)
            //{
            //    resultsTime[i] = averageTime[i].Sum() / testsAmount;
            //    resultsIterations[i] = averageIterations[i].Sum() / testsAmount;
            //}

            //using (StreamWriter file = new StreamWriter(@"C:\Users\black_000\Documents\visual studio 2015\Projects\ConsoleKnapsack\ConsoleKnapsack\out.txt", true))
            //{
            //    file.WriteLine("Iterations:");
            //    for (var i = 0; i < resultsTime.Length; i++)
            //        file.WriteLine(resultsTime[i]);
            //    file.WriteLine("Time:");
            //    for (var i = 0; i < resultsTime.Length; i++)
            //        file.WriteLine(resultsIterations[i]);
            //    file.Close();
            //}
        }
    }
}

