using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gs2d;

namespace gs2d_v1
{
    class Program
    {
        static Driver servo;

        public static class TargetValue
        {
            public const byte CurrentID = 2;
            public const byte NewID = 2;
            public const int Baudrate = 115200;

            public const double Offset = 20;
            public const double Deadband = 5.0;
            public const int PGain = 500;
            public const int IGain = 20;
            public const int DGain = 10;
            public const int MaxTorque = 100;

            public const double CWLimit = -150.0;
            public const double CCWLimit = 150.0;

            public const int LimitCurrent = 3000;
            public const int LimitTemperature = 100;

            public const int DriveMode = 1;

            public const int Torque = 1;

            public const double Speed = 10.0;
            public const double AccelTime = 0.1;
            public const double TargetTime = 0.1;

            public const double TargetPosition1 = 150.0;
            public const double TargetPosition2 = -150.0;
        }


        static void TimeoutEvent()
        {
            Console.WriteLine("接続がタイムアウトしました");
        }

        static void PingCallback(Dictionary<string, ushort> ping)
        {
            Console.WriteLine("Ping Async : " + ping["modelNumber"] + ", " + ping["firmwareVersion"]);
        }

        static void ReadOffsetCallback(double offset)
        {
            Console.WriteLine("Offset Async : " + offset);
        }

        static void DeadbandCallback(double deadband)
        {
            Console.WriteLine("Deadband Async : " + deadband);
        }

        static void PGainCallback(int pgain)
        {
            Console.WriteLine("PGain Async : " + pgain);
        }

        static void IGainCallback(int igain)
        {
            Console.WriteLine("IGain Async : " + igain);
        }

        static void DGainCallback(int dgain)
        {
            Console.WriteLine("DGain Async : " + dgain);
        }

        static void MaxTorqueCallback(int max)
        {
            Console.WriteLine("MaxTorque Async : " + max);
        }

        static void IDCallback(int id)
        {
            Console.WriteLine("ID Async : " + id);
        }

        static void BaudrateCallback(int baudrate)
        {
            Console.WriteLine("Baudrate Async : " + baudrate);
        }

        static void LimitCWPositionCallback(double cwLimit)
        {
            Console.WriteLine("CW Limit Async : " + cwLimit);
        }

        static void LimitCCWPositionCallback(double ccwLimit)
        {
            Console.WriteLine("CCW Limit Async : " + ccwLimit);
        }

        static void LimitTemperatureCallback(int tempLimit)
        {
            Console.WriteLine("Temperature Limit Async : " + tempLimit);
        }

        static void LimitCurrentCallback(int currentLimit)
        {
            Console.WriteLine("Current Limit Async : " + currentLimit);
        }

        static void DriveModeCallback(int driveMode)
        {
            Console.WriteLine("DriveMode Async : " + driveMode);
        }

        static void TorqueCallback(byte torque)
        {
            Console.WriteLine("Torque Enable Async : " + torque);
        }

        static void TemperatureCallback(ushort temperature)
        {
            Console.WriteLine("Temperature Async : " + temperature);
        }

        static void CurrentCallback(int current)
        {
            Console.WriteLine("Current Async : " + current);
        }

        static void VoltageCallback(double voltage)
        {
            Console.WriteLine("Voltage Async : " + voltage);
        }

        static void SpeedCallback(double speed)
        {
            Console.WriteLine("Speed Async : " + speed);
        }

        static void AccelCallback(double accel)
        {
            Console.WriteLine("Accel Async : " + accel);
        }

        static void TargetTimeCallback(double targetTime)
        {
            Console.WriteLine("Target Time Async : " + targetTime);
        }

        static void TargetPositionCallback(double targetPosition)
        {
            Console.WriteLine("Target Position Async : " + targetPosition);
        }

        static void CurrentPositionCallback(double currentPosition)
        {
            Console.WriteLine("Current Position Async : " + currentPosition);
        }

        static void BurstReadPositionCallback(Dictionary<int, double> positions)
        {
//            Console.WriteLine("Burst Read Position Async + " + positions);
        }

