using System;

namespace DS3231
{
    public partial class Ds3231
    {
        /// <inheritdoc />
        public bool IsOscillatorStopped() => _outputController.IsOscillatorStopped();

        /// <inheritdoc />
        public void ClearOscillatorStoppedFlag() => _outputController.ClearOscillatorStoppedFlag();

        /// <inheritdoc />
        public void SetSquareWaveOutput(SquareWaveFrequency frequency) => _outputController.SetSquareWaveOutput(frequency);

        /// <inheritdoc />
        public void DisableSquareWaveOutput() => _outputController.DisableSquareWaveOutput();

        /// <inheritdoc />
        public void Enable32kHzOutput() => _outputController.Enable32kHzOutput();

        /// <inheritdoc />
        public void Disable32kHzOutput() => _outputController.Disable32kHzOutput();

        /// <inheritdoc />
        public bool Is32kHzOutputEnabled() => _outputController.Is32kHzOutputEnabled();

        /// <inheritdoc />
        public sbyte GetAgingOffset() => _outputController.GetAgingOffset();

        /// <inheritdoc />
        public void SetAgingOffset(sbyte offset) => _outputController.SetAgingOffset(offset);

        private sealed class OutputController
        {
            private readonly Ds3231 _rtc;

            public OutputController(Ds3231 rtc)
            {
                _rtc = rtc;
            }

            public bool IsOscillatorStopped()
            {
                return (_rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0] & OscillatorStoppedFlag) != 0;
            }

            public void ClearOscillatorStoppedFlag()
            {
                ClearStatusFlag(OscillatorStoppedFlag);
            }

            public void SetSquareWaveOutput(SquareWaveFrequency frequency)
            {
                if (!Enum.IsDefined(typeof(SquareWaveFrequency), frequency))
                {
                    throw new ArgumentOutOfRangeException(nameof(frequency));
                }

                var control = _rtc.ReadRegisters((byte)Registry.Control, 1)[0];
                control &= unchecked((byte)~(InterruptControl | SquareWaveFrequencyMask));
                control |= (byte)((byte)frequency << 3);
                _rtc.WriteRegisters((byte)Registry.Control, control);
            }

            public void DisableSquareWaveOutput()
            {
                _rtc.UpdateControlRegister(InterruptControl, true);
            }

            public void Enable32kHzOutput()
            {
                UpdateStatusRegister(Enable32kHzOutputFlag, true);
            }

            public void Disable32kHzOutput()
            {
                UpdateStatusRegister(Enable32kHzOutputFlag, false);
            }

            public bool Is32kHzOutputEnabled()
            {
                return (_rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0] & Enable32kHzOutputFlag) != 0;
            }

            public sbyte GetAgingOffset()
            {
                return (sbyte)_rtc.ReadRegisters((byte)Registry.AgingOffset, 1)[0];
            }

            public void SetAgingOffset(sbyte offset)
            {
                _rtc.WriteRegisters((byte)Registry.AgingOffset, unchecked((byte)offset));
            }

            public Ds3231Status CreateStatus(byte status, byte control)
            {
                return new Ds3231Status(
                    (status & OscillatorStoppedFlag) != 0,
                    (status & Alarm1Flag) != 0,
                    (status & Alarm2Flag) != 0,
                    (status & Enable32kHzOutputFlag) != 0,
                    (status & BusyFlag) != 0,
                    (control & InterruptControl) == 0,
                    (SquareWaveFrequency)((control & SquareWaveFrequencyMask) >> 3));
            }

            private void UpdateStatusRegister(byte mask, bool enabled)
            {
                var status = _rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0];
                if (enabled)
                {
                    status |= mask;
                }
                else
                {
                    status &= (byte)~mask;
                }

                _rtc.WriteRegisters((byte)Registry.ControlStatus, status);
            }

            private void ClearStatusFlag(byte flag)
            {
                var status = _rtc.ReadRegisters((byte)Registry.ControlStatus, 1)[0];
                _rtc.WriteRegisters((byte)Registry.ControlStatus, (byte)(status & ~flag));
            }
        }
    }
}
