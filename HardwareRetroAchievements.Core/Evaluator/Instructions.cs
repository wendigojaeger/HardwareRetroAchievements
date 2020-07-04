using HardwareRetroAchievements.Core.Console;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace HardwareRetroAchievements.Core.Evaluator
{
    public class EvaluatorContext
    {
        public bool Succeeded = true;
        public bool? AndNext = null;
        public bool? OrNext = null;
        public int? AddValue = null;
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
        public abstract int GetValue(IConsoleRam ram);
    }

    public class ConstValue : Value
    {
        private readonly int _constValue;

        public ConstValue(int value)
        {
            _constValue = value;
        }

        public override int GetValue(IConsoleRam ram)
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

        public override int GetValue(IConsoleRam ram)
        {
            var current = _child.GetValue(ram);
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

        public override int GetValue(IConsoleRam ram)
        {
            var current = _child.GetValue(ram);

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

        public override int GetValue(IConsoleRam ram)
        {
            switch (Kind)
            {
                case MemoryAddressKind.Bit0:
                    {
                        var value = ram.ReadInt8(Address);
                        return (value & (1 << 0)) >> 0;
                    }
                case MemoryAddressKind.Bit1:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 1)) >> 1);
                    }
                case MemoryAddressKind.Bit2:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 2)) >> 2);
                    }
                case MemoryAddressKind.Bit3:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 3)) >> 3);
                    }
                case MemoryAddressKind.Bit4:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 4)) >> 4);
                    }
                case MemoryAddressKind.Bit5:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 5)) >> 5);
                    }
                case MemoryAddressKind.Bit6:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 6)) >> 6);
                    }
                case MemoryAddressKind.Bit7:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & (1 << 7)) >> 7);
                    }
                case MemoryAddressKind.Lower4:
                    {
                        var value = ram.ReadInt8(Address);
                        return (value & 0x0F);
                    }
                case MemoryAddressKind.Upper4:
                    {
                        var value = ram.ReadInt8(Address);
                        return ((value & 0xF0) >> 4);
                    }
                case MemoryAddressKind.Int8:
                    {
                        return ram.ReadInt8(Address);
                    }
                case MemoryAddressKind.Int16:
                    {
                        return ram.ReadInt16(Address);
                    }
                case MemoryAddressKind.Int24:
                    {
                        return (int)ram.ReadUInt24(Address);
                    }
                case MemoryAddressKind.Int32:
                    {
                        return (int)ram.ReadUInt32(Address);
                    }
                case MemoryAddressKind.BitCount:
                    {
                        var value = ram.ReadInt8(Address);
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

            var leftValue = Left.GetValue(ram);
            var rightValue = Right.GetValue(ram);

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

            if (TargetHitCount > 0 && CurrentHitCount < TargetHitCount)
            {
                var result = CompareInstruction.Evaluate(ram, context);

                if (result)
                {
                    CurrentHitCount++;

                    return CurrentHitCount == TargetHitCount;
                }
                else
                {
                    return false;
                }
            }
            else if (TargetHitCount == 0)
            {
                return CompareInstruction.Evaluate(ram, context);
            }

            return false;
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

    public class AddSourceInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            if (!context.AddValue.HasValue)
            {
                context.AddValue = 0;
            }

            context.AddValue += CompareInstruction.Left.GetValue(ram);

            return true;
        }
    }

    public class SubSourceInstruction : ConditionInstruction
    {
        public override bool Evaluate(IConsoleRam ram, EvaluatorContext context)
        {
            if (!context.AddValue.HasValue)
            {
                context.AddValue = 0;
            }

            context.AddValue += -CompareInstruction.Left.GetValue(ram);

            return true;
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

        public bool Evaluate(AchievementInstruction parent, IConsoleRam ram)
        {
            EvaluatorContext context = new EvaluatorContext();

            // Evaluate Core first
            foreach (var instruction in Conditions)
            {
                if (instruction is PauseIfConditionInstruction 
                    && instruction.TargetHitCount > 0
                    && instruction.CurrentHitCount == instruction.TargetHitCount)
                {
                    return false;
                }

                var currentResult = instruction.Evaluate(ram, context);

                if (context.AndNext.HasValue)
                {
                    currentResult &= context.AndNext.Value;
                    context.AndNext = null;
                }

                if (context.OrNext.HasValue)
                {
                    currentResult |= context.OrNext.Value;
                    context.OrNext = null;
                }

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
                            }
                        }
                        break;
                    case PauseIfConditionInstruction _:
                        {
                            if (currentResult)
                            {
                                context.Succeeded = false;
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
                }

                if (!currentResult)
                {
                    context.Succeeded = false;
                }
            }

            return context.Succeeded;
        }
    }

    public class AchievementInstruction
    {
        public ConditionGroupInstruction Core { get; set; }
        public List<ConditionGroupInstruction> Alternates = new List<ConditionGroupInstruction>();

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
            bool coreSet = Core.Evaluate(this, ram);
          
            if (coreSet && Alternates.Count > 0)
            {
                bool finalResult = false;

                foreach (var alt in Alternates)
                {
                    finalResult |= alt.Evaluate(this, ram);
                }

                return finalResult;
            }

            return coreSet;
        }
    }
}
