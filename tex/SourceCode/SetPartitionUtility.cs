using GAMultidimKnapsack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GAMultidimKnapsack
{
    public partial class SetPartition
    {

        static void Algorithm(int itemsAmount, int dimensions, double maxCost, double[] restrictions, double[] costs, double[,] itemsSet)//version w/o restarts.
        {
            int ConfigsAmount = 10;
            double mutationPercent = 0.20;//FROM 0 TO 1
            GeneticalAlgorithm ga = new GeneticalAlgorithm(itemsAmount, dimensions, restrictions, costs, itemsSet, ConfigsAmount, GeneticalAlgorithm.TwoPointCrossover, GeneticalAlgorithm.SinglePointMutation, mutationPercent);
            int iterationNumber = 0;

            while (ga.GetAbsoluteMaximalKnapsackCost() != maxCost)
            {
                //var watch = new Stopwatch();
                //watch.Start();
                ga.MakeIteration();
                iterationNumber++;
                if (iterationNumber % 10000 == 0)
                {
                    Console.WriteLine(iterationNumber + ") delta with avg is " + (maxCost - ga.GetAbsoluteAverageKnapsackCost()) + "\n delta with max is " + (maxCost - ga.GetAbsoluteMaximalKnapsackCost()));
                    var bestCosts = ga.GetBestConfigsCosts();
                    Console.WriteLine("Top 3 of the best configs pool are {0}, {1}, {2}",
                        (maxCost - bestCosts[0]),
                        (maxCost - bestCosts[1]),
                        (maxCost - bestCosts[2]));
                }
                //  watch.Stop();
            }
            Console.WriteLine("Finished in {0}", iterationNumber);
            Console.ReadKey();
        }

        static void TestAlgorithm()//First proof of concept
        {
            int itemsAmount = 500, dimensions = 6;
            double[] restrictions = new double[] { 100, 600, 1200, 2400, 500, 2000 }, costs = new double[itemsAmount];
            for (int i = 0; i < itemsAmount; i++)
                costs[i] = rand.NextDouble() * 30;
            double[,] itemsSet = new double[itemsAmount, dimensions];
            for (int i = 0; i < itemsAmount; i++)
                for (int j = 0; j < dimensions; j++)
                    itemsSet[i, j] = rand.NextDouble() * 50;
            int ConfigsAmount = 6;
            GeneticalAlgorithm ga = new GeneticalAlgorithm(itemsAmount, dimensions, restrictions, costs, itemsSet, ConfigsAmount, GeneticalAlgorithm.FixedSinglePointCrossover, GeneticalAlgorithm.SinglePointMutation, 0.75);

            int iterationNumber = 0;
            while (true)
            {
                var watch = new Stopwatch();
                watch.Start();

                while (watch.ElapsedMilliseconds < 200)
                {
                    ga.MakeIteration();
                    iterationNumber++;
                    averageValuations.Enqueue(ga.GetNormaizedAveragePoolCost());
                    maxValuations.Enqueue(ga.GetNormalizedMaximalKnapsackCost());
                }
                watch.Stop();
            }
        }
        static void ProcessTestSet(string file) //worked with first files. prowen its efficiency.TODO - check, if that true
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
                    Algorithm(itemsAmount, dimensions, maxCost, restrictions, costs, itemsSet);
                    Thread.Sleep(3000);
                    maxValuations.Enqueue(0);
                    averageValuations.Enqueue(0);
                }
            }
        }

        static List<string> multiThreadAlgorithms(int itemsAmount, int dimensions, double maxCost, double[] restrictions, double[] costs, double[,] itemsSet)//launches multiple algorithms with different start approximations. Does not work
        {
            int ConfigsAmount = 10, algorithmsNumber = 3;
            double mutationPercent = 0.20;

            GeneticalAlgorithm[] gas = new GeneticalAlgorithm[algorithmsNumber];
            for (int i = 0; i < algorithmsNumber; i++)//пока предположим, что первые приближения различны.
                gas[i] = new GeneticalAlgorithm(itemsAmount, dimensions, restrictions, costs, itemsSet, ConfigsAmount, GeneticalAlgorithm.TwoPointCrossover, GeneticalAlgorithm.SinglePointMutation, mutationPercent);
            int iterationNumber = 1;
            var controlWatch = new Stopwatch();
            controlWatch.Start();
            while (!gas.Select(x => x.GetAbsoluteMaximalKnapsackCost()).ToArray().Contains(maxCost))
            {
                Parallel.ForEach(gas, ga =>
                //foreach(GeneticalAlgorithm ga in gas)
                {
                    ga.MakeIteration();                   
                });

                iterationNumber++;
                if (iterationNumber%10==0)
                    Console.WriteLine(iterationNumber);
            }

            controlWatch.Stop();
            List<string> t=new List<string>();
            t.Add(iterationNumber.ToString());
            return t; 
        }

       // transformResults
        //    using (StreamWriter file1 = new StreamWriter(@"C:\Users\black_000\Documents\visual studio 2015\Projects\ConsoleKnapsack\ConsoleKnapsack\out.txt", true))
        //        file1.WriteLine(iterationNumber + " iterations, " + controlWatch.ElapsedMilliseconds + " ms");
        //}

    }
}
