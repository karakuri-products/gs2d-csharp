using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

namespace gs2d
{
    public class RobotisP20 : Driver
    {
        /// <summary>
        /// インストラクションリスト
        /// </summary>
        public static class Instructions
        {
            public const byte Ping = 0x01;
            public const byte Read = 0x02;
            public const byte Write = 0x03;
            public const byte RegWrite = 0x04;
            public const byte Action = 0x05;
            public const byte FactoryReset = 0x06;
            public const byte Reboot = 0x08;
            public const byte SyncRead = 0x82;
            public const byte SyncWrite = 0x83;
            public const byte BulkRead = 0x92;
            public const byte BulkWrite = 0x93;
        }

        /// <summary>
        /// アドレスリスト
        /// </summary>
        public static class Address
        {
            public const byte Id = 7;
            public const byte Baudrate = 8;
            public const byte DriveMode = 10;
            public const byte HomingOffset = 20;
            public const byte TemperatureLimit = 31;
            public const byte CurrentLimit = 38;
            public const byte MaxPositionLimit = 48;
            public const byte MinPositionLimit = 52;

            public const byte TorqueEnable = 64;
            public const byte PositionDGain = 80;
            public const byte PositionIGain = 82;
            public const byte PositionPGain = 84;
            public const byte ProfileAcceleration = 108;
            public const byte ProfileVelocity = 112;
            public const byte GoalPosition = 116;
            public const byte PresentCurrent = 126;
            public const byte PresentVelocity = 128;
            public const byte PresentPosition = 132;
            public const byte PresentVoltage = 144;
            public const byte PresentTemperature = 146;

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudrate"></param>
        /// <param name="stopBits"></param>
        public RobotisP20(string portName, int baudrate = 115200, Parity parity = Parity.None) : base(portName, baudrate, parity)
        {

        }

        /// <summary>
        /// 受信完了チェック関数
        /// </summary>
        /// <param name="data">受信データ</param>
        /// <returns></returns>
        internal override bool IsCompleteResponse(byte[] data)
        {
            if (data.Length < 6) return false;

            byte count = data[5];
            return (data.Length >= 7 + count);
        }

        /// <summary>
        /// ID不正チェック関数
        /// </summary>
        /// <param name="id"></param>
        private void checkId(byte id) { if (id < 0 || id > 254 || id == 253) throw new BadInputParametersException("IDがレンジ外です"); }

        /// <summary>
        /// パラメータ生成関数
        /// </summary>
        /// <param name="address"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] generateParameters(byte address, uint data, byte length)
        {
            byte[] param = new byte[2 + length];

            param[0] = (byte)(address & 0xFF);
            param[1] = (byte)((address >> 8) & 0xFF);
            for (int i = 0; i < length; i++)
            {
                param[2 + i] = (byte)((data >> (i * 8)) & 0xFF);
            }

            return param;
        }

        private byte[] generateParametersSyncRead(byte address, uint length, int[] idList)
        {
            byte[] param = new byte[4 + idList.Length];

            param[0] = (byte)(address & 0xFF);
            param[1] = (byte)((address >> 8) & 0xFF);
            param[2] = (byte)(length & 0xFF);
            param[3] = (byte)((length >> 8) & 0xFF);
            for (int i = 0; i < idList.Length; i++)
            {
                param[4 + i] = (byte)idList[i];
            }

            return param;
        }

        private byte[] generateParametersSyncWrite<T>(byte address, uint length, Dictionary<int, T> dataList)
        {
            byte[] param = new byte[4 + dataList.Count * (1 + length)];

            param[0] = (byte)(address & 0xFF);
            param[1] = (byte)((address >> 8) & 0xFF);
            param[2] = (byte)(length & 0xFF);
            param[3] = (byte)((length >> 8) & 0xFF);

            int count = 0;
            foreach(KeyValuePair<int, T> item in dataList)
            {
                param[4 + count * (1 + length)] = (byte)item.Key;
                for (int i = 0; i < length; i++)
                {
                    param[5 + count * (1 + length) + i] = (byte)(((int)(object)item.Value >> (i * 8)) & 0xFF);
                }
                count++;
            }

            return param;
        }

