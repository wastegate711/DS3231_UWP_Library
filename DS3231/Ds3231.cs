using System;
using System.Device.I2c;
using System.Globalization;
using System.Threading.Tasks;

namespace DS3231
{
    public class Ds3231 : IDs3231
    {
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
        public void Close()
        {
            _ds3231?.Dispose();
            _ds3231 = null;
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
