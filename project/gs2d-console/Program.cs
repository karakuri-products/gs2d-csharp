using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using gs2d;

namespace gs2d_console
{
    class Program
    {
        static Driver servo;

        static void BurstReadPositionCallback(Dictionary<int, double> positions)
        {
            foreach (KeyValuePair<int, double> item in positions)
            {
                Console.WriteLine("Burst Pos Id : " + item.Key + ", Position : " + item.Value);
            }
        }

        static void Main(string[] args)
        {
            Task task = MainTask();

            task.Wait();
        }

        static async Task MainTask()
        {
            servo = new RobotisP20("COM3", 57600);

            Console.WriteLine("Hello World!");
            int[] target = { 1, 2 };
            try
            {
                servo.BurstReadPositions(target, BurstReadPositionCallback);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            await Task.Delay(1000);
        }
    }
}
