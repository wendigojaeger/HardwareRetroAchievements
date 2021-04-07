using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;
using HardwareRetroAchievements.Core.Evaluator;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace HardwareRetroAchievements.Core.Console.SNES
{
    public class Usb2Snes
    {
        private static readonly Uri Address = new Uri("ws://localhost:8080");

        private class Request
        {
            public string Opcode { get; set; }
            public string Space { get; set; } = "SNES";
            public List<string> Flags { get; set; } = new List<string>();
            public List<string> Operands { get; set; } = new List<string>();
        }

        private class Response
        {
            public List<string> Results { get; set; }
        }

        public class Info
        {
            public string FirmwareVersion { get; set; }
            public string DeviceName { get; set; }
            public string CurrentROM { get; set; }
            public List<string> Flags { get; set; }
        }

        private readonly ClientWebSocket _webSocket = new ClientWebSocket();
        private byte[] _receiveBuffer = new byte[1024 * 2];

        public bool IsConnected => _webSocket.State == WebSocketState.Open;

        public async Task Connect(CancellationTokenSource cancelTokenSource)
        {
            await _webSocket.ConnectAsync(Address, cancelTokenSource.Token);
        }

        public async void Disconnect(CancellationTokenSource cancelTokenSource)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnect", cancelTokenSource.Token);
        }

        public async Task<List<string>> GetDeviceList(CancellationTokenSource cancelTokenSource)
        {
            Request request = new Request()
            {
                Opcode = "DeviceList",
            };

            List<string> deviceList = null;

            await sendRequest(request, cancelTokenSource);
            var response = await readResponse(cancelTokenSource);

            if (response != null)
            {
                deviceList = response.Results;
            }

            return deviceList;
        }

        public Task AttachToDevice(string deviceName, CancellationTokenSource cancelTokenSource)
        {
            Request request = new Request
            {
                Opcode = "Attach",
                Operands = new List<string>(new [] { deviceName })
            };

            return sendRequest(request, cancelTokenSource);
        }

        public Task SetApplicationName(string applicationName, CancellationTokenSource cancelTokenSource)
        {
            Request request = new Request()
            {
                Opcode = "Name",
                Operands = new List<string>(new[] { applicationName })
            };

            return sendRequest(request, cancelTokenSource);
        }

        public async Task<Info> GetInfo(CancellationTokenSource cancelTokenSource)
        {
            Request request = new Request()
            {
                Opcode = "Info",
            };

            Info info = null;

            await sendRequest(request, cancelTokenSource);

            var response = await readResponse(cancelTokenSource);

            if (response != null && response.Results.Count >= 3)
            {
                info = new Info
                {
                    FirmwareVersion = response.Results[0],
                    DeviceName = response.Results[1],
                    CurrentROM = response.Results[2],
                    Flags = response.Results.Skip(3).ToList()
                };
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Is null: {0}, if not count={1}", response == null, response?.Results.Count);
            }

            return info;
        }

        public async Task<byte[]> GetAddress(int offset, int size, CancellationTokenSource cancelTokenSource)
        {
            Request request = new Request()
            {
                Opcode = "GetAddress",
                Operands = new List<string>(new[] { offset.ToString("x"), size.ToString("x") })
            };

            await sendRequest(request, cancelTokenSource);

            MemoryStream writeStream = new MemoryStream();

            while (writeStream.Length < size)
            {
                var receiveResult = await _webSocket.ReceiveAsync(_receiveBuffer, cancelTokenSource.Token);
                if (!receiveResult.CloseStatus.HasValue && receiveResult.Count > 0)
                {
                    writeStream.Write(_receiveBuffer[0..receiveResult.Count]);
                }
            }

            return writeStream.ToArray();
        }

        private Task sendRequest(Request request, CancellationTokenSource cancelTokenSource)
        {
            var serializedRequest = requestToByteArray(request);
            return _webSocket.SendAsync(serializedRequest, WebSocketMessageType.Text, true, cancelTokenSource.Token);
        }

        private async Task<Response> readResponse(CancellationTokenSource cancelTokenSource)
        {
            var receiveResult = await _webSocket.ReceiveAsync(_receiveBuffer, cancelTokenSource.Token);

            if (!receiveResult.CloseStatus.HasValue)
            {
                return rawResponseToResponse(_receiveBuffer, receiveResult);
            }

            return null;
        }

        private byte[] requestToByteArray(Request request)
        {
            var stringJson = JsonSerializer.Serialize(request);

            return Encoding.UTF8.GetBytes(stringJson);
        }

        private Response rawResponseToResponse(ReadOnlySpan<byte> receiveBuffer, WebSocketReceiveResult receiveResult)
        {
            string rawResponse = Encoding.UTF8.GetString(receiveBuffer[0..receiveResult.Count]);
            return JsonSerializer.Deserialize<Response>(rawResponse);
        }
    }
}
