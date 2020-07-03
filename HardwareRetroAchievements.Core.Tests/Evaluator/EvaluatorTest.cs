﻿using HardwareRetroAchievements.Core.Evaluator;
using HardwareRetroAchievements.Core.Tests.Helpers;
using System.Collections.Generic;
using Xunit;

namespace HardwareRetroAchievements.Core.Tests.Evaluator
{
    public class EvaluatorTest
    {
        [Fact]
        public void ShouldEvalEqualsMemoryWithConstValue()
        {
            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[4] = 42;

            // 0xH0004 == 42
            ReadMemoryValue readMemory = new ReadMemoryValue
            {
                Address = 0x0004,
                Kind = MemoryAddressKind.Int8
            };

            ConstValue constValue = new ConstValue(42);

            CompareInstruction compareInst = new CompareInstruction
            {
                Left = readMemory,
                Right = constValue,
                Operation = ConditionCompare.Equals
            };

            var result = compareInst.Evaluate(ram);
            Assert.True(result);
        }

        [Fact]
        public void ShouldOnlyReturnTrueWhenHitCountIsReached()
        {
            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[4] = 42;

            // 0xH0004 == 42
            ReadMemoryValue readMemory = new ReadMemoryValue
            {
                Address = 0x0004,
                Kind = MemoryAddressKind.Int8
            };

            ConstValue constValue = new ConstValue(42);

            CompareInstruction compareInst = new CompareInstruction
            {
                Left = readMemory,
                Right = constValue,
                Operation = ConditionCompare.Equals
            };

            ConditionInstruction condition = new ConditionInstruction
            {
                TargetHitCount = 2,
                CompareInstruction = compareInst
            };

            Assert.False(condition.Evaluate(ram));
            Assert.True(condition.Evaluate(ram));
        }

        [Fact]
        public void ShouldEvaluateDeltaValue()
        {
            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[4] = 4;

            // 0xH0004 < d0xH004
            ReadMemoryValue readMemory = new ReadMemoryValue
            {
                Address = 0x0004,
                Kind = MemoryAddressKind.Int8
            };

            DeltaValue deltaValue = new DeltaValue(readMemory);

            CompareInstruction compareInst = new CompareInstruction
            {
                Left = readMemory,
                Right = deltaValue,
                Operation = ConditionCompare.Less
            };

            Assert.False(compareInst.Evaluate(ram));

            ram.Data[4] = 2;

            Assert.True(compareInst.Evaluate(ram));
            Assert.False(compareInst.Evaluate(ram));
        }

        [Fact]
        public void ShouldEvaluatePriorValue()
        {
            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[4] = 25;

            // 0xH0004 < p0xH004
            ReadMemoryValue readMemory = new ReadMemoryValue
            {
                Address = 0x0004,
                Kind = MemoryAddressKind.Int8
            };

            PriorValue deltaValue = new PriorValue(readMemory);

            CompareInstruction compareInst = new CompareInstruction
            {
                Left = readMemory,
                Right = deltaValue,
                Operation = ConditionCompare.Less
            };

            Assert.False(compareInst.Evaluate(ram));
            Assert.False(compareInst.Evaluate(ram));

            ram.Data[4] = 30;

            Assert.False(compareInst.Evaluate(ram));

            ram.Data[4] = 35;

            Assert.False(compareInst.Evaluate(ram));
            Assert.False(compareInst.Evaluate(ram));
            Assert.False(compareInst.Evaluate(ram));

            ram.Data[4] = 10;

            Assert.True(compareInst.Evaluate(ram));
            Assert.True(compareInst.Evaluate(ram));
        }

        [Fact]
        public void ResetIfShouldResetHitCount()
        {
            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[4] = 0;

            ReadMemoryValue levelMemoryValue = new ReadMemoryValue
            {
                Address = 0x0004,
                Kind = MemoryAddressKind.Int8
            };

            ConstValue value = new ConstValue(8);

            ConditionInstruction condition1 = new ConditionInstruction
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = levelMemoryValue,
                    Right = value,
                    Operation = ConditionCompare.Equals
                }
            };

