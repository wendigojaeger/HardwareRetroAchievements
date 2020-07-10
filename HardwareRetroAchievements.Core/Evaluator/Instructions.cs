using HardwareRetroAchievements.Core.Console;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Numerics;

namespace HardwareRetroAchievements.Core.Evaluator
{
    public class EvaluatorContext
    {
        public bool? AndNext = null;
        public bool? OrNext = null;
        public int? AddValue = null;
        public int? AddHits = null;
        public long? AddAddress = null;
        public int? MeasuredValue = null;
        public int? MeasuredTarget = null;
        public bool CanMeasure = true;
        public int TempMeasureValue = 0;
        public int TempTotalHit = 0;
    }

    public enum ReturnAction
    {
        DoNothing,
        ClearAllHitCounts,
        PauseConditionGroup,
        AndNext,
    }

    public abstract class Value
    {
        public abstract int GetValue(IConsoleRam ram, EvaluatorContext context);
    }

    public class ConstValue : Value
    {
        private readonly int _constValue;

        public ConstValue(int value)
        {
            _constValue = value;
        }

        public override int GetValue(IConsoleRam ram, EvaluatorContext context)
        {
            return _constValue;
        }
    }

    public class DeltaValue : Value
    {
        private readonly Value _child;
        private int _previousValue = 0;

        public DeltaValue(Value child)
        {
            _child = child;
        }

        public override int GetValue(IConsoleRam ram, EvaluatorContext context)
        {
            var current = _child.GetValue(ram, context);
            var result = _previousValue;
            _previousValue = current;
            return result;
        }
    }

    public class PriorValue : Value
    {
        private readonly Value _child;
        private int _previousValue = 0;
        private int _priorValue = 0;

        public PriorValue(Value child)
        {
            _child = child;
        }

        public override int GetValue(IConsoleRam ram, EvaluatorContext context)
        {
            var current = _child.GetValue(ram, context);

            if (_previousValue != current)
            {
                _priorValue = _previousValue;
            }

            _previousValue = current;

            return _priorValue;
        }
    }

    public class ReadMemoryValue : Value
    {
        public long Address { get; set; }
        public MemoryAddressKind Kind { get; set; }

