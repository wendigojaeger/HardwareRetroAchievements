using HardwareRetroAchievements.Core.Evaluator;
using Microsoft.VisualBasic;
using System;
using Xunit;

namespace HardwareRetroAchievements.Core.Tests.Evaluator
{
    public class AstToEvaluatorTest
    {
        [Theory]
        [InlineData("0xH1234==42", ConditionCompare.Equals, 42)]
        [InlineData("0xH1234>52", ConditionCompare.Greater, 52)]
        [InlineData("0xH1234>=62", ConditionCompare.GreaterEquals, 62)]
        [InlineData("0xH1234<72", ConditionCompare.Less, 72)]
        [InlineData("0xH1234<=82", ConditionCompare.LessEquals, 82)]
        [InlineData("0xH1234!=92", ConditionCompare.NotEquals, 92)]
        public void ConvertSimpleExpression(string input, ConditionCompare compareOperation, int expectedValue)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var achivementInstruction = AstToEvaluator.FromAST(tree);

            Assert.NotNull(achivementInstruction.Core);

            var conditionGroup = achivementInstruction.Core;

            Assert.Single(conditionGroup.Conditions);

            var condition = conditionGroup.Conditions[0];

            Assert.NotNull(condition.CompareInstruction);

            Assert.Equal(compareOperation, condition.CompareInstruction.Operation);

            Assert.IsType<ReadMemoryValue>(condition.CompareInstruction.Left);
            var readMemoryValue = condition.CompareInstruction.Left as ReadMemoryValue;

            Assert.Equal(0x1234, readMemoryValue.Address);
            Assert.Equal(MemoryAddressKind.Int8, readMemoryValue.Kind);

            Assert.IsType<ConstValue>(condition.CompareInstruction.Right);
            Assert.Equal(expectedValue, condition.CompareInstruction.Right.GetValue(null, null));
        }

        [Theory]
        [InlineData("0xM1234==42", MemoryAddressKind.Bit0)]
        [InlineData("0xN1234==42", MemoryAddressKind.Bit1)]
        [InlineData("0xO1234==42", MemoryAddressKind.Bit2)]
        [InlineData("0xP1234==42", MemoryAddressKind.Bit3)]
        [InlineData("0xQ1234==42", MemoryAddressKind.Bit4)]
        [InlineData("0xR1234==42", MemoryAddressKind.Bit5)]
        [InlineData("0xS1234==42", MemoryAddressKind.Bit6)]
        [InlineData("0xT1234==42", MemoryAddressKind.Bit7)]
        [InlineData("0xL1234==42", MemoryAddressKind.Lower4)]
        [InlineData("0xU1234==42", MemoryAddressKind.Upper4)]
        [InlineData("0xH1234==42", MemoryAddressKind.Int8)]
        [InlineData("0x1234==42", MemoryAddressKind.Int16)]
        [InlineData("0x 1234==42", MemoryAddressKind.Int16)]
        [InlineData("0xW1234==42", MemoryAddressKind.Int24)]
        [InlineData("0xX1234==42", MemoryAddressKind.Int32)]
        [InlineData("0xK1234==42", MemoryAddressKind.BitCount)]
        public void ConvertReadMemoryKind(string input, MemoryAddressKind expectedMemoryKind)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var achivementInstruction = AstToEvaluator.FromAST(tree);

            Assert.NotNull(achivementInstruction.Core);

            var conditionGroup = achivementInstruction.Core;

            Assert.Single(conditionGroup.Conditions);

            var condition = conditionGroup.Conditions[0];

            Assert.NotNull(condition.CompareInstruction);

            Assert.Equal(ConditionCompare.Equals, condition.CompareInstruction.Operation);

            Assert.IsType<ReadMemoryValue>(condition.CompareInstruction.Left);
            var readMemoryValue = condition.CompareInstruction.Left as ReadMemoryValue;

            Assert.Equal(0x1234, readMemoryValue.Address);
            Assert.Equal(expectedMemoryKind, readMemoryValue.Kind);

