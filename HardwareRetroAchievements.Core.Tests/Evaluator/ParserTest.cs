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
        [InlineData("0xH1234=8")]
        [InlineData("0xH1234==8")]
        public void ShouldParseEqualOperator(string input)
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
            Assert.Equal(OperandFlag.None, left.Flag);

            Assert.Equal(8, right.Value);
        }
    }
}
