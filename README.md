# West.RTC.DS3231

Библиотека для работы с часами реального времени DS3231 по шине I²C. Проект собирается как `netstandard2.0` и использует `System.Device.I2c`; доступность I²C зависит от платформы и драйвера, поддерживаемого `System.Device.Gpio`.

## Установка

Установите NuGet-пакет `West.RTC.DS3231` в .NET-приложение на платформе с поддержкой `System.Device.I2c`:

```powershell
dotnet add package West.RTC.DS3231
```

Пакет использует `System.Device.Gpio`. Подключите устройство DS3231 к I²C и убедитесь, что ОС предоставляет драйвер для выбранной шины. Стандартный 7-битный адрес DS3231 — `0x68`.

## Быстрый старт

Для обычного контроллера используйте `InitializeAsync` и дождитесь завершения подключения, прежде чем читать или записывать часы:

```csharp
using DS3231;

using (var rtc = new Ds3231())
{
    await rtc.InitializeAsync(busId: 1, address: 0x68);

    DateTime nowUtc = rtc.GetDateTime();
    string weekday = rtc.GetWeekDay();
    float temperatureCelsius = rtc.GetTemperature();
    Ds3231Reading reading = rtc.GetReading();

    if (reading.Status.OscillatorStopped)
    {
        rtc.SetDateTime(DateTime.UtcNow);
        rtc.ClearOscillatorStoppedFlag();
    }
}
```

`busId` — системный идентификатор I²C-шины; его значение зависит от ОС и драйвера. Вызов `await` должен находиться внутри `async`-метода. Открытие устройства выполняется синхронно underlying API, хотя `InitializeAsync` оставлен для удобства асинхронной инициализации в приложении.

## Класс `Ds3231` и интерфейс `IDs3231`

`Ds3231` реализует `IDs3231`. Методы ниже перечислены по назначению; оба метода инициализации также доступны через `IDs3231`.

### Инициализация и подключение

- `void Initialization(int busId, int address)` — открывает устройство на указанной I²C-шине.
- `Task InitializeAsync(int busId, int address)` — запускает ту же инициализацию и возвращает завершенную задачу; API `System.Device.I2c` открывает устройство синхронно.

Перед вызовом остальных методов сначала инициализируйте устройство.

### Чтение

- `DateTime GetDateTime()` — читает часы и возвращает `DateTime` с `Kind == Utc`.
- `string GetWeekDay()` — читает регистр дня недели. Возвращаемые значения: `Понедельник`, `Вторник`, `Среда`, `Четверг`, `Пятница`, `Суббота`, `Воскресенье`; при неизвестном значении возвращается `Ошибка.`.
- `DayOfWeek GetDayOfWeek()` — возвращает установленное в регистре DS3231 значение `DayOfWeek`; неизвестное значение регистра приводит к `InvalidDataException`.
- `float GetTemperature()` — читает температурные регистры и возвращает значение в градусах Цельсия с шагом 0,25 °C.
- `Ds3231Status GetStatus()` — читает флаги генератора и будильников, состояние преобразования температуры и настройки выходов.
- `Ds3231Reading GetReading()` — совместно считывает дату/время UTC, день недели, температуру и `Ds3231Status`.
- `string GetDeviceId()` — возвращает идентификатор в виде номера I²C-шины и адреса устройства.

### Установка даты и времени

- `void SetDateTime()` — записывает текущие дату и время UTC (`DateTime.UtcNow`) в DS3231.
- `void SetDateTime(DateTime dateTime)` — `DateTimeKind.Local` преобразуется в UTC; `Utc` используется без изменения, а `Unspecified` явно трактуется как UTC. Поддерживаются годы с 2000 по 2199 после преобразования.
- `void SetDateTime(string newDateTime)` — разбирает дату в точном формате `dd.MM.yyyy HH:mm:ss` и трактует результат как UTC; неверный формат приводит к `FormatException`.

DS3231 не хранит информацию о часовом поясе: библиотека представляет его часы в UTC. Время будильников также задавайте в UTC. День недели для будильника устанавливается по той же системе регистра DS3231: понедельник — 1, воскресенье — 7.

Ошибки шины передаются вызывающему коду исключениями `System.Device.I2c`; при успешной записи методы завершаются без результата.

### Будильники

```csharp
rtc.SetAlarm1Daily(new TimeSpan(7, 30, 15));
rtc.SetAlarm2Weekly(DayOfWeek.Monday, new TimeSpan(8, 0, 0));

if (rtc.IsAlarm1Triggered())
{
    // Обработать срабатывание.
    rtc.ClearAlarm1Flag();
}
```

- `SetAlarm1Daily(TimeSpan time)` — будильник 1 каждый день в заданное время, с точностью до секунды.
- `SetAlarm1Weekly(DayOfWeek dayOfWeek, TimeSpan time)` — будильник 1 в заданный день недели и время.
- `SetAlarm1DayOfMonth(int day, TimeSpan time)` — будильник 1 в заданный день каждого месяца и время. Для месяцев без такого дня срабатывания не будет.
- `SetAlarm2Daily(TimeSpan time)` — будильник 2 каждый день в заданные часы и минуты; секунды не задаются.
- `SetAlarm2Weekly(DayOfWeek dayOfWeek, TimeSpan time)` — будильник 2 в заданный день недели, часы и минуты.
- `SetAlarm2DayOfMonth(int day, TimeSpan time)` — будильник 2 в заданный день каждого месяца, часы и минуты.
- `IsAlarm1Triggered()` / `IsAlarm2Triggered()` — проверяют соответствующий флаг срабатывания в регистре состояния.
- `ClearAlarm1Flag()` / `ClearAlarm2Flag()` — сбрасывают флаг; его нужно сбросить, чтобы снять активный сигнал будильника на `INT/SQW`.
- `DisableAlarm1()` / `DisableAlarm2()` — выключают соответствующее разрешение прерывания и сбрасывают его флаг.