            ConditionInstruction condition2 = new ConditionInstruction
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = levelMemoryValue,
                    Right = new DeltaValue(levelMemoryValue),
                    Operation = ConditionCompare.Greater,
                },
                TargetHitCount = 8
            };

            ResetIfConditionInstruction resetIfCondition3 = new ResetIfConditionInstruction
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = levelMemoryValue,
                    Right = new DeltaValue(levelMemoryValue),
                    Operation = ConditionCompare.Less
                }
            };

            AchievementInstruction achievementInstruction = new AchievementInstruction
            {
                Core = new ConditionGroupInstruction(new[] {
                    condition1,
                    condition2,
                    resetIfCondition3
                }
            )
            };

            Assert.False(achievementInstruction.Evaluate(ram));

            ram.Data[4] = 1;

            Assert.False(achievementInstruction.Evaluate(ram));
            Assert.Equal(1, condition2.CurrentHitCount);

            ram.Data[4] = 2;
            achievementInstruction.Evaluate(ram);
            Assert.Equal(2, condition2.CurrentHitCount);

            ram.Data[4] = 3;
            achievementInstruction.Evaluate(ram);
            Assert.Equal(3, condition2.CurrentHitCount);

            ram.Data[4] = 1;
            achievementInstruction.Evaluate(ram);

            Assert.Equal(0, condition2.CurrentHitCount);
        }

        [Fact]
        public void ShouldPauseOnPauseIfCondition()
        {
            // PauseIf mem 0x0002 == 1
            // Mem 0x0003 >= 5, Hit 100

            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[0x0002] = 0;
            ram.Data[0x0003] = 6;

            ReadMemoryValue pauseMemoryValue = new ReadMemoryValue
            {
                Address = 0x0002,
                Kind = MemoryAddressKind.Int8
            };

            ReadMemoryValue otherMemoryValue = new ReadMemoryValue
            {
                Address = 0x0003,
                Kind = MemoryAddressKind.Int8
            };

            PauseIfConditionInstruction pauseIfCondition1 = new PauseIfConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = pauseMemoryValue,
                    Right = new ConstValue(1),
                    Operation = ConditionCompare.Equals
                }
            };

            ConditionInstruction condition2 = new ConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = otherMemoryValue,
                    Right = new ConstValue(5),
                    Operation = ConditionCompare.GreaterEquals
                },
                TargetHitCount = 10
            };

            AchievementInstruction achivement = new AchievementInstruction
            {
                Core = new ConditionGroupInstruction(new[] {
                    pauseIfCondition1,
                    condition2
                })
            };

            achivement.Evaluate(ram);
            achivement.Evaluate(ram);

            Assert.Equal(2, condition2.CurrentHitCount);

            ram.Data[0x0002] = 1;

            achivement.Evaluate(ram);
            Assert.Equal(2, condition2.CurrentHitCount);

            achivement.Evaluate(ram);
            Assert.Equal(2, condition2.CurrentHitCount);
        }

        [Fact]
        public void AndNextShouldWork()
        {
            // AndNext 0x0002 == 1
            // AndNext 0x0003 == 1
            // PauseIf 0x0004 == 1
            // 0x0005 >= 2 Hit 5

            FakeConsoleRam ram = new FakeConsoleRam(0xFF);
            ram.Data[2] = 0x00;
            ram.Data[3] = 0x00;
            ram.Data[4] = 0x01;
            ram.Data[5] = 0x05;

            AndNextConditionInstruction condition1 = new AndNextConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0002,
                        Kind = MemoryAddressKind.Int8
                    },
                    Right = new ConstValue(1),
                    Operation = ConditionCompare.Equals
                }
            };

            AndNextConditionInstruction condition2 = new AndNextConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0003,
                        Kind = MemoryAddressKind.Int8,
                    },
                    Right = new ConstValue(1),
                    Operation = ConditionCompare.Equals
                }
            };

            PauseIfConditionInstruction condition3 = new PauseIfConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0004,
                        Kind = MemoryAddressKind.Int8
                    },
                    Right = new ConstValue(1),
                    Operation = ConditionCompare.Equals
                }
            };

            ConditionInstruction condition4 = new ConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0005,
                        Kind = MemoryAddressKind.Int8
                    },
                    Right = new ConstValue(2),
                    Operation = ConditionCompare.GreaterEquals
                },
                TargetHitCount = 5
            };

            AchievementInstruction achievement = new AchievementInstruction()
            {
                Core = new ConditionGroupInstruction(new[]
                {
                    condition1,
                    condition2,
                    condition3,
                    condition4
                })
            };

            achievement.Evaluate(ram);
            Assert.Equal(1, condition4.CurrentHitCount);

            ram.Data[0x0002] = 1;
            achievement.Evaluate(ram);
            Assert.Equal(2, condition4.CurrentHitCount);

            ram.Data[0x0003] = 1;
            achievement.Evaluate(ram);
            Assert.Equal(2, condition4.CurrentHitCount);
        }

        [Fact]
        public void OrNextShouldWork()
        {
            // OrNext 0x0001 == 1
            // PauseIf 0x0002 == 1
            // 0x0003 >= 2 Hit 5
            FakeConsoleRam ram = new FakeConsoleRam(0x0FF);
            ram.Data[0x0001] = 0;
            ram.Data[0x0002] = 0;
            ram.Data[0x0003] = 4;

            OrNextConditionInstruction condition1 = new OrNextConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0001,
                        Kind = MemoryAddressKind.Int8
                    },
                    Right = new ConstValue(1),
                    Operation = ConditionCompare.Equals,
                }
            };

            PauseIfConditionInstruction condition2 = new PauseIfConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0002,
                        Kind = MemoryAddressKind.Int8
                    },
                    Right = new ConstValue(1)
                }
            };

            ConditionInstruction condition3 = new ConditionInstruction()
            {
                CompareInstruction = new CompareInstruction()
                {
                    Left = new ReadMemoryValue()
                    {
                        Address = 0x0003,
                        Kind = MemoryAddressKind.Int8
                    },
                    Right = new ConstValue(2),
                    Operation = ConditionCompare.GreaterEquals
                },
                TargetHitCount = 5
            };

            AchievementInstruction achievement = new AchievementInstruction()
            {
                Core = new ConditionGroupInstruction(new[] {
                    condition1, condition2, condition3
                })
            };

            achievement.Evaluate(ram);
            Assert.Equal(1, condition3.CurrentHitCount);

            ram.Data[0x0001] = 1;
            achievement.Evaluate(ram);
            Assert.Equal(1, condition3.CurrentHitCount);

            ram.Data[0x0001] = 0;
            ram.Data[0x0002] = 1;
            achievement.Evaluate(ram);
            Assert.Equal(1, condition3.CurrentHitCount);

            ram.Data[0x0002] = 0;
            achievement.Evaluate(ram);
            Assert.Equal(2, condition3.CurrentHitCount);
        }
    }
}
