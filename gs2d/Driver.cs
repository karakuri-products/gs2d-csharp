using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace gs2d
{
    /// <summary>
    /// サーボモーターの基幹抽象クラス
    /// </summary>
    abstract public class Driver
    {
        internal SerialPort serialPort;
        internal CommandHandler commandHandler;
        public event Action<byte> TimeoutCallbackEvent;

        public Driver(string portName, int baudrate = 115200, Parity parity = Parity.None)
        {
            Open(baudrate, portName, parity);

            commandHandler = new CommandHandler(IsCompleteResponse, serialPort);
            commandHandler.TimeoutEvent += TimeoutCallbackInvoker;
        }

        abstract internal bool IsCompleteResponse(byte[] data);

        /// <summary>
        /// タイムアウト処理
        /// </summary>
        virtual internal void TimeoutCallbackInvoker(byte id)
        {
            if (TimeoutCallbackEvent != null) TimeoutCallbackEvent.Invoke(id);
        }

        // ------------------------------------------------------------------------------------------
        virtual public void Close() { serialPort.Close();  }
        virtual public void Open(int baudrate, string portName, Parity parity = Parity.None)
        {
            if (serialPort.IsOpen) serialPort.Close();

            // Serial Port
            serialPort = new SerialPort();

            // Properties
            serialPort.BaudRate = (int)baudrate;
            serialPort.Parity = parity;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;
            serialPort.PortName = portName;
            serialPort.ReadTimeout = 10;
            try
            {
                serialPort.Open();
            }
            catch (Exception ex) { throw ex; }
        }


        // ------------------------------------------------------------------------------------------
        // General
        abstract public byte[] ReadMemory(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null);
        abstract public Task<byte[]> ReadMemoryAsync(byte id, ushort address, ushort length, Action<byte, byte[]> callback = null);
        abstract public void WriteMemory(byte id, ushort address, byte[] data);

        // Ping
        abstract public Dictionary<string, ushort> Ping(byte id, Action<byte, Dictionary<string, ushort>> callback = null);
        abstract public Task<Dictionary<string, ushort>> PingAsync(byte id, Action<byte, Dictionary<string, ushort>> callback = null);

        // Torque 
        abstract public byte ReadTorqueEnable(byte id, Action<byte, byte> callback = null);
        abstract public Task<byte> ReadTorqueEnableAsync(byte id, Action<byte, byte> callback = null);
        abstract public void WriteTorqueEnable(byte id, byte torque);

        // Temperature
        abstract public ushort ReadTemperature(byte id, Action<byte, ushort> callback = null);
        abstract public Task<ushort> ReadTemperatureAsync(byte id, Action<byte, ushort> callback = null);

        // Current
        abstract public int ReadCurrent(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadCurrentAsync(byte id, Action<byte, int> callback = null);

        // Voltage
        abstract public double ReadVoltage(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadVoltageAsync(byte id, Action<byte, double> callback = null);

        // Target Position
        abstract public double ReadTargetPosition(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadTargetPositionAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteTargetPosition(byte id, double position);

        // Current Position
        abstract public double ReadCurrentPosition(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadCurrentPositionAsync(byte id, Action<byte, double> callback = null);

        // Offset
        abstract public double ReadOffset(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadOffsetAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteOffset(byte id, double offset);

        // Deadband
        abstract public double ReadDeadband(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadDeadbandAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteDeadband(byte id, double deadband);

        // Target Time
        abstract public double ReadTargetTime(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadTargetTimeAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteTargetTime(byte id, double targetTime);

        // Accel Time
        abstract public double ReadAccelTime(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadAccelTimeAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteAccelTime(byte id, double accelTime);

        // P Gain
        abstract public int ReadPGain(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadPGainAsync(byte id, Action<byte, int> callback = null);
        abstract public void WritePGain(byte id, int gain);

        // I Gain
        abstract public int ReadIGain(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadIGainAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteIGain(byte id, int gain);

        // D Gain
        abstract public int ReadDGain(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadDGainAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteDGain(byte id, int gain);

        // Max Torque
        abstract public int ReadMaxTorque(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadMaxTorqueAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteMaxTorque(byte id, int maxTorque);

        // Speed
        abstract public double ReadSpeed(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadSpeedAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteSpeed(byte id, double speed);

        // ID
        abstract public int ReadID(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadIDAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteID(byte id, int servoid);

        // ROM
        abstract public void SaveROM(byte id);
        abstract public void LoadROM(byte id);
        abstract public void ResetMemory(byte id);

        // Baudrate
        abstract public int ReadBaudrate(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadBaudrateAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteBaudrate(byte id, int baudrate);

        // CW Limit Position
        abstract public double ReadLimitCWPosition(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadLimitCWPositionAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteLimitCWPosition(byte id, double cwLimit);

        // CCW Limit Position
        abstract public double ReadLimitCCWPosition(byte id, Action<byte, double> callback = null);
        abstract public Task<double> ReadLimitCCWPositionAsync(byte id, Action<byte, double> callback = null);
        abstract public void WriteLimitCCWPosition(byte id, double ccwLimit);

        // Temperature Limit
        abstract public int ReadLimitTemperature(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadLimitTemperatureAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteLimitTemperature(byte id, int temperatureLimit);

        // Current Limit
        abstract public int ReadLimitCurrent(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadLimitCurrentAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteLimitCurrent(byte id, int currentLimit);

        // Drive Mode
        abstract public int ReadDriveMode(byte id, Action<byte, int> callback = null);
        abstract public Task<int> ReadDriveModeAsync(byte id, Action<byte, int> callback = null);
        abstract public void WriteDriveMode(byte id, int driveMode);

        // Burst Function
        abstract public Dictionary<int, byte[]> BurstReadMemory(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null);
        abstract public Task<Dictionary<int, byte[]>> BurstReadMemoryAsync(int[] idList, ushort address, ushort length, Action<Dictionary<int, byte[]>> callback = null);
        abstract public void BurstWriteMemory(Dictionary<int, byte[]> idDataList, ushort address, ushort length);

        // Burst Functions ( Position )
        abstract public Dictionary<int, double> BurstReadPositions(int[] idList, Action<Dictionary<int, double>> callback = null);
        abstract public Task<Dictionary<int, double>> BurstReadPositionsAsync(int[] idList, Action<Dictionary<int, double>> callback = null);
        abstract public void BurstWriteTargetPositions(Dictionary<int, double> idPositionList);
    }
}
