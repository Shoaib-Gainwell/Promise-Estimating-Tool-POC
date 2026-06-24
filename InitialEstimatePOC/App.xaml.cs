using System.Windows;
using System.IO;
using InitialEstimatePOC.Data;

namespace InitialEstimatePOC;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, args) =>
        {
            File.WriteAllText("crash.log", args.Exception.ToString());
            MessageBox.Show(args.Exception.Message, "Startup Error");
            args.Handled = true;
        };

        // Initialize SQLite database and seed weighted values
        using var db = new EstimateDbContext();
        InitialEstimatePOC.Data.DatabaseSeeder.Initialize(db);
        WeightedValues.LoadFromDatabase(db);

        // Seed demo projects for presentation
        DemoDataSeeder.SeedDemoProjects(db);
    }
}
