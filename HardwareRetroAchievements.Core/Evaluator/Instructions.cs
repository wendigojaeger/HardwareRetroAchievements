using HardwareRetroAchievements.Core.Console;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HardwareRetroAchievements.Core.Evaluator
{
    public abstract class Value
    {
        public abstract uint GetValue(IConsoleRam ram);
    }

    public class ConstValue : Value
    {
        private uint _constValue;

        public ConstValue(uint value)
        {
            _constValue = value;
        }

        public override uint GetValue(IConsoleRam ram)
        {
            return _constValue;
        }
    }

    public class DeltaValue : Value
    {
        private Value _child;
        private uint _previousValue = 0;

        public DeltaValue(Value child)
        {
            _child = child;
        }

        public override uint GetValue(IConsoleRam ram)
        {
            var current = _child.GetValue(ram);
            var result = _previousValue;
            _previousValue = current;
            return result;
        }
    }

    public class PriorValue : Value
    {
        private Value _child;
        private uint _previousValue = 0;
        private uint _priorValue = 0;

        public PriorValue(Value child)
        {
            _child = child;
        }

        public override uint GetValue(IConsoleRam ram)
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

        public override uint GetValue(IConsoleRam ram)
        {
            switch (Kind)
            {
                case MemoryAddressKind.Bit0:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 0)) >> 0);
                    }
                case MemoryAddressKind.Bit1:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 1)) >> 1);
                    }
                case MemoryAddressKind.Bit2:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 2)) >> 2);
                    }
                case MemoryAddressKind.Bit3:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 3)) >> 3);
                    }
                case MemoryAddressKind.Bit4:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 4)) >> 4);
                    }
                case MemoryAddressKind.Bit5:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 5)) >> 5);
                    }
                case MemoryAddressKind.Bit6:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 6)) >> 6);
                    }
                case MemoryAddressKind.Bit7:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & (1 << 7)) >> 7);
                    }
                case MemoryAddressKind.Lower4:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)(value & 0x0F);
                    }
                case MemoryAddressKind.Upper4:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)((value & 0xF0) >> 4);
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
                        return ram.ReadUInt24(Address);
                    }
                case MemoryAddressKind.Int32:
                    {
                        return ram.ReadUInt32(Address);
                    }
                case MemoryAddressKind.BitCount:
                    {
                        var value = ram.ReadInt8(Address);
                        return (uint)BitOperations.PopCount(value);
                    }
                default:
                    break;
            }

            return 0;
        }
    }

    public abstract class EvaluateInstruction
    {
        public abstract bool Evaluate(IConsoleRam ram);
    }

    public class CompareInstruction : EvaluateInstruction
    {
        public Value Left { get; set; }
        public Value Right { get; set; }
        public ConditionCompare Operation { get; set; }

        public override bool Evaluate(IConsoleRam ram)
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

    public class ConditionInstruction : EvaluateInstruction
    {
        public int TargetHitCount { get; set; } = 0;
        public int CurrentHitCount { get; set; } = 0;
        public CompareInstruction CompareInstruction { get; set; }

        public override bool Evaluate(IConsoleRam ram)
        {
            if (CompareInstruction == null)
            {
                return false;
            }


            if (TargetHitCount > 0 && CurrentHitCount < TargetHitCount)
            {
                var result = CompareInstruction.Evaluate(ram);

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
                return CompareInstruction.Evaluate(ram);
            }

            return false;
        }
    }
}
