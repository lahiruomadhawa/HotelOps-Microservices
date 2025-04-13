using HotelServices.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Registry.ConsoleUI
{
    public class MenuDisplay
    {
        public static void DisplayMainMenu()
        {
            Console.WriteLine("\nREGISTRY MAIN MENU");
            Console.WriteLine("1. Create New Order");
            Console.WriteLine("2. View Order Status");
            Console.WriteLine("3. View Menu Items");
            Console.WriteLine("4. Exit");
            Console.Write("Select an option: ");
        }

        public static void DisplayMenuItems(List<MenuItem> menuItems)
        {
            Console.WriteLine("\nMENU ITEMS");
            Console.WriteLine("ID | Name | Price | Category");

            foreach (var item in menuItems)
            {
                Console.WriteLine($"{item.Id} | {item.Name} | ${item.Price:F2} | {item.Category}");
            }
        }

        public static void DisplayOrderSummary(Order order)
        {
            Console.WriteLine("\nORDER SUMMARY");
            Console.WriteLine($"Order ID: {order.Id}");
            Console.WriteLine($"Room: {order.RoomNumber}");
            Console.WriteLine($"Guest: {order.GuestName}");
            Console.WriteLine("Items:");

            foreach (var item in order.Items)
            {
                Console.WriteLine($" - {item.Quantity}x {item.Name} (${item.Price:F2}): ${item.Price * item.Quantity:F2}");
                if (!string.IsNullOrWhiteSpace(item.SpecialInstructions))
                {
                    Console.WriteLine($"   Special: {item.SpecialInstructions}");
                }
            }

            Console.WriteLine($"Total: ${order.TotalAmount:F2}");
        }

        public static void DisplayOrderDetails(Order order)
        {
            Console.WriteLine("\nORDER DETAILS");
            Console.WriteLine($"ID: {order.Id}");
            Console.WriteLine($"Room: {order.RoomNumber}");
            Console.WriteLine($"Guest: {order.GuestName}");
            Console.WriteLine($"Amount: ${order.TotalAmount:F2}");
            Console.WriteLine($"Time: {order.OrderTime}");
            Console.WriteLine($"Status: {order.Status}");

            if (order.Items != null && order.Items.Count > 0)
            {
                Console.WriteLine("\nORDER ITEMS");
                foreach (var item in order.Items)
                {
                    Console.WriteLine($"{item.Quantity}x {item.Name} - ${item.Price:F2} each");

                    if (!string.IsNullOrWhiteSpace(item.SpecialInstructions))
                    {
                        Console.WriteLine($"  Special: {item.SpecialInstructions}");
                    }
                }
            }
        }

        public static void DisplayRecentOrders(List<Order> orders)
        {
            Console.WriteLine("\nRECENT ORDERS");
            Console.WriteLine("ID | Room | Guest | Amount | Time | Status");

            foreach (var order in orders)
            {
                Console.WriteLine($"{order.Id} | {order.RoomNumber} | {order.GuestName} | ${order.TotalAmount:F2} | {order.OrderTime} | {order.Status}");
            }
        }
    }
}
