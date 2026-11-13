# SourceCraft CI/CD

Файл `ci.yaml` настраивает сборку и публикацию пакета в Releases зеркального репозитория SourceCraft:

- push в `dev` после успешной Release-сборки создает SourceCraft Pre-release с версией `x.y.z-dev.<UTC timestamp>.<commit SHA>`;
- push в `release` после успешной Release-сборки создает стабильный SourceCraft Release с версией из `DS3231/DS3231.csproj`;
- `.nupkg` прикрепляется к релизу до его публикации;
- GitHub Releases и NuGet.org этим pipeline не затрагиваются.

Релизы создаются в том SourceCraft-репозитории, откуда запущен pipeline; организация и имя репозитория определяются из `SOURCECRAFT_REPO_URL`.

## Настройка секрета

В настройках зеркального репозитория SourceCraft создайте секрет `SOURCECRAFT_TOKEN` со значением Personal Access Token, которому разрешено создавать релизы и загружать вложения в этот репозиторий. Не добавляйте токен в файлы репозитория.

После синхронизации веток `dev` и `release` из GitHub SourceCraft запускает pipeline при получении push. Публикация начинается только после успешной сборки и упаковки.
