## �g�p���@
gs2d.dll���Q�Ƃɒǉ�

## �r���h��
�N���X ���C�u���� .NET Standard 2.0

## ���p��

### ID1�̃T�[�{���[�^�[�����E��90�x��������
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
### ID1�̌��݉��x�̓ǂݍ��݁i�����j
```
using gs2d;
```
```
Driver servo = new RobotisP20("COM3", 115200);
```
```
int temperature = servo.ReadTemperature(1);
```

### ID1�̌��݉��x�̓ǂݍ��݁i�񓯊��j
```
using gs2d;

private void temperatureCallback(byte id, ushort temperature)
{
    Console.WriteLine("Servo ID : " + id.ToString() + ", Temperature : " + temperature.ToString());
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