        /// <summary>
        /// コマンド生成関数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="instruction"></param>
        /// <param name="parameters"></param>
        /// <param name="length"></param>
        private byte[] generateCommand(byte id, byte instruction, byte[] parameters = null, byte length = 0)
        {
            // バイト列の長さを計算
            int bufferLength = 4 + 1 + 2 + 1 + 2;
            if (parameters != null) bufferLength += parameters.Length;

            // バイト列を生成
            byte[] command = new byte[bufferLength];

            // ヘッダとIDを設定
            command[0] = 0xFF; command[1] = 0xFF; command[2] = 0xFD; command[3] = 0x00; command[4] = id;

            // バイト長を設定
            if (length == 0)
            {
                if (parameters != null) length = (byte)(1 + parameters.Length + 2);
                else length = 3;
            }
            command[5] = (byte)(length & 0xFF);
            command[6] = (byte)((length >> 8) & 0xFF);

            // Instruction を設定
            command[7] = instruction;

            // Parameters
            if (parameters != null) Array.Copy(parameters, 0, command, 8, parameters.Length);

            // CheckSumを設定
            ushort crc = CRC16.calculate(command, (ushort)(bufferLength - 2));
            command[bufferLength - 2] = (byte)(crc & 0xFF);
            command[bufferLength - 1] = (byte)((crc >> 8) & 0xFF);

            return command;
        }

        /// <summary>
        /// 同時送受信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="instruction"></param>
        /// <param name="count"></param>
        /// <param name="parameters"></param>
        /// <param name="responseProcess"></param>
        /// <param name="callback"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private Dictionary<int, T> getFunctionBurstRead<T>(byte id, byte instruction, int count = 1, byte[] parameters = null, Func<byte[], T> responseProcess = null, Action<Dictionary<int, T>> callback = null, byte length = 0)
        {
            Dictionary<int, T> dataList = new Dictionary<int, T>();
            bool is_received = false;
            int receivedCount = count;

            int error = 0;

            ReceiveCallbackFunction templateReceiveCallback = (response) =>
            {
                byte[] responseData = null;

                // 最低限の長さがあるか確認
                do
                {
                    if (response.Length < 9) { error = 1; break; }

                    // インストラクション値を確認
                    if (response[7] != 0x55) { error = 1; break; }

                    // Lengthを取得して確認
                    int parameterLength = (response[5] + (response[6] << 8));
                    if (response.Length != 7 + parameterLength) { error = 1; break; }

                    // Errorバイトを確認
                    if (response[8] != 0) { error = 1; break; }

                    // CheckSum検証
                    ushort checkSum = (ushort)(response[response.Length - 2] + (response[response.Length - 1] << 8));
                    if (checkSum != CRC16.calculate(response, (ushort)(response.Length - 2))) { error = 2; break; }

                    // Paramがあれば切りだし
                    if (parameterLength > 4)
                    {
                        responseData = new byte[parameterLength - 4];
                        Array.Copy(response, 9, responseData, 0, parameterLength - 4);
                    }

                    // データを処理
                    // 例外はTODOとして保留
                    try
                    {
                        if (responseProcess != null) dataList.Add(response[4], (T)(object)responseProcess(responseData));
                        else dataList.Add(response[4], (T)(object)responseData);
                    }
                    catch (Exception ex) { }
                } while (false);

                if(--receivedCount == 0)
                {
                    // 終了処理
                    if (callback != null) callback(dataList);

                    is_received = true;
                }
            };

            // コマンド送信
            byte[] command = generateCommand(id, instruction, parameters, length);
            commandHandler.AddCommand(command, templateReceiveCallback, (uint)count);

            // コールバックがあれば任せて終了
            if (callback != null || count == 0) return dataList;

            // タイムアウト関数を登録
            Action timeoutEvent = () => {
                error = 3;
                is_received = true;
            };
            TimeoutCallbackEvent += timeoutEvent;

            // 無ければ受信完了待ち
            while (is_received == false) ;

            // タイムアウトイベントを削除
            TimeoutCallbackEvent -= timeoutEvent;

            switch (error)
            {
                case 1: throw new InvalidResponseDataException("サーボからの返答が不正です");
                case 2: throw new InvalidResponseDataException("チェックサムが不正です");
                case 3: throw new ReceiveDataTimeoutException("受信タイムアウト");
            }

            return dataList;
        }
 
