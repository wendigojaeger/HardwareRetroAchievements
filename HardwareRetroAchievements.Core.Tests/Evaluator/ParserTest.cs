using HardwareRetroAchievements.Core.Evaluator;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HardwareRetroAchievements.Core.Tests.Evaluator
{
    public class ParserTest
    {
        [Theory]
        [InlineData("0xH1234=8", ConditionCompare.Equals)]
        [InlineData("0xH1234==8", ConditionCompare.Equals)]
        [InlineData("0xH1234!=8", ConditionCompare.NotEquals)]
        [InlineData("0xH1234<8", ConditionCompare.Less)]
        [InlineData("0xH1234<=8", ConditionCompare.LessEquals)]
        [InlineData("0xH1234>8", ConditionCompare.Greater)]
        [InlineData("0xH1234>=8", ConditionCompare.GreaterEquals)]
        public void ShouldParseComparisonOperators(string input, ConditionCompare compareOp)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(compareOp, condition.CompareOperator);
            Assert.Equal(0, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<NumberAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            var right = condition.Right as NumberAST;
            Assert.Equal(0x1234, left.Address);
            Assert.Equal(MemoryAddressKind.Int8, left.Kind);
            Assert.Equal(OperandType.Standard, left.Type);
            Assert.Equal(OperandFlag.None, left.Flag);

            Assert.Equal(8, right.Value);
        }
        
        [Theory]
        [InlineData("d0xH1234=8", OperandType.Delta)]
        [InlineData("p0xH1234=8", OperandType.Prior)]
        public void ShouldParseOperandType(string input, OperandType operandType)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(ConditionCompare.Equals, condition.CompareOperator);
            Assert.Equal(0, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<NumberAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            var right = condition.Right as NumberAST;
            Assert.Equal(0x1234, left.Address);
            Assert.Equal(MemoryAddressKind.Int8, left.Kind);
            Assert.Equal(operandType, left.Type);
            Assert.Equal(OperandFlag.None, left.Flag);

            Assert.Equal(8, right.Value);
        }

        [Theory]
        [InlineData("R:0xH1234=8", OperandFlag.ResetIf)]
        [InlineData("P:0xH1234=8", OperandFlag.PauseIf)]
        [InlineData("A:0xH1234=8", OperandFlag.AddSource)]
        [InlineData("B:0xH1234=8", OperandFlag.SubSource)]
        [InlineData("C:0xH1234=8", OperandFlag.AddHit)]
        [InlineData("M:0xH1234=8", OperandFlag.Measured)]
        [InlineData("Q:0xH1234=8", OperandFlag.MeasuredIf)]
        [InlineData("I:0xH1234=8", OperandFlag.AddAddress)]
        [InlineData("N:0xH1234=8", OperandFlag.ANDNext)]
        [InlineData("O:0xH1234=8", OperandFlag.ORNext)]
        public void ShouldParseOperandFlags(string input, OperandFlag operandFlag)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(ConditionCompare.Equals, condition.CompareOperator);
            Assert.Equal(0, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<NumberAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            var right = condition.Right as NumberAST;
            Assert.Equal(0x1234, left.Address);
            Assert.Equal(MemoryAddressKind.Int8, left.Kind);
            Assert.Equal(OperandType.Standard, left.Type);
            Assert.Equal(operandFlag, left.Flag);

            Assert.Equal(8, right.Value);
        }

        [Theory]
        [InlineData("0xH1234=8(1)", 1)]
        [InlineData("0xH1234=8.1.", 1)]
        [InlineData("0xH1234=8(1000)", 1000)]
        public void ShouldParseHitCount(string input, int hitCount)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(ConditionCompare.Equals, condition.CompareOperator);
            Assert.Equal(hitCount, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<NumberAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            var right = condition.Right as NumberAST;
            Assert.Equal(0x1234, left.Address);
            Assert.Equal(MemoryAddressKind.Int8, left.Kind);
            Assert.Equal(OperandType.Standard, left.Type);
            Assert.Equal(OperandFlag.None, left.Flag);

            Assert.Equal(8, right.Value);
        }

        [Theory]
        [InlineData("0xH1234=8", 0x1234)]
        [InlineData("0xH5678=8", 0x5678)]
        [InlineData("0xH9ABC=8", 0x9ABC)]
        [InlineData("0xHDEFF=8", 0xDEFF)]
        [InlineData("0xH801234=8", 0x801234)]
        [InlineData("0xH20801234=8", 0x20801234)]
        public void ShouldParseMemoryAddress(string input, long address)
        {

            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(ConditionCompare.Equals, condition.CompareOperator);
            Assert.Equal(0, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<NumberAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            var right = condition.Right as NumberAST;
            Assert.Equal(address, left.Address);
            Assert.Equal(MemoryAddressKind.Int8, left.Kind);
            Assert.Equal(OperandType.Standard, left.Type);
            Assert.Equal(OperandFlag.None, left.Flag);

            Assert.Equal(8, right.Value);
        }

        [Theory]
        [InlineData("0xM1234=8", MemoryAddressKind.Bit0)]
        [InlineData("0xN1234=8", MemoryAddressKind.Bit1)]
        [InlineData("0xO1234=8", MemoryAddressKind.Bit2)]
        [InlineData("0xP1234=8", MemoryAddressKind.Bit3)]
        [InlineData("0xQ1234=8", MemoryAddressKind.Bit4)]
        [InlineData("0xR1234=8", MemoryAddressKind.Bit5)]
        [InlineData("0xS1234=8", MemoryAddressKind.Bit6)]
        [InlineData("0xT1234=8", MemoryAddressKind.Bit7)]
        [InlineData("0xL1234=8", MemoryAddressKind.Lower4)]
        [InlineData("0xU1234=8", MemoryAddressKind.Upper4)]
        [InlineData("0xH1234=8", MemoryAddressKind.Int8)]
        [InlineData("0x 1234=8", MemoryAddressKind.Int16)]
        [InlineData("0x1234=8", MemoryAddressKind.Int16)]
        [InlineData("0xW1234=8", MemoryAddressKind.Int24)]
        [InlineData("0xX1234=8", MemoryAddressKind.Int32)]
        [InlineData("0xK1234=8", MemoryAddressKind.BitCount)]
        public void ShouldParseMemoryAddressKind(string input, MemoryAddressKind addressKind)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(ConditionCompare.Equals, condition.CompareOperator);
            Assert.Equal(0, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<NumberAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            var right = condition.Right as NumberAST;
            Assert.Equal(0x1234, left.Address);
            Assert.Equal(addressKind, left.Kind);
            Assert.Equal(OperandType.Standard, left.Type);
            Assert.Equal(OperandFlag.None, left.Flag);

            Assert.Equal(8, right.Value);
        }

        [Theory]
        [InlineData("0x1234=0xH4567", MemoryAddressKind.Int16, 0x1234, MemoryAddressKind.Int8, 0x4567)]
        [InlineData("0xL4567=0xXBEEF", MemoryAddressKind.Lower4, 0x4567, MemoryAddressKind.Int32, 0xBEEF)]
        public void ShouldParseMemoryToMemoryComparison(string input, MemoryAddressKind leftKind, long leftAddress, MemoryAddressKind rightKind, long rightAddress)
        {
            Parser parser = new Parser();
            var tree = parser.Parse(input);

            var condition = tree.Core.Conditions[0];

            Assert.Equal(ConditionCompare.Equals, condition.CompareOperator);
            Assert.Equal(0, condition.HitCount);
            Assert.IsType<MemoryAddressAST>(condition.Left);
            Assert.IsType<MemoryAddressAST>(condition.Right);

            var left = condition.Left as MemoryAddressAST;
            Assert.Equal(leftAddress, left.Address);
            Assert.Equal(leftKind, left.Kind);
            Assert.Equal(OperandType.Standard, left.Type);
            Assert.Equal(OperandFlag.None, left.Flag);

            var right = condition.Right as MemoryAddressAST;
            Assert.Equal(rightAddress, right.Address);
            Assert.Equal(rightKind, right.Kind);
            Assert.Equal(OperandType.Standard, right.Type);
            Assert.Equal(OperandFlag.None, right.Flag);
        }

        [Theory]
        [InlineData("H0x1234==0")]
        [InlineData("0xDEAD")]
        [InlineData("0x1234=1.2")]
        [InlineData("0.1234==0")]
        [InlineData("0==0.1234")]
        public void ShoudlThrowExceptionOnParserError(string input)
        {
            Parser parser = new Parser();
            Assert.Throws<ParserException>(() => parser.Parse(input));
        }
    }
}
