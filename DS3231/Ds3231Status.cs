namespace DS3231
{
    /// <summary>
    /// Состояние и флаги DS3231, прочитанные из регистров состояния и управления.
    /// </summary>
    public sealed class Ds3231Status
    {
        internal Ds3231Status(
            bool oscillatorStopped,
            bool alarm1Triggered,
            bool alarm2Triggered,
            bool is32kHzOutputEnabled,
            bool isBusy,
            bool isSquareWaveOutputEnabled,
            SquareWaveFrequency squareWaveFrequency)
        {
            OscillatorStopped = oscillatorStopped;
            Alarm1Triggered = alarm1Triggered;
            Alarm2Triggered = alarm2Triggered;
            Is32kHzOutputEnabled = is32kHzOutputEnabled;
            IsBusy = isBusy;
            IsSquareWaveOutputEnabled = isSquareWaveOutputEnabled;
            SquareWaveFrequency = squareWaveFrequency;
        }

        /// <summary>Возвращает true, если генератор остановился и время может быть недостоверным.</summary>
        public bool OscillatorStopped { get; }

        /// <summary>Возвращает true, если установлен флаг срабатывания будильника 1.</summary>
        public bool Alarm1Triggered { get; }

        /// <summary>Возвращает true, если установлен флаг срабатывания будильника 2.</summary>
        public bool Alarm2Triggered { get; }

        /// <summary>Возвращает true, если включен выход 32 кГц.</summary>
        public bool Is32kHzOutputEnabled { get; }

        /// <summary>Возвращает true, если DS3231 выполняет преобразование температуры.</summary>
        public bool IsBusy { get; }

        /// <summary>Возвращает true, если вывод INT/SQW настроен на генерацию сигнала.</summary>
        public bool IsSquareWaveOutputEnabled { get; }

        /// <summary>Возвращает настроенную частоту выхода INT/SQW.</summary>
        public SquareWaveFrequency SquareWaveFrequency { get; }
    }
}
