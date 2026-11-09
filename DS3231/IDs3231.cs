using System;
using System.Threading.Tasks;

namespace DS3231
{
    public interface IDs3231
    {
        /// <summary>
        /// Открывает DS3231 на указанной шине I2C по заданному адресу устройства.
        /// </summary>
        /// <param name="busId">Идентификатор шины I2C, зависящий от платформы.</param>
        /// <param name="address">7-битный адрес устройства I2C.</param>
        void Initialization(int busId, int address);

        /// <summary>
        /// Открывает DS3231 на указанной шине I2C по заданному адресу устройства.
        /// </summary>
        /// <param name="busId">Идентификатор шины I2C, зависящий от платформы.</param>
        /// <param name="address">7-битный адрес устройства I2C.</param>
        Task InitializeAsync(int busId, int address);

        /// <summary>
        /// Считывает температуру DS3231 в градусах Цельсия.
        /// </summary>
        float GetTemperature();

        /// <summary>
        /// Освобождает ресурсы устройства I2C.
        /// </summary>
        void Close();

        /// <summary>
        /// Возвращает номер шины I2C и адрес открытого устройства.
        /// </summary>
        string GetDeviceId();

        /// <summary>
        /// Устанавливает в часах системные местные дату и время.
        /// </summary>
        void SetDateTime();

        /// <summary>
        /// Устанавливает дату и время в формате dd.MM.yyyy HH:mm:ss.
        /// </summary>
        void SetDateTime(string newDateTime);

        /// <summary>
        /// Устанавливает в часах указанную дату и время.
        /// </summary>
        void SetDateTime(DateTime dateTime);

        /// <summary>
        /// Считывает дату и время из часов реального времени.
        /// </summary>
        DateTime GetDateTime();

        /// <summary>
        /// Считывает установленный в часах день недели.
        /// </summary>
        string GetWeekDay();
    }
}