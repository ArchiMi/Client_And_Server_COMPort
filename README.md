Client And Server


=== Client (C# 'WPF' - Visual Studio IDE) ===
1) Установить последовательное соединение с сервером (atmega328p) Baud-256000, MHz-16000000UL (реализовано)
2) Отправка запросов и получение ответов от контроллера atmega328p. (реализовано)
3) Формировать CRC8 передаваемого пакета. (реализовано)
4) Определиться со списком команд и параметров.
5) Определиться с форматом передчи параметров для каждой из команд.

=== Server (Arduino IDE) === (Отказался от этой затеи)

=== Server (C++ - Atmel Studio) ===
1) Определиться с параметрами инициализации при работе с контроллером atmega8
2) Получение запроса и отправка ответа клиенту. (реализовано) 
3) Проверять CRC8 полученного пакета.
