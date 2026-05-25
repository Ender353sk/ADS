using System;

namespace LabHash_Variant6
{

    // Основний клас фігури
    public class Trapezoid
    {
        // Координати вершин: A, B, C, D
        public double X1 { get; }
        public double Y1 { get; } // Точка A
        public double X2 { get; }
        public double Y2 { get; } // Точка B
        public double X3 { get; }
        public double Y3 { get; } // Точка C
        public double X4 { get; }
        public double Y4 { get; } // Точка D

        public Trapezoid(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
            X3 = x3; Y3 = y3;
            X4 = x4; Y4 = y4;
        }

        // Перевірка правильності фігури(чи є вона дійсно трапецією)
        // Для спрощення генерації будуємо трапеції з паралельними основами відносно осі Ox
        public static bool IsValid(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            // Перевіримо, що основи AB та CD паралельні осі OX, мають різну довжину і висота не нульова
            bool basesParallel = Math.Abs(y1 - y2) < 0.001 && Math.Abs(y3 - y4) < 0.001;
            if (basesParallel && Math.Abs(y1 - y3) > 0.001)
            {
                double base1 = Math.Abs(x2 - x1);
                double base2 = Math.Abs(x3 - x4);
                // Трапеція має основи різної довжини (інакше це паралелограм)
                return Math.Abs(base1 - base2) > 0.001 && base1 > 0 && base2 > 0;
            }
            return false;
        }

        // Обчислення площі трапеції
        public double GetArea()
        {
            double base1 = Math.Abs(X2 - X1);
            double base2 = Math.Abs(X3 - X4);
            double height = Math.Abs(Y3 - Y1);
            return ((base1 + base2) / 2.0) * height;
        }

        // Обчислення периметра трапеції
        public double GetPerimeter()
        {
            double base1 = Math.Abs(X2 - X1);
            double base2 = Math.Abs(X3 - X4);
            double leg1 = Math.Sqrt(Math.Pow(X4 - X1, 2) + Math.Pow(Y4 - Y1, 2));
            double leg2 = Math.Sqrt(Math.Pow(X3 - X2, 2) + Math.Pow(Y3 - Y2, 2));
            return base1 + base2 + leg1 + leg2;
        }

        public override string ToString()
        {
            return string.Format("Трапеція[A({0},{1}), B({2},{3}), C({4},{5}), D({6},{7})] -> Площа: {8:F2}, Периметр: {9:F2}",
                X1, Y1, X2, Y2, X3, Y3, X4, Y4, GetArea(), GetPerimeter());
        }
    }

    // КЛАС: Слот Хеш-таблиці
    public class HashSlot
    {
        public Trapezoid Value { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsDeleted { get; set; } // Потрібно для лінійного зондування (Рівень 3)

        public HashSlot()
        {
            Value = null;
            IsOccupied = false;
            IsDeleted = false;
        }
    }

    // КЛАС: Хеш-таблиця з відкритою адресацією
    public class HashTable
    {
        private readonly HashSlot[] _table;
        private readonly int _size;
        private readonly int _level; // 1, 2 або 3 рівень завдання

        public HashTable(int size, int level)
        {
            _size = size;
            _level = level;
            _table = new HashSlot[size];
            for (int i = 0; i < size; i++)
            {
                _table[i] = new HashSlot();
            }
        }

        // Хеш-функція: Метод Множення
        private int HashFunction(double key)
        {
            double A = (Math.Sqrt(5) - 1) / 2; // Стала Кнута ~0.6180339887
            double fractionalPart = (key * A) % 1;
            if (fractionalPart < 0) fractionalPart += 1; // Захист від від'ємних значень
            return (int)Math.Floor(_size * fractionalPart);
        }

        // Вставлення елемента
        public bool Insert(Trapezoid item)
        {
            double key = item.GetArea(); // Ключ — площа
            int baseIndex = HashFunction(key);

            // --- РІВЕНЬ 1: Без вирішення колізій ---
            if (_level == 1)
            {
                if (_table[baseIndex].IsOccupied)
                {
                    return false; // Позиція зайнята, повертаємо false
                }
                _table[baseIndex].Value = item;
                _table[baseIndex].IsOccupied = true;
                return true;
            }

            // --- РІВЕНЬ 2 та 3: Лінійне зондування ---
            for (int i = 0; i < _size; i++)
            {
                int index = (baseIndex + i) % _size;

                // Якщо слот вільний або помічений як видалений
                if (!_table[index].IsOccupied)
                {
                    _table[index].Value = item;
                    _table[index].IsOccupied = true;
                    _table[index].IsDeleted = false;
                    return true;
                }
            }

            return false; // Таблиця повністю заповнена
        }

        // --- РІВЕНЬ 3: Видалення за критерієм (Периметр в заданому діапазоні) ---
        public int DeleteByPerimeterRange(double min, double max)
        {
            int deletedCount = 0;
            for (int i = 0; i < _size; i++)
            {
                if (_table[i].IsOccupied)
                {
                    double p = _table[i].Value.GetPerimeter();
                    if (p >= min && p <= max)
                    {
                        _table[i].IsOccupied = false;
                        _table[i].IsDeleted = true;
                        _table[i].Value = null;
                        deletedCount++;
                    }
                }
            }
            return deletedCount;
        }

