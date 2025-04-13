using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelServices.Core.Interfaces;
using HotelServices.Core.Models;
using Npgsql;

namespace HotelServices.Infrastructure.Database
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;

        public MenuRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<MenuItem>> GetAllAvailableMenuItemsAsync()
        {
            var menuItems = new List<MenuItem>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand("SELECT * FROM menu_items WHERE available = TRUE", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                menuItems.Add(new MenuItem
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                    Category = reader.GetString(3),
                                    Available = reader.GetBoolean(4)
                                });
                            }
                        }
                    }
                }

                return menuItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading menu items: {ex.Message}");
                throw;
            }
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand("SELECT * FROM menu_items WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new MenuItem
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                    Category = reader.GetString(3),
                                    Available = reader.GetBoolean(4)
                                };
                            }

                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving menu item: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateMenuItemAvailabilityAsync(int id, bool available)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand("UPDATE menu_items SET available = @available WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("id", id);
                        command.Parameters.AddWithValue("available", available);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating menu item: {ex.Message}");
                throw;
            }
        }
    }
}
