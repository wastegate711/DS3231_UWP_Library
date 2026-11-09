# West.RTC.DS3231

Библиотека для работы с часами реального времени DS3231 по шине I²C. Проект собирается как `netstandard2.0`; фактическая работа с `Windows.Devices.I2c` требует Windows-приложения/устройства и доступной I²C-шины. Сам по себе `netstandard2.0` не делает I²C API кроссплатформенным.

## Установка

Установите NuGet-пакет `West.RTC.DS3231` в приложение, работающее на Windows и поддерживающее WinRT I²C API:

```powershell
dotnet add package West.RTC.DS3231
```

Пакет использует Windows SDK Contracts для ссылок на `Windows.Devices.I2c`. Подключите устройство DS3231 к I²C и убедитесь, что приложение/устройство настроено на доступ к соответствующей шине. Стандартный 7-битный адрес DS3231 — `0x68`.

## Быстрый старт

Для обычного контроллера используйте `InitializeAsync` и дождитесь завершения подключения, прежде чем читать или записывать часы:

```csharp
using DS3231;
using Windows.Devices.I2c;

var rtc = new Ds3231();
await rtc.InitializeAsync(0x68, I2cBusSpeed.StandardMode);

try
{
    DateTime now = rtc.GetDateTime();
    string weekday = rtc.GetWeekDay();
    float temperatureCelsius = rtc.GetTemperature();

    var result = rtc.SetDateTime(new DateTime(2026, 11, 9, 12, 30, 0));
}
finally
{
    rtc.Close();
}
```

Если контроллер нужно выбрать по имени, например `i2c1`, используйте перегрузку:

```csharp
await rtc.InitializeAsync("i2c1", 0x68, I2cBusSpeed.StandardMode);
```

В приложении вызовы `await` должны находиться внутри `async`-метода. Выберите `I2cBusSpeed.FastMode`, если устройство, контроллер и проводка поддерживают Fast-mode.

## Класс `Ds3231` и интерфейс `IDs3231`

`Ds3231` реализует `IDs3231`. Методы ниже перечислены по назначению; оба метода инициализации также доступны через `IDs3231`.

### Инициализация и подключение

- `Task InitializeAsync(int address, I2cBusSpeed busSpeed)` — асинхронно получает контроллер по умолчанию и открывает устройство с указанным I²C-адресом и скоростью.
- `Task InitializeAsync(string interfaceName, int address, I2cBusSpeed busSpeed)` — находит контроллер по имени (например, `i2c1`) и открывает устройство.
- `void Initialization(int address, I2cBusSpeed busSpeed)` — прежний совместимый метод инициализации по умолчанию. Он запускает асинхронное подключение, но не позволяет дождаться его завершения; в новом коде используйте `InitializeAsync`.
- `void Initialization(string interfaceName, int address, I2cBusSpeed busSpeed)` — прежний совместимый вариант с выбором контроллера по имени. Также предпочтителен `InitializeAsync`.

Для всех остальных операций сначала дождитесь завершения `InitializeAsync`. Методы доступа к регистрам синхронные и используют уже открытое устройство.

### Чтение

- `DateTime GetDateTime()` — читает секунды, минуты, часы, дату, месяц и год DS3231 и возвращает `DateTime`.
- `string GetWeekDay()` — читает регистр дня недели. Возвращаемые значения: `Понедельник`, `Вторник`, `Среда`, `Четверг`, `Пятница`, `Суббота`, `Воскресенье`; при неизвестном значении возвращается `Ошибка.`.
- `float GetTemperature()` — читает температурные регистры и возвращает значение в градусах Цельсия с шагом 0,25 °C.
- `string GetDeviceId()` — возвращает системный идентификатор открытого I²C-устройства.

### Установка даты и времени

- `I2cTransferResult SetDateTime()` — записывает системные локальные дату и время (`DateTime.Now`) в DS3231.
- `I2cTransferResult SetDateTime(DateTime dateTime)` — записывает явно переданные дату и время. Значение записывается как компоненты часов без преобразования часового пояса; передайте корректное локальное время для своего сценария.
- `I2cTransferResult SetDateTime(string newDateTime)` — разбирает строку через `DateTime.Parse` с текущей культурой среды и затем записывает время. Для предсказуемого формата и обработки ошибок предпочтительнее перегрузка с `DateTime`.

Перегрузки `SetDateTime` возвращают `I2cTransferResult` из Windows I²C API. Проверяйте `Status` и `BytesTransferred`, чтобы убедиться в результате операции.

### Закрытие

- `void Close()` — освобождает открытый `I2cDevice`. Вызывайте после завершения работы; метод допускает повторный вызов.

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

Пакет включает README и компактную NuGet-иконку `west-icon.png`, подготовленную из пользовательского логотипа `DS3231/Images/west.png`. Исходный логотип сохраняется в репозитории без изменений. Версия по умолчанию задается в `DS3231/DS3231.csproj`; при публикации GitHub Actions берет версию из тега релиза (с удалением префикса `v`).

## Gitflow и релизы

- `dev` — основная ветка разработки; изменения интегрируются сюда.
- `release` — ветка стабильных релизов.
- Для публикации создайте GitHub Release с тегом версии, например `v1.0.12`. Workflow `.github/workflows/release.yml` соберет `.nupkg`, прикрепит его к релизу и опубликует в NuGet.org через Trusted Publishing.

Для Trusted Publishing на NuGet.org создайте политику для владельца репозитория `wastegate711`, репозитория `DS3231_UWP_Library` и workflow-файла `release.yml` (без пути `.github/workflows/`). В настройках GitHub Actions добавьте секрет репозитория `NUGET_USER` со **строковым именем пользователя NuGet.org**, не адресом электронной почты. Политику NuGet.org и секрет необходимо настроить до публикации релиза.
