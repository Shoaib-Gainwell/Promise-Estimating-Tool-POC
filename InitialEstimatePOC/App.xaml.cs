using System.Windows;
using System.IO;
using System.Globalization;
using System.Threading;
using InitialEstimatePOC.Data;

namespace InitialEstimatePOC;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Force US date/number formatting app-wide (MM/dd/yyyy)
        var usCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = usCulture;
        Thread.CurrentThread.CurrentUICulture = usCulture;
        CultureInfo.DefaultThreadCurrentCulture = usCulture;
        CultureInfo.DefaultThreadCurrentUICulture = usCulture;

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

        // Initialize detailed estimate weighted values
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        // Seed demo projects for presentation
        DemoDataSeeder.SeedDemoProjects(db);
    }
}
