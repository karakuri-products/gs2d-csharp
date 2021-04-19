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

        uint sinBase = 0;

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        internal CancellationToken token;

        public Form1()
        {
            InitializeComponent();
        }

        void TimeoutEvent()
        {
            // タイムアウト時は通信用タスクを終了させる
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

            // ポート発見/未発見で処理変更
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
                servo = new RobotisP20(port, 115200);

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

        void TemperatureCallback1(ushort temperature)
        {
            this.Invoke((MethodInvoker)(() => motorNumericUpDown1.Value = temperature));
        }

        void TemperatureCallback2(ushort temperature)
        {
            this.Invoke((MethodInvoker)(() => motorNumericUpDown2.Value = temperature));
        }

        void TemperatureCallback3(ushort temperature)
        {
            this.Invoke((MethodInvoker)(() => motorNumericUpDown3.Value = temperature));
        }

        void TemperatureCallback4(ushort temperature)
        {
            this.Invoke((MethodInvoker)(() => motorNumericUpDown4.Value = temperature));
        }

        private async Task MainTask(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                servo.ReadTemperature(1, TemperatureCallback1);
                servo.WriteTargetPosition(1, targetPosition1);

                servo.ReadTemperature(2, TemperatureCallback2);
                servo.WriteTargetPosition(2, targetPosition2);

                servo.ReadTemperature(3, TemperatureCallback3);
                servo.WriteTargetPosition(3, targetPosition3);

                servo.ReadTemperature(4, TemperatureCallback4);
                servo.WriteTargetPosition(4, targetPosition4);

                await Task.Delay(40);
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

        private void sinWaveButton_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int deg = (int)(150 * System.Math.Sin(System.Math.PI * sinBase / 180.0));

            motorTrackBar1.Value = deg;
            motorTrackBar2.Value = deg;
            motorTrackBar3.Value = deg;
            motorTrackBar4.Value = deg;

            targetPosition1 = motorTrackBar1.Value / 10.0;
            targetPosition2 = motorTrackBar2.Value / 10.0;
            targetPosition3 = motorTrackBar3.Value / 10.0;
            targetPosition4 = motorTrackBar4.Value / 10.0;

            sinBase += 10;

            sinBase %= 360;
        }
    }
}
