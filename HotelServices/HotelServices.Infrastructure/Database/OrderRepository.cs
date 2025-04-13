using HotelServices.Core.Interfaces;
using HotelServices.Core.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelServices.Infrastructure.Database
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> SaveOrderAsync(Order order)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            // Insert order
                            using (var command = new NpgsqlCommand())
                            {
                                command.Connection = connection;
                                command.Transaction = transaction;

                                command.CommandText = @"
                                    INSERT INTO orders (id, room_number, guest_name, total_amount, order_time, status)
                                    VALUES (@id, @roomNumber, @guestName, @totalAmount, @orderTime, @status)";

                                command.Parameters.AddWithValue("id", order.Id);
                                command.Parameters.AddWithValue("roomNumber", order.RoomNumber);
                                command.Parameters.AddWithValue("guestName", order.GuestName);
                                command.Parameters.AddWithValue("totalAmount", order.TotalAmount);
                                command.Parameters.AddWithValue("orderTime", order.OrderTime);
                                command.Parameters.AddWithValue("status", order.Status);

                                await command.ExecuteNonQueryAsync();
                            }

                            // Insert order items
                            foreach (var item in order.Items)
                            {
                                using (var command = new NpgsqlCommand())
                                {
                                    command.Connection = connection;
                                    command.Transaction = transaction;

                                    command.CommandText = @"
                                        INSERT INTO order_items (order_id, menu_item_id, name, quantity, price, special_instructions)
                                        VALUES (@orderId, @menuItemId, @name, @quantity, @price, @specialInstructions)";

                                    command.Parameters.AddWithValue("orderId", order.Id);
                                    command.Parameters.AddWithValue("menuItemId", item.MenuItemId);
                                    command.Parameters.AddWithValue("name", item.Name);
                                    command.Parameters.AddWithValue("quantity", item.Quantity);
                                    command.Parameters.AddWithValue("price", item.Price);
                                    command.Parameters.AddWithValue("specialInstructions",
                                        string.IsNullOrEmpty(item.SpecialInstructions) ? DBNull.Value : (object)item.SpecialInstructions);

                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            await transaction.CommitAsync();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            Console.WriteLine($"Error saving order: {ex.Message}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                return false;
            }
        }

        public async Task<Order> GetOrderByIdAsync(Guid id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Get order
                    Order order = null;

                    using (var command = new NpgsqlCommand("SELECT * FROM orders WHERE id = @orderId", connection))
                    {
                        command.Parameters.AddWithValue("orderId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                order = new Order
                                {
                                    Id = reader.GetGuid(0),
                                    RoomNumber = reader.GetInt32(1),
                                    GuestName = reader.GetString(2),
                                    TotalAmount = reader.GetDecimal(3),
                                    OrderTime = reader.GetDateTime(4),
                                    Status = reader.GetString(5)
                                };
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }

                    // Get order items
                    using (var command = new NpgsqlCommand("SELECT * FROM order_items WHERE order_id = @orderId", connection))
                    {
                        command.Parameters.AddWithValue("orderId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                order.Items.Add(new OrderItem
                                {
                                    MenuItemId = reader.GetInt32(2),
                                    Name = reader.GetString(3),
                                    Quantity = reader.GetInt32(4),
                                    Price = reader.GetDecimal(5),
                                    SpecialInstructions = reader.IsDBNull(6) ? null : reader.GetString(6)
                                });
                            }
                        }
                    }

                    return order;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving order: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
        {
            var orders = new List<Order>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand($"SELECT * FROM orders ORDER BY order_time DESC LIMIT {count}", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                orders.Add(new Order
                                {
                                    Id = reader.GetGuid(0),
                                    RoomNumber = reader.GetInt32(1),
                                    GuestName = reader.GetString(2),
                                    TotalAmount = reader.GetDecimal(3),
                                    OrderTime = reader.GetDateTime(4),
                                    Status = reader.GetString(5)
                                });
                            }
                        }
                    }

                    // We're not loading order items for all orders in the list view
                    // for performance reasons

                    return orders;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving recent orders: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, string status)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand("UPDATE orders SET status = @status WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.Parameters.AddWithValue("status", status);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                throw;
            }
        }
    }
}
