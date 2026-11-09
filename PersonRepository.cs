using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace PersonLab
{
    public class PersonRepository
    {
        private List<Person> _persons = new List<Person>();

        private static readonly XmlSerializer Serializer =
            new XmlSerializer(typeof(List<Person>), new XmlRootAttribute("Persons"));

        public IReadOnlyList<Person> GetAll() => _persons;

        //CRUD
        public void Add(Person p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            _persons.Add(p);
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= _persons.Count) return false;
            _persons.RemoveAt(index);
            return true;
        }

        public bool Update(int index, Person newPerson)
        {
            if (index < 0 || index >= _persons.Count || newPerson == null) return false;
            _persons[index] = newPerson;
            return true;
        }

        //Фильтрация (LINQ) 
        public List<Person> Filter(Func<Person, bool> predicate) =>
            _persons.Where(predicate).ToList();

        public List<Person> FilterByDocumentType(DocumentType type) =>
            _persons.Where(p => p.Document != null && p.Document.Type == type).ToList();

        public List<Person> FilterByCity(string city) =>
            _persons.Where(p => p.HomeAddress != null &&
                                p.HomeAddress.Settlement.IndexOf(city,
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

        public List<Person> FilterByLastName(string part) =>
            _persons.Where(p => p.LastName.IndexOf(part,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

        public List<Person> FilterByAgeRange(int minAge, int maxAge) =>
            _persons.Where(p => p.Age >= minAge && p.Age <= maxAge).ToList();

        public List<Person> FilterByBirthYear(int year) =>
            _persons.Where(p => p.BirthDate.Year == year).ToList();

        //Сортировка (LINQ)
        public List<Person> SortByLastName(bool desc = false) =>
            desc ? _persons.OrderByDescending(p => p.LastName).ThenBy(p => p.FirstName).ToList()
                 : _persons.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();

        public List<Person> SortByBirthDate(bool desc = false) =>
            desc ? _persons.OrderByDescending(p => p.BirthDate).ToList()
                 : _persons.OrderBy(p => p.BirthDate).ToList();

        public List<Person> SortByCity(bool desc = false) =>
            desc ? _persons.OrderByDescending(p => p.HomeAddress?.Settlement).ToList()
                 : _persons.OrderBy(p => p.HomeAddress?.Settlement).ToList();

        public List<Person> SortByDocumentType(bool desc = false) =>
            desc ? _persons.OrderByDescending(p => p.Document?.Type).ToList()
                 : _persons.OrderBy(p => p.Document?.Type).ToList();

        //XML
        public void SaveToXml(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
                Serializer.Serialize(fs, _persons);
        }

        public void LoadFromXml(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("XML-файл не найден", path);

            using (var fs = new FileStream(path, FileMode.Open))
            {
                var loaded = (List<Person>)Serializer.Deserialize(fs);
                _persons = loaded ?? new List<Person>();
            }
        }

        //Загрузка из текстового (CSV) файла
        public int LoadFromCsv(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("CSV-файл не найден", path);

            int added = 0;
            foreach (var raw in File.ReadAllLines(path, Encoding.UTF8))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;

                var p = line.Split(';');
                if (p.Length < 13) continue; // заголовок / битая строка

                if (!DateTime.TryParseExact(p[3].Trim(), "dd.MM.yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var birth))
                    continue;
                if (!DateTime.TryParseExact(p[12].Trim(), "dd.MM.yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var issue))
                    continue;
                if (!int.TryParse(p[9].Trim(), out int dt)) continue;
                if (dt < 1 || dt > 3) continue;

                _persons.Add(new Person
                {
                    LastName = p[0].Trim(),
                    FirstName = p[1].Trim(),
                    MiddleName = p[2].Trim(),
                    BirthDate = birth,
                    BirthPlace = p[4].Trim(),
                    HomeAddress = new Address
                    {
                        Settlement = p[5].Trim(),
                        Street = p[6].Trim(),
                        Building = p[7].Trim(),
                        Block = p[8].Trim()
                    },
                    Document = new Document
                    {
                        Type = (DocumentType)dt,
                        Series = p[10].Trim(),
                        Number = p[11].Trim(),
                        IssueDate = issue
                    }
                });
                added++;
            }
            return added;
        }
    }
}