## �g�p���@
�\�[�X���v���W�F�N�g�Ɋ܂߂�

TODO:
(gs2d.dll���v���W�F�N�g�Ɋ܂߁A�Q�Ƃł���悤�ɂ��� nuget�Ȃǁj

## �r���h��
�N���X ���C�u���� .NET Standard 2.0

## ���p��

### ID1�̃T�[�{���[�^�[�����E�ɓ�����
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