        /// <summary>
        /// 受信系
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instruction"></param>
        /// <param name="parameters"></param>
        /// <param name="id"></param>
        /// <param name="responseProcess"></param>
        /// <param name="callback"></param>
        /// <param name="length"></param>
        private T getFunction<T>(byte id, byte instruction, byte[] parameters = null, Func<byte[], T> responseProcess = null, Action<byte, T> callback = null, byte length = 0)
        {
            bool is_received = false;
            T data = default(T);

            int error = 0;

            // 受信用コールバック
            ReceiveCallbackFunction templateReceiveCallback = (response) =>
            {
                byte[] responseData = null;

                do
                {
//                    for (int i = 0; i < response.Length; i++) Console.Write(response[i] + ", ");
//                    Console.WriteLine("");
                    // 最低限の長さがあるか確認
                    if (response.Length < 9) { error = 1; break; }

                    // インストラクション値を確認
                    if (response[7] != 0x55) { error = 1; break; }

                    // Lengthを取得して確認
                    int parameterLength = (response[5] + (response[6] << 8));
                    if (response.Length != 7 + parameterLength) { error = 1; break; }

                    // Errorバイトを確認
                    if (response[8] != 0) { error = 1; break; }

                    // CheckSum検証
                    ushort checkSum = (ushort)(response[response.Length - 2] + (response[response.Length - 1] << 8));
                    if (checkSum != CRC16.calculate(response, (ushort)(response.Length - 2))) { error = 2; break; }

                    // Paramがあれば切りだし
                    if (parameterLength > 4)
                    {
                        responseData = new byte[parameterLength - 4];
                        Array.Copy(response, 9, responseData, 0, parameterLength - 4);
                    }

                    // データを処理
                    // 例外はTODOとして保留
//                    Console.WriteLine("Callback");
                    try
                    {
                        if (responseProcess != null) data = (T)(object)responseProcess(responseData);
                        else data = (T)(object)responseData;
                    }
                    catch (Exception ex) { }

                } while (false);

                // 終了処理
                if (callback != null) callback(id, data);

                is_received = true;

            };

            // コマンド送信
            byte[] command = generateCommand(id, instruction, parameters, length);
            commandHandler.AddCommand(command, templateReceiveCallback);

            // コールバックがあれば任せて終了
            if (callback != null)
            {
                return default(T);
            }
            // タイムアウト関数を登録
            Action timeoutEvent = () => {
                error = 3;
                is_received = true;
            };
            TimeoutCallbackEvent += timeoutEvent;

            // 無ければ受信完了待ち
            while (is_received == false) ;

            // タイムアウトイベントを削除
            TimeoutCallbackEvent -= timeoutEvent;

            switch (error)
            {
                case 1: throw new InvalidResponseDataException("サーボからの返答が不正です");
                case 2: throw new InvalidResponseDataException("チェックサムが不正です");
                case 3: throw new ReceiveDataTimeoutException("受信タイムアウト");
            }

            return data;
        }

        private void defaultWriteCallback(byte id, byte[] data)
        {
            if (data != null) throw new InvalidResponseDataException("データが多すぎます");
        }

        // ------------------------------------------------------------------------------------------
        // General
        public override byte[] ReadMemory(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters((byte)address, (byte)length, 2);

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
            byte[] param = generateParameters((byte)address, BitConverter.ToUInt32(data, 0), (byte)data.Length);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Torque 
        public override byte ReadTorqueEnable(byte id, Action<byte, byte> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.TorqueEnable, 1, 2);

            Func<byte[], byte> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1) return response[0];
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

            // パラメータ生成
            byte[] param = generateParameters(Address.TorqueEnable, torque, 1);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Target Position
        public override double ReadTargetPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.GoalPosition, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    return (BitConverter.ToUInt32(response, 0) * 360.0 / 4095.0 - 180.0);

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

            // 値を変換
            if (position > 180.0) position = 180.0;
            else if (position < -180.0) position = -180.0;

