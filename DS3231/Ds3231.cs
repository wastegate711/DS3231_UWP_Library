using System;
using System.Device.I2c;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace DS3231
{
    public partial class Ds3231 : IDs3231
    {
        private const byte Alarm1InterruptEnable = 0x01;
        private const byte Alarm2InterruptEnable = 0x02;
        private const byte InterruptControl = 0x04;
        private const byte Alarm1Flag = 0x01;
        private const byte Alarm2Flag = 0x02;
        private const byte AlarmDayOfWeekMode = 0x40;
        private const byte AlarmMask = 0x80;
        private const byte OscillatorStoppedFlag = 0x80;
        private const byte Enable32kHzOutputFlag = 0x08;
        private const byte BusyFlag = 0x04;
        private const byte SquareWaveFrequencyMask = 0x18;

        private I2cDevice _ds3231;
        private readonly AlarmController _alarmController;
        private readonly OutputController _outputController;

        public Ds3231()
        {
            _alarmController = new AlarmController(this);
            _outputController = new OutputController(this);
        }

        /// <inheritdoc />
        public void Initialization(int busId, int address)
        {
            _ds3231?.Dispose();
            _ds3231 = I2cDevice.Create(new I2cConnectionSettings(busId, address));
        }

        /// <inheritdoc />
        public Task InitializeAsync(int busId, int address)
        {
            return Task.Run(() => Initialization(busId, address));
        }

        /// <inheritdoc />
        public string GetDeviceId()
        {
            var settings = GetDevice().ConnectionSettings;
            return $"I2C bus {settings.BusId}, address 0x{settings.DeviceAddress:X2}";
        }

        /// <inheritdoc />
        public float GetTemperature()
        {
            return ConvertTemperature(ReadRegisters((byte)Registry.TempMsb, 2));
        }

        /// <inheritdoc />
        public DateTime GetDateTime()
        {
            return ConvertDateTime(ReadRegisters((byte)Registry.Seconds, 7));
        }

        /// <inheritdoc />
        public string GetWeekDay()
        {
            var dayOfWeek = ReadRegisters((byte)Registry.WeakDay, 1)[0];
            return TryConvertWeekDay(dayOfWeek, out var weekDay) ? weekDay : "Ошибка.";
        }

        /// <inheritdoc />
        public Ds3231Status GetStatus()
        {
            var status = ReadRegisters((byte)Registry.ControlStatus, 1)[0];
            var control = ReadRegisters((byte)Registry.Control, 1)[0];
            return _outputController.CreateStatus(status, control);
        }

        /// <inheritdoc />
        public Ds3231Reading GetReading()
        {
            var dateTimeRegisters = ReadRegisters((byte)Registry.Seconds, 7);
            var temperatureRegisters = ReadRegisters((byte)Registry.TempMsb, 2);
            var status = ReadRegisters((byte)Registry.ControlStatus, 1)[0];
            var control = ReadRegisters((byte)Registry.Control, 1)[0];

            if (!TryConvertWeekDay(dateTimeRegisters[3], out var weekDay))
            {
                throw new InvalidDataException("Регистр дня недели DS3231 содержит недопустимое значение.");
            }

            return new Ds3231Reading(
                ConvertDateTime(dateTimeRegisters),
                weekDay,
                ConvertTemperature(temperatureRegisters),
                _outputController.CreateStatus(status, control));
        }

        /// <inheritdoc />
        public void SetDateTime()
        {
            SetDateTime(DateTime.Now);
        }

        /// <inheritdoc />
        public void SetDateTime(string newDateTime)
        {
            if (newDateTime == null)
            {
                throw new ArgumentNullException(nameof(newDateTime));
            }

            var dateTime = DateTime.ParseExact(
                newDateTime,
                "dd.MM.yyyy HH:mm:ss",
                CultureInfo.InvariantCulture);
            SetDateTime(dateTime);
        }

        /// <inheritdoc />
        public void SetDateTime(DateTime dateTime)
        {
            if (dateTime.Year < 2000 || dateTime.Year > 2199)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), "DS3231 поддерживает годы с 2000 по 2199.");
            }

            var month = ConvertDecToBcd((byte)dateTime.Month);
            if (dateTime.Year >= 2100)
            {
                month |= 0x80;
            }

            var registers = new byte[]
            {
                (byte)Registry.Seconds,
                ConvertDecToBcd((byte)dateTime.Second),
                ConvertDecToBcd((byte)dateTime.Minute),
                ConvertDecToBcd((byte)dateTime.Hour),
                ConvertDecToBcd((byte)GetDayOfWeekRegisterValue(dateTime.DayOfWeek)),
                ConvertDecToBcd((byte)dateTime.Day),
                month,
                ConvertDecToBcd((byte)(dateTime.Year % 100))
            };

            GetDevice().Write(registers);
        }

        /// <inheritdoc />
        public void Close()
        {
            _ds3231?.Dispose();
            _ds3231 = null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Close();
        }

        private static DateTime ConvertDateTime(byte[] registers)
        {
            var second = ConvertBcdToDec((byte)(registers[0] & 0x7F));
            var minute = ConvertBcdToDec((byte)(registers[1] & 0x7F));
            var hour = ConvertHour(registers[2]);
            var day = ConvertBcdToDec((byte)(registers[4] & 0x3F));
            var monthRegister = registers[5];
            var month = ConvertBcdToDec((byte)(monthRegister & 0x1F));
            var year = 2000 + ConvertBcdToDec(registers[6]);

            if ((monthRegister & 0x80) != 0)
            {
                year += 100;
            }

            return new DateTime(year, month, day, hour, minute, second);
        }

        private static bool TryConvertWeekDay(byte data, out string weekDay)
        {
            switch (data & 0x07)
            {
                case 1: weekDay = "Понедельник"; return true;
                case 2: weekDay = "Вторник"; return true;
                case 3: weekDay = "Среда"; return true;
                case 4: weekDay = "Четверг"; return true;
                case 5: weekDay = "Пятница"; return true;
                case 6: weekDay = "Суббота"; return true;
                case 7: weekDay = "Воскресенье"; return true;
                default: weekDay = null; return false;
            }
        }

        private static float ConvertTemperature(byte[] temperature)
        {
            var wholeDegrees = (sbyte)temperature[0];
            var fractionalDegrees = (temperature[1] >> 6) * 0.25f;
            return wholeDegrees + fractionalDegrees;
        }

        private I2cDevice GetDevice()
        {
            return _ds3231 ?? throw new InvalidOperationException("Устройство DS3231 не инициализировано.");
        }

        private byte[] ReadRegisters(byte register, int count)
        {
            var address = new[] { register };
            var data = new byte[count];
            GetDevice().WriteRead(address, data);
            return data;
        }

        private void WriteRegisters(byte register, params byte[] values)
        {
            var data = new byte[values.Length + 1];
            data[0] = register;
            Array.Copy(values, 0, data, 1, values.Length);
            GetDevice().Write(data);
        }

        private void UpdateControlRegister(byte interruptEnable, bool enabled)
        {
            var control = ReadRegisters((byte)Registry.Control, 1)[0];
            control |= InterruptControl;
            if (enabled)
            {
                control |= interruptEnable;
            }
            else
            {
                control &= (byte)~interruptEnable;
            }

            WriteRegisters((byte)Registry.Control, control);
        }

        private static int ConvertHour(byte data)
        {
            if ((data & 0x40) == 0)
            {
                return ConvertBcdToDec((byte)(data & 0x3F));
            }

            var hour = ConvertBcdToDec((byte)(data & 0x1F)) % 12;
            return (data & 0x20) != 0 ? hour + 12 : hour;
        }

        private static int GetDayOfWeekRegisterValue(DayOfWeek dayOfWeek)
        {
            if (dayOfWeek < DayOfWeek.Sunday || dayOfWeek > DayOfWeek.Saturday)
            {
                throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
            }

            return dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;
        }

        private static byte ConvertBcdToDec(byte data)
        {
            return (byte)((data >> 4) * 10 + (data & 0x0F));
        }

        private static byte ConvertDecToBcd(byte data)
        {
            return (byte)((data / 10 << 4) | (data % 10));
        }
    }
}
