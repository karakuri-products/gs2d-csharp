## �g�p���@
gs2d.dll���v���W�F�N�g�Ɋ܂߁A�Q�Ƃ���B�inuget�ɖ��Ή��j

## ���p��

### ID1�̃T�[�{���[�^�[�����E�ɓ�����
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
### ID1�̌��݉��x�̓ǂݍ��݁i�����j
```
using gs2d;
```
```
ServoBase servo = new RobotisP20("COM3",Baudrate.Baudrate1000000);
```
```
int temperature = servo.ReadTemperature(1);
```

### ID1�̌��݉��x�̓ǂݍ��݁i�񓯊��j
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
�R�[���o�b�N�֐��̈����Ɏg����\���̂ł��B
�S�ẴR�[���o�b�N�֐������̌^�œ��ꂳ��Ă��܂��B
```
public class CallbackEventArgs
{
        public CallbackResult Data;
        public byte Id;
        public byte Error;
        public uint Address;
}
```
* �����o�ϐ�
    * Data : �C�x���g�̌ŗL�f�[�^�B�����ɂ����Int�^��double�^�ǂ��炩�ŕԂ����B
    * Id : �C�x���g�������T�[�{��ID
    * Error : �C�x���g�������̃G���[�ԍ�
    * Address : �C�x���g�������T�[�{��ROM/RAM�A�h���X
#### Baudrate
Baudrate�ݒ莞�Ɏg�p�����Ή��{�[���[�g�񋓑̂ł��B
kr-SAC001�ł�3Mbps�܂őΉ����Ă��܂��B
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
�ʐM�̃G���[���e�̗񋓑̂ł��B
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
* CommSuccess : �ُ�Ȃ�
* ReadSuccess : �ǂݍ��ݐ���
* WriteSuccess : �������ݐ���
* CommTimeout : �ʐM�^�C���A�E�g
* CheckSumError : �`�F�b�N�T���s��v
* BufferIsEmpty : ���M�o�b�t�@����
* BufferIsFull : ���M�ҋ@�o�b�t�@�ɋ󂫖���
* FuncNotExist : �T�[�{���֐��̋@�\�ɑΉ����Ă��Ȃ�
* NoEEPROMData : EEPROM���ǂݍ��܂�Ă��Ȃ�����EEPROM�̏������݂��o���Ȃ��iKRS�V���[�Y��p�j
### Torque

#### ReadTorque
```
byte ReadTorque(byte id, bool async = false);
```
- ����
  - id : �T�[�{ID
  - async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM�����
- �߂�l
  - �g���N�l�B0 : Torque Off, 1 : Torque On�B�񓯊��̏ꍇ�͏��0���Ԃ����B

#### WriteTorque
```
byte WriteTorque(byte id, byte torque, bool async = false);
```
* ����
  * id : �T�[�{ID
  * torque : 0 : Torque Off, 1 : Torque On
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM�����
* �߂�l
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
- ����
  - id : �T�[�{ID
  - async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM�����
- �߂�l
  - �p�x�i�P�� : degree)�B�񓯊��̏ꍇ�͏��0���Ԃ����B

#### PositionCallbackEvent
```
event CallbackEventHandler PositionCallbackEvent
```
### Target Position
#### ReadTargetPosition
```
double ReadTargetPosition(byte id, bool async = false);
```
- ����
  - id : �T�[�{ID
  - async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM�����B
- �߂�l
  - �ڕW�ʒu�i�P�� : degree�j�B�񓯊��̏ꍇ�͏��0���Ԃ����B

#### WriteTargetPosition
```
byte WriteTargetPosition(byte id, double position, bool async = false);
```
* ����
  * id : �T�[�{ID
  * position : �ڕW�ʒu�i�P�� : degree�j
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���݂̉��x�i�P�� : degree�j�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### TemperatureCallbackEvent
```
event CallbackEventHandler TemperatureCallbackEvent;
```
### Current
#### ReadCurrent
```
ushort ReadCurrent(byte id, bool async = false);
```
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���דd���l�i�P�� : mA�j�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B

#### CurrentCallbackEvent
```
event CallbackEventHandler CurrentCallbackEvent;
```

### Voltage
#### ReadVoltage
```
ushort ReadVoltage(byte id, bool async = false);
```
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���͓d���i�P�� : mV�j�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### VoltageCallbackEvent
```
event CallbackEventHandler VoltageCallbackEvent;
```
### P Gain
#### ReadPGain
```
ushort ReadPGain(byte id, bool async = false);
```
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���݂�P�Q�C���l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WritePGain
```
byte WritePGain(byte id, ushort pGain, bool async = false);
```
* ����
  * id : �T�[�{ID
  * pGain : P�Q�C���l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���݂�I�Q�C���l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteIGain
```
byte WriteIGain(byte id, ushort iGain, bool async = false);
```
* ����
  * id : �T�[�{ID
  * iGain : I�Q�C���l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���݂�D�Q�C���l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteDGain
```
byte WriteDGain(byte id, ushort dGain, bool async = false);
```
* ����
  * id : �T�[�{ID
  * dGain : D�Q�C���l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �ΏۃT�[�{��ID�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteID
```
byte WriteID(byte id, byte newid, bool async = false);
```
* ����
  * id : �T�[�{ID
  * newID : �V�����T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �ΏۃT�[�{�̒ʐM�{�[���[�g�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteBaudrate
```
byte WriteBaudrate(byte id, Baudrate baudrate, bool async = false);
```
* ����
  * id : �T�[�{ID
  * baudrate : �V����Baudrate�l�BBaudrate�񋓑̂ɂ��Ă̓y�[�W�㕔���Q��
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �I�t�Z�b�g�l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteOffset
```
byte WriteOffset(byte id, double offset, bool async = false);
```
* ����
  * id : �T�[�{ID
  * offset : �I�t�Z�b�g�l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �f�b�h�o���h�l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteDeadband
```
byte WriteDeadband(byte id, double deadband, bool async = false);
```
* ����
  * id : �T�[�{ID
  * deadband : �f�b�h�o���h�l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���v�������̉�]�p�̐����B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteCWLimit
```
byte WriteCWLimit(byte id, double cwLimit, bool async = false);
```
* ����
  * id : �T�[�{ID
  * cwLimit : ���v�������̉�]�p�̐���
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �����v�������̉�]�p�̐����B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteCCWLimit
```
byte WriteCCWLimit(byte id, double ccwLimit, bool async = false);
```
* ����
  * id : �T�[�{ID
  * ccwLimit : �����v�������̉�]�p�̐���
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * ���x���~�b�g�l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteTemperatureLimit
```
byte WriteTemperatureLimit(byte id, ushort temperatureLimit, bool async = false);
```
* ����
  * id : �T�[�{ID
  * tempLimit : ���x���~�b�g�l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �d�����~�b�g�l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteCurrentLimit
```
byte WriteCurrentLimit(byte id, ushort currentLimit, bool async = false);
```
* ����
  * id : �T�[�{ID
  * currentLimit : �d�����~�b�g�l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �ڕW�X�s�[�h�l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteSpeed
```
byte WriteSpeed(byte id, ushort speed, bool async = false);
```
* ����
  * id : �T�[�{ID
  * speed : �ڕW�X�s�[�h�l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
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
* ����
  * id : �T�[�{ID
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �����x�l�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteAcceleration
```
byte WriteAcceleration(byte id, ushort accel, bool async = false);
```
* ����
  * id : �T�[�{ID
  * accel : �����x�l
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * FuncNotExist, CommSuccess, WriteSuccess, CommTimeout
#### AccelCallbackEvent
```
event CallbackEventHandler AccelCallbackEvent;
```
### Burst R/W Position
�����̃T�[�{��Position�𓯎��ɓǂݏ�������@�\�ł��B
Read�̏ꍇ��Current Position���AWrite�̏ꍇ��Target Position���Q�Ƃ���܂��B
#### BurstReadPosition
```
byte BurstReadPosition(IEnumerable<byte> idList, ushort num, double[] data, bool async = false);
```
* ����
  * idList : �T�[�{ID���i�[���ꂽ�z��
  * num : �ΏۃT�[�{��
  * data : �ʒu�f�[�^���i�[�����z��
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * 0 �܂��� FuncNotExist
#### BurstWritePosition
```
public byte BurstWritePosition(IEnumerable<byte> idList, ushort num, IEnumerable<double> data, bool async = false);
```
* ����
  * idList : �T�[�{ID���i�[���ꂽ�z��
  * num : �ΏۃT�[�{��
  * data : �ʒu�f�[�^���i�[���ꂽ�z��
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * 0 �܂��� FuncNotExist

### General Burst R/W
#### BurstReadMemory
```
public byte BurstReadMemory<T>(IEnumerable<byte> idList, ushort address, ushort length, ushort num, T[] data, bool async = false) where T : struct;
```
* ����
  * idList : �T�[�{ID���i�[���ꂽ�z��
  * address : �ΏۃA�h���X
  * length : �ǂݍ��݃o�C�g��
  * num : �ΏۃT�[�{��
  * data : �ʒu�f�[�^���i�[�����z��
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * 0 �܂��� FuncNotExist
#### BurstWriteMemory
```
public byte BurstWriteMemory<T>(IEnumerable<byte> idList, ushort address, ushort length, ushort num, IEnumerable<T> data, bool async = false) where T : struct;
```
* ����
  * idList : �T�[�{ID���i�[���ꂽ�z��
  * address : �ΏۃA�h���X
  * length : �ǂݍ��݃o�C�g��
  * num : �ΏۃT�[�{��
  * data : �ʒu�f�[�^���i�[���ꂽ�z��
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * 0 �܂��� FuncNotExist

### ROM
ROM�̕ۑ��@�\������T�[�{�̂ݑΉ��BRAM��ɓW�J���ꂽ�f�[�^��ROM�ɕۑ����܂��B
#### SaveRom
```
void SaveROM(byte id);
```
* ����
  * id : �T�[�{ID
* �߂�l
  * �Ȃ�

### General
�S�T�[�{���ʂ̓ǂݏ����֐��B�A�h���X�͊e�ЃT�[�{���[�^�̃}�j���A�����Q�ƁB
#### ReadMemory
```
uint ReadMemory(byte id, ushort address, ushort length, bool async = false);
```
* ����
  * id : �T�[�{ID
  * address : �ǂݍ��ݑΏۃA�h���X
  * length : �ǂݍ��݃o�C�g��
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * �ΏۃA�h���X�̃f�[�^�B�񓯊����[�h�̏ꍇ�͏��0���Ԃ����B
#### WriteMemory
```
byte WriteMemory(byte id, ushort address, ushort length, uint data, bool async = false);
```
* ����
  * id : �T�[�{ID
  * address : �������ݑΏۃA�h���X
  * length : �������݃o�C�g��
  * data : �������݃f�[�^
  * async : �񓯊��t���O�Btrue�̏ꍇ�񓯊��ʐM���[�h�ő��M�����B
* �߂�l
  * CommSuccess, WriteSuccess, CommTimeout

#### CallbackEvent
```
event CallbackEventHandler CallbackEvent;
```
ReadMemory�֐��œǂݍ��񂾃f�[�^�݂̂��̃R�[���o�b�N���g�p�����B
#### TimeoutCallbackEvent
```
event CallbackEventHandler TimeoutCallbackEvent;
```
�^�C���A�E�g����������Ɠ����񓯊��֌W�Ȃ��g�p�����B
#### WriteCallbackEvent
```
event CallbackEventHandler WriteCallbackEvent;
```
�񓯊���Write���s�����ꍇ�Ɏg�p�����B
## License
Generic Serial-bus Servo Driver library uses Apache License 2.0.