            // パラメータ生成
            byte[] param = generateParameters(Address.GoalPosition, (uint)((position + 180.0) * 4095.0 / 360.0), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Ping
        public override Dictionary<string, ushort> Ping(byte id, Action<byte, Dictionary<string, ushort>> callback = null)
        {

            // IDチェック
            checkId(id);

            // パラメータ生成
            Func<byte[], Dictionary<string, ushort>> responseProcess = (response) =>
            {
                if (response != null && response.Length == 3)
                {
                    ushort modelNumber = BitConverter.ToUInt16(response.Take(2).ToArray(), 0);
                    ushort firmwareVersion = response[2];
                    return new Dictionary<string, ushort>()
                    {
                        {"modelNumber", modelNumber},
                        {"firmwareVersion", firmwareVersion}
                    };

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction(id, Instructions.Ping, null, responseProcess, callback, 0);
        }
        public override async Task<Dictionary<string, ushort>> PingAsync(byte id, Action<byte, Dictionary<string, ushort>> callback = null)
        {
            return await Task.Run(() => Ping(id, callback));
        }

        // Temperature
        public override ushort ReadTemperature(byte id, Action<byte, ushort> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.PresentTemperature, 1, 2);

            Func<byte[], ushort> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<ushort>(id, Instructions.Read, param, responseProcess, callback);
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
            byte[] param = generateParameters(Address.PresentCurrent, 2, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    int current = (int)(BitConverter.ToUInt16(response, 0) * 2.69);
                    return current;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
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
            byte[] param = generateParameters(Address.PresentVoltage, 2, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    double voltage = BitConverter.ToUInt16(response, 0) / 10.0;
                    return voltage;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadVoltageAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadVoltage(id, callback));
        }

        // Current Position
        public override double ReadCurrentPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.PresentPosition, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    double position = BitConverter.ToInt32(response, 0) * 360.0 / 4095.0 - 180.0;
                    return position;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
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
            byte[] param = generateParameters(Address.HomingOffset, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    double offset = BitConverter.ToInt32(response, 0) * 0.088;
                    return offset;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadOffsetAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadOffset(id, callback));
        }
        public override void WriteOffset(byte id, double offset)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            offset /= 0.088;

            if (offset > 1044479) offset = 1044479;
            else if (offset < -1044479) offset = -1044479;

            // パラメータ生成
            byte[] param = generateParameters(Address.HomingOffset, (uint)(offset), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Deadband
        public override double ReadDeadband(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("RobotisP20ではReadDeadbandに対応していません。");
        }
        public override Task<double> ReadDeadbandAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("RobotisP20ではReadDeadbandAsyncに対応していません。");
        }
        public override void WriteDeadband(byte id, double deadband)
        {
            throw new NotSupportedException("RobotisP20ではWriteDeadbandに対応していません。");
        }

        // Target Time
        public override double ReadTargetTime(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.ProfileVelocity, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    double targetTime = BitConverter.ToUInt32(response, 0) / 1000.0;
                    return targetTime;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadTargetTimeAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadTargetTime(id, callback));
        }
        public override void WriteTargetTime(byte id, double targetTime)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (targetTime < 0) targetTime = 0;
            else if (targetTime > 32.737) targetTime = 32.737;

            targetTime *= 1000.0;
            // パラメータ生成
            byte[] param = generateParameters(Address.ProfileVelocity, (uint)(targetTime), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Accel Time
        public override double ReadAccelTime(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.ProfileAcceleration, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    double targetTime = BitConverter.ToUInt32(response, 0) / 1000.0;
                    return targetTime;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadAccelTimeAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadAccelTime(id, callback));
        }
        public override void WriteAccelTime(byte id, double accelTime)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (accelTime < 0) accelTime = 0;
            else if (accelTime > 32.737) accelTime = 32.737;

            accelTime *= 1000.0;
            // パラメータ生成
            byte[] param = generateParameters(Address.ProfileAcceleration, (uint)(accelTime), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // P Gain
        public override int ReadPGain(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.PositionPGain, 2, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return BitConverter.ToUInt16(response, 0);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadPGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadPGain(id, callback));
        }
        public override void WritePGain(byte id, int gain)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (gain < 0) gain = 0;
            else if (gain > 16383) gain = 16383;

            // パラメータ生成
            byte[] param = generateParameters(Address.PositionPGain, (uint)(gain), 2);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // I Gain
        public override int ReadIGain(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.PositionIGain, 2, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return BitConverter.ToUInt16(response, 0);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadIGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadIGain(id, callback));
        }
        public override void WriteIGain(byte id, int gain)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (gain < 0) gain = 0;
            else if (gain > 16383) gain = 16383;

            // パラメータ生成
            byte[] param = generateParameters(Address.PositionIGain, (uint)(gain), 2);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // D Gain
        public override int ReadDGain(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.PositionDGain, 2, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return BitConverter.ToUInt16(response, 0);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadDGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadDGain(id, callback));
        }
        public override void WriteDGain(byte id, int gain)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (gain < 0) gain = 0;
            else if (gain > 16383) gain = 16383;

