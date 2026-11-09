using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PersonLab
{
    internal class Program
    {
        private static readonly PersonRepository Repo = new PersonRepository();
        private const string XmlPath = "persons.xml";
        private const string CsvPath = "persons.csv";

        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Программа ищет файлы в папке: " + Directory.GetCurrentDirectory());
            if (File.Exists(CsvPath))
            {
                Console.WriteLine("Файл persons.csv найден");
            }
            else
            {
                Console.WriteLine("Файл persons.csv НЕ НАЙДЕН!");
            }

            // Автозагрузка
            if (File.Exists(CsvPath))
            {
                try
                {
                    int n = Repo.LoadFromCsv(CsvPath);
                    Console.WriteLine($"Автозагрузка из {CsvPath}: добавлено {n} записей.\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось загрузить CSV: {ex.Message}\n");
                }
            }

            bool exit = false;
            while (!exit)
            {
                PrintMenu();
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": ShowPersons(Repo.GetAll()); break;
                    case "2": AddPerson(); break;
                    case "3": DeletePerson(); break;
                    case "4": UpdatePerson(); break;
                    case "5": SortMenu(); break;
                    case "6": FilterMenu(); break;
                    case "7": SaveXml(); break;
                    case "8": LoadXml(); break;
                    case "9": LoadCsv(); break;
                    case "0": exit = true; break;
                    default: Console.WriteLine("Нет такого пункта."); break;
                }
            }
        }

        private static void PrintMenu()
        {
            Console.WriteLine("\n===== БАЗА ДАННЫХ ЛЮДЕЙ =====");
            Console.WriteLine("1. Показать все записи");
            Console.WriteLine("2. Добавить запись");
            Console.WriteLine("3. Удалить запись");
            Console.WriteLine("4. Изменить запись");
            Console.WriteLine("5. Сортировка");
            Console.WriteLine("6. Фильтрация (LINQ)");
            Console.WriteLine("7. Сохранить в XML");
            Console.WriteLine("8. Загрузить из XML");
            Console.WriteLine("9. Загрузить из CSV-файла");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");
        }

        private static void ShowPersons(IEnumerable<Person> list)
        {
            int i = 1;
            bool any = false;
            foreach (var p in list)
            {
                any = true;
                Console.WriteLine($"\n[{i++}] {p}");
            }
            if (!any) Console.WriteLine("Записей нет.");
            Console.WriteLine($"Всего: {i - 1}");
        }

        //Ввод с консоли
        private static string ReadString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim() ?? "";
        }

        private static DateTime ReadDate(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (дд.мм.гггг): ");
                if (DateTime.TryParseExact(Console.ReadLine()?.Trim(), "dd.MM.yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                    return d;
                Console.WriteLine("Неверный формат даты, повторите.");
            }
        }

        private static DocumentType ReadDocType()
        {
            while (true)
            {
                Console.WriteLine("Тип документа: 1 - Паспорт, 2 - Свидетельство о рождении, 3 - Загранпаспорт");
                Console.Write("Введите 1/2/3: ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": return DocumentType.Passport;
                    case "2": return DocumentType.BirthCertificate;
                    case "3": return DocumentType.ForeignPassport;
                }
                Console.WriteLine("Неверный ввод.");
            }
        }

        private static Person ReadPerson()
        {
            var p = new Person
            {
                LastName = ReadString("Фамилия: "),
                FirstName = ReadString("Имя: "),
                MiddleName = ReadString("Отчество: "),
                BirthDate = ReadDate("Дата рождения"),
                BirthPlace = ReadString("Место рождения: ")
            };
            Console.WriteLine("--- Адрес ---");
            p.HomeAddress = new Address
            {
                Settlement = ReadString("Населённый пункт: "),
                Street = ReadString("Улица: "),
                Building = ReadString("Дом: "),
                Block = ReadString("Корпус (пусто, если нет): ")
            };
            Console.WriteLine("--- Документ ---");
            p.Document = new Document
            {
                Type = ReadDocType(),
                Series = ReadString("Серия: "),
                Number = ReadString("Номер: "),
                IssueDate = ReadDate("Дата выдачи")
            };
            return p;
        }

        //CRUD
        private static void AddPerson()
        {
            Repo.Add(ReadPerson());
            Console.WriteLine("Запись добавлена.");
        }

        private static void DeletePerson()
        {
            ShowPersons(Repo.GetAll());
            Console.Write("Номер записи для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int n) && Repo.RemoveAt(n - 1))
                Console.WriteLine("Удалено.");
            else
                Console.WriteLine("Неверный номер.");
        }

        private static void UpdatePerson()
        {
            ShowPersons(Repo.GetAll());
            Console.Write("Номер записи для изменения: ");
            if (!int.TryParse(Console.ReadLine(), out int n))
            {
                Console.WriteLine("Неверный номер.");
                return;
            }
            Console.WriteLine("Введите новые данные:");
            var p = ReadPerson();
            Console.WriteLine(Repo.Update(n - 1, p) ? "Изменено." : "Неверный номер.");
        }

        //Сортировка
        private static void SortMenu()
        {
            Console.WriteLine("Сортировать по:");
            Console.WriteLine("1. Фамилии (возр.)");
            Console.WriteLine("2. Фамилии (убыв.)");
            Console.WriteLine("3. Дате рождения (возр.)");
            Console.WriteLine("4. Дате рождения (убыв.)");
            Console.WriteLine("5. Городу");
            Console.WriteLine("6. Типу документа");
            Console.Write("Выбор: ");

            List<Person> sorted;
            switch (Console.ReadLine()?.Trim())
            {
                case "1": sorted = Repo.SortByLastName(false); break;
                case "2": sorted = Repo.SortByLastName(true); break;
                case "3": sorted = Repo.SortByBirthDate(false); break;
                case "4": sorted = Repo.SortByBirthDate(true); break;
                case "5": sorted = Repo.SortByCity(); break;
                case "6": sorted = Repo.SortByDocumentType(); break;
                default: Console.WriteLine("Неверный пункт."); return;
            }
            ShowPersons(sorted);
        }

        //Фильтрация
        private static void FilterMenu()
        {
            Console.WriteLine("Фильтр по:");
            Console.WriteLine("1. Типу документа");
            Console.WriteLine("2. Городу");
            Console.WriteLine("3. Фамилии (содержит)");
            Console.WriteLine("4. Возрасту (диапазон)");
            Console.WriteLine("5. Году рождения");
            Console.Write("Выбор: ");

            List<Person> result;
            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    result = Repo.FilterByDocumentType(ReadDocType());
                    break;
                case "2":
                    result = Repo.FilterByCity(ReadString("Часть названия города: "));
                    break;
                case "3":
                    result = Repo.FilterByLastName(ReadString("Часть фамилии: "));
                    break;
                case "4":
                    Console.Write("Мин. возраст: ");
                    int.TryParse(Console.ReadLine(), out int min);
                    Console.Write("Макс. возраст: ");
                    int.TryParse(Console.ReadLine(), out int max);
                    result = Repo.FilterByAgeRange(min, max);
                    break;
                case "5":
                    Console.Write("Год: ");
                    int.TryParse(Console.ReadLine(), out int y);
                    result = Repo.FilterByBirthYear(y);
                    break;
                default:
                    Console.WriteLine("Неверный пункт.");
                    return;
            }
            ShowPersons(result);
        }

        //XML / CSV
        private static void SaveXml()
        {
            Repo.SaveToXml(XmlPath);
            Console.WriteLine($"Сохранено в {XmlPath}");
        }

        private static void LoadXml()
        {
            Repo.LoadFromXml(XmlPath);
            Console.WriteLine($"Загружено из {XmlPath}. Записей: {Repo.GetAll().Count}");
        }

        private static void LoadCsv()
        {
            var path = ReadString($"Путь к CSV (Enter — {CsvPath}): ");
            if (string.IsNullOrWhiteSpace(path)) path = CsvPath;
            int n = Repo.LoadFromCsv(path);
            Console.WriteLine($"Добавлено записей: {n}");
        }
    }
}