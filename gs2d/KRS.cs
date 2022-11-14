using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace gs2d
{
    public class KRS : Driver
    {
        class KRSRom
        {
            public byte id = 0;
            public byte[] data = new byte[64];
        }

        List<KRSRom> eepromList = new List<KRSRom>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudrate"></param>
        /// <param name="stopBits"></param>
        public KRS(string portName, int baudrate = 115200, Parity parity = Parity.None) : base(portName, baudrate, parity)
        {

        }

        public KRS() : base()
        {

        }
        ~KRS()
        {

        }

        internal override bool IsCompleteResponse(byte[] data)
        {
            if (data.Length == 0) return false;

            byte header = (byte)((data[0] & 0b11100000) >> 5);

            switch (header)
            {
                case 0: // ポジション設定の返信コマンド
                case 4:
                    return (data.Length >= 3);
                case 1: // 読み出しコマンド
                    if (data.Length < 2) return false;
                    switch (data[1])
                    {
                        case 0: return (data.Length >= 66);
                        case 5: return (data.Length >= 4);
                        default: return (data.Length >= 3);
                    }
                case 2: // 書き込み返信コマンド
                    if (data.Length < 2) return false;
                    switch (data[1])
                    {
                        case 0: return (data.Length >= 2);
                        default: return (data.Length >= 3);
                    }
                case 7:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// ID不正チェック関数
        /// </summary>
        /// <param name="id"></param>
        private void checkId(byte id) { if (id < 0 || id > 31) throw new BadInputParametersException("IDがレンジ外です"); }

        private T getFunction<T>(byte[] command, Func<byte[], T> responseProcess = null, Action<byte, T> callback = null)
        {
            bool isReceived = false;
            T data = default(T);

            byte id = 0;

            ReceiveCallbackFunction templateReceiveCallback = (response) =>
            {
                byte[] responseData;
                KRSRom rom = new KRSRom();

                // コマンドから必要なデータを抜き出し
                byte header = (byte)((response[0] & 0b11100000) >> 5);
                byte responseId = (byte)((response[0] & 0b11111));
                switch (header)
                {
                    case 0: responseData = response.Skip(1).Take(2).ToArray(); break;
                    case 1:
                        switch (response[1])
                        {
                            case 0:
                                responseData = response.Skip(2).Take(64).ToArray();

                                for(int i = 0; i < eepromList.Count; i++)
                                {
                                    if(eepromList[i].id == responseId)
                                    {
                                        Array.Copy(responseData, eepromList[i].data, 64);
                                        break;
                                    }

                                    if(i == eepromList.Count - 1)
                                    {
                                        rom.id = responseId;
                                        Array.Copy(responseData, rom.data, 64);
                                        eepromList.Add(rom);
                                    }
                                }

                                break;
                            case 5: responseData = response.Skip(2).Take(2).ToArray();
                                break;
                            default: responseData = response.Skip(2).Take(1).ToArray();
                                break;
                        }
                        break;
                    case 7: responseData = response.Take(1).ToArray(); break;
                    case 4: responseData = response.Skip(1).Take(2).ToArray(); break;
                    default: responseData = null; break;
                }

                // データを処理
                // 例外はTODOとして保留
                try
                {
                    if (responseProcess != null) data = (T)(object)responseProcess(responseData);
                    else data = (T)(object)responseData;
                }
                catch (Exception ex) { }

                // 終了処理
                if (callback != null) callback(id, data);

                isReceived = true;
            };

            id = (byte)(command[0] & 0b11111);

            // コマンド送信
            commandHandler.AddCommand(command, templateReceiveCallback, 1, new byte[1] { (byte)(command[0] & 0b11111) });

            // コールバックがあれば任せて終了
            if (callback != null) return default(T);

            // タイムアウト関数を登録
            Action<byte> timeoutEvent = (byte target) => { isReceived = true; };
            TimeoutCallbackEvent += timeoutEvent;

            // 無ければ受信完了待ち
            while (isReceived == false) ;

            // タイムアウトイベントを削除
            TimeoutCallbackEvent -= timeoutEvent;

            return data;
        }

        /// <summary>
        /// Write関数用コールバック
        /// </summary>
        /// <param name="data"></param>
        private void defaultWriteCallback(byte id, byte[] data)
        {

        }

        int isRomDataAvailable(byte id)
        {
            for (int t = 0; t < eepromList.Count; t++)
            {
                if (eepromList[t].id == id) return t;
            }
            return -1;
        }

        // ------------------------------------------------------------------------------------------
        // General
        public override byte[] ReadMemory(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x00 };

            checkId(id);
            if (address + length > 64) throw new BadInputParametersException("アドレスが範囲外です");

            Func<byte[], byte[]> responseProcess = (response) =>
            {
                if (response != null && response.Length == 64) return response.Skip(address).Take(length).ToArray();
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(command, responseProcess, callback);
        }

        public override async Task<byte[]> ReadMemoryAsync(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null)
        {
            return await Task.Run(() => ReadMemory(id, address, length, callback));
        }
        public override void WriteMemory(byte id, ushort address, byte[] data)
        {
            checkId(id);
            int listPos = isRomDataAvailable(id);
            if (listPos < 0) throw new NotSupportException("KRSサーボの場合、書き込みの前に一度EEPROMを読み込んでください");
            if (address + data.Length > 64) throw new BadInputParametersException("アドレスが範囲外です");

            // EEPROMデータを書き換え
            for (int i = 0; i < data.Length; i++)
            {
                eepromList[listPos].data[address + i] = data[i];
            }

            byte[] command = new byte[66];
            command[0] = (byte)(0b11000000 | id);
            command[1] = 0;
            Array.Copy(eepromList[listPos].data, 0, command, 2, 64);
            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // Ping
        public override Dictionary<string, ushort> Ping(byte id, Action<byte, Dictionary<string, ushort>> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x00 };

            checkId(id);

            Func<byte[], Dictionary<string, ushort>> responseProcess = (response) =>
            {
                if (response != null && response.Length == 64)
                {
                    byte[] bkup = response.Skip(0).Take(2).ToArray();
                    ushort backUpChara = (ushort)((bkup[0] << 4) + bkup[1]);
                    return new Dictionary<string, ushort>()
                    {
                        {"modelNumber", backUpChara},
                        {"firmwareVersion", 0}
                    };
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };
            return getFunction(command, responseProcess, callback);
        }
        public override async Task<Dictionary<string, ushort>> PingAsync(byte id, Action<byte, Dictionary<string, ushort>> callback = null)
        {
            return await Task.Run(() => Ping(id, callback));
        }

        // Torque 
        public override byte ReadTorqueEnable(byte id, Action<byte, byte> callback = null)
        {
            throw new NotSupportedException("KRSではReadTorqueEnableに対応していません。");
        }
        public override async Task<byte> ReadTorqueEnableAsync(byte id, Action<byte, byte> callback = null)
        {
            throw new NotSupportedException("KRSではReadTorqueEnableAsyncに対応していません。");
        }
        public override void WriteTorqueEnable(byte id, byte torque)
        {
            throw new NotSupportedException("KRSではWriteTorqueEnableに対応していません。");
        }

        /// <summary>
        /// 単位無しの値から温度への変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private ushort ValueToTemperature(ushort value)
        {
            return (ushort)(100 - (value - 30) / 1.425);
        }

        private ushort TemperatureToValue(ushort temperature)
        {
            return (ushort)((100 - temperature) * 1.425 + 30);
        }

        // Temperature
        public override ushort ReadTemperature(byte id, Action<byte, ushort> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x04 };

            checkId(id);

            Func<byte[], ushort> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1) return ValueToTemperature(response[0]);
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(command, responseProcess, callback);
        }

        public override async Task<ushort> ReadTemperatureAsync(byte id, Action<byte, ushort> callback = null)
        {
            return await Task.Run(() => ReadTemperature(id, callback));
        }

        // Current
        public override int ReadCurrent(byte id, Action<byte, int> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x03 };

            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    if (response[0] < 63) return response[0] * 100;
                    else return (response[0] - 64) * 100;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction<int>(command, responseProcess, callback);
        }
        public override async Task<int> ReadCurrentAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadCurrent(id, callback));
        }

        // Voltage
        public override double ReadVoltage(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadVoltageに対応していません。");
        }
        public override async Task<double> ReadVoltageAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadVoltageAsyncに対応していません。");
        }

        // Target Position
        public override double ReadTargetPosition(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadTargetPositionに対応していません。");
        }
        public override async Task<double> ReadTargetPositionAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadTargetPositionAsyncに対応していません。");
        }
        public override void WriteTargetPosition(byte id, double position)
        {
            byte[] command = new byte[3] { (byte)(0b10000000 | id), 0, 0 };

            // Positionの値チェック
            if (position < -135) position = -135;
            else if (position > 135) position = 135;

            // PositionをKRS用に変換
            ushort tch = (ushort)(7500 - 29.629 * position);

            command[1] = (byte)((tch >> 7) & 0x7F);
            command[2] = (byte)((tch) & 0x7F);

            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // Current Position
        public override double ReadCurrentPosition(byte id, Action<byte, double> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x05 };

            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 2)
                {
                    int position = ((response[0] << 7) + response[1]);
                    return (7500 - position) / 29.629;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(command, responseProcess, callback);
        }
        public override async Task<double> ReadCurrentPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadCurrentPosition(id, callback));
        }

        // Offset
        public override double ReadOffset(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadOffsetに対応していません。");
        }
        public override async Task<double> ReadOffsetAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadOffsetAsyncに対応していません。");
        }
        public override void WriteOffset(byte id, double offset)
        {
            throw new NotSupportedException("KRSではWriteOffsetに対応していません。");
        }

        // Deadband
        public override double ReadDeadband(byte id, Action<byte, double> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x00 };

            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 64)
                {
                    byte[] res = response.Skip(8).Take(2).ToArray();
                    ushort deadband = (ushort)((res[0] << 4) + res[1]);
                    return (double)deadband;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };
            return getFunction(command, responseProcess, callback);
        }
        public override async Task<double> ReadDeadbandAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadDeadband(id, callback));
        }
        public override void WriteDeadband(byte id, double deadband)
        {
            checkId(id);
            int listPos = isRomDataAvailable(id);
            if (listPos < 0) throw new NotSupportException("KRSサーボの場合、書き込みの前に一度EEPROMを読み込んでください");
            if (deadband < 0) deadband = 0;
            else if (deadband > 5) deadband = 5;

            eepromList[listPos].data[8] = 0; eepromList[listPos].data[9] = (byte)deadband;

            byte[] command = new byte[66];
            command[0] = (byte)(0b11000000 | id);
            command[1] = 0;
            Array.Copy(eepromList[listPos].data, 0, command, 2, 64);
            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // Target Time
        public override double ReadTargetTime(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadTargetTimeに対応していません。");
        }
        public override async Task<double> ReadTargetTimeAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadTargetTimeAsyncに対応していません。");
        }
        public override void WriteTargetTime(byte id, double targetTime)
        {
            throw new NotSupportedException("KRSではWriteTargetTimeに対応していません。");
        }

        // Accel Time
        public override double ReadAccelTime(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadAccelTimeに対応していません。");
        }
        public override async Task<double> ReadAccelTimeAsync(byte id, Action<byte, double> callback = null)
        {
            throw new NotSupportedException("KRSではReadAccelTimeAsyncに対応していません。");
        }
        public override void WriteAccelTime(byte id, double accelTime)
        {
            throw new NotSupportedException("KRSではWriteAccelTimeに対応していません。");
        }

        // P Gain
        public override int ReadPGain(byte id, Action<byte, int> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x01 };

            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(command, responseProcess, callback);
        }
        public override async Task<int> ReadPGainAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadPGain(id, callback));
        }
        public override void WritePGain(byte id, int gain)
        {
            byte[] command = new byte[3] { (byte)(0b11000000 | id), 1, 0 };

            // Positionの値チェック
            if (gain < 1) gain = 1;
            else if (gain > 127) gain = 127;

            // PositionをKRS用に変換
            command[2] = (byte)gain;

            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // I Gain
        public override int ReadIGain(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadIGainに対応していません。");
        }
        public override async Task<int> ReadIGainAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadIGainAsyncに対応していません。");
        }
        public override void WriteIGain(byte id, int gain)
        {
            throw new NotSupportedException("KRSではWriteIGainに対応していません。");
        }

        // D Gain
        public override int ReadDGain(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadDGainに対応していません。");
        }
        public override async Task<int> ReadDGainAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadDGainAsyncに対応していません。");
        }
        public override void WriteDGain(byte id, int gain)
        {
            throw new NotSupportedException("KRSではWriteDGainに対応していません。");
        }

        // Max Torque
        public override int ReadMaxTorque(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadMaxTorqueに対応していません。");
        }
        public override async Task<int> ReadMaxTorqueAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadMaxTorqueAsyncに対応していません。");
        }
        public override void WriteMaxTorque(byte id, int maxTorque)
        {
            throw new NotSupportedException("KRSではWriteMaxTorqueに対応していません。");
        }

        // Speed
        public override double ReadSpeed(byte id, Action<byte, double> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x02 };

            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0];
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(command, responseProcess, callback);
        }
        public override async Task<double> ReadSpeedAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadSpeed(id, callback));
        }
        public override void WriteSpeed(byte id, double speed)
        {
            byte[] command = new byte[3] { (byte)(0b11000000 | id), 2, 0 };

            // Positionの値チェック
            if (speed < 1) speed = 1;
            else if (speed > 127) speed = 127;

            // PositionをKRS用に変換
            command[2] = (byte)speed;

            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // ID
        public override int ReadID(byte id, Action<byte, int> callback = null)
        {
            byte[] command = new byte[4] { 0xFF, 0, 0, 0 };
            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 1)
                {
                    return response[0] & 0b11111;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };

            return getFunction(command, responseProcess, callback);
        }
        public override async Task<int> ReadIDAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadID(id, callback));
        }
        public override void WriteID(byte id, int servoid)
        {
            byte[] command = new byte[4] { 0b11100000, 1, 1, 1 };

            checkId(id);
            checkId((byte)servoid);

            command[0] += (byte)servoid;

            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // ROM
        public override void SaveROM(byte id)
        {
            throw new NotSupportedException("KRSではSaveROMに対応していません。");
        }
        public override void LoadROM(byte id)
        {
            throw new NotSupportedException("KRSではLoadROMに対応していません。");
        }
        public override void ResetMemory(byte id)
        {
            throw new NotSupportedException("KRSではResetMemoryに対応していません。");
        }

        // Baudrate
        public override int ReadBaudrate(byte id, Action<byte, int> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x00 };

            checkId(id);

            Func<byte[], int> responseProcess = (response) =>
            {
                if (response != null && response.Length == 64)
                {
                    byte[] res = response.Skip(26).Take(2).ToArray();
                    ushort baudrate = (ushort)((res[0] << 4) | res[1]);
                    switch (baudrate)
                    {
                        case 0x0A: return 115200;
                        case 0x01: return 625000;
                        case 0b00: return 1250000;
                    }
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };
            return getFunction(command, responseProcess, callback);
        }
        public override async Task<int> ReadBaudrateAsync(byte id, Action<byte, int> callback = null)
        {
            return await Task.Run(() => ReadBaudrate(id, callback));
        }
        public override void WriteBaudrate(byte id, int baudrate)
        {
            checkId(id);
            int listPos = isRomDataAvailable(id);
            if (listPos < 0) throw new NotSupportException("KRSサーボの場合、書き込みの前に一度EEPROMを読み込んでください");
            if (!(baudrate == 115200 || baudrate == 625000 || baudrate == 1250000))
            {
                throw new BadInputParametersException("指定された通信速度は使用できません");
            }
            if (baudrate == 115200) eepromList[listPos].data[27] = 0x0A;
            else if (baudrate == 625000) eepromList[listPos].data[27] = 0x01;
            else eepromList[listPos].data[27] = 0x00;

            byte[] command = new byte[66];
            command[0] = (byte)(0b11000000 | id);
            command[1] = 0;
            Array.Copy(eepromList[listPos].data, 0, command, 2, 64);
            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // CW Limit Position
        public override double ReadLimitCWPosition(byte id, Action<byte, double> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x00 };

            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 64)
                {
                    byte[] res = response.Skip(16).Take(4).ToArray();
                    ushort cw = (ushort)((res[0] << 12) | (res[1] << 8) | (res[2] << 4) | (res[3]));

                    return (7500 - cw) / 29.629;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };
            return getFunction(command, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCWPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadLimitCWPosition(id, callback));
        }
        public override void WriteLimitCWPosition(byte id, double cwLimit)
        {
            checkId(id);
            int listPos = isRomDataAvailable(id);
            if (listPos < 0) throw new NotSupportException("KRSサーボの場合、書き込みの前に一度EEPROMを読み込んでください");
            if (cwLimit < -135) cwLimit = -135;
            else if (cwLimit > 135) cwLimit = 135;

            int position = (int)(7500 - cwLimit * 29.629);

            eepromList[listPos].data[16] = (byte)((position >> 12) & 0x0F);
            eepromList[listPos].data[17] = (byte)((position >> 8) & 0x0F);
            eepromList[listPos].data[18] = (byte)((position >> 4) & 0x0F);
            eepromList[listPos].data[19] = (byte)((position >> 0) & 0x0F);

            byte[] command = new byte[66];
            command[0] = (byte)(0b11000000 | id);
            command[1] = 0;
            Array.Copy(eepromList[listPos].data, 0, command, 2, 64);
            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // CCW Limit Position
        public override double ReadLimitCCWPosition(byte id, Action<byte, double> callback = null)
        {
            byte[] command = new byte[2] { (byte)(0b10100000 | id), 0x00 };

            checkId(id);

            Func<byte[], double> responseProcess = (response) =>
            {
                if (response != null && response.Length == 64)
                {
                    byte[] res = response.Skip(20).Take(4).ToArray();
                    ushort cw = (ushort)((res[0] << 12) | (res[1] << 8) | (res[2] << 4) | (res[3]));

                    return (7500 - cw) / 29.629;
                }
                throw new InvalidResponseDataException("サーボからのレスポンスが不正です");
            };
            return getFunction(command, responseProcess, callback);
        }
        public override async Task<double> ReadLimitCCWPositionAsync(byte id, Action<byte, double> callback = null)
        {
            return await Task.Run(() => ReadLimitCCWPosition(id, callback));
        }
        public override void WriteLimitCCWPosition(byte id, double ccwLimit)
        {
            checkId(id);
            int listPos = isRomDataAvailable(id);
            if (listPos < 0) throw new NotSupportException("KRSサーボの場合、書き込みの前に一度EEPROMを読み込んでください");
            if (ccwLimit < -135) ccwLimit = -135;
            else if (ccwLimit > 135) ccwLimit = 135;

            int position = (int)(7500 - ccwLimit * 29.629);

            eepromList[listPos].data[20] = (byte)((position >> 12) & 0x0F);
            eepromList[listPos].data[21] = (byte)((position >> 8) & 0x0F);
            eepromList[listPos].data[22] = (byte)((position >> 4) & 0x0F);
            eepromList[listPos].data[23] = (byte)((position >> 0) & 0x0F);

            byte[] command = new byte[66];
            command[0] = (byte)(0b11000000 | id);
            command[1] = 0;
            Array.Copy(eepromList[listPos].data, 0, command, 2, 64);
            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // Temperature Limit
        public override int ReadLimitTemperature(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadLimitTemperatureに対応していません。");
        }
        public override async Task<int> ReadLimitTemperatureAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadLimitTemperatureAsyncに対応していません。");
        }
        public override void WriteLimitTemperature(byte id, int temperatureLimit)
        {
            byte[] command = new byte[3] { (byte)(0b11000000 | id), 2, 0 };

            temperatureLimit = TemperatureToValue((ushort)temperatureLimit);

            // Positionの値チェック
            if (temperatureLimit < 1) temperatureLimit = 1;
            else if (temperatureLimit > 127) temperatureLimit = 127;

            // PositionをKRS用に変換
            command[2] = (byte)temperatureLimit;

            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // Current Limit
        public override int ReadLimitCurrent(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadLimitCurrentに対応していません。");
        }
        public override async Task<int> ReadLimitCurrentAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadLimitCurrentAsyncに対応していません。");
        }
        public override void WriteLimitCurrent(byte id, int currentLimit)
        {
            byte[] command = new byte[3] { (byte)(0b11000000 | id), 3, 0 };

            currentLimit = (int)(currentLimit / 100);

            // Positionの値チェック
            if (currentLimit < 1) currentLimit = 1;
            else if (currentLimit > 127) currentLimit = 127;

            // PositionをKRS用に変換
            command[2] = (byte)currentLimit;

            getFunction<byte[]>(command, null, defaultWriteCallback);
        }

        // Drive Mode
        public override int ReadDriveMode(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadDriveModeに対応していません。");
        }
        public override async Task<int> ReadDriveModeAsync(byte id, Action<byte, int> callback = null)
        {
            throw new NotSupportedException("KRSではReadDriveModeAsyncに対応していません。");
        }
        public override void WriteDriveMode(byte id, int driveMode)
        {
            throw new NotSupportedException("KRSではWriteDriveModeに対応していません。");
        }

        // Burst Function
        public override Dictionary<int, byte[]> BurstReadMemory(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            throw new NotSupportedException("KRSではBurstReadMemoryに対応していません。");
        }
        public override async Task<Dictionary<int, byte[]>> BurstReadMemoryAsync(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null)
        {
            throw new NotSupportedException("KRSではBurstReadMemoryAsyncに対応していません。");
        }
        public override void BurstWriteMemory(Dictionary<int, byte[]> idDataList, ushort address, ushort length)
        {
            throw new NotSupportedException("KRSではWriteReadMemoryに対応していません。");
        }

        // Burst Functions ( Position )
        public override Dictionary<int, double> BurstReadPositions(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            throw new NotSupportedException("KRSではBurstReadPositionsに対応していません。");
        }
        public override async Task<Dictionary<int, double>> BurstReadPositionsAsync(int[] idList, Action<Dictionary<int, double>> callback = null)
        {
            throw new NotSupportedException("KRSではBurstReadPositionsAsyncに対応していません。");
        }
        public override void BurstWriteTargetPositions(Dictionary<int, double> idPositionList)
        {
            throw new NotSupportedException("KRSではBurstWriteTargetPositionsに対応していません。");
        }
    }
}
