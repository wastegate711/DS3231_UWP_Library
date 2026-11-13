namespace DS3231
{
    /// <summary>
    /// Частота сигнала на выходе INT/SQW в режиме генератора.
    /// </summary>
    public enum SquareWaveFrequency : byte
    {
        /// <summary>1 Гц.</summary>
        Hz1 = 0,

        /// <summary>1,024 кГц.</summary>
        Hz1024 = 1,

        /// <summary>4,096 кГц.</summary>
        Hz4096 = 2,

        /// <summary>8,192 кГц.</summary>
        Hz8192 = 3
    }
}
