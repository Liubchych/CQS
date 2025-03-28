using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    static string logFilePath = "log.txt"; // Файл логу
    static DateTime lastReceivedTime = DateTime.MinValue; // Останній час отримання даних

    static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>(); // Черга для запису в файл
    static AutoResetEvent logSignal = new AutoResetEvent(false); // Сигнал для потоку запису

    static void Main()
    {
        string portName = "COM4";  // Порт, який слухаємо
        int baudRate = 57600;      // Швидкість порту

        using (SerialPort port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One))
        {
            try
            {
                port.Handshake = Handshake.None;
                port.DataReceived += Port_DataReceived; // Обробник події отримання даних

                string logFilePath = "log.txt"; // Файл логу
                // Створюємо файл, якщо його немає
                if (!File.Exists(logFilePath))
                {
                    File.Create(logFilePath).Close();
                }

                // Запускаємо фоновий потік для запису у файл
                Thread logThread = new Thread(ProcessLogQueue)
                {
                    IsBackground = true
                };
                logThread.Start();

                port.Open();  // Відкриваємо порт
                Console.WriteLine($"Listening on {portName} at {baudRate} baud...");

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(); // Очікує натискання клавіші для виходу
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
    }

    private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        SerialPort port = (SerialPort)sender;
        string data = port.ReadLine();

        Console.WriteLine(data); // Виводимо лише отримані дані


        // Парсимо отримані дані
        ParsedData parsedData = DataParsing.ParseData(data);

        //------------------------------------------creatin log.txt------------------------------------------------

        string logFilePath = $"{parsedData.RobotID}log.txt"; // Файл логу
        // Створюємо файл, якщо його немає
        if (!File.Exists(logFilePath))
        {
            File.Create(logFilePath).Close();
        }
        DateTime now = DateTime.Now;
        long delta = (lastReceivedTime == DateTime.MinValue) ? 0 : (long)(now - lastReceivedTime).TotalMilliseconds;
        lastReceivedTime = now;
        string logEntry = $"{now:yyyy-MM-dd HH:mm:ss.fff}\t{delta}\t{data}";
        // Додаємо запис у чергу
        logQueue.Enqueue(logEntry);
        logSignal.Set(); // Сигналізуємо потоку запису
    }

    private static void ProcessLogQueue()
    {
        while (true)
        {
            logSignal.WaitOne(); // Чекаємо на сигнал

            while (logQueue.TryDequeue(out string logEntry))
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);

            }
        }
    }
}

