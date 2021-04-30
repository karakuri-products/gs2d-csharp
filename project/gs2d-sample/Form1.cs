using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using gs2d;

namespace gs2d_sample
{
    public partial class Form1 : Form
    {
        static Driver servo;

        double targetPosition1 = 0.0;
        double targetPosition2 = 0.0;
        double targetPosition3 = 0.0;
        double targetPosition4 = 0.0;

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        internal CancellationToken token;

        public Form1()
        {
            InitializeComponent();
        }

        void TimeoutEvent()
        {
            this.Invoke((MethodInvoker)(() =>
            {
                tokenSource.Cancel();
                MessageBox.Show("接続がタイムアウトしました");
            }));
        }

        private void ReloadComPort()
        {
            // ポート名を取得（デバイス名は省略）
            string[] ports = SerialPort.GetPortNames();

            comComboBox.Items.Clear();

            if(ports.Length == 0)
            {
                comComboBox.Items.Add("-");
                openButton.Enabled = false;
            }
            else
            {
                openButton.Enabled = true;
                foreach (string port in ports)
                {
                    comComboBox.Items.Add(port);
                }
                comComboBox.SelectedIndex = 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 15 ~-15 のGUIソフトを用意
            // Sinウェーブ
            // 補完について残す

            ReloadComPort();
        }

        private void reloadButton_Click(object sender, EventArgs e)
        {
            ReloadComPort();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if(openButton.Text == "Open")
            {
                // 指定のCOMポートを開く
                string port = (string)comComboBox.SelectedItem;
                servo = new RobotisP20(port, 57600);

                servo.TimeoutCallbackEvent += TimeoutEvent;

                servo.WriteTorqueEnable(1, 1);
                servo.WriteTorqueEnable(2, 1);
                servo.WriteTorqueEnable(3, 1);
                servo.WriteTorqueEnable(4, 1);

                openButton.Text = "Close";

                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                Task.Run(async () => { await MainTask(token); });
            }
            else
            {
                tokenSource.Cancel();
            }
        }

        void TemperatureCallback(byte id, ushort temperature)
        {
            switch (id)
            {
                case 1:
                    this.Invoke((MethodInvoker)(() => motorNumericUpDown1.Value = temperature));
                    break;
                case 2:
                    this.Invoke((MethodInvoker)(() => motorNumericUpDown2.Value = temperature));
                    break;
                case 3:
                    this.Invoke((MethodInvoker)(() => motorNumericUpDown3.Value = temperature));
                    break;
                case 4:
                    this.Invoke((MethodInvoker)(() => motorNumericUpDown4.Value = temperature));
                    break;
            }
        }

        private async Task MainTask(CancellationToken cancelToken)
        {
            while(!cancelToken.IsCancellationRequested)
            {
                servo.ReadTemperature(1, TemperatureCallback);
                servo.ReadTemperature(2, TemperatureCallback);
                servo.ReadTemperature(3, TemperatureCallback);
                servo.ReadTemperature(4, TemperatureCallback);
                

                Dictionary<int, double> target = new Dictionary<int, double>();

                target.Add(1, targetPosition1);
                target.Add(2, targetPosition2);
                target.Add(3, targetPosition3);
                target.Add(4, targetPosition4);

                servo.BurstWriteTargetPositions(target);

                await Task.Delay(20);
            }

            servo.Close();
            openButton.Text = "Open";
        }

        private void motorTrackBar1_Scroll(object sender, EventArgs e)
        {
            targetPosition1 = motorTrackBar1.Value / 10.0;
        }

        private void motorTrackBar2_Scroll(object sender, EventArgs e)
        {
            targetPosition2 = motorTrackBar2.Value / 10.0;
        }

        private void motorTrackBar3_Scroll(object sender, EventArgs e)
        {
            targetPosition3 = motorTrackBar3.Value / 10.0;
        }

        private void motorTrackBar4_Scroll(object sender, EventArgs e)
        {
            targetPosition4 = motorTrackBar4.Value / 10.0;
        }

        int deg = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            deg += 1;
            deg %= 360;

            motorTrackBar1.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);
            motorTrackBar2.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);
            motorTrackBar3.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);
            motorTrackBar4.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);

            targetPosition1 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
            targetPosition2 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
            targetPosition3 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
            targetPosition4 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }
    }
}
