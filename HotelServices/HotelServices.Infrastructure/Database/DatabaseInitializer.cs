using System;
using System.Threading.Tasks;
using Npgsql;

namespace HotelServices.Infrastructure.Database
{
    public class DatabaseInitializer
    {
        private readonly string _connectionString;

        public DatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("Initializing database connection...");

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Create tables if they don't exist
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;

                        // Create menu_items table
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS menu_items (
                                id SERIAL PRIMARY KEY,
                                name VARCHAR(100) NOT NULL,
                                price DECIMAL(10,2) NOT NULL,
                                category VARCHAR(50) NOT NULL,
                                available BOOLEAN DEFAULT TRUE
                            )";
                        await command.ExecuteNonQueryAsync();

                        // Create orders table
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS orders (
                                id UUID PRIMARY KEY,
                                room_number INTEGER NOT NULL,
                                guest_name VARCHAR(100) NOT NULL,
                                total_amount DECIMAL(10,2) NOT NULL,
                                order_time TIMESTAMP NOT NULL,
                                status VARCHAR(20) NOT NULL
                            )";
                        await command.ExecuteNonQueryAsync();

                        // Create order_items table
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS order_items (
                                id SERIAL PRIMARY KEY,
                                order_id UUID REFERENCES orders(id),
                                menu_item_id INTEGER REFERENCES menu_items(id),
                                name VARCHAR(100) NOT NULL,
                                quantity INTEGER NOT NULL,
                                price DECIMAL(10,2) NOT NULL,
                                special_instructions VARCHAR(200)
                            )";
                        await command.ExecuteNonQueryAsync();

                        // Insert sample menu items if the table is empty
                        command.CommandText = "SELECT COUNT(*) FROM menu_items";
                        int count = Convert.ToInt32(await command.ExecuteScalarAsync());

                        if (count == 0)
                        {
                            command.CommandText = @"
                                INSERT INTO menu_items (name, price, category, available) VALUES
                                ('Burger', 12.99, 'Main', TRUE),
                                ('Pizza', 15.99, 'Main', TRUE),
                                ('Salad', 8.99, 'Appetizer', TRUE),
                                ('Pasta', 14.99, 'Main', TRUE),
                                ('Cheesecake', 7.99, 'Dessert', TRUE),
                                ('Soft Drink', 2.99, 'Beverage', TRUE),
                                ('Coffee', 3.99, 'Beverage', TRUE),
                                ('Ice Cream', 5.99, 'Dessert', TRUE)";
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                Console.WriteLine("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                throw;
            }
        }
    }
}
