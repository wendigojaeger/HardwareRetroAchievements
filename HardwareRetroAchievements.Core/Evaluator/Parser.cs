using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace HardwareRetroAchievements.Core.Evaluator
{
    public enum MemoryAddressKind
    {
        Bit0,
        Bit1,
        Bit2,
        Bit3,
        Bit4,
        Bit5,
        Bit6,
        Bit7,
        Lower4,
        Upper4,
        Int8,
        Int16,
        Int24,
        Int32,
        BitCount
    }

    public static class MemoryAddressKindExtensions
    {
        public static int ByteSize(this MemoryAddressKind kind)
        {
            return kind switch
            {
                MemoryAddressKind.Bit0 => 1,
                MemoryAddressKind.Bit1 => 1,
                MemoryAddressKind.Bit2 => 1,
                MemoryAddressKind.Bit3 => 1,
                MemoryAddressKind.Bit4 => 1,
                MemoryAddressKind.Bit5 => 1,
                MemoryAddressKind.Bit6 => 1,
                MemoryAddressKind.Bit7 => 1,
                MemoryAddressKind.Lower4 => 1,
                MemoryAddressKind.Upper4 => 1,
                MemoryAddressKind.Int8 => 1,
                MemoryAddressKind.Int16 => 2,
                MemoryAddressKind.Int24 => 3,
                MemoryAddressKind.Int32 => 4,
                MemoryAddressKind.BitCount => 1,
                _ => 0,
            };
        }
    }

    public enum OperandType
    {
        Standard,
        Delta,
        Prior
    }

    public enum OperandFlag
    {
        None,
        PauseIf,
        ResetIf,
        AddSource,
        SubSource,
        AddHits,
        ANDNext,
        ORNext,
        Measured,
        MeasuredIf,
        AddAddress,
    }

    public enum ConditionCompare
    {
        Equals,
        NotEquals,
        Less,
        Greater,
        LessEquals,
        GreaterEquals,
    }

    public static class ConditionCompareExtensions
    {
        public static string ToStr(this ConditionCompare value)
        {
            return value switch
            {
                ConditionCompare.Equals => "==",
                ConditionCompare.NotEquals => "!=",
                ConditionCompare.Less => "<",
                ConditionCompare.Greater => ">",
                ConditionCompare.LessEquals => "<=",
                ConditionCompare.GreaterEquals => ">=",
                _ => "",
            };
        }
    }

    public class AST
    {
    }

    public class RootAST : AST
    {
        public ConditionGroupAST Core { get; set; }
        public List<ConditionGroupAST> Alternates { get; set; } = new List<ConditionGroupAST>();
    }

    public class ConditionGroupAST : AST
    {
        public List<ConditionAST> Conditions { get; set; } = new List<ConditionAST>();
    }

    public class ConditionAST : AST
    {
        public OperandAST Left { get; set; }
        public OperandAST Right { get; set; }
        public ConditionCompare CompareOperator { get; set; }
        public int HitCount { get; set; }
    }

    public class OperandAST : AST
    {
        public OperandType Type { get; set; } = OperandType.Standard;
        public OperandFlag Flag { get; set; } = OperandFlag.None;
    }

    public class MemoryAddressAST : OperandAST
    {
        public int Address { get; set; }
        public MemoryAddressKind Kind { get; set; }
    }

    public class NumberAST : OperandAST
    {
        public int Value { get; set; }
    }

    // Achievement condition parser grammar (ANTLR style)
    // NUMBER: [0-9]+
    // HEX_NUMBER: [0-9a-fA-F]+
    //
    //root: (condition_group)('S' condition_group)+ ;
    //
    //condition_group: (condition)('_' condition)+ ;
    //
    //condition: compare hit_counts? ;
    //
    //hit_counts: '.' NUMBER '.' // Legacy format
    //    | '(' NUMBER ')'
    //    ;
    //
    //compare: expr '=' expr
    //    | expr '==' expr
    //    | expr '!=' expr
    //    | expr '<' expr
    //    | expr '>' expr
    //    | expr '>=' expr
    //    | expr '<=' expr
    //    ;
    //
    //expr: operand ;
    //
    //operand: flags? type? mem_address? number;
    //
    //type: 'd' // Delta - Compare value from previous frame
    //    | 'p' // Prior - It is similar to Delta, except it's only updated when the current values changes, whereas Delta is updated every frame.
    //    ;
    //
    //flags: 'P:' // Pause If - Pause activity in the current group
    //    | 'R:' // Reset If - Reset all hit counts
    //    | 'A:' // Add source - Add memory or value to the next condition
    //    | 'B:' // Sub source - Sub memory or value to the next condition
    //    | 'C:' // Add Hit - Add hit(s) to the next condition
    //    | 'N:' // AND next - Add conditions to Pause If,  Reset If or Hit Count
    //    | 'O:' // OR next - Add conditions to Pause If,  Reset If or Hit Count
    //    | 'M:' // Measured - Progress tracker for an achievement
    //    | 'Q:' // Measured If - conditional measured
    //    | 'I:' // Add Address - Modify address of the next condition
    //    ;
    //
    //memory_address: '0x' mem_address_kind? ;
    //
    //memory_address_kind: 'M' // bit-0
    //    | 'N' // bit-1
    //    | 'O' // bit-2
    //    | 'P' // bit-3
    //    | 'Q' // bit-4
    //    | 'R' // bit-5
    //    | 'S' // bit-6
    //    | 'T' // bit-7
    //    | 'L' // lower 4-bit
    //    | 'U' // upper 4-bit
    //    | 'H' // 8-bit
    //    | ' ' // 16-bit
    //    | 'W' // 24-bit
    //    | 'X' // 32-bit
    //    | 'K' // bit count
    //    ;
    //
    //number: NUMBER
    //    | HEX_NUMBER
    //    ;

    public class ParserException : Exception
    {
        public ParserException(string message) : base(message)
        {
        }
    }

    public class Parser
    {
        private string _input;
        private int _index = 0;

        private bool IsEOF
        {
            get
            {
                return _index >= _input.Length;
            }
        }

        public RootAST Parse(string input)
        {
            _input = input;
            return parseRoot();
        }

        private RootAST parseRoot()
        {
            RootAST root = new RootAST();
            root.Core = parseConditionGroup();

            while(!IsEOF)
            {
                // Eat 'S'
                _ = eatToken();

                var alt = parseConditionGroup();
                root.Alternates.Add(alt);
            }

            return root;
        }

        private ConditionGroupAST parseConditionGroup()
        {
            ConditionGroupAST group = new ConditionGroupAST();

            // Parse first condition
            var firstCondition = parseCondition();
            if (firstCondition != null)
            {
                group.Conditions.Add(firstCondition);
            }

            while (!IsEOF && peekToken() == '_')
            {
                // Eat _
                _ = eatToken();

                var condition = parseCondition();
                if (condition != null)
                {
                    group.Conditions.Add(condition);
                }
            }

            return group;
        }

        private ConditionAST parseCondition()
        {
            var startIndex = _index;

            ConditionAST condition = parseCompare();

            if (condition != null)
            {
                var token = peekToken();
                if (token == '.' || token == '(')
                {
                    parseHitCount(condition);
                }
            }

            return condition;
        }

        private ConditionAST parseCompare()
        {
            var startIndex = _index;

            OperandAST left = parseExpression();

            if (left != null)
            {
                if (isConditionCompare(peekToken()))
                {
                    ConditionCompare? compareOperator = parseCompareOperator();
                    if (compareOperator.HasValue)
                    {
                        startIndex = _index;
                        OperandAST right = parseExpression();

                        if (right != null)
                        {
                            return new ConditionAST
                            {
                                Left = left,
                                Right = right,
                                CompareOperator = compareOperator.Value
                            };
                        }
                        else
                        {
                            throw new ParserException($"Invalid right operand at column {startIndex}");
                        }
                    }
                    else
                    {
                        throw new ParserException($"Invalid or inexistant comparison operator at column {_index}");
                    }
                }
                else
                {
                    throw new ParserException($"Invalid or inexistant comparison operator at column {_index}");
                }
            }
            else
            {
                throw new ParserException($"Invalid left operand at column {startIndex}");
            }
        }

        private ConditionCompare? parseCompareOperator()
        {
            var token = eatToken();

            switch (token)
            {
                case '=':
                    {
                        if (peekToken() == '=')
                        {
                            _ = eatToken();
                        }
                        return ConditionCompare.Equals;
                    }
                case '<':
                    {
                        if (peekToken() == '=')
                        {
                            _ = eatToken();
                            return ConditionCompare.LessEquals;
                        }
                        else
                        {
                            return ConditionCompare.Less;
                        }
                    }
                case '>':
                    {
                        if (peekToken() == '=')
                        {
                            _ = eatToken();
                            return ConditionCompare.GreaterEquals;
                        }
                        else
                        {
                            return ConditionCompare.Greater;
                        }
                    }
                case '!':
                    {
                        if (peekToken() == '=')
                        {
                            _ = eatToken();
                            return ConditionCompare.NotEquals;
                        }
                        else
                        {
                            return null;
                        }
                    }
                default:
                    break;
            }

            return null;
        }

        private void parseHitCount(ConditionAST condition)
        {
            // Eat ( or .
            _ = eatToken();

            condition.HitCount = Convert.ToInt32(parseNumber());

            var peek = peekToken();
            if (peek == '.' || peek == ')')
            {
                // Eat ) or .
                _ = eatToken();
            }
            else
            { 
                throw new ParserException($"Invalid hit count token at column {_index}");
            }
        }

        private OperandAST parseExpression()
        {
            return parseOperand();
        }

        private OperandAST parseOperand()
        {
            OperandFlag flag = OperandFlag.None;
            OperandType type = OperandType.Standard;

            // Look for <Flags>:
            var firstPeek = lookAhead(0);
            var secondPeek = lookAhead(1);

            // Found flags
            if (isFlag(firstPeek) && secondPeek == ':')
            {
                // Parse and eat the flag
                flag = parseFlag(eatToken());

                // Eat :
                _ = eatToken();
            }

            // Look for <type>
            firstPeek = lookAhead(0);
            if (isType(firstPeek))
            {
                // Parse and eat the type
                type = parseType(eatToken());
            }

            firstPeek = lookAhead(0);
            secondPeek = lookAhead(1);

            // It's a memory addresss
            if (firstPeek == '0' && secondPeek == 'x')
            {
                MemoryAddressAST memoryOperand = parseMemoryAddress();
                if (memoryOperand != null)
                {
                    memoryOperand.Flag = flag;
                    memoryOperand.Type = type;
                    return memoryOperand;
                }
            }
            // Else it's a value
            else if (char.IsNumber(firstPeek))
            {
                return new NumberAST
                {
                    Flag = flag,
                    Type = type,
                    Value = (int)parseNumber()
                };
            }
            else
            {
                throw new ParserException($"Invalid token at column {_index}");
            }

            return null;
        }

        private MemoryAddressAST parseMemoryAddress()
        {
            // Eat 0 and x
            _ = eatToken();
            _ = eatToken();

            var kindPeek = parseMemoryAddressKind(peekToken());

            if (kindPeek.HasValue)
            {
                // Eat kind if not an number
                if (!isHexDigit(peekToken()))
                {
                    _ = eatToken();
                }

                return new MemoryAddressAST
                {
                    Kind = kindPeek.Value,
                    Address = (int)parseHexNumber()
                };
            }
            else
            {
                throw new ParserException($"Invalid memory address kind at column {_index}");
            }
        }

        private long parseHexNumber()
        {
            StringBuilder stringNumber = new StringBuilder();

            var peek = peekToken();
            while (!IsEOF && isHexDigit(peek))
            {
                var digit = eatToken();
                stringNumber.Append(digit);

                peek = peekToken();
            }

            if (stringNumber.Length == 0)
            {
                throw new ParserException($"No valid hex number at column {_index}");
            }

            long result = 0;

            for (int i = 0; i < stringNumber.Length; ++i)
            {
                var digitChar = char.ToLowerInvariant(stringNumber[i]);
                long digit;
                if (digitChar >= '0' && digitChar <= '9')
                {
                    digit = digitChar - '0';
                }
                else
                {
                    digit = 10 + (digitChar - 'a');
                }

                result |= (digit << 4*(stringNumber.Length - i - 1));
            }

            return result;
        }

        private long parseNumber()
        {
            StringBuilder stringNumber = new StringBuilder();

            var peek = peekToken();
            while (!IsEOF && char.IsDigit(peek))
            {
                var digit = eatToken();
                stringNumber.Append(digit);

                peek = peekToken();
            }

            if (stringNumber.Length == 0)
            {
                throw new ParserException($"No valid number at column {_index}");
            }

            return long.Parse(stringNumber.ToString());
        }

        private char peekToken()
        {
            return lookAhead(0);
        }

        private char lookAhead(int ahead)
        {
            var lookaheadPosition = _index + ahead;
            if (lookaheadPosition < _input.Length)
            {
                return _input[lookaheadPosition];
            }
            else
            {
                return '\0';
            }
        }

        private char eatToken()
        {
            var result = !IsEOF ? _input[_index] : '\0';
            _index++;
            return result;
        }

        private bool isFlag(char value)
        {
            return parseFlag(value) != OperandFlag.None;
        }

        private OperandFlag parseFlag(char value)
        {
            return char.ToLowerInvariant(value) switch
            {
                'p' => OperandFlag.PauseIf,
                'r' => OperandFlag.ResetIf,
                'a' => OperandFlag.AddSource,
                'b' => OperandFlag.SubSource,
                'c' => OperandFlag.AddHits,
                'n' => OperandFlag.ANDNext,
                'o' => OperandFlag.ORNext,
                'm' => OperandFlag.Measured,
                'q' => OperandFlag.MeasuredIf,
                'i' => OperandFlag.AddAddress,
                _ => OperandFlag.None
            };
        }

        private bool isType(char value)
        {
            return parseType(value) != OperandType.Standard;
        }

        private OperandType parseType(char value)
        {
            return value switch
            {
                'd' => OperandType.Delta,
                'p' => OperandType.Prior,
                _ => OperandType.Standard
            };
        }

        private MemoryAddressKind? parseMemoryAddressKind(char value)
        {
            return char.ToLowerInvariant(value) switch
            {
                'm' => MemoryAddressKind.Bit0,
                'n' => MemoryAddressKind.Bit1,
                'o' => MemoryAddressKind.Bit2,
                'p' => MemoryAddressKind.Bit3,
                'q' => MemoryAddressKind.Bit4,
                'r' => MemoryAddressKind.Bit5,
                's' => MemoryAddressKind.Bit6,
                't' => MemoryAddressKind.Bit7,
                'l' => MemoryAddressKind.Lower4,
                'u' => MemoryAddressKind.Upper4,
                'h' => MemoryAddressKind.Int8,
                _ when value == ' ' || isHexDigit(value) => MemoryAddressKind.Int16,
                'w' => MemoryAddressKind.Int24,
                'x' => MemoryAddressKind.Int32,
                'k' => MemoryAddressKind.BitCount,
                _ => null
            };
        }

        private bool isConditionCompare(char value)
        {
            return value switch
            {
                '=' => true,
                '>' => true,
                '<' => true,
                '!' => true,
                _ => false
            };
        }

        private bool isHexDigit(char value)
        {
            return char.ToLowerInvariant(value) switch
            {
                _ when char.IsDigit(value) => true,
                'a' => true,
                'b' => true,
                'c' => true,
                'd' => true,
                'e' => true,
                'f' => true,
                _ => false
            };
        }
    }

}