Вызов настройки включает соответствующий будильник и переводит вывод `INT/SQW` в режим прерывания. При совпадении времени DS3231 **не вызывает обработчик приложения сам**: он устанавливает флаг будильника и тянет общий вывод `INT/SQW` в низкий уровень (активный низкий, open-drain). Подтяните вывод к питанию согласно схеме устройства и подключите его к GPIO микроконтроллера/компьютера. Чтобы получить программное GPIO-прерывание, приложение должно отдельно настроить обработчик на спад сигнала. Пока флаг не сброшен через `ClearAlarm1Flag()` или `ClearAlarm2Flag()`, линия остается низкой; при использовании обоих будильников очистите флаги обоих сработавших будильников.

### Флаги генератора, выходы и калибровка

```csharp
if (rtc.IsOscillatorStopped())
{
    rtc.SetDateTime(DateTime.UtcNow);
    rtc.ClearOscillatorStoppedFlag();
}

rtc.SetSquareWaveOutput(SquareWaveFrequency.Hz1024);
rtc.Enable32kHzOutput();
sbyte agingOffset = rtc.GetAgingOffset();
```

- `IsOscillatorStopped()` / `ClearOscillatorStoppedFlag()` — проверяют и сбрасывают OSF. Сброс OSF сам по себе не исправляет время: если флаг установлен, установите достоверное время перед его очисткой.
- `SetSquareWaveOutput(SquareWaveFrequency)` — настраивает INT/SQW на `Hz1`, `Hz1024`, `Hz4096` или `Hz8192`. Переводит общий вывод INT/SQW из режима будильников в режим генератора.
- `DisableSquareWaveOutput()` — переводит INT/SQW обратно в режим прерываний; разрешения будильников сохраняются.
- `Enable32kHzOutput()`, `Disable32kHzOutput()` и `Is32kHzOutputEnabled()` — управляют отдельным выходом 32 кГц.
- `GetAgingOffset()` / `SetAgingOffset(sbyte)` — читают и записывают знаковый регистр aging offset в диапазоне от -128 до 127. Это аппаратная тонкая коррекция частоты; изменяйте значение постепенно и проверяйте результат измерением.

### Закрытие

- `void Close()` — освобождает открытый `I2cDevice`; повторный вызов безопасен.
- `Ds3231` реализует `IDisposable`, поэтому экземпляр можно использовать в `using`.

## Перечисление `Registry`

`Registry` содержит адреса регистров DS3231 (тип `byte`):

| Имя | Адрес |
| --- | ---: |
| `Seconds` | `0x00` |
| `Minutes` | `0x01` |
| `Hours` | `0x02` |
| `WeakDay` | `0x03` |
| `Date` | `0x04` |
| `Month` | `0x05` |
| `Year` | `0x06` |
| `Alarm1Seconds` | `0x07` |
| `Alarm1Minutes` | `0x08` |
| `Alarm1Hours` | `0x09` |
| `Alarm1DayDate` | `0x0A` |
| `Alarm2Minutes` | `0x0B` |
| `Alarm2Hours` | `0x0C` |
| `Alarm2DayDate` | `0x0D` |
| `Control` | `0x0E` |
| `ControlStatus` | `0x0F` |
| `AgingOffset` | `0x10` |
| `TempMsb` | `0x11` |
| `TempLsb` | `0x12` |

## Сборка из исходников

```powershell
dotnet restore DS3231/DS3231.csproj
dotnet build DS3231/DS3231.csproj --configuration Release
dotnet pack DS3231/DS3231.csproj --configuration Release --output artifacts
```

Пакет включает README и компактную NuGet-иконку `west-icon.png`, подготовленную из пользовательского логотипа `DS3231/Images/west.png`. Исходный логотип сохраняется в репозитории без изменений. Версия по умолчанию задается в `DS3231/DS3231.csproj`.

## Gitflow и релизы

Если вы хотите внести свои изменения, создайте ветку `feature/name`, затем создайте Pull Request в ветку `dev`. Для запрета слияния Pull Request с ошибкой сборки в настройках защиты ветки `dev` сделайте обязательной проверку GitHub Actions `Build`.

- `dev` — основная ветка разработки. Каждый Pull Request и push проходят Release-сборку. GitHub Actions после успешной сборки push публикует prerelease-пакет вида `1.0.12-dev.<номер запуска GitHub Actions>` в GitHub Pre-release и NuGet.org.
- `release` — ветка стабильных релизов. Перед публикацией увеличьте `<Version>` в `DS3231/DS3231.csproj`, чтобы версия и тег были уникальными. GitHub Actions после успешной сборки push создает стабильный GitHub Release с приложенным `.nupkg` и публикует стабильную версию в NuGet.org.

В зеркале SourceCraft настроен отдельный pipeline `.sourcecraft/ci.yaml`: при push в `dev` или `release` он собирает и упаковывает библиотеку, затем публикует пакет в Releases самого зеркального репозитория SourceCraft. Этот pipeline не меняет GitHub Releases workflow и ничего не отправляет в NuGet.org. Для публикации добавьте секрет `SOURCECRAFT_TOKEN` в настройках зеркального репозитория; подробности — в [`.sourcecraft/README.md`](.sourcecraft/README.md).

Для публикации из GitHub Actions используется настроенный NuGet Trusted Publishing.
