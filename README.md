## 使用方法
ソースをプロジェクトに含める

TODO:
(gs2d.dllをプロジェクトに含め、参照できるようにする nugetなど）

## ビルド環境
クラス ライブラリ .NET Standard 2.0

## 利用例

### ID1のサーボモーターを左右に動かす
```
using gs2d;
```
```
Driver servo = new RobotisP20("COM3", 115200);
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
Driver servo = new RobotisP20("COM3", 115200);
```
```
int temperature = servo.ReadTemperature(1);
```

### ID1の現在温度の読み込み（非同期）
```
using gs2d;

private void temperatureCallback(ushort temperature)
{
    Console.WriteLine(temperature.ToString());
}
```
```
Driver servo = new RobotisP20("COM3", 115200);
```
```
servo.ReadTemperature(1, temperatureCallback);
```

## API
### Servo Class
* RobotisP20
* KRS
* B3M
* Futaba

## License
Generic Serial-bus Servo Driver library uses Apache License 2.0.
