using System;
using System.Collections.Generic;
using System.Text;

namespace HardwareRetroAchievements.Core.Console
{
    public interface IConsoleRam
    {
        public byte ReadInt8(long address);
        public ushort ReadInt16(long address);
        public uint ReadUInt24(long address);
        public uint ReadUInt32(long address);
    }
}
