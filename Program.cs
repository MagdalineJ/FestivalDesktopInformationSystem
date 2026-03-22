using System;
using FestivalDesktopInformationSystem.Controllers;
using FestivalDesktopInformationSystem.Helpers;
using FestivalDesktopInformationSystem.Repositories;
using FestivalDesktopInformationSystem.Services;
using FestivalDesktopInformationSystem.Views;

namespace FestivalDesktopInformationSystem
{
    // Application entry point.
    // This class wires the layers together and starts the controller.
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var databaseManager = new DatabaseManager();

                if (!databaseManager.TestConnection())
                {
                    Console.WriteLine("Database connection failed. Please check your MySQL settings.");
                    return;
                }

                var repository = new MySqlPersonRepository(databaseManager);
                var validator = new ValidationHelper();
                var csvExporter = new CsvExporter();
                var reportService = new ReportService();

                var festivalService = new FestivalService(
                    repository,
                    validator,
                    csvExporter,
                    reportService);

                IFestivalView view = new ConsoleView();
                var controller = new FestivalController(festivalService, view);

                controller.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }
    }
}