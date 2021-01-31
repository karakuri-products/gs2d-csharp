using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Text;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Http.Headers;
using System.Globalization;

namespace gs2d
{
    public class Futaba : Driver
    {
        public static class Address
        {
            public const byte ModelNumber = 0x00;
            public const byte Id = 0x04;
            public const byte Baudrate = 0x06;
            public const byte CCWLimit = 0x0A;
            public const byte CWLimit = 0x08;
            public const byte TemperatureLimit = 0x0E;
            public const byte TargetPosition = 0x1E;
            public const byte CurrentPosition = 0x2A;
            public const byte Current = 0x30;
            public const byte Temperature = 0x32;
            public const byte Voltage = 0x34;
            public const byte TorqueEnable = 0x24;
            public const byte TargetTime = 0x20;
            public const byte CurrentSpeed = 0x2E;
            public const byte PGain = 0x26;
            public const byte MaxTorque = 0x23;
            public const byte WriteFlashRom = 0xFF;
            public const byte ResetMemory = 0xFF;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudrate"></param>
        /// <param name="stopBits"></param>
        public Futaba(string portName, int baudrate = 115200, Parity parity = Parity.None) : base(portName, baudrate, parity)
        {

        }

        /// <summary>
        /// 受信完了チェック関数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal override bool IsCompleteResponse(byte[] data)
        {
            if (data.Length < 6) return false;
            return data.Length == (8 + data[5]);
        }

        /// <summary>
        /// チェックサム生成
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private byte calculateCheckSum(byte[] command)
        {
            byte sum = 0;
            for (int i = 2; i < command.Length - 1; i++) sum ^= command[i];
            return (byte)(sum & 0xFF);
        }

        /// <summary>
        /// ID不正チェック関数
        /// </summary>
        /// <param name="id"></param>
        private void checkId(byte id) { if (id < 1 || id > 127) throw new BadInputParametersException("IDがレンジ外です。1から127のIDを設定してください"); }

        /// <summary>
        /// コマンド生成関数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="flag"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] generateCommand(byte id, byte address, byte[] data = null, byte flag = 0, byte count = 1, byte targetLength = 0)
        {
            int length = 8 + ((data != null) ? data.Length : 0);
            byte[] command = new byte[length];

            // コマンドを生成
            command[0] = 0xFA; command[1] = 0xAF;
            command[2] = id; command[3] = flag; command[4] = address;
            if(targetLength != 0)
            {
                command[5] = targetLength;
            }
            else
            {
                if (data == null) command[5] = 0;
                else command[5] = (byte)data.Length;
            }
            command[6] = count;

            if (data != null) Array.Copy(data, 0, command, 7, data.Length);
            command[length - 1] = calculateCheckSum(command);

            return command;
        }

        /// <summary>
        /// バーストライト用コマンド
        /// </summary>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="data"></param>
        /// <param name="flag"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] generateBurstCommand(byte id, byte address, byte length, Dictionary<int, byte[]> data, byte flag = 0, byte count = 1)
        {
            int commandLength = 8 + (length * data.Count);
            int pos = 0;
            byte[] command = new byte[commandLength];

            // コマンドを生成
            command[pos++] = 0xFA; command[pos++] = 0xAF;
            command[pos++] = id; command[pos++] = flag; command[pos++] = address;
            command[pos++] = length;
            command[pos++] = (byte)data.Count;

            foreach (KeyValuePair<int, byte[]> item in data)
            {
                command[pos++] = (byte)item.Key;
                Array.Copy(item.Value, 0, command, pos, item.Value.Length);
                pos += item.Value.Length;
            }

            command[pos] = calculateCheckSum(command);

            return command;
        }

