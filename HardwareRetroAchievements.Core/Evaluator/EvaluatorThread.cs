using HardwareRetroAchievements.Core.AchievementData;
using HardwareRetroAchievements.Core.Console.SNES;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Linq;

namespace HardwareRetroAchievements.Core.Evaluator
{
    public class EvaluatorThread
    {
        class AchievementInstance
        {
            public Achievement AchievementInfo;
            public AchievementInstruction Evaluator;
            public bool WasAchieved = false;
        }

        private readonly List<AchievementInstance> _achievementInstances = new List<AchievementInstance>();
        private Thread _threadInstance;

        public CancellationTokenSource CancelToken { get; private set; } = new CancellationTokenSource();

        public int RefreshTime { get; set; }
        public SnesConsoleRam ConsoleRam { get; set; }

        public event Action<Achievement> AchievementTriggered;

        public void Start()
        {
            _threadInstance = new Thread(ThreadMain);
            _threadInstance.Start();
        }

        public void Stop()
        {
            _threadInstance?.Abort();
        }

        struct MinAddressEntry
        {
            public int Address;
            public int Size;
        }

        public void Setup(AchievementSet achievementSet)
        {
            foreach (var achievement in achievementSet.Achievements)
            {
                if (achievement.Flags == 3)
                {
                    AchievementInstance newInstance = new AchievementInstance
                    {
                        AchievementInfo = achievement,
                        Evaluator = AstToEvaluator.FromString(achievement.MemAddr)
                    };
                    _achievementInstances.Add(newInstance);
                }
            }

            List<MinAddressEntry> entries = new List<MinAddressEntry>();
            foreach (var instance in _achievementInstances)
            {
                foreach (var condition in instance.Evaluator.AllConditions())
                {
                    if (condition.CompareInstruction.Left is ReadMemoryValue leftReadAddress)
                    {
                        entries.Add(new MinAddressEntry
                        {
                            Address = leftReadAddress.Address,
                            Size = leftReadAddress.Kind.ByteSize()
                        });
                    }

                    if (condition.CompareInstruction.Right is ReadMemoryValue rightReadAddress)
                    {
                        entries.Add(new MinAddressEntry
                        {
                            Address = rightReadAddress.Address,
                            Size = rightReadAddress.Kind.ByteSize()
                        });
                    }
                }
            }

            entries.Sort((x, y) => x.Address - y.Address);

            entries = entries.Distinct().ToList();

            int maxCount = Math.Max(entries.Count, 100);
            int maxDelta = 0xF;
            for (int times = 0; times < maxCount; ++times)
            {
                int i = 0;
                while (i < entries.Count - 1)
                {
                    var left = entries[i];
                    var right = entries[i + 1];

                    var delta = right.Address - left.Address;
                    if (delta < maxDelta)
                    {
                        entries[i] = new MinAddressEntry()
                        {
                            Address = left.Address,
                            Size = Math.Max(delta + 1, right.Size + left.Size + 1),
                        };

                        entries.RemoveAt(i + 1);
                    }
                    else
                    {
                        ++i;
                    }
                }

                maxDelta = Math.Min(maxDelta + 0xF, 512);
            }

            foreach(var entry in entries)
            {
                ConsoleRam.AddCacheEntry(entry.Address, entry.Size);
            }
        }

        public async void ThreadMain()
        {
            while (!CancelToken.IsCancellationRequested)
            {
                double updateMs;
                {
                    Stopwatch updateTimer = Stopwatch.StartNew();
                    await ConsoleRam.Update(CancelToken);
                    updateTimer.Stop();

                    updateMs = updateTimer.Elapsed.TotalMilliseconds;
                }

                double evaluateMs;
                {
                    Stopwatch evaluateTimer = Stopwatch.StartNew();
                    foreach (var instance in _achievementInstances)
                    {
                        if (!instance.WasAchieved)
                        {
                            //System.Diagnostics.Debug.Write($"\nEvaluating {instance.AchievementInfo.Title}\n==============================\n");
                            if (instance.Evaluator.Evaluate(ConsoleRam))
                            {
                                instance.WasAchieved = true;
                                AchievementTriggered?.Invoke(instance.AchievementInfo);
                            }
                        }
                    }
                    evaluateTimer.Stop();

                    evaluateMs = evaluateTimer.Elapsed.TotalMilliseconds;
                }

                System.Diagnostics.Trace.WriteLine($"Update={updateMs:F2} ms Evaluate={evaluateMs:F2} ms Total={updateMs+evaluateMs} ms");
            }
        }
    }
}
