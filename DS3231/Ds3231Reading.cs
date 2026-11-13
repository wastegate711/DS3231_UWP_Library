using System;

namespace DS3231
{
    /// <summary>
    /// Совместное чтение времени, дня недели, температуры и состояния DS3231.
    /// </summary>
    public sealed class Ds3231Reading
    {
        internal Ds3231Reading(DateTime dateTime, string weekDay, float temperatureCelsius, Ds3231Status status)
        {
            DateTime = dateTime;
            WeekDay = weekDay;
            TemperatureCelsius = temperatureCelsius;
            Status = status;
        }

        /// <summary>Дата и время устройства без указания часового пояса.</summary>
        public DateTime DateTime { get; }

        /// <summary>День недели на русском языке.</summary>
        public string WeekDay { get; }

        /// <summary>Температура в градусах Цельсия.</summary>
        public float TemperatureCelsius { get; }

        /// <summary>Флаги и состояние устройства.</summary>
        public Ds3231Status Status { get; }
    }
}
