# Максимальная производительность

- Используйте <span style="color: red; font-weight: bold;">авто генерацию кода</span> для сериализации.
  - Декларируют до <span style="color: red; font-weight: bold;">40%</span> прироста именно на Сериализацию/Десериализацию
  - Обязательна для AOT

- Стандартная сериализация чисел с плавающей точкой медленная
  - Используйте библиотеку <span style="color: red; font-weight: bold;">Ryu</span>

- <span style="color: red; font-weight: bold;">Векторные типы данных</span> дают ощутимый прирост производительности
  - Выполняются с помощью SIMD
  - На любом типе процессоров

- Работа с памятью может занимать самое существенное время, там где это возможно использовать:
  - <span style="color: red; font-weight: bold;">Буферы</span>
  - <span style="color: red; font-weight: bold;">Аллокаторы, например Arena</span>
  - <span style="color: red; font-weight: bold;">ArrayPool</span>
  - Выделение небольших массивов в стеке <span style="color: red; font-weight: bold;">stackalloc</span>

- Меньше классов, больше <span style="color: red; font-weight: bold;">Value типы</span> (Есть нюансы)

- Меньше массивов, больше <span style="color: red; font-weight: bold;">генераторов/итераторов</span> (но каждый случай нужно изучать)

- <span style="color: red; font-weight: bold;">Профилируйте ваш код</span>
