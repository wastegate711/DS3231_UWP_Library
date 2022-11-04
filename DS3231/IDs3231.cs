using System;
using Windows.Devices.I2c;

namespace DS3231
{
    public interface IDs3231
    {
        /// <summary>
        /// Получает I2C контроллер по умолчанию (в Raspberry pi 2 и 3 это I2С1)
        /// </summary>
        /// <param name="address">Адрес устройства.</param>
        /// <param name="busSpeed">Скорость.</param>
        void Initialization(int address, I2cBusSpeed busSpeed);

        /// <summary>
        /// Получает I2C контроллер.
        /// </summary>
        /// <param name="interfaceName">Имя контроллера (пример "i2c1" или "i2c2")</param>
        /// <param name="address">Адрес устройства.</param>
        /// <param name="busSpeed">Скорость.</param>
        void Initialization(string interfaceName, int address, I2cBusSpeed busSpeed);

        /// <summary>
        /// Считывает данные датчика температуры.
        /// </summary>
        /// <returns></returns>
        float GetTemperature();

        /// <summary>
        /// Закрывает соединение и закрывает дескриптор.
        /// </summary>
        void Close();

        /// <summary>
        /// Получает идентификатор устройства.
        /// </summary>
        /// <returns>Вернет строку содержащую имя контроллера и его guid.</returns>
        string GetDeviceId();

        /// <summary>
        /// Устанавливает текущую на компьютере дату и время в микросхеме RTC.
        /// </summary>
        /// <returns>Вернет результат выполненой операции.</returns>
        I2cTransferResult SetDateTime();

        /// <summary>
        /// Устанавливает дату и время в микросхеме RTC.
        /// </summary>
        /// <param name="newDateTime">Дата и время в формате dd.mm.yyyy hh:mm:ss</param>
        /// <returns>Вернет результат выполненой операции.</returns>
        I2cTransferResult SetDateTime(string newDateTime);

        /// <summary>
        /// Устанавливает дату и время в микросхеме RTC.
        /// </summary>
        /// <returns>Вернет результат выполненой операции.</returns>
        I2cTransferResult SetDateTime(DateTime dateTime);

        /// <summary>
        /// Считывает из микросхемы RTC показания.
        /// </summary>
        /// <returns>Вернет дату и время в формате DateTime.</returns>
        DateTime GetDateTime();

        /// <summary>
        /// Считывает день недели.
        /// </summary>
        /// <returns>Вернет день недели в формате (Вторник, Среда)</returns>
        string GetWeekDay();
    }
}