            // パラメータ生成
            byte[] param = generateParameters(Address.PositionDGain, (uint)(gain), 2);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Max Torque
        public override int ReadMaxTorque(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("RobotisP20ではReadMaxTorqueに対応していません。");
        }
        public override Task<int> ReadMaxTorqueAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("RobotisP20ではReadMaxTorqueAsyncに対応していません。");
        }
        public override void WriteMaxTorque(byte id, int maxTorque)
        {
            throw new NotSupportedException("RobotisP20ではWriteMaxTorqueに対応していません。");
        }

        // Speed
        public override double ReadSpeed(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.PresentVelocity, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    return BitConverter.ToUInt32(response,0 ) * 0.229 * 6;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadSpeedAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadSpeed(id, callback));
        }
        public override void WriteSpeed(byte id, double speed)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            int rev_min = (int)(speed / 0.229 / 6.0);

            if (rev_min < 0) rev_min = 0;
            else if (rev_min > 32737) rev_min = 32737;

            // パラメータ生成
            byte[] param = generateParameters(Address.ProfileVelocity, (uint)(rev_min), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // ID
        public override int ReadID(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.Id, 1, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadIDAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadID(id, callback));
        }
        public override void WriteID(byte id, int servoid)
        {
            // IDチェック
            checkId(id);
            checkId((byte)servoid);

            // 値を変換
            if (servoid < 0) servoid = 0;
            else if (servoid > 252) servoid = 252;

            // パラメータ生成
            byte[] param = generateParameters(Address.Id, (uint)(servoid), 1);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // ROM
        public override void SaveROM(byte id)
        {
            throw new NotSupportedException("RobotisP20ではSaveROMに対応していません。");
        }
        public override void LoadROM(byte id)
        {
            throw new NotSupportedException("RobotisP20ではLoadROMに対応していません。");
        }
        public override void ResetMemory(byte id)
        {
            byte[] param = new byte[1] { 0x02 };
            getFunction<byte[]>(id, Instructions.FactoryReset, param);
        }

        // Baudrate
        public override int ReadBaudrate(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.Baudrate, 1, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1 && response[0] <= 7)
                {
                    int[] baudrateList = new int[8] { 9600, 57600, 115200, 1000000, 2000000, 3000000, 4000000, 4500000 };
                    return baudrateList[response[0]];
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadBaudrateAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadBaudrate(id, callback));
        }
        public override void WriteBaudrate(byte id, int baudrate)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            int[] baudrateList = new int[8] { 9600, 57600, 115200, 1000000, 2000000, 3000000, 4000000, 4500000 };
            byte baudrateIndex = 100;

            for (byte i = 0; i < 8; i++)
            {
                if (baudrateList[i] == baudrate) baudrateIndex = i;
            }
            if (baudrateIndex == 100)
            {
                throw new BadInputParametersException("Baudrateがレンジ外です");
            }

            // パラメータ生成
            byte[] param = generateParameters(Address.Baudrate, (uint)(baudrateIndex), 1);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // CW Limit Position
        public override double ReadLimitCWPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.MinPositionLimit, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    return BitConverter.ToUInt32(response, 0) * 360.0 / 4095.0 - 180;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCWPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadLimitCWPosition(id, callback));
        }
        public override void WriteLimitCWPosition(byte id, double cwLimit)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (cwLimit > 0 || cwLimit < -180)
            {
                throw new BadInputParametersException("cwLimitがレンジ外です");
            }
            cwLimit = (cwLimit + 180) / 360.0 * 4095.0;

            // パラメータ生成
            byte[] param = generateParameters(Address.MinPositionLimit, (uint)(cwLimit), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // CCW Limit Position
        public override double ReadLimitCCWPosition(byte id, Action<byte, double> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.MaxPositionLimit, 4, 2);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    return BitConverter.ToUInt32(response, 0) * 360.0 / 4095.0 - 180;

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<double>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCCWPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadLimitCCWPosition(id, callback));
        }
        public override void WriteLimitCCWPosition(byte id, double ccwLimit)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (ccwLimit > 180 || ccwLimit < 0)
            {
                throw new BadInputParametersException("cwLimitがレンジ外です");
            }
            ccwLimit = (ccwLimit + 180) / 360.0 * 4095.0;

            // パラメータ生成
            byte[] param = generateParameters(Address.MaxPositionLimit, (uint)(ccwLimit), 4);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Temperature Limit
        public override int ReadLimitTemperature(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.TemperatureLimit, 1, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadLimitTemperatureAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadLimitTemperature(id, callback));
        }
        public override void WriteLimitTemperature(byte id, int temperatureLimit)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (temperatureLimit < 0 || temperatureLimit > 100)
            {
                throw new BadInputParametersException("temperatureLimitがレンジ外です");
            }

            // パラメータ生成
            byte[] param = generateParameters(Address.TemperatureLimit, (uint)(temperatureLimit), 1);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Current Limit
        public override int ReadLimitCurrent(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.CurrentLimit, 2, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    return (int)(BitConverter.ToInt16(response, 0) * 2.69);
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadLimitCurrentAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadLimitCurrent(id, callback));
        }
        public override void WriteLimitCurrent(byte id, int currentLimit)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (currentLimit < 0 || currentLimit > 3210)
            {
                throw new BadInputParametersException("currentLimitがレンジ外です");
            }

            currentLimit = (int)(currentLimit / 2.69);

            // パラメータ生成
            byte[] param = generateParameters(Address.CurrentLimit, (uint)(currentLimit), 2);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Drive Mode
        public override int ReadDriveMode(byte id, Action<byte, int> callback = null)
        {
            // IDチェック
            checkId(id);

            // パラメータ生成
            byte[] param = generateParameters(Address.DriveMode, 1, 2);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];

                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            // 送信
            return getFunction<int>(id, Instructions.Read, param, responseProcess, callback);
        }
        public override async Task<int> ReadDriveModeAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadDriveMode(id, callback));
        }
        public override void WriteDriveMode(byte id, int driveMode)
        {
            // IDチェック
            checkId(id);

            // 値を変換
            if (driveMode > 5)
            {
                throw new BadInputParametersException("driveModeがレンジ外です");
            }

            // パラメータ生成
            byte[] param = generateParameters(Address.DriveMode, (uint)(driveMode), 1);
            getFunction<byte[]>(id, Instructions.Write, param, null, defaultWriteCallback);
        }

        // Burst Function
        public override Dictionary<int, byte[]> BurstReadMemory(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            byte[] param = generateParametersSyncRead((byte)address, length, idList);
            return getFunctionBurstRead<byte[]>(0xFE, Instructions.SyncRead, idList.Length, param, null, callback);
        }
        public override async Task<Dictionary<int, byte[]>> BurstReadMemoryAsync(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            return await Task.Run(() => BurstReadMemory(idList, address, length, callback));
        }

        public override void BurstWriteMemory(Dictionary<int, byte[]> idDataList, ushort address, ushort length)
        {
            byte[] param = generateParametersSyncWrite((byte)address, length, idDataList);
            getFunctionBurstRead<byte[]>(0xFE, Instructions.SyncWrite, 0, param, null, null);
            ;
        }

        // Burst Functions ( Position )
        public override Dictionary<int, double> BurstReadPositions(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 4)
                {
                    return BitConverter.ToInt32(response, 0) * 360.0 / 4095.0 - 180.0;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            byte[] param = generateParametersSyncRead(Address.PresentPosition, 4, idList);
            return getFunctionBurstRead(0xFE, Instructions.SyncRead, idList.Length, param, responseProcess, callback);
        }

        public override async Task<Dictionary<int, double>> BurstReadPositionsAsync(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            return await Task.Run(() => BurstReadPositions(idList, callback));
        }

        public override void BurstWriteTargetPositions(Dictionary<int, double> idPositionList)
        {
            Dictionary<int, int> idPositionListInt = new Dictionary<int, int>();

            foreach(KeyValuePair<int, double> item in idPositionList)
            {
                double deg = item.Value;
                if (deg < -180) deg = -180;
                else if (deg > 180) deg = 180;
                deg += 180;

                idPositionListInt.Add(item.Key, (int)(deg * 4095.0 / 360.0));
            }

            byte[] param = generateParametersSyncWrite(Address.GoalPosition, 4, idPositionListInt);
            getFunctionBurstRead<byte[]>(0xFE, Instructions.SyncWrite, 0, param, null, null);
        }
    }
}