        public override int GetValue(IConsoleRam ram, EvaluatorContext context)
        {
            var effectiveAddress = Address;
            if (context.AddAddress.HasValue)
            {
                effectiveAddress += context.AddAddress.Value;
                context.AddAddress = null;
            }

            switch (Kind)
            {
                case MemoryAddressKind.Bit0:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return (value & (1 << 0)) >> 0;
                    }
                case MemoryAddressKind.Bit1:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 1)) >> 1);
                    }
                case MemoryAddressKind.Bit2:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 2)) >> 2);
                    }
                case MemoryAddressKind.Bit3:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 3)) >> 3);
                    }
                case MemoryAddressKind.Bit4:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 4)) >> 4);
                    }
                case MemoryAddressKind.Bit5:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 5)) >> 5);
                    }
                case MemoryAddressKind.Bit6:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 6)) >> 6);
                    }
                case MemoryAddressKind.Bit7:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & (1 << 7)) >> 7);
                    }
                case MemoryAddressKind.Lower4:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return (value & 0x0F);
                    }
                case MemoryAddressKind.Upper4:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return ((value & 0xF0) >> 4);
                    }
                case MemoryAddressKind.Int8:
                    {
                        return ram.ReadInt8(effectiveAddress);
                    }
                case MemoryAddressKind.Int16:
                    {
                        return ram.ReadInt16(effectiveAddress);
                    }
                case MemoryAddressKind.Int24:
                    {
                        return (int)ram.ReadUInt24(effectiveAddress);
                    }
                case MemoryAddressKind.Int32:
                    {
                        return (int)ram.ReadUInt32(effectiveAddress);
                    }
                case MemoryAddressKind.BitCount:
                    {
                        var value = ram.ReadInt8(effectiveAddress);
                        return BitOperations.PopCount(value);
                    }
                default:
                    break;
            }

            return 0;
        }
    }

    public class CompareInstruction
    {
        public Value Left { get; set; }
        public Value Right { get; set; }
        public ConditionCompare Operation { get; set; }

        public bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            if (Left == null)
            {
                return false;
            }

            if (Right == null)
            {
                return false;
            }

            var leftValue = Left.GetValue(ram, context);
            var rightValue = Right.GetValue(ram, context);

            if (context.AddValue.HasValue)
            {
                leftValue = context.AddValue.Value + leftValue;
                context.AddValue = null;
            }

            return Operation switch
            {
                ConditionCompare.Equals => leftValue == rightValue,
                ConditionCompare.Greater => leftValue > rightValue,
                ConditionCompare.GreaterEquals => leftValue >= rightValue,
                ConditionCompare.Less => leftValue < rightValue,
                ConditionCompare.LessEquals => leftValue <= rightValue,
                ConditionCompare.NotEquals => leftValue != rightValue,
                _ => false
            };
        }
    }

    public class ConditionInstruction
    {
        public int TargetHitCount { get; set; } = 0;
        public int CurrentHitCount { get; set; } = 0;
        public CompareInstruction CompareInstruction { get; set; }

        public virtual bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            if (CompareInstruction == null)
            {
                return false;
            }

            bool result = CompareInstruction.Evaluate(ram, context);

            if (context.AndNext.HasValue)
            {
                result &= context.AndNext.Value;
                context.AndNext = null;
            }

            if (context.OrNext.HasValue)
            {
                result |= context.OrNext.Value;
                context.OrNext = null;
            }

            if (result)
            {
                if (TargetHitCount == 0)
                {
                    ++CurrentHitCount;
                }
                else if (CurrentHitCount < TargetHitCount)
                {
                    ++CurrentHitCount;
                    result = (CurrentHitCount == TargetHitCount);
                }
            }
            else if (CurrentHitCount > 0)
            {
                result = CurrentHitCount == TargetHitCount;
            }

            context.TempTotalHit = CurrentHitCount;

            if (TargetHitCount > 0 && context.AddHits.HasValue)
            {
                var hitCountToCheck = CurrentHitCount + context.AddHits.Value;
                result = hitCountToCheck >= TargetHitCount;
                context.TempTotalHit = hitCountToCheck;
                context.AddHits = null;
            }

            return result;
        }
    }

    public class ResetIfConditionInstruction : ConditionInstruction
    {
    }

    public class PauseIfConditionInstruction : ConditionInstruction
    {
    }

    public class AndNextConditionInstruction : ConditionInstruction
    {
    }

    public class OrNextConditionInstruction : ConditionInstruction
    {
    }

    public class AddSourceConditionInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            if (!context.AddValue.HasValue)
            {
                context.AddValue = 0;
            }

            context.AddValue += CompareInstruction.Left.GetValue(ram, context);

            return true;
        }
    }

    public class SubSourceConditionInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            if (!context.AddValue.HasValue)
            {
                context.AddValue = 0;
            }

            context.AddValue += -CompareInstruction.Left.GetValue(ram, context);

            return true;
        }
    }
    
    public class AddHitsConditionInstruction : ConditionInstruction
    {
    }

    public class AddAddressConditionInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            context.AddAddress = CompareInstruction.Left.GetValue(ram, context);
            return true;
        }
    }

    public class MeasureConditionInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            context.TempTotalHit = 0;

            if (TargetHitCount == 0)
            {
                context.TempMeasureValue = CompareInstruction.Left.GetValue(ram, context);
                if (context.AddValue.HasValue)
                {
                    context.TempMeasureValue += context.AddValue.Value;
                }

                if (!context.MeasuredTarget.HasValue)
                {
                    context.MeasuredTarget = CompareInstruction.Right.GetValue(ram, context);
                }
            }

            var result = base.Evaluate(ram, context);

            if (TargetHitCount > 0)
            {
                context.TempMeasureValue = context.TempTotalHit;
                if (!context.MeasuredTarget.HasValue)
                {
                    context.MeasuredTarget = TargetHitCount;
                }
            }

            return result;
        }
    }

    public class MeasureIfConditionInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            var result = base.Evaluate(ram, context);
            context.CanMeasure = result;
            return result;
        }
    }

    public class ConditionGroupInstruction
    {
        public List<ConditionInstruction> Conditions { get; set; } = new List<ConditionInstruction>();

        public ConditionGroupInstruction()
        {
        }

        public ConditionGroupInstruction(IEnumerable<ConditionInstruction> items)
        {
            Conditions = new List<ConditionInstruction>(items);
        }

        public bool Evaluate(AchievementInstruction parent, IConsoleRam ram, EvaluatorContext context)
        {
            bool groupValue = true;

            foreach (var instruction in Conditions)
            {
                if (instruction is PauseIfConditionInstruction 
                    && instruction.TargetHitCount > 0
                    && instruction.CurrentHitCount == instruction.TargetHitCount)
                {
                    return false;
                }

                var currentResult = instruction.Evaluate(ram, context);

                switch (instruction)
                {
                    case ResetIfConditionInstruction _:
                        {
                            if (currentResult)
                            {
                                foreach (var childCondition in parent.AllConditions())
                                {
                                    childCondition.CurrentHitCount = 0;
                                }

                                context.MeasuredTarget = null;
                                context.MeasuredValue = null;
                            }
                        }
                        break;
                    case PauseIfConditionInstruction _:
                        {
                            if (currentResult)
                            {
                                return false;
                            }
                            break;
                        }
                    case AndNextConditionInstruction _:
                        {
                            context.AndNext = currentResult;
                            break;
                        }
                    case OrNextConditionInstruction _:
                        {
                            context.OrNext = currentResult;
                            break;
                        }
                    case AddHitsConditionInstruction _:
                        {
                            currentResult = true;
                            context.AddHits = instruction.CurrentHitCount;
                            break;
                        }
                    default:
                        break;
                }

                groupValue &= currentResult;
            }

            if (context.TempMeasureValue > 0 && context.CanMeasure)
            {
                if ((context.MeasuredValue.HasValue && context.TempMeasureValue > context.MeasuredValue.Value)
                    || !context.MeasuredValue.HasValue)
                {
                    context.MeasuredValue = context.TempMeasureValue;
                }
            }

            return groupValue;
        }
    }

    public class AchievementInstruction
    {
        public ConditionGroupInstruction Core { get; set; }
        public List<ConditionGroupInstruction> Alternates = new List<ConditionGroupInstruction>();
        public EvaluatorContext Context { get; private set; } = new EvaluatorContext();

        internal IEnumerable<ConditionInstruction> AllConditions()
        {
            foreach (var instruction in Core.Conditions)
            {
                yield return instruction;
            }

            foreach (var alt in Alternates)
            {
                foreach (var instruction in alt.Conditions)
                {
                    yield return instruction;
                }
            }
        }

        public bool Evaluate(IConsoleRam ram)
        {
            Context.TempMeasureValue = 0;
            Context.MeasuredValue = null;

            bool coreSet = Core.Evaluate(this, ram, Context);
          
            if (coreSet && Alternates.Count > 0)
            {
                bool finalResult = false;

                foreach (var alt in Alternates)
                {
                    finalResult |= alt.Evaluate(this, ram, Context);
                }

                return finalResult;
            }

            return coreSet;
        }
    }
}
