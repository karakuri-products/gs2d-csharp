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

        bool[] enableFlag = new bool[4] { true, true, true, true };

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        internal CancellationToken token;

        public Form1()
        {
            InitializeComponent();
        }

        void TimeoutEvent(byte id)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                tokenSource.Cancel();
                MessageBox.Show("ID" + id.ToString() + "の接続がタイムアウトしました");
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
            servoTypeComboBox.SelectedIndex = 0;
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

                switch (servoTypeComboBox.SelectedIndex)
                {
                    case 0: servo = new RobotisP20(port, 115200); break;
                    case 1: servo = new B3M(port, 115200); break;
                    case 2: servo = new KRS(port, 115200); break;
                    case 3: servo = new Futaba(port, 115200); break;
                }
                

                servo.TimeoutCallbackEvent += TimeoutEvent;

                if (enableFlag[0]) servo.WriteTorqueEnable(1, 1);
                if (enableFlag[1]) servo.WriteTorqueEnable(2, 1);
                if (enableFlag[2]) servo.WriteTorqueEnable(3, 1);
                if (enableFlag[3]) servo.WriteTorqueEnable(4, 1);

                openButton.Text = "Close";

                enable1CheckBox.Enabled = enable2CheckBox.Enabled = enable3CheckBox.Enabled = enable4CheckBox.Enabled = false;

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
                if (enableFlag[0]) servo.ReadTemperature(1, TemperatureCallback);
                if (enableFlag[1]) servo.ReadTemperature(2, TemperatureCallback);
                if (enableFlag[2]) servo.ReadTemperature(3, TemperatureCallback);
                if (enableFlag[3]) servo.ReadTemperature(4, TemperatureCallback);
                

                Dictionary<int, double> target = new Dictionary<int, double>();

                if (enableFlag[0]) target.Add(1, targetPosition1);
                if (enableFlag[1]) target.Add(2, targetPosition2);
                if (enableFlag[2]) target.Add(3, targetPosition3);
                if (enableFlag[3]) target.Add(4, targetPosition4);

                servo.BurstWriteTargetPositions(target);

                await Task.Delay(50);
            }

            servo.Close();

            this.Invoke((MethodInvoker)(() =>
            {
                enable1CheckBox.Enabled = enable2CheckBox.Enabled = enable3CheckBox.Enabled = enable4CheckBox.Enabled = true;
                openButton.Text = "Open";
            }));
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

            if (enableFlag[0]) motorTrackBar1.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);
            if (enableFlag[1]) motorTrackBar2.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);
            if (enableFlag[2]) motorTrackBar3.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);
            if (enableFlag[3]) motorTrackBar4.Value = (int)(Math.Sin(deg * (Math.PI / 180.0)) * 150.0);

            if (enableFlag[0]) targetPosition1 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
            if (enableFlag[1]) targetPosition2 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
            if (enableFlag[2]) targetPosition3 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
            if (enableFlag[3]) targetPosition3 = (Math.Sin(deg * (Math.PI / 180.0)) * 15.0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
        }

        private void enable1CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableFlag[0] = enable1CheckBox.Checked;
            motorTrackBar1.Enabled = motorNumericUpDown1.Enabled = enableFlag[0];
        }

        private void enable2CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableFlag[1] = enable2CheckBox.Checked;
            motorTrackBar2.Enabled = motorNumericUpDown2.Enabled = enableFlag[1];
        }

        private void enable3CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableFlag[2] = enable3CheckBox.Checked;
            motorTrackBar3.Enabled = motorNumericUpDown3.Enabled = enableFlag[2];
        }

        private void enable4CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enableFlag[3] = enable4CheckBox.Checked;
            motorTrackBar4.Enabled = motorNumericUpDown4.Enabled = enableFlag[3];
        }
    }
}
