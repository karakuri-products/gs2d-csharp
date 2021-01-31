## 使用方法
gs2d.dllをプロジェクトに含め、参照する。（nugetに未対応）

## 利用例

### ID1のサーボモーターを左右に動かす
```
using gs2d;
```
```
ServoBase servo = new RobotisP20("COM3",Baudrate.Baudrate1000000);
```
```
servo.WriteTargetPosition(1, 90.0);
servo.WriteTargetPosition(1, -90.0);
```
### ID1の現在温度の読み込み（同期）
```
using gs2d;
```
```
ServoBase servo = new RobotisP20("COM3",Baudrate.Baudrate1000000);
```
```
int temperature = servo.ReadTemperature(1);
```

### ID1の現在温度の読み込み（非同期）
```
using gs2d;

private void temperatureEventHandler(CallbackEventArgs e)
{
    Invoke((MethodInvoker)delegate
    {
        int temperature = e.Data;
    });
}
```
```
ServoBase servo = new RobotisP20("COM3",Baudrate.Baudrate1000000);
servo.TemperatureCallbackEvent += temperatureEventHandler;
```
```
servo.ReadTemperature(1, true);
```

## API
### Servo Class
* RobotisP20
* KondoKRS
* KondoB3M
* Futaba
### Type
#### CallbackEventArgs
コールバック関数の引数に使われる構造体です。
全てのコールバック関数がこの型で統一されています。
```
public class CallbackEventArgs
{
        public CallbackResult Data;
        public byte Id;
        public byte Error;
        public uint Address;
}
```
* メンバ変数
    * Data : イベントの固有データ。代入先によってInt型かdouble型どちらかで返される。
    * Id : イベント発生元サーボのID
    * Error : イベント発生時のエラー番号
    * Address : イベント発生元サーボのROM/RAMアドレス
#### Baudrate
Baudrate設定時に使用される対応ボーレート列挙体です。
kr-SAC001では3Mbpsまで対応しています。
```
    public enum Baudrate
    {
        Baudrate9600 = 9600,
        Baudrate19200 = 19200,
        Baudrate57600 = 57600,
        Baudrate115200 = 115200,
        Baudrate230400 = 230400,
        Baudrate625000 = 625000,
        Baudrate1000000 = 1000000,
        Baudrate1250000 = 1250000,
        Baudrate1500000 = 1500000,
        Baudrate2000000 = 2000000,
        Baudrate3000000 = 3000000,

        // Not Supported on SAC
        Baudrate4000000 = 4000000,
        Baudrate4500000 = 4500000
    }
```
#### CommResult
通信のエラー内容の列挙体です。
```
    public enum CommResult
    {
        CommSuccess = 0,
        ReadSuccess = 1,
        WriteSuccess = 2,
        CommTimeout = 3,
        CheckSumError = 4,
        BufferIsEmpty = 5,
        BufferIsFull = 6,
        FuncNotExist = 7,
        NoEEPROMData = 8
    }
```
* CommSuccess : 異常なし
* ReadSuccess : 読み込み成功
* WriteSuccess : 書き込み成功
* CommTimeout : 通信タイムアウト
* CheckSumError : チェックサム不一致
* BufferIsEmpty : 送信バッファが空
* BufferIsFull : 送信待機バッファに空き無し
* FuncNotExist : サーボが関数の機能に対応していない
* NoEEPROMData : EEPROMが読み込まれていないためEEPROMの書き込みが出来ない（KRSシリーズ専用）
### Torque

#### ReadTorque
```
byte ReadTorque(byte id, bool async = false);
```
- 引数
  - id : サーボID
  - async : 非同期フラグ。trueの場合非同期通信される
- 戻り値
  - トルク値。0 : Torque Off, 1 : Torque On。非同期の場合は常に0が返される。

#### WriteTorque
```
byte WriteTorque(byte id, byte torque, bool async = false);
```
* 引数
  * id : サーボID
  * torque : 0 : Torque Off, 1 : Torque On
  * async : 非同期フラグ。trueの場合非同期通信される
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### TorqueCallbackEvent
```
event CallbackEventHandler TorqueCallbackEvent;
```

### Current Position
#### ReadPosition
```
double ReadPosition(byte id, bool async = false);
```
- 引数
  - id : サーボID
  - async : 非同期フラグ。trueの場合非同期通信される
- 戻り値
  - 角度（単位 : degree)。非同期の場合は常に0が返される。

#### PositionCallbackEvent
```
event CallbackEventHandler PositionCallbackEvent
```
### Target Position
#### ReadTargetPosition
```
double ReadTargetPosition(byte id, bool async = false);
```
- 引数
  - id : サーボID
  - async : 非同期フラグ。trueの場合非同期通信される。
- 戻り値
  - 目標位置（単位 : degree）。非同期の場合は常に0が返される。

