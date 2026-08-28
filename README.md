# Сборка

### Из корня проекта выполните:

#### Windows

```text
dotnet publish -c Release -r win-x64 --self-contained
```

#### Linux

```text
dotnet publish -c release -r linux-x64 --self-contained
```

### ***Комментарии***

В проекте C# в конфигурации Release указана платформа x64


### Результат на Windows:

```text
bin\Release\net10.0\win-x64\publish\apiTest.exe
```

### Результат на Linux:

```text
bin\Release\net10.0\linux-x64\publish\apiTest
```
