
using Microsoft.Data.SqlClient;
using System;

try {
    string connString = "Data Source=.\\SQLEXPRESS;Initial Catalog=TrainWiseDb;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;Connect Timeout=5";
    using var conn = new SqlConnection(connString);
    Console.WriteLine("Attempting to connect to SQL Server...");
    conn.Open();
    Console.WriteLine("Connection SUCCESSFUL!");
} catch (Exception ex) {
    Console.WriteLine("Connection FAILED!");
    Console.WriteLine(ex.Message);
}
