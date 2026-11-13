using System;

namespace DS3231
{
    public partial class Ds3231
    {
        /// <inheritdoc />
        public void SetAlarm1Daily(TimeSpan time) => _alarmController.SetAlarm1Daily(time);

        /// <inheritdoc />
        public void SetAlarm1Weekly(DayOfWeek dayOfWeek, TimeSpan time) => _alarmController.SetAlarm1Weekly(dayOfWeek, time);

        /// <inheritdoc />
        public void SetAlarm1DayOfMonth(int day, TimeSpan time) => _alarmController.SetAlarm1DayOfMonth(day, time);

        /// <inheritdoc />
        public void SetAlarm2Daily(TimeSpan time) => _alarmController.SetAlarm2Daily(time);

        /// <inheritdoc />
        public void SetAlarm2Weekly(DayOfWeek dayOfWeek, TimeSpan time) => _alarmController.SetAlarm2Weekly(dayOfWeek, time);

        /// <inheritdoc />
        public void SetAlarm2DayOfMonth(int day, TimeSpan time) => _alarmController.SetAlarm2DayOfMonth(day, time);

        /// <inheritdoc />
        public bool IsAlarm1Triggered() => _alarmController.IsAlarm1Triggered();

        /// <inheritdoc />
        public bool IsAlarm2Triggered() => _alarmController.IsAlarm2Triggered();

        /// <inheritdoc />
        public void ClearAlarm1Flag() => _alarmController.ClearAlarm1Flag();

        /// <inheritdoc />
        public void ClearAlarm2Flag() => _alarmController.ClearAlarm2Flag();

        /// <inheritdoc />
        public void DisableAlarm1() => _alarmController.DisableAlarm1();

        /// <inheritdoc />
        public void DisableAlarm2() => _alarmController.DisableAlarm2();

        private sealed class AlarmController
        {
            private readonly Ds3231 _rtc;

            public AlarmController(Ds3231 rtc)
            {
                _rtc = rtc;
            }

            public void SetAlarm1Daily(TimeSpan time)
            {
                ValidateAlarmTime(time);
                SetAlarm1(
                    ConvertDecToBcd((byte)time.Seconds),
                    ConvertDecToBcd((byte)time.Minutes),
                    ConvertDecToBcd((byte)time.Hours),
                    AlarmMask);
            }

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

            public void SetAlarm2Daily(TimeSpan time)
            {
                ValidateAlarm2Time(time);
                SetAlarm2(
                    ConvertDecToBcd((byte)time.Minutes),
                    ConvertDecToBcd((byte)time.Hours),
                    AlarmMask);
            }

            public void SetAlarm2Weekly(DayOfWeek dayOfWeek, TimeSpan time)
            {
                ValidateAlarm2Time(time);
                var day = GetDayOfWeekRegisterValue(dayOfWeek);
                SetAlarm2(
                    ConvertDecToBcd((byte)time.Minutes),
                    ConvertDecToBcd((byte)time.Hours),
                    (byte)(AlarmDayOfWeekMode | day));
            }

            public void SetAlarm2DayOfMonth(int day, TimeSpan time)
            {
                ValidateAlarm2Time(time);
                ValidateDayOfMonth(day);
                SetAlarm2(
                    ConvertDecToBcd((byte)time.Minutes),
                    ConvertDecToBcd((byte)time.Hours),
                    ConvertDecToBcd((byte)day));
            }

            public bool IsAlarm1Triggered()
            {
                return (_rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0] & Alarm1Flag) != 0;
            }

            public bool IsAlarm2Triggered()
            {
                return (_rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0] & Alarm2Flag) != 0;
            }

            public void ClearAlarm1Flag()
            {
                ClearAlarmFlag(Alarm1Flag);
            }

            public void ClearAlarm2Flag()
            {
                ClearAlarmFlag(Alarm2Flag);
            }

            public void DisableAlarm1()
            {
                _rtc.UpdateControlRegister(Alarm1InterruptEnable, false);
                ClearAlarm1Flag();
            }

            public void DisableAlarm2()
            {
                _rtc.UpdateControlRegister(Alarm2InterruptEnable, false);
                ClearAlarm2Flag();
            }

            private void SetAlarm1(byte second, byte minute, byte hour, byte dayDate)
            {
                _rtc.WriteRegisters((byte)Registry.Alarm1Seconds, second, minute, hour, dayDate);
                ClearAlarm1Flag();
                _rtc.UpdateControlRegister(Alarm1InterruptEnable, true);
            }

            private void SetAlarm2(byte minute, byte hour, byte dayDate)
            {
                _rtc.WriteRegisters((byte)Registry.Alarm2Minutes, minute, hour, dayDate);
                ClearAlarm2Flag();
                _rtc.UpdateControlRegister(Alarm2InterruptEnable, true);
            }

            private void ClearAlarmFlag(byte flag)
            {
                var status = _rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0];
                _rtc.WriteRegisters((byte)Registry.ControlStatus, (byte)(status & ~flag));
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
        }
    }
}
