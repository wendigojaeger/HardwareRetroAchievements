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
    }
}
