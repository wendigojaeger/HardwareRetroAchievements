using HardwareRetroAchievements.Core.Console;
using System;

namespace HardwareRetroAchievements.Core.Tests.Helpers
{
    class FakeConsoleRam : IConsoleRam
    {
        public byte[] Data;

        public FakeConsoleRam(int ramSize)
        {
            Data = new byte[ramSize];
        }

        public byte ReadInt8(int address)
        {
            return Data[address];
        }

        public ushort ReadInt16(int address)
        {
            return BitConverter.ToUInt16(Data, address);
        }

        public uint ReadUInt24(int address)
        {
            var byte1 = Data[address];
            var byte2 = Data[address + 1];
            var byte3 = Data[address + 2];

            return (uint)(byte1 | (byte2 << 8) | (byte3 << 16));
        }

        public uint ReadUInt32(int address)
        {
            return BitConverter.ToUInt32(Data, (int)address);
        }
    }
}
