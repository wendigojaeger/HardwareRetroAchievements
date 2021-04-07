using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareRetroAchievements.Core.Console.SNES
{
    public class SnesConsoleRam : IConsoleRam
    {
        public struct CacheEntry
        {
            public int Address;
            public int Size;
            public byte[] Data;

            public bool InRange(int value)
            {
                return (value >= Address) && (value < (Address + Size));
            }

            public override string ToString()
            {
                return $"0x{Address:x}-0x{(Address + Size):x} ({Size})";
            }
        }

        private readonly List<CacheEntry> _cache = new List<CacheEntry>();
        private readonly Usb2Snes _console;

        public SnesConsoleRam(Usb2Snes console)
        {
            _console = console;
        }

        public void AddCacheEntry(int address, int size)
        {
            _cache.Add(new CacheEntry()
            {
                Address = address,
                Size = size,
                Data = new byte[size]
            });
        }

        public async Task Update(CancellationTokenSource cancellationToken)
        {
            foreach (var entry in _cache)
            {
                var readBytes = await _console.GetAddress(entry.Address | 0xF50000, entry.Size, cancellationToken);
                Array.Copy(readBytes, entry.Data, Math.Min(readBytes.Length, entry.Size));
            }
        }

        public byte ReadInt8(int address)
        {
            foreach (var entry in _cache)
            {
                if (entry.InRange(address))
                {
                    var offset = address - entry.Address;
                    return entry.Data[offset];
                }
            }

            return 0;
        }

        public ushort ReadInt16(int address)
        {
            foreach (var entry in _cache)
            {
                if (entry.InRange(address))
                {
                    var offset = address - entry.Address;
                    return BitConverter.ToUInt16(entry.Data, offset);
                }
            }

            return 0;
        }

        public uint ReadUInt24(int address)
        {
            foreach (var entry in _cache)
            {
                if (entry.InRange((int)address))
                {
                    var offset = address - entry.Address;

                    var byte1 = entry.Data[offset];
                    var byte2 = entry.Data[offset + 1];
                    var byte3 = entry.Data[offset + 2];

                    return (uint)(byte1 | (byte2 << 8) | (byte3 << 16));
                }
            }

            return 0;
        }

        public uint ReadUInt32(int address)
        {
            foreach (var entry in _cache)
            {
                if (entry.InRange(address))
                {
                    var offset = address - entry.Address;
                    return BitConverter.ToUInt32(entry.Data, offset);
                }
            }

            return 0;
        }
    }
}