        static void Main(string[] args)
        {
            Task task = MainTask();

            task.Wait();
        }
        static async Task MainTask()
        {
            byte id = TargetValue.CurrentID;

            Console.WriteLine("Start...");

            servo = new RobotisP20("COM3", 115200);
//            servo = new B3M("COM3", 115200);
//            servo = new Futaba("COM3", 115200);
//            servo = new KRS("COM3", 115200, Parity.Even);

            servo.TimeoutCallbackEvent += TimeoutEvent;

            // Ping : check model number
            try { await servo.PingAsync(id, PingCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Dictionary<string, ushort> ping = servo.Ping(id); Console.WriteLine("Ping : " + ping["modelNumber"] + ", " + ping["firmwareVersion"]); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Offset : degree
            try { await servo.ReadOffsetAsync(id, ReadOffsetCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Offset Write: "); servo.WriteOffset(id, TargetValue.Offset); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Offset Read: " + servo.ReadOffset(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Deadband : deg/sec
            try { await servo.ReadDeadbandAsync(id, DeadbandCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Deadband Write: "); servo.WriteDeadband(id, TargetValue.Deadband); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Deadband Read: " + servo.ReadDeadband(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // PGain
            try { await servo.ReadPGainAsync(id, PGainCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("PGain Write: "); servo.WritePGain(id, TargetValue.PGain); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("PGain Read: " + servo.ReadPGain(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // IGain
            try { await servo.ReadIGainAsync(id, IGainCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("IGain Write: "); servo.WriteIGain(id, TargetValue.IGain); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("IGain Read: " + servo.ReadIGain(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // DGain
            try { await servo.ReadDGainAsync(id, DGainCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("DGain Write: "); servo.WriteDGain(id, TargetValue.DGain); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("DGain Read: " + servo.ReadDGain(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // MaxTorque
            try { await servo.ReadMaxTorqueAsync(id, MaxTorqueCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("MaxTorque Write: "); servo.WriteMaxTorque(id, TargetValue.MaxTorque); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("MaxTorque Read: " + servo.ReadMaxTorque(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Id
            try { await servo.ReadIDAsync(id, IDCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("ID Write: "); servo.WriteID(id, TargetValue.NewID); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            id = TargetValue.NewID;
            try { Console.WriteLine("ID Read: " + servo.ReadID(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // ROM
            try { Console.WriteLine("SaveROM: "); servo.SaveROM(id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("LoadROM: "); servo.LoadROM(id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("ResetMemory: "); servo.ResetMemory(id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            System.Threading.Thread.Sleep(1000);

            // Baudrate
            try { await servo.ReadBaudrateAsync(id, BaudrateCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Baudrate Write: "); servo.WriteBaudrate(id, TargetValue.Baudrate); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Baudrate Read: " + servo.ReadBaudrate(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // CW Limit Position
            try { await servo.ReadLimitCWPositionAsync(id, LimitCWPositionCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("CW Position Limit Write: "); servo.WriteLimitCWPosition(id, TargetValue.CWLimit); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("CW Position Limit Read: " + servo.ReadLimitCWPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // CCW Limit Position : TODO
            try { await servo.ReadLimitCWPositionAsync(id, LimitCCWPositionCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("CCW Position Limit Write: "); servo.WriteLimitCCWPosition(id, TargetValue.CCWLimit); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("CCW Position Limit Read: " + servo.ReadLimitCCWPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Temperature Limit
            try { await servo.ReadLimitTemperatureAsync(id, LimitTemperatureCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Temperature Limit Write: "); servo.WriteLimitTemperature(id, TargetValue.LimitTemperature); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Temperature Limit Read: " + servo.ReadLimitTemperature(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Current Limit
            try { await servo.ReadLimitCurrentAsync(id, LimitCurrentCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Current Limit Write: "); servo.WriteLimitCurrent(id, TargetValue.LimitCurrent); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Current Limit Read: " + servo.ReadLimitCurrent(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Drive Mode
            try { await servo.ReadDriveModeAsync(id, DriveModeCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Drive Mode Write: "); servo.WriteDriveMode(id, TargetValue.DriveMode); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Drive Mode Read: " + servo.ReadDriveMode(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Torque Enable
            try { await servo.ReadTorqueEnableAsync(id, TorqueCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Torque Write: "); servo.WriteTorqueEnable(id, TargetValue.Torque); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Torque Read: " + servo.ReadTorqueEnable(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Temperature
            try { await servo.ReadTemperatureAsync(id, TemperatureCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Temperature Read: " + servo.ReadTemperature(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Current
            try { await servo.ReadCurrentAsync(id, CurrentCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Current Read: " + servo.ReadCurrent(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Voltage
            try { await servo.ReadVoltageAsync(id, VoltageCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Voltage Read: " + servo.ReadVoltage(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Speed 
            try { await servo.ReadSpeedAsync(id, SpeedCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Speed Write: "); servo.WriteSpeed(id, TargetValue.Speed); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Speed Read: " + servo.ReadSpeed(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Accel Time
            try { await servo.ReadAccelTimeAsync(id, AccelCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Accel Time Write: "); servo.WriteAccelTime(id, TargetValue.AccelTime); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Accel Time Read: " + servo.ReadAccelTime(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Target Time
            try { await servo.ReadTargetTimeAsync(id, TargetTimeCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Target Time Write: "); servo.WriteTargetTime(id, TargetValue.TargetTime); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Target Time Read: " + servo.ReadTargetTime(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Target Position
            try { await servo.ReadTargetPositionAsync(id, TargetPositionCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Target Position Write: "); servo.WriteTargetPosition(id, TargetValue.TargetPosition1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            System.Threading.Thread.Sleep(1000);
            try { Console.WriteLine("Target Position Write: "); servo.WriteTargetPosition(id, TargetValue.TargetPosition2); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            System.Threading.Thread.Sleep(1000);
            try { Console.WriteLine("Target Position Read: " + servo.ReadTargetPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Current Position 
            try { await servo.ReadCurrentPositionAsync(id, CurrentPositionCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Current Position Read: " + servo.ReadCurrentPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            // Burst Position 
            try { await servo.BurstReadPositionsAsync(new int[1] { id }, BurstReadPositionCallback); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Dictionary<int, double> target = new Dictionary<int, double>();
            target.Add(id, TargetValue.TargetPosition1);
            try { Console.WriteLine("Burst Position Write: "); servo.BurstWriteTargetPositions(target); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            System.Threading.Thread.Sleep(1000);
//            try { Console.WriteLine("Burst Position Read: " + servo.BurstReadPositions(new int[1] {id})[id]); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.WriteLine("");

            servo.Close();
        }
    }
}
