using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using gs2d;

namespace gs2d_v1
{
    class Program
    {
        static Driver servo;

        static void TimeoutEvent()
        {
            Console.WriteLine("接続がタイムアウトしました");
        }

        static void Main(string[] args)
        {
            byte id = 1;

            Console.WriteLine("Start...");

//            servo = new RobotisP20("COM3", 115200);
//            servo = new B3M("COM3", 1500000);
            servo = new Futaba("COM3", 115200);
//            servo = new KRS("COM3", 115200, Parity.Even);

            servo.TimeoutCallbackEvent += TimeoutEvent;

            // Ping : check model number
            try { Console.WriteLine("Ping : " + servo.Ping(id)["modelNumber"]); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Offset : degree
            try { Console.WriteLine("Offset Write: "); servo.WriteOffset(id, 5.0); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Offset Read: " + servo.ReadOffset(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Deadband : deg/sec
            try { Console.WriteLine("Deadband Write: "); servo.WriteDeadband(id, 5.0); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Deadband Read: " + servo.ReadDeadband(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // PGain
            try { Console.WriteLine("PGain Write: "); servo.WritePGain(id, 80); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("PGain Read: " + servo.ReadPGain(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // IGain
            try { Console.WriteLine("IGain Write: "); servo.WriteIGain(id, 500); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("IGain Read: " + servo.ReadIGain(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // DGain
            try { Console.WriteLine("DGain Write: "); servo.WriteDGain(id, 400); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("DGain Read: " + servo.ReadDGain(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // MaxTorque
            try { Console.WriteLine("MaxTorque Write: "); servo.WriteMaxTorque(id, 90); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("MaxTorque Read: " + servo.ReadMaxTorque(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Id
//            try { Console.WriteLine("ID Write: "); servo.WriteID(id,id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("ID Read: " + servo.ReadID(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // ROM
            try { Console.WriteLine("SaveROM: "); servo.SaveROM(id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("LoadROM: "); servo.LoadROM(id); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("ResetMemory: "); servo.ResetMemory(id); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            System.Threading.Thread.Sleep(1000);

            // Baudrate: TODO
            try { Console.WriteLine("Baudrate Write: "); servo.WriteBaudrate(id, 115200); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Baudrate Read: " + servo.ReadBaudrate(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // CW Limit Position
            try { Console.WriteLine("CW Position Limit Write: "); servo.WriteLimitCWPosition(id, -135); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("CW Position Limit Read: " + servo.ReadLimitCWPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // CCW Limit Position : TODO
            try { Console.WriteLine("CCW Position Limit Write: "); servo.WriteLimitCCWPosition(id, 135); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("CCW Position Limit Read: " + servo.ReadLimitCCWPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Temperature Limit
            try { Console.WriteLine("Temperature Limit Write: "); servo.WriteLimitTemperature(id, 80); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Temperature Limit Read: " + servo.ReadLimitTemperature(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Current Limit
            try { Console.WriteLine("Current Limit Write: "); servo.WriteLimitCurrent(id, 5000); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Current Limit Read: " + servo.ReadLimitCurrent(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Drive Mode
            try { Console.WriteLine("Drive Mode Write: "); servo.WriteDriveMode(id, 0); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Drive Mode Read: " + servo.ReadDriveMode(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Torque Enable
            try { Console.WriteLine("Torque Write: "); servo.WriteTorqueEnable(id, 1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Torque Read: " + servo.ReadTorqueEnable(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Temperature
            try { Console.WriteLine("Temperature Read: " + servo.ReadTemperature(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Current
            try { Console.WriteLine("Current Read: " + servo.ReadCurrent(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Voltage
            try { Console.WriteLine("Voltage Read: " + servo.ReadVoltage(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Speed 
            try { Console.WriteLine("Speed Write: "); servo.WriteSpeed(id, 100); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Speed Read: " + servo.ReadSpeed(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Accel Time
            try { Console.WriteLine("Accel Time Write: "); servo.WriteAccelTime(id, 0.1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Accel Time Read: " + servo.ReadAccelTime(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Target Time : TODO
            try { Console.WriteLine("Target Time Write: "); servo.WriteTargetTime(id, 0.1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            try { Console.WriteLine("Target Time Read: " + servo.ReadTargetTime(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Target Position: TODO
            try { Console.WriteLine("Target Position Write: "); servo.WriteTargetPosition(id, 90); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            System.Threading.Thread.Sleep(1000);
            try { Console.WriteLine("Target Position Write: "); servo.WriteTargetPosition(id, -45); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            System.Threading.Thread.Sleep(1000);
            try { Console.WriteLine("Target Position Read: " + servo.ReadTargetPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Current Position : TODO
            try { Console.WriteLine("Current Position Read: " + servo.ReadCurrentPosition(id)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

            // Burst Position 
            Dictionary<int, double> target = new Dictionary<int, double>();
            target.Add(id, 45);
            try { Console.WriteLine("Burst Position Write: "); servo.BurstWriteTargetPositions(target); } catch (Exception ex) { Console.WriteLine(ex.Message); }
            System.Threading.Thread.Sleep(1000);
            try { Console.WriteLine("Burst Position Read: " + servo.BurstReadPositions(new int[1] {id})[id]); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}