        // Виведення вмісту таблиці за допомогою форматованого виводу
        public void PrintTable()
        {
            Console.WriteLine(new string('-', 105));
            Console.WriteLine(string.Format("| {0,-5} | {1,-12} | {2,-80} |", "Індекс", "Ключ (Площа)", "Об'єкт (Трапеція)"));
            Console.WriteLine(new string('-', 105));

            for (int i = 0; i < _size; i++)
            {
                if (_table[i].IsOccupied)
                {
                    Console.WriteLine(string.Format("| {0,-6} | {1,-12:F2} | {2,-80} |",
                        i, _table[i].Value.GetArea(), _table[i].Value.ToString()));
                }
                else if (_table[i].IsDeleted)
                {
                    Console.WriteLine(string.Format("| {0,-6} | {1,-12} | {2,-80} |",
                        i, "<DEL>", "[Елемент видалено]"));
                }
                else
                {
                    Console.WriteLine(string.Format("| {0,-6} | {1,-12} | {2,-80} |",
                        i, "---", "[Вільний слот]"));
                }
            }
            Console.WriteLine(new string('-', 105));
        }
    }

    // ГОЛОВНИЙ КЛАС ПРОГРАМИ
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Random rand = new Random();

            Console.Write("Введіть розмір хеш-таблиці: ");
            if (!int.TryParse(Console.ReadLine(), out int size) || size <= 0)
            {
                size = 10; // Значення за замовчуванням
            }

            // ГЕНЕРАЦІЯ ВИПАДКОВИХ ТРАПЕЦІЙ
            // Генерує трохи більше елементів, ніж розмір таблиці, щоб спровокувати колізії
            int countToGenerate = size + 3;
            Trapezoid[] generatedTrapezoids = new Trapezoid[countToGenerate];

            for (int i = 0; i < countToGenerate; i++)
            {
                while (true)
                {
                    // Випадкові координати для горизонтальних основ
                    double y1 = rand.Next(1, 10);
                    double y2 = y1;
                    double y3 = rand.Next(11, 20);
                    double y4 = y3;

                    double x1 = rand.Next(1, 10);
                    double x2 = rand.Next(12, 25);
                    double x4 = rand.Next(1, 12);
                    double x3 = rand.Next(14, 30);

                    if (Trapezoid.IsValid(x1, y1, x2, y2, x3, y3, x4, y4))
                    {
                        generatedTrapezoids[i] = new Trapezoid(x1, y1, x2, y2, x3, y3, x4, y4);
                        break;
                    }
                }
            }

            // РІВЕНЬ 1: Вставлення без обробки колізій
            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine(" ЗАВДАННЯ ПЕРШОГО РІВНЯ (Без колізій)");
            Console.WriteLine(new string('=', 40));

            HashTable tableLevel1 = new HashTable(size, level: 1);
            for (int i = 0; i < countToGenerate; i++)
            {
                bool success = tableLevel1.Insert(generatedTrapezoids[i]);
                if (!success)
                {
                    Console.WriteLine($"[Колізія!] Не вдалося вставити елемент з площею {generatedTrapezoids[i].GetArea():F2} (Слот зайнято).");
                }
            }
            Console.WriteLine("\nВміст хеш-таблиці (Рівень 1):");
            tableLevel1.PrintTable();


            // РІВЕНЬ 2: Вставлення з Лінійним зондуванням
            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine(" ЗАВДАННЯ ДРУГОГО РІВНЯ (Лінійне зондування)");
            Console.WriteLine(new string('=', 40));

            HashTable tableLevel2 = new HashTable(size, level: 2);
            for (int i = 0; i < countToGenerate; i++)
            {
                bool success = tableLevel2.Insert(generatedTrapezoids[i]);
                if (!success)
                {
                    Console.WriteLine($"[Помилка] Таблиця заповнена! Неможливо додати площу {generatedTrapezoids[i].GetArea():F2}");
                }
            }
            Console.WriteLine("\nВміст хеш-таблиці (Рівень 2):");
            tableLevel2.PrintTable();


            // РІВЕНЬ 3: Видалення елементів за діапазоном периметра
            Console.WriteLine("\n" + new string('=', 40));
            Console.WriteLine(" ЗАВДАННЯ ТРЕТЬОГО РІВНЯ (Видалення за критерієм)");
            Console.WriteLine(new string('=', 40));

            // Задає випадковий або фіксований діапазон периметра для видалення
            double minPerimeter = 40.0;
            double maxPerimeter = 65.0;

            Console.WriteLine($"Критерій видалення: Периметр у діапазоні від {minPerimeter} до {maxPerimeter}");

            int removed = tableLevel2.DeleteByPerimeterRange(minPerimeter, maxPerimeter);
            Console.WriteLine($"Успішно видалено елементів: {removed}");

            Console.WriteLine("\nВміст хеш-таблиці після видалення (Рівень 3):");
            tableLevel2.PrintTable();

            Console.ReadLine();
        }
    }
}
