using System;
using System.Device.I2c;
using System.Globalization;
using System.Threading.Tasks;

namespace DS3231
{
    public class Ds3231 : IDs3231
    {
        private const byte Alarm1InterruptEnable = 0x01;
        private const byte Alarm2InterruptEnable = 0x02;
        private const byte InterruptControl = 0x04;
        private const byte Alarm1Flag = 0x01;
        private const byte Alarm2Flag = 0x02;
        private const byte AlarmDayOfWeekMode = 0x40;
        private const byte AlarmMask = 0x80;

        private I2cDevice _ds3231;

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
            var temperature = ReadRegisters((byte)Registry.TempMsb, 2);
            var wholeDegrees = (sbyte)temperature[0];
            var fractionalDegrees = (temperature[1] >> 6) * 0.25f;

            return wholeDegrees + fractionalDegrees;
        }

        /// <inheritdoc />
        public DateTime GetDateTime()
        {
            var registers = ReadRegisters((byte)Registry.Seconds, 7);
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

        /// <inheritdoc />
        public string GetWeekDay()
        {
            var dayOfWeek = ReadRegisters((byte)Registry.WeakDay, 1)[0] & 0x07;

            switch (dayOfWeek)
            {
                case 1: return "Понедельник";
                case 2: return "Вторник";
                case 3: return "Среда";
                case 4: return "Четверг";
                case 5: return "Пятница";
                case 6: return "Суббота";
                case 7: return "Воскресенье";
                default: return "Ошибка.";
            }
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
        public void SetAlarm1Daily(TimeSpan time)
        {
            ValidateAlarmTime(time);
            SetAlarm1(
                ConvertDecToBcd((byte)time.Seconds),
                ConvertDecToBcd((byte)time.Minutes),
                ConvertDecToBcd((byte)time.Hours),
                AlarmMask);
        }

        /// <inheritdoc />
        public void SetAlarm1Weekly(DayOfWeek dayOfWeek, TimeSpan time)
        {
            ValidateAlarmTime(time);
            var day = GetDayOfWeekRegisterValue(dayOfWeek);
            SetAlarm1(
                ConvertDecToBcd((byte)time.Seconds),
                ConvertDecToBcd((byte)time.Minutes),
                ConvertDecToBcd((byte)time.Hours),
                (byte)(AlarmDayOfWeekMode | day));
        }

        /// <inheritdoc />
        public void SetAlarm1DayOfMonth(int day, TimeSpan time)
        {
            ValidateAlarmTime(time);
            ValidateDayOfMonth(day);
            SetAlarm1(
                ConvertDecToBcd((byte)time.Seconds),
                ConvertDecToBcd((byte)time.Minutes),
                ConvertDecToBcd((byte)time.Hours),
                ConvertDecToBcd((byte)day));
        }

        /// <inheritdoc />
        public void SetAlarm2Daily(TimeSpan time)
        {
            ValidateAlarm2Time(time);
            SetAlarm2(
                ConvertDecToBcd((byte)time.Minutes),
                ConvertDecToBcd((byte)time.Hours),
                AlarmMask);
        }

        /// <inheritdoc />
        public void SetAlarm2Weekly(DayOfWeek dayOfWeek, TimeSpan time)
        {
            ValidateAlarm2Time(time);
            var day = GetDayOfWeekRegisterValue(dayOfWeek);
            SetAlarm2(
                ConvertDecToBcd((byte)time.Minutes),
                ConvertDecToBcd((byte)time.Hours),
                (byte)(AlarmDayOfWeekMode | day));
        }

        /// <inheritdoc />
        public void SetAlarm2DayOfMonth(int day, TimeSpan time)
        {
            ValidateAlarm2Time(time);
            ValidateDayOfMonth(day);
            SetAlarm2(
                ConvertDecToBcd((byte)time.Minutes),
                ConvertDecToBcd((byte)time.Hours),
                ConvertDecToBcd((byte)day));
        }

        /// <inheritdoc />
        public bool IsAlarm1Triggered()
        {
            return (ReadRegisters((byte)Registry.ControlStatus, 1)[0] & Alarm1Flag) != 0;
        }

        /// <inheritdoc />
        public bool IsAlarm2Triggered()
        {
            return (ReadRegisters((byte)Registry.ControlStatus, 1)[0] & Alarm2Flag) != 0;
        }

        /// <inheritdoc />
        public void ClearAlarm1Flag()
        {
            ClearAlarmFlag(Alarm1Flag);
        }

        /// <inheritdoc />
        public void ClearAlarm2Flag()
        {
            ClearAlarmFlag(Alarm2Flag);
        }

        /// <inheritdoc />
        public void DisableAlarm1()
        {
            UpdateControlRegister(Alarm1InterruptEnable, false);
            ClearAlarm1Flag();
        }

        /// <inheritdoc />
        public void DisableAlarm2()
        {
            UpdateControlRegister(Alarm2InterruptEnable, false);
            ClearAlarm2Flag();
        }

        /// <inheritdoc />
        public void Close()
        {
            _ds3231?.Dispose();
            _ds3231 = null;
        }

        private void SetAlarm1(byte second, byte minute, byte hour, byte dayDate)
        {
            WriteRegisters((byte)Registry.Alarm1Seconds, second, minute, hour, dayDate);
            ClearAlarm1Flag();
            UpdateControlRegister(Alarm1InterruptEnable, true);
        }

        private void SetAlarm2(byte minute, byte hour, byte dayDate)
        {
            WriteRegisters((byte)Registry.Alarm2Minutes, minute, hour, dayDate);
            ClearAlarm2Flag();
            UpdateControlRegister(Alarm2InterruptEnable, true);
        }

        private void ClearAlarmFlag(byte flag)
        {
            var status = ReadRegisters((byte)Registry.ControlStatus, 1)[0];
            WriteRegisters((byte)Registry.ControlStatus, (byte)(status & ~flag));
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

        private static void ValidateAlarmTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1) || time.Ticks % TimeSpan.TicksPerSecond != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Время будильника должно быть от 00:00:00 до 23:59:59 с точностью до секунды.");
            }
        }

        private static void ValidateAlarm2Time(TimeSpan time)
        {
            ValidateAlarmTime(time);
            if (time.Seconds != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(time), "Будильник 2 настраивается с точностью до минуты.");
            }
        }

        private static void ValidateDayOfMonth(int day)
        {
            if (day < 1 || day > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(day), "День месяца должен быть от 1 до 31.");
            }
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
