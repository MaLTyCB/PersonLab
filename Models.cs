using System;
using System.Xml.Serialization;

namespace PersonLab
{
    //Тип документа
    public enum DocumentType
    {
        [XmlEnum("1")] Passport = 1,
        [XmlEnum("2")] BirthCertificate = 2,
        [XmlEnum("3")] ForeignPassport = 3
    }

    public static class DocumentTypeHelper
    {
        public static string ToRussian(this DocumentType t)
        {
            switch (t)
            {
                case DocumentType.Passport: return "Паспорт";
                case DocumentType.BirthCertificate: return "Свидетельство о рождении";
                case DocumentType.ForeignPassport: return "Загранпаспорт";
                default: return t.ToString();
            }
        }
    }

    //Документ, удостоверяющий личность
    public class Document
    {
        public DocumentType Type { get; set; }
        public string Series { get; set; } = "";
        public string Number { get; set; } = "";
        public DateTime IssueDate { get; set; }

        public override string ToString() =>
            $"{Type.ToRussian()} {Series} {Number}, выдан {IssueDate:dd.MM.yyyy}";
    }

    //Адрес
    public class Address
    {
        public string Settlement { get; set; } = ""; // населённый пункт
        public string Street { get; set; } = "";
        public string Building { get; set; } = ""; // дом
        public string Block { get; set; } = ""; // корпус (может отсутствовать)

        public override string ToString()
        {
            string block = string.IsNullOrWhiteSpace(Block) ? "" : $", корп. {Block}";
            return $"{Settlement}, ул. {Street}, д. {Building}{block}";
        }
    }

    //Человек
    public class Person
    {
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; } = "";
        public Address HomeAddress { get; set; } = new Address();
        public Document Document { get; set; } = new Document();

        [XmlIgnore] public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        [XmlIgnore]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                int age = today.Year - BirthDate.Year;
                if (BirthDate > today.AddYears(-age)) age--;
                return age;
            }
        }

        public override string ToString() =>
            $"{FullName}, {BirthDate:dd.MM.yyyy} ({Age} лет)\n" +
            $"   Место рождения: {BirthPlace}\n" +
            $"   Адрес: {HomeAddress}\n" +
            $"   Документ: {Document}";
    }
}