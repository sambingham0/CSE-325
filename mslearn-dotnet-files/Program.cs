using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using System.Text;

var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");
var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);
var salesFiles = FindFiles(storesDirectory);
var salesTotal = CalculateSalesTotal(salesFiles);
File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");

// sales summary report
GenerateSalesSummaryReport(salesFiles, salesTotalDir);

IEnumerable<string> FindFiles(string folderName)
{
    List<string> salesFiles = new List<string>();

    var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);

    foreach (var file in foundFiles)
    {
        var extension = Path.GetExtension(file);
        var fileName = Path.GetFileName(file);
        if (extension == ".json" && fileName.Equals("sales.json", StringComparison.OrdinalIgnoreCase))
        {
            salesFiles.Add(file);
        }
    }

    return salesFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
    double salesTotal = 0;
    foreach (var file in salesFiles)
    {
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
        salesTotal += data?.Total ?? 0;
    }
    return salesTotal;
}

void GenerateSalesSummaryReport(IEnumerable<string> salesFiles, string outputDirectory)
{
    var report = new StringBuilder();
    double totalSales = 0;
    var salesDetails = new List<(string filename, double total)>();

    // data from each file
    foreach (var file in salesFiles)
    {
        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
        double fileTotal = data?.Total ?? 0;
        totalSales += fileTotal;
        string relativePath = Path.GetRelativePath(storesDirectory, file);
        salesDetails.Add((relativePath, fileTotal));
    }

    // report
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");
    report.AppendLine($" Total Sales: {totalSales.ToString("C")}");
    report.AppendLine();
    report.AppendLine(" Details:");

    foreach (var (filename, total) in salesDetails)
    {
        report.AppendLine($"  {filename}: {total.ToString("C")}");
    }

    // Write report to file
    string reportPath = Path.Combine(outputDirectory, "sales-summary.txt");
    File.WriteAllText(reportPath, report.ToString());
    Console.WriteLine($"Sales summary report generated: {reportPath}");
}

record SalesData (double Total);