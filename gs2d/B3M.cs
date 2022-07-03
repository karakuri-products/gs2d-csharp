using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace gs2d
{
    public class B3M : Driver
    {
        private byte modeSetting = 0x02;

        public static class Instructions
        {
            public const byte Load = 0x01;
            public const byte Save = 0x02;
            public const byte Read = 0x03;
            public const byte Write = 0x04;
            public const byte Reset = 0x05;
            public const byte Position = 0x06;
        }

        public static class Address
        {
            public const byte Id = 0x00;
            public const byte Baudrate = 0x01;
            public const byte Offset = 0x09;
            public const byte Deadband = 0x1C;
            public const byte Mode = 0x28;
            public const byte TargetPosition = 0x2A;
            public const byte CurrentPosition = 0x2C;
            public const byte Speed = 0x30;
            public const byte CurrentSpeed = 0x32;
            public const byte TargetTime = 0x36;
            public const byte Temperature = 0x46;
            public const byte Current = 0x48;
            public const byte Voltage = 0x4A;
            public const byte PGain = 0x5E;
            public const byte IGain = 0x62;
            public const byte DGain = 0x66;

            public const byte TemperatureLimit = 0x0E;
            public const byte CCWLimit = 0x05;
            public const byte CWLimit = 0x07;
            public const byte CurrentLimit = 0x11;

        }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudrate"></param>
        /// <param name="stopBits"></param>
        public B3M(string portName, int baudrate = 115200, Parity parity = Parity.None) : base(portName, baudrate, parity)
        {

        }

        public B3M() : base()
        {

        }

        ~B3M()
        {

        }

        /// <summary>
        /// 受信完了チェック関数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal override bool IsCompleteResponse(byte[] data)
        {
            if (data.Length == 0) return false;
            return (data.Length == data[0]);
        }

        /// <summary>
        /// IDチェック関数
        /// </summary>
        /// <param name="id"></param>
        private void checkId(byte id) { if (id < 0 || id > 254) throw new BadInputParametersException("IDがレンジ外です"); }

        private byte calculateCheckSum(byte[] command)
        {
            int sum = 0;
            for (int i = 0; i < command.Length - 1; i++) sum += command[i];
            return (byte)(sum & 0xFF);
        }

        private byte[] generateCommand(byte id, byte instruction, byte[] parameters = null, byte option = 0)
        {
            // バイト列の長さを計算
            int bufferLength = 5;
            if (parameters != null) bufferLength += parameters.Length;

            // バイト列を生成
            byte[] command = new byte[bufferLength];

            // ヘッダとIDを設定
            command[0] = (byte)bufferLength; command[1] = instruction;
            command[2] = option; command[3] = id;

            // Parameter
            if (parameters != null) Array.Copy(parameters, 0, command, 4, parameters.Length);

            // CheckSumを設定
            command[bufferLength - 1] = calculateCheckSum(command);

            return command;
        }

        private T getFunction<T>(byte id, byte instruction, byte[] parameters = null, Func<byte[], T> responseProcess = null, Action<byte, T> callback = null, byte count = 1)
        {
            bool isReceived = false;
            T data = default(T);

            int error = 0;

            // 受信用コールバック
            ReceiveCallbackFunction templateReceiveCallback = (response) =>
            {
                byte[] responseData = null;

                do
                {
                    // 長さが正しいか確認
                    if (response.Length != response[0]) { error = 1; break; }

                    // CheckSum検証
                    if (calculateCheckSum(response) != response[response.Length - 1]) { error = 1; break; }

                    // Paramがあれば切りだし
                    if (response.Length > 5)
                    {
                        responseData = new byte[response.Length - 5];
                        Array.Copy(response, 4, responseData, 0, responseData.Length);
                    }
                } while (false);

                // データを処理
                // 例外はTODO
                try
                {
                    if (responseProcess != null) data = (T)(object)responseProcess(responseData);
                    else data = (T)(object)responseData;
                }catch(Exception ex) { }

                // 終了処理
                if (callback != null) callback(id, data);

                isReceived = true;
            };

            // コマンド送信
            byte[] command = generateCommand(id, instruction, parameters);
            commandHandler.AddCommand(command, templateReceiveCallback, count, new byte[1] { id });

            // コールバックがあれば任せて終了
            if (callback != null || count == 0) return default(T);

            // タイムアウト関数を登録
            Action<byte> timeoutEvent = (byte target) => { isReceived = true; };
            TimeoutCallbackEvent += timeoutEvent;

            // 無ければ受信完了待ち
            while (isReceived == false) ;

            // タイムアウトイベントを削除
            TimeoutCallbackEvent -= timeoutEvent;

            if(error != 0) throw new InvalidResponseDataException("サーボからの返答が不正です");

            return data;
        }

        /// <summary>
        /// 送信コールバック
        /// </summary>
        /// <param name="data"></param>
        private void defaultWriteCallback(byte id, byte[] data)
        {
        }

        // ------------------------------------------------------------------------------------------
        // General
        public override byte[] ReadMemory(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { (byte)address, (byte)length };

            // 送信
            return getFunction(id, Instructions.Read, param, null, callback);
        }
        public override async Task<byte[]> ReadMemoryAsync(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null)
        {
            return await Task.Run(() => ReadMemory(id, address, length, callback));
        }
        public override void WriteMemory(byte id, ushort address, byte[] data)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[data.Length + 2];

            Array.Copy(data, param, data.Length);
            param[param.Length - 2] = (byte)address;
            param[param.Length - 1] = 1;

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Ping
        public override Dictionary<string, ushort> Ping(byte id, Action<byte, Dictionary<string, ushort>> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { 0xA2, 12 };

            Func<byte[], Dictionary<string, ushort>> responseProcess = (response) =>
            {
                if (response != null && response.Length == 12)
                {
                    ushort modelNumber = response[1];
                    ushort firmwareVersion = response[8];
                    return new Dictionary<string, ushort>()
                    {
                        {"modelNumber", modelNumber},
                        {"firmwareVersion", firmwareVersion}
                    };

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<Dictionary<string, ushort>> PingAsync(byte id, Action<byte, Dictionary<string, ushort>> callback = null)
        {
            return await Task.Run(() => Ping(id, callback));
        }

        // Torque 
        public override byte ReadTorqueEnable(byte id, Action<byte, byte> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Mode, 1 };

            Func<byte[], byte> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    modeSetting = response[0];
                    return (byte)(((response[0] & 0b10) == 0b10) ? 0 : 1);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<byte> ReadTorqueEnableAsync(byte id, Action<byte, byte> callback = null)
        {
            return await Task.Run(() => ReadTorqueEnable(id, callback));
        }
        public override void WriteTorqueEnable(byte id, byte torque)
        {
            // IDチェック
            checkId(id);

            modeSetting &= 0b11111100;
            if (torque == 0) modeSetting |= 0b10;

            // パラメータ生成
            byte[] param = new byte[3] { modeSetting, Address.Mode, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Temperature
        public override ushort ReadTemperature(byte id, Action<byte, ushort> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Temperature, 2 };

            Func<byte[], ushort> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return (ushort)((response[0] + (response[1] << 8)) / 100);
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<ushort> ReadTemperatureAsync(byte id, Action<byte, ushort> callback = null)
        {
            return await Task.Run(() => ReadTemperature(id, callback));
        }

        // Current
        public override int ReadCurrent(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Current, 2 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return ((response[0] + (response[1] << 8)));
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadCurrentAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadCurrent(id, callback));
        }

        // Voltage
        public override double ReadVoltage(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Voltage, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2) return ((response[0] + (response[1] << 8)) / 1000.0);
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadVoltageAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadVoltage(id, callback));
        }

        // Target Position
        public override double ReadTargetPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.TargetPosition, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    short position = (short)(response[0] + (response[1] << 8));
                    return -position / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadTargetPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadTargetPosition(id, callback));
        }
        public override void WriteTargetPosition(byte id, double position)
        {
            // IDチェック
            checkId(id);

            ushort p = (ushort)(short)(position * -100);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(p & 0xFF), (byte)((p >> 8) & 0xFF), Address.TargetPosition, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Current Position
        public override double ReadCurrentPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.CurrentPosition, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    short position = (short)(response[0] + (response[1] << 8));
                    return -position / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadCurrentPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadCurrentPosition(id, callback));
        }

        // Offset
        public override double ReadOffset(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Offset, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    short position = (short)(response[0] + (response[1] << 8));
                    return -position / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadOffsetAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadOffset(id, callback));
        }
        public override void WriteOffset(byte id, double offset)
        {
            // IDチェック
            checkId(id);

            ushort p = (ushort)(short)(offset * -100);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(p & 0xFF), (byte)((p >> 8) & 0xFF), Address.Offset, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Deadband
        public override double ReadDeadband(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Deadband, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    short deadband = (short)(response[0] + (response[1] << 8));
                    return deadband / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadDeadbandAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadDeadband(id, callback));
        }
        public override void WriteDeadband(byte id, double deadband)
        {
            // IDチェック
            checkId(id);

            ushort p = (ushort)(short)(deadband * 100);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(p & 0xFF), (byte)((p >> 8) & 0xFF), Address.Deadband, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Target Time
        public override double ReadTargetTime(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.TargetTime, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    ushort time = (ushort)(response[0] + (response[1] << 8));
                    return time / 1000.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadTargetTimeAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadTargetTime(id, callback));
        }
        public override void WriteTargetTime(byte id, double targetTime)
        {
            // IDチェック
            checkId(id);

            ushort p = (ushort)(targetTime * 1000.0);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(p & 0xFF), (byte)((p >> 8) & 0xFF), Address.TargetTime, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Accel Time
        public override double ReadAccelTime(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("B3MではReadAccelTimeに対応していません。");
        }
        public override async Task<double> ReadAccelTimeAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("B3MではReadAccelTimeAsyncに対応していません。");
        }
        public override void WriteAccelTime(byte id, double accelTime)
        {
            throw new NotSupportedException("B3MではWriteAccelTimeに対応していません。");
        }

        // P Gain
        public override int ReadPGain(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.PGain, 4 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    uint gain = (uint)(response[0] + (response[1] << 8) + (response[2] << 16) + (response[3] << 24));
                    return (int)gain;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadPGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadPGain(id, callback));
        }
        public override void WritePGain(byte id, int gain)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[6] { (byte)(gain & 0xFF), (byte)((gain >> 8) & 0xFF), (byte)((gain >> 16) & 0xFF), (byte)((gain >> 24) & 0xFF), Address.PGain, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // I Gain
        public override int ReadIGain(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.IGain, 4 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    uint gain = (uint)(response[0] + (response[1] << 8) + (response[2] << 16) + (response[3] << 24));
                    return (int)gain;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadIGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadIGain(id, callback));
        }
        public override void WriteIGain(byte id, int gain)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[6] { (byte)(gain & 0xFF), (byte)((gain >> 8) & 0xFF), (byte)((gain >> 16) & 0xFF), (byte)((gain >> 24) & 0xFF), Address.IGain, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // D Gain
        public override int ReadDGain(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.DGain, 4 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    uint gain = (uint)(response[0] + (response[1] << 8) + (response[2] << 16) + (response[3] << 24));
                    return (int)gain;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadDGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadDGain(id, callback));
        }
        public override void WriteDGain(byte id, int gain)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[6] { (byte)(gain & 0xFF), (byte)((gain >> 8) & 0xFF), (byte)((gain >> 16) & 0xFF), (byte)((gain >> 24) & 0xFF), Address.DGain, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Max Torque
        public override int ReadMaxTorque(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("B3MではReadMaxTorqueに対応していません。");
        }
        public override async Task<int> ReadMaxTorqueAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("B3MではReadMaxTorqueAsyncに対応していません。");
        }
        public override void WriteMaxTorque(byte id, int maxTorque)
        {
            throw new NotSupportedException("B3MではWriteMaxTorqueに対応していません。");
        }

        // Speed
        public override double ReadSpeed(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.CurrentSpeed, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    short speed = (short)(response[0] + (response[1] << 8));
                    return speed / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadSpeedAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadSpeed(id, callback));
        }
        public override void WriteSpeed(byte id, double speed)
        {
            // IDチェック
            checkId(id);

            int speedInt = (int)(speed * 100);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(speedInt & 0xFF), (byte)((speedInt >> 8) & 0xFF), Address.Speed, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // ID
        public override int ReadID(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Id, 1 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadIDAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadIDAsync(id, callback));
        }
        public override void WriteID(byte id, int servoid)
        {
            // IDチェック
            checkId(id);
            checkId((byte)servoid);

            // パラメータ生成
            byte[] param = new byte[3] { (byte)(servoid), Address.Id, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // ROM
        public override void SaveROM(byte id)
        {
            // IDチェック
            checkId(id);

            // 送信
            getFunction<byte[]>(id, Instructions.Save, null, null, defaultWriteCallback);
        }
        public override void LoadROM(byte id)
        {
            // IDチェック
            checkId(id);

            // 送信
            getFunction<byte[]>(id, Instructions.Load, null, null, defaultWriteCallback);
        }
        public override void ResetMemory(byte id)
        {
            throw new NotSupportedException("B3MではResetMemoryに対応していません。");
        }

        // Baudrate
        public override int ReadBaudrate(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Baudrate, 4 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    uint baudrate = BitConverter.ToUInt32(response, 0);
                    return (int)baudrate;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadBaudrateAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadBaudrateAsync(id, callback));
        }
        public override void WriteBaudrate(byte id, int baudrate)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[6] { (byte)(baudrate & 0xFF), (byte)((baudrate >> 8) & 0xFF), (byte)((baudrate >> 16) & 0xFF), (byte)((baudrate >> 24) & 0xFF), Address.Baudrate, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // CW Limit Position
        public override double ReadLimitCWPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.CWLimit, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    int position = BitConverter.ToInt16(response, 0);
                    return -position / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCWPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadLimitCWPosition(id, callback));
        }
        public override void WriteLimitCWPosition(byte id, double cwLimit)
        {
            // IDチェック
            checkId(id);

            ushort position = (ushort)(short)(-cwLimit * 100.0);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(position & 0xFF), (byte)((position >> 8) & 0xFF), Address.CWLimit, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // CCW Limit Position
        public override double ReadLimitCCWPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.CCWLimit, 2 };

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    int position = BitConverter.ToInt16(response, 0);
                    return -position / 100.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCCWPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadLimitCCWPosition(id, callback));
        }
        public override void WriteLimitCCWPosition(byte id, double ccwLimit)
        {
            // IDチェック
            checkId(id);

            ushort position = (ushort)(short)(-ccwLimit * 100.0);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(position & 0xFF), (byte)((position >> 8) & 0xFF), Address.CCWLimit, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Temperature Limit
        public override int ReadLimitTemperature(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.TemperatureLimit, 2 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    int limit = BitConverter.ToInt16(response, 0);
                    return limit / 100;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadLimitTemperatureAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadLimitTemperature(id, callback));
        }
        public override void WriteLimitTemperature(byte id, int temperatureLimit)
        {
            // IDチェック
            checkId(id);

            int limit = temperatureLimit * 100;

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(limit & 0xFF), (byte)((limit >> 8) & 0xFF), Address.TemperatureLimit, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Current Limit
        public override int ReadLimitCurrent(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.CurrentLimit, 2 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return (int)(BitConverter.ToInt16(response, 0));
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadLimitCurrentAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadLimitCurrent(id, callback));
        }
        public override void WriteLimitCurrent(byte id, int currentLimit)
        {
            // IDチェック
            checkId(id);

            int limit = (int)(currentLimit);

            // パラメータ生成
            byte[] param = new byte[4] { (byte)(limit & 0xFF), (byte)((limit >> 8) & 0xFF), Address.CurrentLimit, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Drive Mode
        public override int ReadDriveMode(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = new byte[2] { Address.Mode, 1 };

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    modeSetting = response[0];
                    return ((response[0] & 0b1100) >> 2);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadDriveModeAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadDriveMode(id, callback));
        }
        public override void WriteDriveMode(byte id, int driveMode)
        {
            // IDチェック
            checkId(id);

            modeSetting &= 0b11110011;
            modeSetting |= (byte)(driveMode << 2);

            // パラメータ生成
            byte[] param = new byte[3] { modeSetting, Address.Mode, 0x01 };

            // 送信
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Burst Function
        public override Dictionary<int, byte[]> BurstReadMemory(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            throw new NotSupportedException("B3MでBurstReadMemoryに対応していません。");
        }
        public override async Task<Dictionary<int, byte[]>> BurstReadMemoryAsync(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            throw new NotSupportedException("B3MでBurstReadMemoryAsyncに対応していません。");
        }
        public override void BurstWriteMemory(Dictionary<int, byte[]> idDataList, ushort address, ushort length)
        {
            byte[] param = new byte[(length + 1) * (idDataList.Count) + 1];
            int pos = 0;
            byte firstId = 0;

            foreach(KeyValuePair<int, byte[]> item in idDataList)
            {
                if (pos != 0) param[pos++] = (byte)item.Key;
                else firstId = (byte)item.Key;
                Array.Copy(item.Value, 0, param, pos, item.Value.Length);
                pos += item.Value.Length;
            }

            param[pos++] = (byte)address;
            param[pos] = (byte)idDataList.Count;

            getFunction<byte[]>(firstId, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Burst Functions ( Position )
        public override Dictionary<int, double> BurstReadPositions(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            throw new NotSupportedException("B3MでBurstReadPositionsに対応していません。");
        }
        public override async Task<Dictionary<int, double>> BurstReadPositionsAsync(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            throw new NotSupportedException("B3MでBurstReadPositionsAsyncに対応していません。");
        }
        public override void BurstWriteTargetPositions(Dictionary<int, double> idPositionList)
        {
            byte[] param = new byte[3 * (idPositionList.Count) + 1];
            int pos = 0;
            byte firstId = 0;

            foreach (KeyValuePair<int, double> item in idPositionList)
            {
                if (pos != 0) param[pos++] = (byte)item.Key;
                else firstId = (byte)item.Key;

                ushort position = (ushort)(short)(item.Value * -100.0);
                param[pos++] = (byte)(position & 0xFF);
                param[pos++] = (byte)((position >> 8) & 0xFF);
            }

            param[pos++] = Address.TargetPosition;
            param[pos] = (byte)idPositionList.Count;

            getFunction<byte[]>(firstId, Instructions.Write, param, null, null, (byte)((idPositionList.Count == 1) ? 1 : 0));
        }
    }
}