        private T getFunction<T>(byte id, byte address, byte flag, byte length, byte[] data = null, Func<byte[], T> responseProcess = null, Action<T> callback = null)
        {
            bool isReceived = false;
            T receiveData = default(T);

            ReceiveCallbackFunction templateReceiveCallback = (response) =>
            {
                byte[] responseData = null;

                // チェックサムを確認
                if (response[response.Length - 1] != calculateCheckSum(response))
                {
                    throw new InvalidResponseDataException("サーボからの返答が不正です");
                }

                if (response.Length > 7 + length)
                {
                    responseData = response.Skip(7).Take(length).ToArray();
                }

                if (responseProcess != null) receiveData = (T)(object)responseProcess(responseData);
                else receiveData = (T)(object)responseData;

                if (callback != null) callback(receiveData);

                isReceived = true;
            };

            byte[] command = generateCommand(id, address, data, flag, 0, length);
            commandHandler.AddCommand(command, templateReceiveCallback);

            if (callback != null) return default(T);

            // タイムアウト関数を登録
            Action timeoutEvent = () => {
                isReceived = true;
            };
            TimeoutCallbackEvent += timeoutEvent;

            // 無ければ受信完了待ち
            while (isReceived == false) ;

            // タイムアウトイベントを削除
            TimeoutCallbackEvent -= timeoutEvent;

            return receiveData;
        }

        // ------------------------------------------------------------------------------------------
        // General
        public override byte[] ReadMemory(byte id, ushort address, ushort length, Action<byte[]> callback = null)
        {
            // IDチェック
            checkId(id);

            return getFunction<byte[]>(id, (byte)address, 0x0F, (byte)length, null, null, callback);
        }
        public override async Task<byte[]> ReadMemoryAsync(byte id, ushort address, ushort length, Action<byte[]> callback = null)
        {
            return await Task.Run(() => ReadMemory(id, address, length, callback));
        }
        public override void WriteMemory(byte id, ushort address, byte[] data)
        {
            // IDチェック
            checkId(id);

            byte[] command = generateCommand(id, (byte)address, data);
            commandHandler.AddCommand(command);
        }

        // Ping
        public override Dictionary<string, ushort> Ping(byte id, Action<Dictionary<string, ushort>> callback = null)
        {
            // IDチェック
            checkId(id);

            Func<byte[], Dictionary<string, ushort>> responseProcess = (response) =>
            {
                if (response != null && response.Length == 3)
                {
                    Dictionary<string, ushort> res = new Dictionary<string, ushort>();
                    res.Add("modelNumber", BitConverter.ToUInt16(response.Take(2).ToArray()));
                    res.Add("firmwareVersion", response[2]);
                    return res;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.ModelNumber, 0x0F, 3, null, responseProcess, callback);
        }
        public override async Task<Dictionary<string, ushort>> PingAsync(byte id, Action<Dictionary<string, ushort>> callback = null)
        {
            return await Task.Run(() => Ping(id, callback));
        }

        // Torque 
        public override byte ReadTorqueEnable(byte id, Action<byte> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], byte> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1) return response[0];
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.TorqueEnable, 0xF, 1, null, responseProcess, callback);
        }
        public override async Task<byte> ReadTorqueEnableAsync(byte id, Action<byte> callback = null)
        {
            return await Task.Run(() => ReadTorqueEnable(id, callback));
        }
        public override void WriteTorqueEnable(byte id, byte torque)
        {
            checkId(id);
            byte[] command = generateCommand(id, Address.TorqueEnable, new byte[1] { torque });
            commandHandler.AddCommand(command);
        }

        // Temperature
        public override ushort ReadTemperature(byte id, Action<ushort> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], ushort> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return BitConverter.ToUInt16(response);
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.Temperature, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<ushort> ReadTemperatureAsync(byte id, Action<ushort> callback = null)
        {
            return await Task.Run(() => ReadTemperature(id, callback));
        }

