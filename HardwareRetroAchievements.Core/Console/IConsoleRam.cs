using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareRetroAchievements.Core.Console
{
    public interface IConsoleRam
    {
        public byte ReadInt8(int address);
        public ushort ReadInt16(int address);
        public uint ReadUInt24(int address);
        public uint ReadUInt32(int address);
    }
}
