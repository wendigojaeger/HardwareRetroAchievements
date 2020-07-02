using HardwareRetroAchievements.Core.Evaluator;
using HardwareRetroAchievements.Core.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
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

            ConditionInstruction condition = new ConditionInstruction();
            condition.TargetHitCount = 2;
            condition.CompareInstruction = compareInst;

            Assert.False(condition.Evaluate(ram).Succeeded);
            Assert.True(condition.Evaluate(ram).Succeeded);
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

            ConditionInstruction condition1 = new ConditionInstruction();
            condition1.CompareInstruction = new CompareInstruction()
            {
                Left = levelMemoryValue,
                Right = value,
                Operation = ConditionCompare.Equals
            };

            ConditionInstruction condition2 = new ConditionInstruction();
            condition2.CompareInstruction = new CompareInstruction()
            {
                Left = levelMemoryValue,
                Right = new DeltaValue(levelMemoryValue),
                Operation = ConditionCompare.Greater,
            };
            condition2.TargetHitCount = 8;

            ResetIfConditionInstruction resetIfCondition3 = new ResetIfConditionInstruction();
            resetIfCondition3.CompareInstruction = new CompareInstruction()
            {
                Left = levelMemoryValue,
                Right = new DeltaValue(levelMemoryValue),
                Operation = ConditionCompare.Less
            };

            AchievementInstruction achievementInstruction = new AchievementInstruction();
            achievementInstruction.Core = new ConditionGroupInstruction()
            {
                Conditions = new List<ConditionInstruction>(new[] {
                    condition1,
                    condition2,
                    resetIfCondition3
                })
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
    }
}