            Assert.IsType<ConstValue>(condition.CompareInstruction.Right);
            Assert.Equal(42, condition.CompareInstruction.Right.GetValue(null, null));
        }

        [Theory]
        [InlineData("d0xH1234==42", typeof(DeltaValue))]
        [InlineData("p0xH1234==42", typeof(PriorValue))]
        public void ConvertDeltaAndPriorValue(string input, Type expectedType)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var achivementInstruction = AstToEvaluator.FromAST(tree);

            Assert.NotNull(achivementInstruction.Core);

            var conditionGroup = achivementInstruction.Core;

            Assert.Single(conditionGroup.Conditions);

            var condition = conditionGroup.Conditions[0];

            Assert.NotNull(condition.CompareInstruction);

            Assert.IsType(expectedType, condition.CompareInstruction.Left);
        }

        [Theory]
        [InlineData("P:0xH1234==42", typeof(PauseIfConditionInstruction))]
        [InlineData("R:0xH1234==42", typeof(ResetIfConditionInstruction))]
        [InlineData("A:0xH1234==42", typeof(AddSourceConditionInstruction))]
        [InlineData("B:0xH1234==42", typeof(SubSourceConditionInstruction))]
        [InlineData("C:0xH1234==42", typeof(AddHitsConditionInstruction))]
        [InlineData("N:0xH1234==42", typeof(AndNextConditionInstruction))]
        [InlineData("O:0xH1234==42", typeof(OrNextConditionInstruction))]
        [InlineData("M:0xH1234==42", typeof(MeasureConditionInstruction))]
        [InlineData("Q:0xH1234==42", typeof(MeasureIfConditionInstruction))]
        [InlineData("I:0xH1234==42", typeof(AddAddressConditionInstruction))]
        public void ConvertConditionFlags(string input, Type expectedType)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var achivementInstruction = AstToEvaluator.FromAST(tree);

            Assert.NotNull(achivementInstruction.Core);

            var conditionGroup = achivementInstruction.Core;

            Assert.Single(conditionGroup.Conditions);

            var condition = conditionGroup.Conditions[0];
            Assert.IsType(expectedType, condition);
        }

        [Fact]
        public void ConvertMultipleConditions()
        {
            string input = "0xH1234==42_0xW80A0B0>0xW80A0B3";

            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var achievementInstruction = AstToEvaluator.FromAST(tree);

            Assert.NotNull(achievementInstruction.Core);

            Assert.Equal(2, achievementInstruction.Core.Conditions.Count);

            var condition2 = achievementInstruction.Core.Conditions[1];

            Assert.Equal(ConditionCompare.Greater, condition2.CompareInstruction.Operation);

            Assert.IsType<ReadMemoryValue>(condition2.CompareInstruction.Left);
            var left = condition2.CompareInstruction.Left as ReadMemoryValue;
            Assert.Equal(0x80A0B0, left.Address);
            Assert.Equal(MemoryAddressKind.Int24, left.Kind);

            Assert.IsType<ReadMemoryValue>(condition2.CompareInstruction.Right);
            var right = condition2.CompareInstruction.Right as ReadMemoryValue;
            Assert.Equal(0x80A0B3, right.Address);
            Assert.Equal(MemoryAddressKind.Int24, right.Kind);
        }

        [Fact]
        public void ConvertMultipleConditionGroup()
        {
            string input = "0xH1234==42S0xBEEF==49153S0xDEAD=65261";

            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var achievementInstruction = AstToEvaluator.FromAST(tree);

            Assert.Equal(2, achievementInstruction.Alternates.Count);

            // Alt 1
            var alt1 = achievementInstruction.Alternates[0];
            var alt1Compare = alt1.Conditions[0].CompareInstruction;

            Assert.IsType<ReadMemoryValue>(alt1Compare.Left);
            var left = alt1Compare.Left as ReadMemoryValue;
            Assert.Equal(0xBEEF, left.Address);
            Assert.Equal(MemoryAddressKind.Int16, left.Kind);

            Assert.IsType<ConstValue>(alt1Compare.Right);
            var right = alt1Compare.Right as ConstValue;
            Assert.Equal(0xC001, right.GetValue(null, null));

            // Alt 2
            var alt2 = achievementInstruction.Alternates[1];
            var alt2Compare = alt2.Conditions[0].CompareInstruction;

            Assert.IsType<ReadMemoryValue>(alt2Compare.Left);
            left = alt2Compare.Left as ReadMemoryValue;
            Assert.Equal(0xDEAD, left.Address);
            Assert.Equal(MemoryAddressKind.Int16, left.Kind);

            Assert.IsType<ConstValue>(alt2Compare.Right);
            right = alt2Compare.Right as ConstValue;
            Assert.Equal(0xFEED, right.GetValue(null, null));
        }
    }
}