#### WriteTargetPosition
```
byte WriteTargetPosition(byte id, double position, bool async = false);
```
* 引数
  * id : サーボID
  * position : 目標位置（単位 : degree）
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### TargetPositionCallbackEvent
```
event CallbackEventHandler TargetPositionCallbackEvent;
```
### Temperature
#### ReadTemperature
```
ushort ReadTemperature(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 現在の温度（単位 : degree）。非同期モードの場合は常に0が返される。
#### TemperatureCallbackEvent
```
event CallbackEventHandler TemperatureCallbackEvent;
```
### Current
#### ReadCurrent
```
ushort ReadCurrent(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 負荷電流値（単位 : mA）。非同期モードの場合は常に0が返される。

#### CurrentCallbackEvent
```
event CallbackEventHandler CurrentCallbackEvent;
```

### Voltage
#### ReadVoltage
```
ushort ReadVoltage(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 入力電圧（単位 : mV）。非同期モードの場合は常に0が返される。
#### VoltageCallbackEvent
```
event CallbackEventHandler VoltageCallbackEvent;
```
### P Gain
#### ReadPGain
```
ushort ReadPGain(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 現在のPゲイン値。非同期モードの場合は常に0が返される。
#### WritePGain
```
byte WritePGain(byte id, ushort pGain, bool async = false);
```
* 引数
  * id : サーボID
  * pGain : Pゲイン値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout

#### PGainCallbackEvent
```
event CallbackEventHandler PGainCallbackEvent;
```
### I Gain
#### ReadIGain
```
ushort ReadIGain(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 現在のIゲイン値。非同期モードの場合は常に0が返される。
#### WriteIGain
```
byte WriteIGain(byte id, ushort iGain, bool async = false);
```
* 引数
  * id : サーボID
  * iGain : Iゲイン値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### IGainCallbackEvent;
```
event CallbackEventHandler IGainCallbackEvent;
```
### D Gain
#### ReadDGain
```
ushort ReadDGain(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 現在のDゲイン値。非同期モードの場合は常に0が返される。
#### WriteDGain
```
byte WriteDGain(byte id, ushort dGain, bool async = false);
```
* 引数
  * id : サーボID
  * dGain : Dゲイン値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### DGainCallbackEvent
```
event CallbackEventHandler DGainCallbackEvent;
```
### ID
#### ReadID
```
byte ReadID(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 対象サーボのID。非同期モードの場合は常に0が返される。
#### WriteID
```
byte WriteID(byte id, byte newid, bool async = false);
```
* 引数
  * id : サーボID
  * newID : 新しいサーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### IDCallbackEvent
```
event CallbackEventHandler IDCallbackEvent
```
### Baudrate
#### ReadBaudrate
```
Baudrate ReadBaudrate(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 対象サーボの通信ボーレート。非同期モードの場合は常に0が返される。
#### WriteBaudrate
```
byte WriteBaudrate(byte id, Baudrate baudrate, bool async = false);
```
* 引数
  * id : サーボID
  * baudrate : 新しいBaudrate値。Baudrate列挙体についてはページ上部を参照
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### BaudrateCallbackEvent
```
event CallbackEventHandler BaudrateCallbackEvent;
```
### Offset
#### ReadOffset
```
double ReadOffset(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * オフセット値。非同期モードの場合は常に0が返される。
#### WriteOffset
```
byte WriteOffset(byte id, double offset, bool async = false);
```
* 引数
  * id : サーボID
  * offset : オフセット値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### OffsetCallbackEvent
```
event CallbackEventHandler OffsetCallbackEvent;
```
### Deadband
#### ReadDeadband
```
double ReadDeadband(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * デッドバンド値。非同期モードの場合は常に0が返される。
#### WriteDeadband
```
byte WriteDeadband(byte id, double deadband, bool async = false);
```
* 引数
  * id : サーボID
  * deadband : デッドバンド値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### DeadbandCallbackEvent
```
event CallbackEventHandler DeadbandCallbackEvent
```
### CW Position Limit 
#### ReadCWLimit
```
double ReadCWLimit(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 時計回り方向の回転角の制限。非同期モードの場合は常に0が返される。
#### WriteCWLimit
```
byte WriteCWLimit(byte id, double cwLimit, bool async = false);
```
* 引数
  * id : サーボID
  * cwLimit : 時計回り方向の回転角の制限
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### CWLimitCallbackEvent
```
event CallbackEventHandler CWLimitCallbackEvent;
```
### CCW Position Limit 
#### ReadCCWLimit
```
double ReadCCWLimit(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 反時計回り方向の回転角の制限。非同期モードの場合は常に0が返される。
#### WriteCCWLimit
```
byte WriteCCWLimit(byte id, double ccwLimit, bool async = false);
```
* 引数
  * id : サーボID
  * ccwLimit : 反時計回り方向の回転角の制限
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### CCWLimitCallbackEvent
```
event CallbackEventHandler CCWLimitCallbackEvent;
```
### Temperature Limit
#### ReadTemperatureLimit
```
ushort ReadTemperatureLimit(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 温度リミット値。非同期モードの場合は常に0が返される。
#### WriteTemperatureLimit
```
byte WriteTemperatureLimit(byte id, ushort temperatureLimit, bool async = false);
```
* 引数
  * id : サーボID
  * tempLimit : 温度リミット値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### TemperatureLimitCallbackEvent
```
event CallbackEventHandler TemperatureLimitCallbackEvent;
```
### Current Limit
#### ReadCurrentLimit
```
ushort ReadCurrentLimit(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 電流リミット値。非同期モードの場合は常に0が返される。
#### WriteCurrentLimit
```
byte WriteCurrentLimit(byte id, ushort currentLimit, bool async = false);
```
* 引数
  * id : サーボID
  * currentLimit : 電流リミット値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### CurrentLimitCallbackEvent
```
event CallbackEventHandler CurrentLimitCallbackEvent;
```
### Speed
#### ReadSpeed
```
ushort ReadSpeed(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 目標スピード値。非同期モードの場合は常に0が返される。
#### WriteSpeed
```
byte WriteSpeed(byte id, ushort speed, bool async = false);
```
* 引数
  * id : サーボID
  * speed : 目標スピード値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### SpeedCallbackEvent
```
event CallbackEventHandler SpeedCallbackEvent;
```
### Acceleration
#### ReadAcceleration
```
ushort ReadAcceleration(byte id, bool async = false);
```
* 引数
  * id : サーボID
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 加速度値。非同期モードの場合は常に0が返される。
#### WriteAcceleration
```
byte WriteAcceleration(byte id, ushort accel, bool async = false);
```
* 引数
  * id : サーボID
  * accel : 加速度値
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### AccelCallbackEvent
```
event CallbackEventHandler AccelCallbackEvent;
```
### Burst R/W Position
複数のサーボのPositionを同時に読み書きする機能です。
Readの場合はCurrent Positionが、Writeの場合はTarget Positionが参照されます。
#### BurstReadPosition
```
byte BurstReadPosition(IEnumerable<byte> idList, ushort num, double[] data, bool async = false);
```
* 引数
  * idList : サーボIDが格納された配列
  * num : 対象サーボ数
  * data : 位置データが格納される配列
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 0 または FuncNotExist
#### BurstWritePosition
```
public byte BurstWritePosition(IEnumerable<byte> idList, ushort num, IEnumerable<double> data, bool async = false);
```
* 引数
  * idList : サーボIDが格納された配列
  * num : 対象サーボ数
  * data : 位置データが格納された配列
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 0 または FuncNotExist

### General Burst R/W
#### BurstReadMemory
```
public byte BurstReadMemory<T>(IEnumerable<byte> idList, ushort address, ushort length, ushort num, T[] data, bool async = false) where T : struct;
```
* 引数
  * idList : サーボIDが格納された配列
  * address : 対象アドレス
  * length : 読み込みバイト数
  * num : 対象サーボ数
  * data : 位置データが格納される配列
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 0 または FuncNotExist
#### BurstWriteMemory
```
public byte BurstWriteMemory<T>(IEnumerable<byte> idList, ushort address, ushort length, ushort num, IEnumerable<T> data, bool async = false) where T : struct;
```
* 引数
  * idList : サーボIDが格納された配列
  * address : 対象アドレス
  * length : 読み込みバイト数
  * num : 対象サーボ数
  * data : 位置データが格納された配列
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 0 または FuncNotExist

### ROM
ROMの保存機能があるサーボのみ対応。RAM上に展開されたデータをROMに保存します。
#### SaveRom
```
void SaveROM(byte id);
```
* 引数
  * id : サーボID
* 戻り値
  * なし

### General
全サーボ共通の読み書き関数。アドレスは各社サーボモータのマニュアルを参照。
#### ReadMemory
```
uint ReadMemory(byte id, ushort address, ushort length, bool async = false);
```
* 引数
  * id : サーボID
  * address : 読み込み対象アドレス
  * length : 読み込みバイト数
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * 対象アドレスのデータ。非同期モードの場合は常に0が返される。
#### WriteMemory
```
byte WriteMemory(byte id, ushort address, ushort length, uint data, bool async = false);
```
* 引数
  * id : サーボID
  * address : 書き込み対象アドレス
  * length : 書き込みバイト数
  * data : 書き込みデータ
  * async : 非同期フラグ。trueの場合非同期通信モードで送信される。
* 戻り値
  * CommSuccess, WriteSuccess, CommTimeout

#### CallbackEvent
```
event CallbackEventHandler CallbackEvent;
```
ReadMemory関数で読み込んだデータのみこのコールバックが使用される。
#### TimeoutCallbackEvent
```
event CallbackEventHandler TimeoutCallbackEvent;
```
タイムアウトが発生すると同期非同期関係なく使用される。
#### WriteCallbackEvent
```
event CallbackEventHandler WriteCallbackEvent;
```
非同期でWriteを行った場合に使用される。
## License
Generic Serial-bus Servo Driver library uses Apache License 2.0.
