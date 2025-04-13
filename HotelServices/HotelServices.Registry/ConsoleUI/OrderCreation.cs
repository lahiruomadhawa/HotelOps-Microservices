using HotelServices.Core.Interfaces;
using HotelServices.Core.Models;
using HotelServices.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Registry.ConsoleUI
{
    public class OrderCreation
    {
        private readonly IOrderService _orderService;
        private readonly IMenuRepository _menuRepository;

        public OrderCreation(IOrderService orderService, IMenuRepository menuRepository)
        {
            _orderService = orderService;
            _menuRepository = menuRepository;
        }

        public async Task<Order> CreateNewOrderAsync()
        {
            Console.WriteLine("\nCREATE NEW ORDER");

            // Get room number
            Console.Write("Enter room number: ");
            if (!int.TryParse(Console.ReadLine(), out int roomNumber))
            {
                Console.WriteLine("Invalid room number.");
                return null;
            }

            // Get guest name
            Console.Write("Enter guest name: ");
            string guestName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(guestName))
            {
                Console.WriteLine("Guest name cannot be empty.");
                return null;
            }

            // Create new order
            var order = new Order
            {
                RoomNumber = roomNumber,
                GuestName = guestName
            };

            // Get menu items
            var menuItems = await _menuRepository.GetAllAvailableMenuItemsAsync();

            bool addingItems = true;

            while (addingItems)
            {
                // Display menu items
                MenuDisplay.DisplayMenuItems(menuItems);

                // Get menu item ID
                Console.Write("Enter menu item ID (0 to finish): ");
                if (!int.TryParse(Console.ReadLine(), out int menuItemId))
                {
                    Console.WriteLine("Invalid menu item ID.");
                    continue;
                }

                if (menuItemId == 0)
                {
                    addingItems = false;
                    continue;
                }

                // Find menu item
                var menuItem = menuItems.Find(m => m.Id == menuItemId);

                if (menuItem == null)
                {
                    Console.WriteLine("Menu item not found.");
                    continue;
                }

                // Get quantity
                Console.Write("Enter quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
                    continue;
                }

                // Get special instructions
                Console.Write("Enter special instructions (optional): ");
                string specialInstructions = Console.ReadLine();

                // Add order item
                order.Items.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    Quantity = quantity,
                    Price = menuItem.Price,
                    SpecialInstructions = specialInstructions
                });

                // Calculate total amount
                order.TotalAmount = 0;
                foreach (var item in order.Items)
                {
                    order.TotalAmount += item.Price * item.Quantity;
                }

                Console.WriteLine($"Item added. Current total: ${order.TotalAmount:F2}");
                Console.Write("Add another item? (y/n): ");

                if (Console.ReadLine().ToLower() != "y")
                {
                    addingItems = false;
                }
            }

            if (order.Items.Count == 0)
            {
                Console.WriteLine("Order cancelled. No items added.");
                return null;
            }

            // Display order summary
            MenuDisplay.DisplayOrderSummary(order);
            Console.Write("Confirm order? (y/n): ");

            if (Console.ReadLine().ToLower() == "y")
            {
                // Send order for verification
                await _orderService.CreateOrderAsync(order);

                Console.WriteLine($"Order {order.Id} has been sent to the kitchen for verification.");
                Console.WriteLine("Please wait...");

                // Here we would ideally have a proper way to wait for the response
                // For now, we'll just wait a few seconds for demo purposes
                Thread.Sleep(5000);

                // Get the updated order
                var updatedOrder = await _orderService.GetOrderByIdAsync(order.Id);

                if (updatedOrder != null)
                {
                    Console.WriteLine($"Order status: {updatedOrder.Status}");
                    return updatedOrder;
                }
                else
                {
                    Console.WriteLine("Order verification timed out or failed.");
                    return order;
                }
            }
            else
            {
                Console.WriteLine("Order cancelled.");
                return null;
            }
        }
    }
}
