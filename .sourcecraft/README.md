# SourceCraft CI/CD

Файл `ci.yaml` настраивает публикацию пакета библиотеки в GitHub Releases после успешной Release-сборки:

- push в `dev` создает GitHub Pre-release с версией `x.y.z-dev.<UTC timestamp>`;
- push в `release` создает стабильный GitHub Release с версией из `DS3231/DS3231.csproj`;
- `.nupkg` прикрепляется к соответствующему релизу;
- публикация пакета в NuGet.org из SourceCraft pipeline не выполняется.

## Настройка секрета

В настройках репозитория SourceCraft создайте секрет `GITHUB_TOKEN` со значением GitHub Personal Access Token, которому разрешено создавать релизы и загружать assets в `wastegate711/DS3231_UWP_Library` (разрешение `Contents: Read and write`). Не добавляйте токен в файлы репозитория.

После того как ветки `dev` и `release` будут синхронизированы с GitHub, SourceCraft запускает pipeline при получении push в эти ветки. Публикация начинается только после успешных шагов сборки и упаковки.
