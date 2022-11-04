namespace DS3231
{
    public enum Registry : byte
    {
        Seconds = 0x00,
        Minutes = 0x01,
        Hours = 0x02,
        WeakDay = 0x03,
        Date = 0x04,
        Month = 0x05,
        Year = 0x06,
        Alarm1Seconds = 0x07,
        Alarm1Minutes = 0x08,
        Alarm1Hours = 0x09,
        Alarm1DayDate = 0x0A,
        Alarm2Minutes = 0x0B,
        Alarm2Hours = 0x0C,
        Alarm2DayDate = 0x0D,
        Control = 0x0E,
        ControlStatus = 0x0F,
        AgingOffset = 0x10,
        TempMsb = 0x11,
        TempLsb = 0x12
    }
}