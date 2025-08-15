using System;
using System.Collections.Generic;
using System.IO;

public class Student
{
    public int Id { get; }
    public string FullName { get; }
    public int Score { get; }

    public Student(int id, string fullName, int score)
    {
        Id = id;
        FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
        Score = score;
    }

    public string GetGrade()
    {
        return Score switch
        {
            >= 80 and <= 100 => "A",
            >= 70 and <= 79 => "B",
            >= 60 and <= 69 => "C",
            >= 50 and <= 59 => "D",
            < 50 and >= 0 => "F",
            _ => throw new ArgumentOutOfRangeException(nameof(Score), "Score must be between 0 and 100")
        };
    }
}

public class InvalidScoreFormatException : Exception
{
    public InvalidScoreFormatException(string message) : base(message) { }
}

public class MissingFieldException : Exception
{
    public MissingFieldException(string message) : base(message) { }
}

public class StudentResultProcessor
{
    public List<Student> ReadStudentsFromFile(string inputFilePath)
    {
        var students = new List<Student>();

        using (var reader = new StreamReader(inputFilePath))
        {
            string? line;
            int lineNumber = 0;

            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts.Length != 3)
                {
                    throw new MissingFieldException(
                        $"Line {lineNumber}: Expected 3 fields but found {parts.Length}.");
                }

                if (!int.TryParse(parts[0].Trim(), out int id))
                {
                    throw new InvalidScoreFormatException(
                        $"Line {lineNumber}: Invalid student ID format '{parts[0].Trim()}'.");
                }

                string fullName = parts[1].Trim();
                if (string.IsNullOrEmpty(fullName))
                {
                    throw new MissingFieldException(
                        $"Line {lineNumber}: Full name field is missing or empty.");
                }

                if (!int.TryParse(parts[2].Trim(), out int score))
                {
                    throw new InvalidScoreFormatException(
                        $"Line {lineNumber}: Unable to convert score '{parts[2].Trim()}' to an integer.");
                }

                if (score < 0 || score > 100)
                {
                    throw new InvalidScoreFormatException(
                        $"Line {lineNumber}: Score {score} is outside the valid range 0-100.");
                }

                var student = new Student(id, fullName, score);
                students.Add(student);
            }
        }

        return students;
    }

    public void WriteReportToFile(List<Student> students, string outputFilePath)
    {
        using (var writer = new StreamWriter(outputFilePath))
        {
            foreach (var student in students)
            {
                string line = $"{student.FullName} (ID: {student.Id}): Score = {student.Score}, Grade = {student.GetGrade()}";
                writer.WriteLine(line);
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        const string inputFilePath = "students_input.txt";
        const string outputFilePath = "students_report.txt";

        var processor = new StudentResultProcessor();

        try
        {
            var students = processor.ReadStudentsFromFile(inputFilePath);
            processor.WriteReportToFile(students, outputFilePath);
            Console.WriteLine($"Report written successfully to '{outputFilePath}'.");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File error: {ex.Message}");
        }
        catch (InvalidScoreFormatException ex)
        {
            Console.WriteLine($"Score format error: {ex.Message}");
        }
        catch (MissingFieldException ex)
        {
            Console.WriteLine($"Data error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
