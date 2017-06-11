using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAMultidimKnapsack
{
    class GeneticalAlgorithm
    {
        private static int itemsAmount, dimensions;
        private static double[,] itemsSet;//amount of items*their dimensions
        private static double[] restrictions;
        private double[] itemsCosts;

        private int configsInPoolAmount;
        private int bestConfigsAmount;
        private KnapsackConfig[] configsPool;
        private KnapsackConfig[] currentBestConfigs;//however, this pool is resetted over and over again. We should create a permanent pool for practical use. Possibliy
        private KnapsackConfig[] bestConfigsAllTime;
        private double maximalKnapsackCost;

        private Crossover activeCrossover;
        private Mutation activeMutation;
        private static Random rand;
        private double mutationPercentage;

        public GeneticalAlgorithm(int itemsAm, int dim, double[] rest, double[] costs, double[,] myItemsSet, int confAm, Crossover myCrs, Mutation myMt, double mutationPercentage)
        {
            itemsAmount = itemsAm;
            restrictions = rest;
            dimensions = dim;
            itemsSet = new double[itemsAm, dim];
            rand = new Random();

            itemsSet = myItemsSet;
            itemsCosts = costs;
            configsInPoolAmount = confAm;

            activeCrossover = myCrs;
            activeMutation = myMt;

            bestConfigsAmount = configsInPoolAmount;

            configsPool = new KnapsackConfig[configsInPoolAmount];
            this.mutationPercentage = mutationPercentage;
            maximalKnapsackCost = itemsCosts.Sum();

            StartCycling();
        }

        private void StartCycling()
        {
            try
            {
                configsPool[0] = FirstApproachGenerate();

                int active = 0, passive = 0;
                for (int i = 0; i < itemsAmount; i++)
                {
                    if (configsPool[0].isValueActive(i))
                        active++;
                    else passive++;
                }
                if (active == itemsAmount || passive == itemsAmount)
                    return;
                for (int i = 1; i < configsInPoolAmount; i++)
                {
                    configsPool[i] = activeMutation(configsPool[0], rand);
                }
                emptyBestConfigs(ref currentBestConfigs);
                emptyBestConfigs(ref bestConfigsAllTime);

                //int[] emptyConfig = (new int[itemsAmount]).Select(x => 0).ToArray();
                //bestConfigs = (new KnapsackConfig[bestConfigsAmount]).Select(x => new KnapsackConfig(emptyConfig)).ToArray();//HACK
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bugs in initialization");
                return;
            }
        }

        void emptyBestConfigs(ref KnapsackConfig[] targetConfig)
        {
            int[] emptyConfig = (new int[itemsAmount]).Select(x => 0).ToArray();
            targetConfig= (new KnapsackConfig[bestConfigsAmount]).Select(x => new KnapsackConfig(emptyConfig)).ToArray();
        }
        public void RestartAlgorithm(double flushPercent)
        {
            try
            {
                IndexOutOfRangeException ex = new IndexOutOfRangeException();//TODO: write exceptions class;
                if (flushPercent < 0 || flushPercent > 1)
                    throw ex;
                else
                {
                    //int[] emptyConfig = (new int[itemsAmount]).Select(x => 0).ToArray();
                    //bestConfigs = (new KnapsackConfig[bestConfigsAmount]).Select(x => new KnapsackConfig(emptyConfig)).ToArray();//HACK
                    emptyBestConfigs(ref currentBestConfigs);
                    int startFlushpoint = rand.Next(0, itemsAmount), endPoint, itemsToFlush = (int)(itemsAmount * flushPercent);
                    if (startFlushpoint + itemsToFlush >= itemsAmount)
                    {
                        endPoint = itemsToFlush - (itemsAmount - startFlushpoint);
                        foreach (var conf in configsPool)
                        {
                            for (int i = startFlushpoint; i < itemsAmount; i++)
                                conf.setValueToPassive(i);
                            for (int i = 0; i < endPoint; i++)
                                conf.setValueToPassive(i);
                        }
                    }
                    else
                    {
                        endPoint = startFlushpoint + itemsToFlush;
                        foreach (var conf in configsPool)
                        {
                            for (int i = startFlushpoint; i < endPoint; i++)
                                conf.setValueToPassive(i);
                        }
                    }
                    //Сбрасываем целиком BestConfigs
                    //Выносим flushpercent для текущих конфигураций в 0. Конфигурации корректны.
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Flush percent is in [0;1]");
            }
        }
        public void MakeIteration()
        {
            if (GetKnapsackCost(configsPool[0]) == maximalKnapsackCost) return;
            List<int> positions = new List<int>();
            while (positions.Count < mutationPercentage * configsInPoolAmount)
            {
                positions.Add(rand.Next(configsInPoolAmount));
                positions.Distinct();
            }
            for (int i = 0; i < configsInPoolAmount; i++)
            {
                configsPool[i] = activeMutation(configsPool[i], rand);
            }
            KnapsackConfig[] CrossoverPool = new KnapsackConfig[configsInPoolAmount * 2 - 2];//not very well, if i want to customize Crossover ,but works
            for (int j = 0; j < configsInPoolAmount - 1; j++)
            {
                CrossoverPool[j] = activeCrossover(configsPool[j], configsPool[j + 1], true);
                CrossoverPool[(CrossoverPool.Length - 1) - j] = activeCrossover(configsPool[j], configsPool[j + 1], false);
            }
            var tempConfigs = CrossoverPool
                .OrderByDescending(config => GetKnapsackCost(config))
                .Take(Convert.ToInt32(configsInPoolAmount))
                .ToArray();
            configsPool = tempConfigs;

            var tuningCoeff = 0.01;
            updateConfigs(ref currentBestConfigs, tuningCoeff);
            updateConfigs(ref bestConfigsAllTime, tuningCoeff);
        }

        private void updateConfigs(ref KnapsackConfig[] currentPool, double tuningCoeff)
        {
            if (currentPool.Length >= configsPool.Length &&
    GetKnapsackCost(currentPool[0]) * (1 - tuningCoeff) > GetKnapsackCost(configsPool[0]))
            {
                for (int i = 0; i < configsInPoolAmount; i++)
                    configsPool[i] = new KnapsackConfig(currentPool[i]);
                return;
            }
            currentPool = currentPool
                .Concat(configsPool)
                .OrderByDescending(config => GetKnapsackCost(config))
                .Distinct()
                .Take(bestConfigsAmount)
                .ToArray();
        }
        private KnapsackConfig FirstApproachGenerate()
        {
            KnapsackConfig result = new KnapsackConfig(itemsAmount);

            for (var i = 0; i < itemsAmount; i++)
            {
                result.setValueToActive(i);
            }
            Random rand = new Random();
            while (!IsValid(result))
            {
                int positionNumber = rand.Next(itemsAmount);
                while (!result.isValueActive(positionNumber))
                {
                    positionNumber = rand.Next(itemsAmount);
                }
                result.setValueToPassive(positionNumber);
            }
            return result;
        }

        public delegate KnapsackConfig Crossover(KnapsackConfig sack1, KnapsackConfig sack2, bool isLeft);

        public static KnapsackConfig FixedSinglePointCrossover(KnapsackConfig sack1, KnapsackConfig sack2, bool isLeft)
        {
            int[] crossItems = new int[itemsAmount];
            if (isLeft)
            {
                for (var i = 0; i < itemsAmount / 2; i++)
                    crossItems[i] = sack2.valueAt(i);
                for (var i = itemsAmount / 2; i < itemsAmount; i++)
                    crossItems[i] = sack1.valueAt(i);
            }
            else
            {
                for (var i = 0; i < itemsAmount / 2; i++)
                    crossItems[i] = sack1.valueAt(i);
                for (var i = itemsAmount / 2; i < itemsAmount; i++)
                    crossItems[i] = sack2.valueAt(i);
            }

            KnapsackConfig CrossoverResult = new KnapsackConfig(crossItems);
            if (!IsValid(CrossoverResult))
                CrossoverResult = MakeValid(CrossoverResult);

            return CrossoverResult;
        }

        public static KnapsackConfig SinglePointCrossover(KnapsackConfig sack1, KnapsackConfig sack2, bool isLeft)
        {
            int[] crossItems = new int[itemsAmount];
            int CrossoverPoint = rand.Next(itemsAmount);
            if (isLeft)
            {
                for (var i = 0; i < CrossoverPoint; i++)
                    crossItems[i] = sack2.valueAt(i);
                for (var i = CrossoverPoint; i < itemsAmount; i++)
                    crossItems[i] = sack1.valueAt(i);
            }
            else
            {
                {
                    for (var i = 0; i < CrossoverPoint; i++)
                        crossItems[i] = sack1.valueAt(i);
                    for (var i = CrossoverPoint; i < itemsAmount; i++)
                        crossItems[i] = sack2.valueAt(i);
                }
            }

            KnapsackConfig CrossoverResult = new KnapsackConfig(crossItems);
            if (!IsValid(CrossoverResult))
                CrossoverResult = MakeValid(CrossoverResult);
            return CrossoverResult;
        }

        public static KnapsackConfig BitByBitCrossover(KnapsackConfig sack1, KnapsackConfig sack2, bool isLeft)
        {
            int[] crossItems = new int[itemsAmount];
            if (isLeft)
            {
                for (var i = 0; i < itemsAmount; i++)
                {
                    if (i % 2 == 0)
                        crossItems[i] = sack2.valueAt(i);
                    else
                        crossItems[i] = sack1.valueAt(i);
                }
            }
            else
            {
                for (var i = 0; i < itemsAmount; i++)
                {
                    if (i % 2 == 0)
                        crossItems[i] = sack1.valueAt(i);
                    else
                        crossItems[i] = sack2.valueAt(i);
                }
            }

            KnapsackConfig CrossoverResult = new KnapsackConfig(crossItems);
            if (!IsValid(CrossoverResult))
                CrossoverResult = MakeValid(CrossoverResult);
            return CrossoverResult;
        }

        public static KnapsackConfig TwoPointCrossover(KnapsackConfig sack1, KnapsackConfig sack2, bool isLeft)
        {
            int firstPoint = rand.Next(itemsAmount - 1), secondPoint = rand.Next(firstPoint + 1, itemsAmount);
            int[] crossItems = new int[itemsAmount];
            if (isLeft)
            {
                for (var i = 0; i < firstPoint; i++)
                    crossItems[i] = sack1.valueAt(i);
                for (var i = firstPoint; i < secondPoint; i++)
                    crossItems[i] = sack2.valueAt(i);
                for (var i = secondPoint; i < itemsAmount; i++)
                    crossItems[i] = sack1.valueAt(i);
            }
            else
            {
                for (var i = 0; i < firstPoint; i++)
                    crossItems[i] = sack2.valueAt(i);
                for (var i = firstPoint; i < secondPoint; i++)
                    crossItems[i] = sack1.valueAt(i);
                for (var i = secondPoint; i < itemsAmount; i++)
                    crossItems[i] = sack2.valueAt(i);
            }
            KnapsackConfig sack = new KnapsackConfig(crossItems);
            if (!IsValid(sack))
                return (MakeValid(sack));
            return sack;
        }

        public delegate KnapsackConfig Mutation(KnapsackConfig sack, Random rand);

        public static KnapsackConfig SinglePointMutation(KnapsackConfig sack, Random rand)
        {
            KnapsackConfig mutatedSack = new KnapsackConfig(sack);//copy constructor
            int mutationPosition = rand.Next(itemsAmount);
            var count = 0;
            var iterationsToResque = 100000;
            while (mutatedSack.Equals(sack) && count < iterationsToResque)//TODO - not mutate empty sack
            {
                mutatedSack.swapValue(mutationPosition);
                if (!IsValid(mutatedSack))//somehow unrealistic
                {
                    mutatedSack.swapValue(mutationPosition);
                    mutationPosition = rand.Next(itemsAmount);
                }
                count++;
            }
            if (count == iterationsToResque)
            {
                return MakeValid(mutatedSack);
            }
            return mutatedSack;
        }

        public static KnapsackConfig MutateHalf(KnapsackConfig sack, Random rand)
        {
            KnapsackConfig mutatedSack = new KnapsackConfig(sack);
            int mutationPosition = rand.Next(itemsAmount);
            if (rand.Next() % 2 == 0)
            {
                for (var i = 0; i < itemsAmount / 2; i++)
                {
                    mutatedSack.swapValue(i);
                }
            }
            else
            {
                for (var i = itemsAmount / 2; i < itemsAmount; i++)
                {
                    mutatedSack.swapValue(i);
                }
            }
            if (!IsValid(mutatedSack))
                return (MakeValid(mutatedSack));
            return (mutatedSack);
        }
        private static bool IsValid(KnapsackConfig config)
        {
            double[] summ = new double[dimensions];
            for (var i = 0; i < itemsAmount; i++)
            {
                if (config.isValueActive(i))
                {
                    for (var j = 0; j < dimensions; j++)
                    {
                        summ[j] += itemsSet[i, j];
                        if (summ[j] > restrictions[j]) return false;
                    }
                }
                //Amount of items is much bigger than number of dimensions, so we can do checks on every turn. 
            }
            return true;
        }

        private double GetKnapsackCost(KnapsackConfig sack)
        {
            double count = 0;
            for (int i = 0; i < itemsAmount; i++)
                if (sack.isValueActive(i))
                    count += itemsCosts[i];

            return count;
        }

        private static KnapsackConfig MakeValid(KnapsackConfig sack)
        {
            for (var i = 0; i < sack.Length() && !IsValid(sack); i++)
            {
                sack.setValueToPassive(i);
            }
            return sack;
        }

        //methods for current active pool
        public double GetNormalizedMaximalKnapsackCost()
        {
            return GetAbsoluteMaximalKnapsackCost() / maximalKnapsackCost;
        }

        public double GetNormaizedAveragePoolCost()
        {
            return GetAbsoluteAverageKnapsackCost() / maximalKnapsackCost;
        }

        public double GetAbsoluteMaximalKnapsackCost()
        {
            return GetKnapsackCost(configsPool[0]);
        }

        public double GetAbsoluteAverageKnapsackCost()
        {
            if (GetKnapsackCost(configsPool[0]) == maximalKnapsackCost) return GetKnapsackCost(configsPool[0]);
            double averagePoolCost = 0;
            foreach (var config in configsPool)
            {
                averagePoolCost += GetKnapsackCost(config);
            }
            averagePoolCost /= configsInPoolAmount;
            return averagePoolCost;
        }

        public List<double> GetBestConfigsCosts()
        {
            return currentBestConfigs.Select(x => GetKnapsackCost(x)).ToList();
        }

        //methods for best all-time configs
        public double GetAbsoluteMaximalCostAllTime()
        {
            return GetKnapsackCost(bestConfigsAllTime[0]);//does it work is uncertain
        }
    }
}
