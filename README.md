# Конкурентное программирование
Мельников Михей, ФТ-303

| # | Задание | Замечания | Баллы |
|--|--|--|--|
| 1 | Экспериментальным путём определить квант времени, выделяемый потоку планировщиком потоков. **Напоминание:** не забудьте привязать ваш процесс к последнему ядру и выставить процессу приоритет RealTime. Это значительно повысит точность вычислений. Но убедитесь, что процесс не будет запущен слишком долго - даже операционная система не вытесняет RealTime процессы. **Подсказка:** значение должно получаться около 32 мс (если вы не меняли настройки производительности в вашей ОС) | нет | 1 |
| 2 | Написать реализацию для интерфейса  `public interface IMultiLock { public IDisposable AcquireLock(params string[] keys);}` При вызове метода `AcquireLock` должна захватываться блокировка по набору ключей. Это значит, что если другой поток попытается захватить блокировку (используя этот же метод) и в его набор ключей будет входить хотя бы один из ранее захваченных - то этот другой поток заблокируется. Метод должен возвращать `IDisposable` объект - при вызове `Dispose()` блокировка со всех ключей из набора должна сниматься. Так же необходимо, чтобы любые два невложенных вызова `AcquireLock` не приводили к взаимоблокировке. При необходимости вы можете принимать в конструкторе `MultiLock`-а набор ключей, по которым допустима блокировка. | - | - |
