using System;
using System.Globalization;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace DS3231
{
    public class Ds3231 : IDs3231
    {
        private I2cDevice _ds3231;

        /// <inheritdoc />
        public async void Initialization(int address, I2cBusSpeed busSpeed)
        {
            var connectSetting = new I2cConnectionSettings(address);
            connectSetting.BusSpeed = busSpeed;
            var controller = await I2cController.GetDefaultAsync();
            _ds3231 = controller.GetDevice(connectSetting);

            if (_ds3231 == null)
                throw new NullReferenceException("_ds3231");
        }

        /// <inheritdoc />
        public async void Initialization(string interfaceName, int address, I2cBusSpeed busSpeed)
        {
            var list = I2cDevice.GetDeviceSelector(interfaceName);
            var connectSetting = new I2cConnectionSettings(address);
            connectSetting.BusSpeed = busSpeed;
            var devInfo = await DeviceInformation.FindAllAsync(list);
            _ds3231 = await I2cDevice.FromIdAsync(devInfo.First().Id, connectSetting);

            if (_ds3231 == null)
                throw new NullReferenceException("_ds3231");
        }

        /// <inheritdoc />
        public string GetDeviceId()
        {
            return _ds3231.DeviceId;
        }

        /// <inheritdoc />
        public float GetTemperature()
        {
            byte[] getTemp = new byte[2];
            getTemp[0] = (byte)Registry.TempMsb;//задаем адрес регистра с которого будем читать

            _ds3231.Write(getTemp);//передаем команду на чтение с адресом
            _ds3231.Read(getTemp);//считываем данные из регистров

            var tempM = getTemp[0];
            var tempL = getTemp[1];

            float temp = (float)(tempM + ((tempL >> 6) * 0.25));
            return temp;
        }

        /// <inheritdoc />
        public DateTime GetDateTime()
        {
            byte[] txData = new byte[1];
            byte[] rxData = new byte[7];
            byte second;
            byte minute;
            byte hour;
            byte day;
            byte month;
            byte year;

            txData[0] = (byte)Registry.Seconds;
            _ds3231.Write(txData);
            _ds3231.Read(rxData);
            second = ConvertBcdToDec(rxData[0]);
            minute = ConvertBcdToDec(rxData[1]);
            hour = ConvertBcdToDec(rxData[2]);
            day = ConvertBcdToDec(rxData[4]);
            month = ConvertBcdToDec(rxData[5]);
            year = ConvertBcdToDec(rxData[6]);

            return DateTime.Parse($"{day}.{month}.{year} {hour}:{minute}:{second}",new CultureInfo("Ru-ru"));
        }

        /// <inheritdoc />
        public string GetWeekDay()
        {
            byte[] txData = new byte[1];
            byte[] rxData = new byte[1];
            string weekDay;

            txData[0] = (byte)Registry.WeakDay;

            _ds3231.WriteRead(txData, rxData);

            switch (rxData[0])
            {
                case 1:
                    weekDay = "Понедельник";
                    break;
                case 2:
                    weekDay = "Вторник";
                    break;
                case 3:
                    weekDay = "Среда";
                    break;
                case 4:
                    weekDay = "Четверг";
                    break;
                case 5:
                    weekDay = "Пятница";
                    break;
                case 6:
                    weekDay = "Суббота";
                    break;
                case 7:
                    weekDay = "Воскресенье";
                    break;
                default:
                    weekDay = "Ошибка.";
                    break;
            }

            return weekDay;
        }

        /// <inheritdoc />
        public I2cTransferResult SetDateTime()
        {
            byte[] setDate = new byte[8];

            setDate[0] = (byte)Registry.Seconds; //адрес регистра в который будем писать.
            setDate[1] = ConvertDecToBcd((byte)DateTime.Now.Second);
            setDate[2] = ConvertDecToBcd((byte)DateTime.Now.Minute);
            setDate[3] = ConvertDecToBcd((byte)DateTime.Now.Hour);
            setDate[4] = ConvertDecToBcd((byte)DateTime.Now.DayOfWeek);
            setDate[5] = ConvertDecToBcd((byte)DateTime.Now.Day);
            setDate[6] = ConvertDecToBcd((byte)DateTime.Now.Month);
            setDate[7] = ConvertDecToBcd((byte)(DateTime.Now.Year % 2000));

            return _ds3231.WritePartial(setDate);
        }

        /// <inheritdoc />
        public I2cTransferResult SetDateTime(string newDateTime)
        {
            DateTime dateTime = DateTime.Parse(newDateTime);
            byte[] setDate = new byte[8];

            setDate[0] = (byte)Registry.Seconds; //адрес регистра в который будем писать.
            setDate[1] = ConvertDecToBcd((byte)dateTime.Second);
            setDate[2] = ConvertDecToBcd((byte)dateTime.Minute);
            setDate[3] = ConvertDecToBcd((byte)dateTime.Hour);
            setDate[4] = ConvertDecToBcd((byte)dateTime.DayOfWeek);
            setDate[5] = ConvertDecToBcd((byte)dateTime.Day);
            setDate[6] = ConvertDecToBcd((byte)dateTime.Month);
            setDate[7] = ConvertDecToBcd((byte)(dateTime.Year % 2000));

            return _ds3231.WritePartial(setDate);
        }

        /// <inheritdoc />
        public I2cTransferResult SetDateTime(DateTime dateTime)
        {
            byte[] setDate = new byte[8];

            setDate[0] = 0x00; //адрес регистра в который будем писать.
            setDate[1] = ConvertDecToBcd((byte)dateTime.Second);
            setDate[2] = ConvertDecToBcd((byte)dateTime.Minute);
            setDate[3] = ConvertDecToBcd((byte)dateTime.Hour);
            setDate[4] = ConvertDecToBcd((byte)dateTime.DayOfWeek);
            setDate[5] = ConvertDecToBcd((byte)dateTime.Day);
            setDate[6] = ConvertDecToBcd((byte)dateTime.Month);
            setDate[7] = ConvertDecToBcd((byte)(dateTime.Year % 2000));

            return _ds3231.WritePartial(setDate);
        }

        /// <inheritdoc />
        public void Close()
        {
            _ds3231?.Dispose();
        }

        //конвертирует двоично-десятичное число в десятичное
        private byte ConvertBcdToDec(byte data)
        {
            var dec = (byte)((data >> 4) * 10 + (0x0F & data));
            return dec;
        }

        //конвертирует десятичное число в двоично-десятичный формат
        private byte ConvertDecToBcd(byte data)
        {
            var bcd = (byte)((data / 10 << 4) | (data % 10));
            return bcd;
        }
    }
}
