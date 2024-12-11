using Microsoft.Data.Sqlite;
using Dapper;

public class DatabaseInitializer
{
    private const string ConnectionString = "Data Source=clients.db";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        // Create table
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Clients (
                Name TEXT NOT NULL,
                Address TEXT NOT NULL,
                AccountNumber TEXT NOT NULL,
                Balance REAL NOT NULL,
                EmailAddress TEXT NOT NULL,
                PhoneNumber TEXT NOT NULL,
                LastTransactionDate TEXT NOT NULL
            )");

        // Clear existing data
        connection.Execute("DELETE FROM Clients");

        // Insert seed data
        var clients = new[]
        {
            new { Name = "John Doe", Address = "123 Maple St, Springfield", AccountNumber = "ACCT-1001", Balance = (double)2500.75m, EmailAddress = "john.doe@email.com", PhoneNumber = "555-0101", LastTransactionDate = "2024-12-09" },
            new { Name = "Jane Smith", Address = "456 Oak Ave, Metropolis", AccountNumber = "ACCT-1002", Balance = (double)10250.50m, EmailAddress = "jane.smith@email.com", PhoneNumber = "555-0102", LastTransactionDate = "2024-12-10" },
            new { Name = "Carlos Reyes", Address = "789 Pine Rd, Smalltown", AccountNumber = "ACCT-1003", Balance = (double)500.00m, EmailAddress = "carlos.r@email.com", PhoneNumber = "555-0103", LastTransactionDate = "2024-12-08" },
            new { Name = "Linda Carter", Address = "321 Birch Blvd, Capital City", AccountNumber = "ACCT-1004", Balance = (double)9875.99m, EmailAddress = "linda.c@email.com", PhoneNumber = "555-0104", LastTransactionDate = "2024-12-10" },
            new { Name = "Michael Chang", Address = "567 Cedar Ln, Riverside", AccountNumber = "ACCT-1005", Balance = (double)15750.25m, EmailAddress = "m.chang@email.com", PhoneNumber = "555-0105", LastTransactionDate = "2024-12-09" },
            new { Name = "Sarah Wilson", Address = "890 Elm St, Lakeside", AccountNumber = "ACCT-1006", Balance = (double)3200.00m, EmailAddress = "s.wilson@email.com", PhoneNumber = "555-0106", LastTransactionDate = "2024-12-07" },
            new { Name = "Robert Brown", Address = "432 Walnut Ave, Highland", AccountNumber = "ACCT-1007", Balance = (double)6700.50m, EmailAddress = "r.brown@email.com", PhoneNumber = "555-0107", LastTransactionDate = "2024-12-10" },
            new { Name = "Emily Davis", Address = "765 Spruce Dr, Valley View", AccountNumber = "ACCT-1008", Balance = (double)12400.75m, EmailAddress = "e.davis@email.com", PhoneNumber = "555-0108", LastTransactionDate = "2024-12-08" },
            new { Name = "David Martinez", Address = "234 Aspen Ct, Mountain City", AccountNumber = "ACCT-1009", Balance = (double)8900.25m, EmailAddress = "d.martinez@email.com", PhoneNumber = "555-0109", LastTransactionDate = "2024-12-09" },
            new { Name = "Lisa Anderson", Address = "876 Redwood Rd, Forest Hills", AccountNumber = "ACCT-1010", Balance = (double)4300.00m, EmailAddress = "l.anderson@email.com", PhoneNumber = "555-0110", LastTransactionDate = "2024-12-10" },
            new { Name = "James Wilson", Address = "543 Magnolia Blvd, Sunnydale", AccountNumber = "ACCT-1011", Balance = (double)7600.50m, EmailAddress = "j.wilson@email.com", PhoneNumber = "555-0111", LastTransactionDate = "2024-12-07" },
            new { Name = "Maria Garcia", Address = "789 Willow Way, Bayside", AccountNumber = "ACCT-1012", Balance = (double)5100.25m, EmailAddress = "m.garcia@email.com", PhoneNumber = "555-0112", LastTransactionDate = "2024-12-09" },
            new { Name = "Thomas Lee", Address = "321 Sycamore St, Eastwood", AccountNumber = "ACCT-1013", Balance = (double)9200.75m, EmailAddress = "t.lee@email.com", PhoneNumber = "555-0113", LastTransactionDate = "2024-12-08" },
            new { Name = "Patricia White", Address = "654 Juniper Ln, Westbrook", AccountNumber = "ACCT-1014", Balance = (double)11300.50m, EmailAddress = "p.white@email.com", PhoneNumber = "555-0114", LastTransactionDate = "2024-12-10" }
        };

        connection.Execute(@"
            INSERT INTO Clients (Name, Address, AccountNumber, Balance, EmailAddress, PhoneNumber, LastTransactionDate) 
            VALUES (@Name, @Address, @AccountNumber, @Balance, @EmailAddress, @PhoneNumber, @LastTransactionDate)", clients);
    }
}
