using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports; // Nugetで追加必須
using System.Timers;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;

namespace gs2d
{
    internal delegate bool IsCompleteResponseFunction(byte[] data);
    internal delegate void ReceiveCallbackFunction(byte[] data);

    /// <summary>
    /// コマンドバッファ用の型
    /// </summary>
    internal struct CommandBufferType
    {
        public byte[] data;
        public ReceiveCallbackFunction callback;
        public uint servoCount;


        public CommandBufferType(byte[] data, ReceiveCallbackFunction callback, uint servoCount = 1)
        {
            this.data = new byte[data.Length];
            Array.Copy(data, this.data, data.Length);
            this.callback = callback;
            this.servoCount = servoCount;
        }
    }

    internal class CommandHandler
    {
        // 受信タイムアウト（秒）
        internal ushort receiveDataTimeoutSec = 10;

        // シリアルポート
        internal SerialPort serialPort;

        // コマンドバッファ
        internal List<CommandBufferType> commandQueue;
        internal CommandBufferType currentCommand;

        // 受信データ保存バッファ
        internal byte[] receiveBuffer;
        internal uint receivePos;

        // 受信完了チェック関数
        public IsCompleteResponseFunction isCompleteResponse;

        // タイムアウト処理用関数
        public event Action TimeoutEvent;
        private bool timeoutFlag = false;

        // タイムアウト監視用タイマ
        internal System.Timers.Timer timeoutTimer;

        // 処理ステータス
        private bool isTrafficFree = true;

        // スレッド操作用
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        internal CancellationToken token;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="function">受信完了を判定する関数</param>
        /// <param name="extSerialPort">使用するシリアルポート</param>
        public CommandHandler(IsCompleteResponseFunction function, SerialPort extSerialPort)
        {
            // 受信完了関数を設定
            isCompleteResponse = function;

            // バッファを生成
            commandQueue = new List<CommandBufferType>();
            receiveBuffer = new byte[100];

            // シリアルポートを登録
            serialPort = extSerialPort;

            // タイムアウト用タイマを設定
            timeoutTimer = new System.Timers.Timer();

            timeoutTimer.Interval = receiveDataTimeoutSec * 100;
            timeoutTimer.Elapsed += async (s, e) => {
                await Task.Run(() =>
                {
                    // タイムアウトハンドラを呼び出し
                    if (TimeoutEvent != null) TimeoutEvent.Invoke();
                    timeoutFlag = true;
                });
            };
            timeoutTimer.AutoReset = false;
            timeoutTimer.Enabled = false;

            token = tokenSource.Token;
            Task.Run(() => { DataReceivedHandler(token); });
        }

        /// <summary>
        /// コマンドをキューに追加する
        /// </summary>
        /// <param name="data">送信するバイト列</param>
        /// <param name="receiveCallback">受信時のコールバック</param>
        /// <param name="servoCount">受信するコマンド数</param>
        public void AddCommand(byte[] data, ReceiveCallbackFunction receiveCallback = null, uint servoCount = 1)
        {
            // コマンドキューにコマンドを追加
            commandQueue.Add(new CommandBufferType(data, receiveCallback, servoCount));
        }

        internal void Stop()
        {
            tokenSource.Cancel();
        }

        /// <summary>
        /// コマンドを実際に送信する
        /// </summary>
        private void SendCommand()
        {
            // 送信不可なら何もせず終了
            if (!serialPort.IsOpen) return;

            isTrafficFree = false;

            // 受信バッファを初期化
            Array.Clear(receiveBuffer, 0, 100);
            receivePos = 0;

            // コマンドを送信
            currentCommand = commandQueue[0];
            serialPort.Write(currentCommand.data, 0, currentCommand.data.Length);


            // バスを使用中に切り替え
            if (currentCommand.callback != null && currentCommand.servoCount != 0)
            {
                // タイマの再起動
                // ToDo : 処理の見直し
                StartTimeoutTimer();
            }
            else
            {
                // コマンドキューから削除
                commandQueue.RemoveAt(0);

                // キューに続きがある場合は送信開始
                if (commandQueue.Count != 0) SendCommand();
                else isTrafficFree = true;
            }
        }

        /// <summary>
        /// タイムアウト用タイマの起動
        /// </summary>
        private void StartTimeoutTimer()
        {
            // ToDo : 処理の見直し
            timeoutTimer.Enabled = false;
            timeoutTimer.Enabled = true;
        }

        /// <summary>
        /// シリアルポート受信イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DataReceivedHandler(CancellationToken cancelToken)
        {
            while (true)
            {
                SerialPort sp = serialPort;
                byte[] receiveData = new byte[100];
                int length = 0;

                if (cancelToken.IsCancellationRequested)
                {
                    return;
                }

                if(timeoutFlag)
                {
                    // コマンドキューから削除
                    commandQueue.RemoveAt(0);

                    // キューに続きがある場合は送信開始
                    if (commandQueue.Count != 0) SendCommand();
                    else
                    {
                        // タイマ停止
                        timeoutTimer.Enabled = false;
                        // バスを開放
                        isTrafficFree = true;
                    }

                    timeoutFlag = false;
                }

                // 無意味なデータを無視
                if (isTrafficFree)
                {
                    if (commandQueue.Count != 0) SendCommand();
                    else continue;
                }

                // 100バイト以下受信
                await Task.Run(() =>
                {
                    // TODO ここのエラーをどうするか考える
                    try
                    {
                        length = sp.Read(receiveData, 0, 100);
                    }
                    catch (Exception ex) { }
                });

                for (int pos = 0; pos < length; pos++)
                {

                    // 受信データを1Byteバッファに保存
                    receiveBuffer[receivePos++] = receiveData[pos];

                    // 受信完了チェック
                    if (isCompleteResponse(receiveBuffer.Take((int)receivePos).ToArray()))
                    {
                        // タイマ停止
                        timeoutTimer.Enabled = false;

                        // 全サーボ受信完了チェック
                        currentCommand.servoCount--;
                        if (currentCommand.servoCount == 0)
                        {
                            // コールバックを呼び出し
                            currentCommand.callback(receiveBuffer.Take((int)receivePos).ToArray());

                            // コマンドキューから削除
                            commandQueue.RemoveAt(0);

                            // キューに続きがある場合は送信開始
                            if (commandQueue.Count != 0) SendCommand();
                            else isTrafficFree = true;
                        }
                        else
                        {
                            // 受信バッファの初期化
                            Array.Clear(receiveBuffer, 0, 100);
                            receivePos = 0;

                            // タイムアウトタイマを初期化
                            StartTimeoutTimer();
                        }
                    }
                }
            }
        }
    }
}
