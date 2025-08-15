using System;
using System.Collections.Generic;
using System.Linq;

public class Repository<T>
{
    private readonly List<T> _items = new();

    public void Add(T item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        _items.Add(item);
    }

    public List<T> GetAll()
    {
        return new List<T>(_items);
    }

    public T? GetById(Func<T, bool> predicate) where T : class
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return _items.FirstOrDefault(predicate);
    }

    public bool Remove(Func<T, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var toRemove = _items.Where(predicate).ToList();

        if (!toRemove.Any())
            return false;

        foreach (var item in toRemove)
        {
            _items.Remove(item);
        }

        return true;
    }
}

public class Patient
{
    public int Id { get; }
    public string Name { get; }
    public int Age { get; }
    public string Gender { get; }

    public Patient(int id, string name, int age, string gender)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));
        if (string.IsNullOrWhiteSpace(gender)) throw new ArgumentException("Gender cannot be empty", nameof(gender));
        if (age < 0) throw new ArgumentOutOfRangeException(nameof(age), "Age cannot be negative");

        Id = id;
        Name = name;
        Age = age;
        Gender = gender;
    }

    public override string ToString()
    {
        return $"Patient Id: {Id}, Name: {Name}, Age: {Age}, Gender: {Gender}";
    }
}

public class Prescription
{
    public int Id { get; }
    public int PatientId { get; }
    public string MedicationName { get; }
    public DateTime DateIssued { get; }

    public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
    {
        if (string.IsNullOrWhiteSpace(medicationName)) throw new ArgumentException("Medication name cannot be empty", nameof(medicationName));

        Id = id;
        PatientId = patientId;
        MedicationName = medicationName;
        DateIssued = dateIssued;
    }

    public override string ToString()
    {
        return $"Prescription Id: {Id}, Medication: {MedicationName}, Date Issued: {DateIssued:d}";
    }
}

public class HealthSystemApp
{
    private readonly Repository<Patient> _patientRepo = new();
    private readonly Repository<Prescription> _prescriptionRepo = new();

    private Dictionary<int, List<Prescription>> _prescriptionMap = new();

    public void SeedData()
    {
        _patientRepo.Add(new Patient(1, "Alice Smith", 29, "Female"));
        _patientRepo.Add(new Patient(2, "Bob Johnson", 47, "Male"));
        _patientRepo.Add(new Patient(3, "Carol Williams", 35, "Female"));

        _prescriptionRepo.Add(new Prescription(1, 1, "Amoxicillin", DateTime.Today.AddDays(-10)));
        _prescriptionRepo.Add(new Prescription(2, 1, "Ibuprofen", DateTime.Today.AddDays(-5)));
        _prescriptionRepo.Add(new Prescription(3, 2, "Atorvastatin", DateTime.Today.AddDays(-2)));
        _prescriptionRepo.Add(new Prescription(4, 3, "Metformin", DateTime.Today.AddDays(-7)));
        _prescriptionRepo.Add(new Prescription(5, 3, "Lisinopril", DateTime.Today.AddDays(-1)));
    }

    public void BuildPrescriptionMap()
    {
        var prescriptions = _prescriptionRepo.GetAll();

        _prescriptionMap = prescriptions
            .GroupBy(p => p.PatientId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public List<Prescription> GetPrescriptionsByPatientId(int patientId)
    {
        return _prescriptionMap.TryGetValue(patientId, out var prescriptions) ? prescriptions : new List<Prescription>();
    }

    public void PrintAllPatients()
    {
        Console.WriteLine("All Patients:");
        foreach (var patient in _patientRepo.GetAll())
        {
            Console.WriteLine(patient);
        }
        Console.WriteLine();
    }

    public void PrintPrescriptionsForPatient(int patientId)
    {
        var patient = _patientRepo.GetById(p => p.Id == patientId);
        if (patient == null)
        {
            Console.WriteLine($"No patient found with Id = {patientId}");
            return;
        }

        var prescriptions = GetPrescriptionsByPatientId(patientId);
        Console.WriteLine($"Prescriptions for {patient.Name}:");

        if (!prescriptions.Any())
        {
            Console.WriteLine("  No prescriptions found.");
            return;
        }

        foreach (var prescription in prescriptions)
        {
            Console.WriteLine($"  {prescription}");
        }
        Console.WriteLine();
    }
}

public class Program
{
    public static void Main()
    {
        var app = new HealthSystemApp();

        app.SeedData();
        app.BuildPrescriptionMap();

        app.PrintAllPatients();

        int patientIdToShow = 1;
        app.PrintPrescriptionsForPatient(patientIdToShow);
    }
}