        // Current
        public override int ReadCurrent(byte id, Action<int> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return BitConverter.ToUInt16(response);
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.Current, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<int> ReadCurrentAsnyc(byte id, Action<int> callback = null)
        {
            return await Task.Run(() => ReadCurrent(id, callback));
        }

        // Voltage
        public override double ReadVoltage(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return BitConverter.ToUInt16(response) / 100.0;
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.Voltage, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadVoltageAsnyc(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadVoltage(id, callback));
        }

        // Target Position
        public override double ReadTargetPosition(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return -BitConverter.ToInt16(response) / 10.0;
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.TargetPosition, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadTargetPositionAsync(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadTargetPosition(id, callback));
        }
        public override void WriteTargetPosition(byte id, double position)
        {
            // IDをチェック
            checkId(id);

            if (position < -150) position = -150;
            else if (position > 150) position = 150;

            short positionInt = (short)(position * -10);

            checkId(id);
            byte[] command = generateCommand(id, Address.TargetPosition, BitConverter.GetBytes(positionInt));
            commandHandler.AddCommand(command);
        }

        // Current Position
        public override double ReadCurrentPosition(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return -BitConverter.ToInt16(response) / 10.0;
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.CurrentPosition, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadCurrentPositionAsnyc(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadCurrentPosition(id, callback));
        }

        // Offset
        public override double ReadOffset(byte id, Action<double> callback = null)
        {
            throw new NotSupportedException("FutabaではReadOffsetに対応していません。");
        }
        public override async Task<double> ReadOffsetAsnyc(byte id, Action<double> callback = null)
        {
            throw new NotSupportedException("FutabaではReadOffsetAsyncに対応していません。");
        }
        public override void WriteOffset(byte id, double offset)
        {
            throw new NotSupportedException("FutabaではWriteOffsetに対応していません。");
        }

        // Deadband
        public override double ReadDeadband(byte id, Action<double> callback = null)
        {
            throw new NotSupportedException("FutabaではReadDeadbandに対応していません。");
        }
        public override async Task<double> ReadDeadbandAsnyc(byte id, Action<double> callback = null)
        {
            throw new NotSupportedException("FutabaではReadDeadbandAsyncに対応していません。");
        }
        public override void WriteDeadband(byte id, double deadband)
        {
            throw new NotSupportedException("FutabaではWriteDeadbandに対応していません。");
        }

        // Target Time
        public override double ReadTargetTime(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return BitConverter.ToUInt16(response) / 100.0;
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.TargetTime, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadTargetTimeAsnyc(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadTargetTime(id, callback));
        }
        public override void WriteTargetTime(byte id, double targetTime)
        {
            if (targetTime < 0) targetTime = 0;
            else if (targetTime > 163.83) targetTime = 163.83;

            ushort targetTimeInt = (ushort)(targetTime * 100);

            checkId(id);
            byte[] command = generateCommand(id, Address.TargetTime, BitConverter.GetBytes(targetTimeInt));
            commandHandler.AddCommand(command);
        }

        // Accel Time
        public override double ReadAccelTime(byte id, Action<double> callback = null)
        {
            throw new NotSupportedException("FutabaではReadAccelTimeに対応していません。");
        }
        public override async Task<double> ReadAccelTimeAsnyc(byte id, Action<double> callback = null)
        {
            throw new NotSupportedException("FutabaではReadAccelTimeAsyncに対応していません。");
        }
        public override void WriteAccelTime(byte id, double accelTime)
        {
            throw new NotSupportedException("FutabaではWriteAccelTimeに対応していません。");
        }

        // P Gain
        public override int ReadPGain(byte id, Action<int> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1) return response[0];
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.PGain, 0xF, 1, null, responseProcess, callback);
        }
        public override async Task<int> ReadPGainAsnyc(byte id, Action<int> callback = null)
        {
            return await Task.Run(() => ReadPGain(id, callback));
        }
        public override void WritePGain(byte id, int gain)
        {
            if (gain < 1) gain = 1;
            else if (gain > 255) gain = 255;

            checkId(id);
            byte[] command = generateCommand(id, Address.PGain, new byte[1] { (byte)gain });
            commandHandler.AddCommand(command);
        }

        // I Gain
        public override int ReadIGain(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadIGainに対応していません。");
        }
        public override async Task<int> ReadIGainAsnyc(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadIGainAsyncに対応していません。");
        }
        public override void WriteIGain(byte id, int gain)
        {
            throw new NotSupportedException("FutabaではWriteIGainに対応していません。");
        }

        // D Gain
        public override int ReadDGain(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadDGainに対応していません。");
        }
        public override async Task<int> ReadDGainAsnyc(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadDGainAsyncに対応していません。");
        }
        public override void WriteDGain(byte id, int gain)
        {
            throw new NotSupportedException("FutabaではWriteDGainに対応していません。");
        }

        // Max Torque
        public override int ReadMaxTorque(byte id, Action<int> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1) return response[0];
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.MaxTorque, 0xF, 1, null, responseProcess, callback);
        }
        public override async Task<int> ReadMaxTorqueAsnyc(byte id, Action<int> callback = null)
        {
            return await Task.Run(() => ReadMaxTorque(id, callback));
        }
        public override void WriteMaxTorque(byte id, int maxTorque)
        {
            // IDをチェック
            checkId(id);

            if (maxTorque < 1) maxTorque = 1;
            else if (maxTorque > 100) maxTorque = 100;

            byte[] command = generateCommand(id, Address.MaxTorque, new byte[1] { (byte)maxTorque });
            commandHandler.AddCommand(command);
        }

        // Speed
        public override double ReadSpeed(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return BitConverter.ToInt16(response);
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.CurrentSpeed, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadSpeedAsnyc(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadSpeed(id, callback));
        }
        public override void WriteSpeed(byte id, double speed)
        {
            throw new NotSupportedException("FutabaではWriteSpeedに対応していません。");
        }

        // ID
        public override int ReadID(byte id, Action<int> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1) return response[0];
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.Id, 0xF, 1, null, responseProcess, callback);
        }
        public override async Task<int> ReadIDAsync(byte id, Action<int> callback = null)
        {
            return await Task.Run(() => ReadID(id, callback));
        }
        public override void WriteID(byte id, int servoid)
        {
            // IDをチェック
            checkId(id);
            checkId((byte)servoid);

            byte[] command = generateCommand(id, Address.Id, new byte[1] { (byte)servoid });
            commandHandler.AddCommand(command);
        }

        // ROM
        public override void SaveROM(byte id)
        {
            // IDをチェック
            checkId(id);

            byte[] command = generateCommand(id, Address.WriteFlashRom, null, 0x40, 0);
            commandHandler.AddCommand(command);
        }
        public override void LoadROM(byte id)
        {
            throw new NotSupportedException("FutabaではLoadROMに対応していません。");
        }
        public override void ResetMemory(byte id)
        {
            // IDをチェック
            checkId(id);

            byte[] command = generateCommand(id, Address.ResetMemory, null, 0x10, 0);
            commandHandler.AddCommand(command);
        }

        // Baudrate
        public override int ReadBaudrate(byte id, Action<int> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                int[] baudrateList = new int[] { 9600, 14400, 19200, 28800, 38400, 57600, 76800, 115200, 153600, 230400 };
                if (response != null && response.Length == 1 && response[0] < 10) return baudrateList[response[0]];
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.Baudrate, 0xF, 1, null, responseProcess, callback);
        }
        public override async Task<int> ReadBaudrateAsync(byte id, Action<int> callback = null)
        {
            return await Task.Run(() => ReadBaudrate(id, callback));
        }
        public override void WriteBaudrate(byte id, int baudrate)
        {
            // IDをチェック
            checkId(id);

            int[] baudrateList = new int[] { 9600, 14400, 19200, 28800, 38400, 57600, 76800, 115200, 153600, 230400 };
            byte baudrateId = 100;

            for (byte i = 0; i < 10; i++)
            {
                if(baudrateList[i] == baudrate) { baudrateId = i; break; }
            }

            if (baudrateId == 100) throw new BadInputParametersException("Baudrateが不正です");

            byte[] command = generateCommand(id, Address.Baudrate, new byte[1] { (byte)baudrateId });
            commandHandler.AddCommand(command);
        }

        // CW Limit Position
        public override double ReadLimitCWPosition(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return -BitConverter.ToInt16(response) / 10.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.CWLimit, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCWPositionAsnyc(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadLimitCWPosition(id, callback));
        }
        public override void WriteLimitCWPosition(byte id, double cwLimit)
        {
            // IDをチェック
            checkId(id);

            if(-150 > cwLimit || cwLimit > 0)
            {
                throw new BadInputParametersException("cwLimitが不正です。");
            }

            short cwLimitShort = (short)(cwLimit * -10);

            byte[] command = generateCommand(id, Address.CWLimit, BitConverter.GetBytes(cwLimitShort) );
            commandHandler.AddCommand(command);
        }

        // CCW Limit Position
        public override double ReadLimitCCWPosition(byte id, Action<double> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return -BitConverter.ToInt16(response) / 10.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.CCWLimit, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCCWPositionAsnyc(byte id, Action<double> callback = null)
        {
            return await Task.Run(() => ReadLimitCCWPosition(id, callback));
        }
        public override void WriteLimitCCWPosition(byte id, double ccwLimit)
        {
            // IDをチェック
            checkId(id);

            if (150 < ccwLimit || ccwLimit < 0)
            {
                throw new BadInputParametersException("ccwLimitが不正です。");
            }

            short ccwLimitShort = (short)(ccwLimit * -10);

            byte[] command = generateCommand(id, Address.CCWLimit, BitConverter.GetBytes(ccwLimitShort));
            commandHandler.AddCommand(command);
        }

        // Temperature Limit
        public override int ReadLimitTemperature(byte id, Action<int> callback = null)
        {
            // IDをチェック
            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return BitConverter.ToInt16(response);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(id, Address.TemperatureLimit, 0xF, 2, null, responseProcess, callback);
        }
        public override async Task<int> ReadLimitTemperatureAsync(byte id, Action<int> callback = null)
        {
            return await Task.Run(() => ReadLimitTemperature(id, callback));
        }
        public override void WriteLimitTemperature(byte id, int temperatureLimit)
        {
            throw new NotSupportedException("FutabaではWriteLimitTemperatureに対応していません。");
        }

        // Current Limit
        public override int ReadLimitCurrent(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadLimitCurrentに対応していません。");
        }
        public override async Task<int> ReadLimitCurrentAsync(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadLimitCurrentAsyncに対応していません。");
        }
        public override void WriteLimitCurrent(byte id, int currentLimit)
        {
            throw new NotSupportedException("FutabaではWriteLimitCurrentに対応していません。");
        }

        // Drive Mode
        public override int ReadDriveMode(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadDriveModeに対応していません。");
        }
        public override async Task<int> ReadDriveModeAsync(byte id, Action<int> callback = null)
        {
            throw new NotSupportedException("FutabaではReadDriveModeAsyncに対応していません。");
        }
        public override void WriteDriveMode(byte id, int driveMode)
        {
            throw new NotSupportedException("FutabaではWriteDriveModeに対応していません。");
        }

        // Burst Function
        public override Dictionary<int, byte[]> BurstReadMemory(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            throw new NotSupportedException("FutabaではBurstReadMemoryに対応していません。");
        }
        public override async Task<Dictionary<int, byte[]>> BurstReadMemoryAsync(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            throw new NotSupportedException("FutabaではBurstReadMemoryAsyncに対応していません。");
        }
        public override void BurstWriteMemory(Dictionary<int, byte[]> idDataList, ushort address, ushort length)
        {
            foreach(KeyValuePair<int, byte[]> item in idDataList)
            {
                checkId((byte)item.Key);
            }

            byte[] command = generateBurstCommand(0, (byte)address, (byte)length, idDataList, 0, (byte)idDataList.Count);
            commandHandler.AddCommand(command);
        }

        // Burst Functions ( Position )
        public override Dictionary<int, double> BurstReadPositions(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            throw new NotSupportedException("FutabaではBurstReadPositionsに対応していません。");
        }
        public override async Task<Dictionary<int, double>> BurstReadPositionsAsync(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            throw new NotSupportedException("FutabaではBurstReadPositionsAsyncに対応していません。");
        }
        public override void BurstWriteTargetPositions(Dictionary<int, double> idPositionList)
        {
            Dictionary<int, byte[]> idDataList = new Dictionary<int, byte[]>();

            foreach (KeyValuePair<int, double> item in idPositionList)
            {
                checkId((byte)item.Key);

                int position = (int)(item.Value * -10);

                if (position < -1500) position = -1500;
                else if (position > 1500) position = 1500;

                idDataList.Add(item.Key, BitConverter.GetBytes((short)position));
            }

            byte[] command = generateBurstCommand(0, (byte)Address.TargetPosition, (byte)3, idDataList, 0, (byte)idDataList.Count);

            commandHandler.AddCommand(command);
        }
    }
}
