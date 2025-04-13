using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HotelServices.Core.Interfaces;
using HotelServices.Core.Services;
using HotelServices.Infrastructure.Configuration;
using HotelServices.Infrastructure.Database;
using HotelServices.Infrastructure.Messaging;
using HotelServices.Registry.ConsoleUI;

namespace HotelServices.Registry
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Registry/Cashier Service");
            Console.WriteLine("----------------------");

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Create service collection
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Initialize database
            await InitializeDatabaseAsync(serviceProvider);

            // Run the application
            await RunApplicationAsync(serviceProvider);
        }

        static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            // Register configuration
            services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"));

            var connectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();

            // Register services
            services.AddSingleton<DatabaseInitializer>(provider =>
                new DatabaseInitializer(connectionStrings.PostgreSQL));

            services.AddSingleton<IMenuRepository>(provider =>
                new MenuRepository(connectionStrings.PostgreSQL));

            services.AddSingleton<IOrderRepository>(provider =>
                new OrderRepository(connectionStrings.PostgreSQL));

            services.AddSingleton<IMessageBroker>(provider =>
                new RabbitMQClient(connectionStrings.RabbitMQ ?? "localhost"));

            services.AddSingleton<IOrderService, OrderService>();

            services.AddTransient<OrderCreation>();
        }

        static async Task InitializeDatabaseAsync(ServiceProvider serviceProvider)
        {
            var dbInitializer = serviceProvider.GetService<DatabaseInitializer>();
            await dbInitializer.InitializeAsync();
        }

        static async Task RunApplicationAsync(ServiceProvider serviceProvider)
        {
            var menuRepository = serviceProvider.GetService<IMenuRepository>();
            var orderService = serviceProvider.GetService<IOrderService>();
            var orderCreation = serviceProvider.GetService<OrderCreation>();

            bool exit = false;

            while (!exit)
            {
                MenuDisplay.DisplayMainMenu();

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        await orderCreation.CreateNewOrderAsync();
                        break;
                    case "2":
                        await ViewOrderStatusAsync(orderService);
                        break;
                    case "3":
                        var menuItems = await menuRepository.GetAllAvailableMenuItemsAsync();
                        MenuDisplay.DisplayMenuItems(menuItems);
                        break;
                    case "4":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }

            // Dispose services
            ((IDisposable)serviceProvider)?.Dispose();
        }

        static async Task ViewOrderStatusAsync(IOrderService orderService)
        {
            Console.WriteLine("\nVIEW ORDER STATUS");
            Console.Write("Enter order ID (leave empty to list recent orders): ");
            string orderIdInput = Console.ReadLine();

            try
            {
                if (string.IsNullOrWhiteSpace(orderIdInput))
                {
                    // List recent orders
                    var recentOrders = await orderService.GetRecentOrdersAsync();

                    if (recentOrders.Count == 0)
                    {
                        Console.WriteLine("No orders found.");
                        return;
                    }

                    MenuDisplay.DisplayRecentOrders(recentOrders);
                }
                else
                {
                    // View specific order
                    if (!Guid.TryParse(orderIdInput, out Guid orderId))
                    {
                        Console.WriteLine("Invalid order ID format.");
                        return;
                    }

                    var order = await orderService.GetOrderByIdAsync(orderId);

                    if (order == null)
                    {
                        Console.WriteLine("Order not found.");
                        return;
                    }

                    MenuDisplay.DisplayOrderDetails(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
