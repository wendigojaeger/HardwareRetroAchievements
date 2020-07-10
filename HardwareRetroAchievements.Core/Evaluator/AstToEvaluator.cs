using HardwareRetroAchievements.Core.AchievementData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HardwareRetroAchievements.Core.Evaluator
{
    public static class AstToEvaluator
    {
        public static AchievementInstruction FromAST(RootAST rootAst)
        {
            return enterRootAST(rootAst);
        }

        private static AchievementInstruction enterRootAST(RootAST rootAst)
        {
            AchievementInstruction achievement = new AchievementInstruction();

            achievement.Core = enterConditionGroupAST(rootAst.Core);

            foreach (var alt in rootAst.Alternates)
            {
                achievement.Alternates.Add(enterConditionGroupAST(alt));
            }
            return achievement;
        }

        private static ConditionGroupInstruction enterConditionGroupAST(ConditionGroupAST conditionGroupAst)
        {
            ConditionGroupInstruction conditionGroup = new ConditionGroupInstruction();

            foreach (var condition in conditionGroupAst.Conditions)
            {
                conditionGroup.Conditions.Add(enterConditionAST(condition));
            }

            return conditionGroup;
        }

        private static ConditionInstruction enterConditionAST(ConditionAST conditionAst)
        {
            ConditionInstruction condition = createConditionInstruction(conditionAst);

            condition.CompareInstruction = new CompareInstruction()
            {
                Left = enterOperand(conditionAst.Left),
                Right = enterOperand(conditionAst.Right),
                Operation = conditionAst.CompareOperator,
            };

            condition.TargetHitCount = conditionAst.HitCount;

            return condition;
        }

        private static Value enterOperand(OperandAST operandAst)
        {
            return operandAst.Type switch
            {
                OperandType.Delta => new DeltaValue(createValue(operandAst)),
                OperandType.Prior => new PriorValue(createValue(operandAst)),
                _ => createValue(operandAst),
            };
        }

        private static Value createValue(OperandAST operandAst)
        {
            return operandAst switch
            {
                MemoryAddressAST memory => new ReadMemoryValue()
                {
                    Address = memory.Address,
                    Kind = memory.Kind
                },
                NumberAST number => new ConstValue((int)number.Value),
                _ => null,
            };
        }

        private static ConditionInstruction createConditionInstruction(ConditionAST conditionAst)
        {
            return conditionAst.Left.Flag switch
            {
                OperandFlag.AddAddress => new AddAddressConditionInstruction(),
                OperandFlag.AddHits => new AddHitsConditionInstruction(),
                OperandFlag.AddSource => new AddSourceConditionInstruction(),
                OperandFlag.ANDNext => new AndNextConditionInstruction(),
                OperandFlag.Measured => new MeasureConditionInstruction(),
                OperandFlag.MeasuredIf => new MeasureIfConditionInstruction(),
                OperandFlag.ORNext => new OrNextConditionInstruction(),
                OperandFlag.PauseIf => new PauseIfConditionInstruction(),
                OperandFlag.ResetIf => new ResetIfConditionInstruction(),
                OperandFlag.SubSource => new SubSourceConditionInstruction(),
                _ => new ConditionInstruction(),
            };
        }
    }
}
