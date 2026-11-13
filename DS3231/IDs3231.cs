using System;
using System.Threading.Tasks;

namespace DS3231
{
    public interface IDs3231 : IDisposable
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
        /// Устанавливает в часах текущие локальные дату и время.
        /// </summary>
        void SetDateTime();

        /// <summary>
        /// Устанавливает дату и время в формате dd.MM.yyyy HH:mm:ss без преобразования часового пояса.
        /// </summary>
        void SetDateTime(string newDateTime);

        /// <summary>
        /// Записывает компоненты указанной даты и времени без преобразования часового пояса.
        /// </summary>
        void SetDateTime(DateTime dateTime);

        /// <summary>
        /// Считывает дату и время часов без указания часового пояса.
        /// </summary>
        DateTime GetDateTime();

        /// <summary>
        /// Считывает установленный в часах день недели и возвращает его русское название.
        /// </summary>
        string GetWeekDay();

        /// <summary>
        /// Считывает время, день недели на русском языке, температуру и флаги/состояние устройства.
        /// </summary>
        Ds3231Reading GetReading();

        /// <summary>
        /// Считывает флаги состояния и настройки выходов DS3231.
        /// </summary>
        Ds3231Status GetStatus();

        /// <summary>
        /// Настраивает будильник 1 на ежедневное срабатывание в указанное локальное время, включая секунды.
        /// </summary>
        void SetAlarm1Daily(TimeSpan time);

        /// <summary>
        /// Настраивает будильник 1 на еженедельное срабатывание в указанный день и время.
        /// </summary>
        void SetAlarm1Weekly(DayOfWeek dayOfWeek, TimeSpan time);

        /// <summary>
        /// Настраивает будильник 1 на ежемесячное срабатывание в указанный день и время.
        /// </summary>
        void SetAlarm1DayOfMonth(int day, TimeSpan time);

        /// <summary>
        /// Настраивает будильник 2 на ежедневное срабатывание в указанное время с точностью до минуты.
        /// </summary>
        void SetAlarm2Daily(TimeSpan time);

        /// <summary>
        /// Настраивает будильник 2 на еженедельное срабатывание в указанный день и время.
        /// </summary>
        void SetAlarm2Weekly(DayOfWeek dayOfWeek, TimeSpan time);

        /// <summary>
        /// Настраивает будильник 2 на ежемесячное срабатывание в указанный день и время.
        /// </summary>
        void SetAlarm2DayOfMonth(int day, TimeSpan time);

        /// <summary>
        /// Возвращает true, если установлен флаг срабатывания будильника 1.
        /// </summary>
        bool IsAlarm1Triggered();

        /// <summary>
        /// Возвращает true, если установлен флаг срабатывания будильника 2.
        /// </summary>
        bool IsAlarm2Triggered();

        /// <summary>
        /// Сбрасывает флаг срабатывания будильника 1.
        /// </summary>
        void ClearAlarm1Flag();

        /// <summary>
        /// Сбрасывает флаг срабатывания будильника 2.
        /// </summary>
        void ClearAlarm2Flag();

        /// <summary>
        /// Отключает прерывание от будильника 1 и сбрасывает его флаг срабатывания.
        /// </summary>
        void DisableAlarm1();

        /// <summary>
        /// Отключает прерывание от будильника 2 и сбрасывает его флаг срабатывания.
        /// </summary>
        void DisableAlarm2();

        /// <summary>
        /// Возвращает true, если генератор остановился и время может быть недостоверным.
        /// </summary>
        bool IsOscillatorStopped();

        /// <summary>
        /// Сбрасывает флаг остановки генератора; устанавливайте время заново, если оно недостоверно.
        /// </summary>
        void ClearOscillatorStoppedFlag();

        /// <summary>
        /// Включает сигнал выбранной частоты на выводе INT/SQW, отключая режим прерываний этого вывода.
        /// </summary>
        void SetSquareWaveOutput(SquareWaveFrequency frequency);

        /// <summary>
        /// Выключает режим генератора на INT/SQW и переводит вывод в режим прерываний.
        /// </summary>
        void DisableSquareWaveOutput();

        /// <summary>Включает выход 32 кГц.</summary>
        void Enable32kHzOutput();

        /// <summary>Выключает выход 32 кГц.</summary>
        void Disable32kHzOutput();

        /// <summary>Возвращает true, если выход 32 кГц включен.</summary>
        bool Is32kHzOutputEnabled();

        /// <summary>Считывает signed aging offset для коррекции частоты генератора.</summary>
        sbyte GetAgingOffset();

        /// <summary>Устанавливает signed aging offset в диапазоне от -128 до 127.</summary>
        void SetAgingOffset(sbyte offset);
    }
}