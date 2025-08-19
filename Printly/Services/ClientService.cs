using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Printly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printly.Services
{
    internal class ClientService
    {
        private readonly PrintlyDBContext _context;

        public ClientService(PrintlyDBContext context)
        {
            _context = context;
        }
        public void AddClient(string clientName,string clientPhone)
        {
            var client = new Client
            {
               Name = clientName,
               Phone = clientPhone
            };
            
            using (var _context = new PrintlyDBContext())
            {
               _context.Clients.Add(client);
               _context.SaveChanges();
            }
        }
        public async Task<int> AddOrGetClientIdAsync(string name, string phone)
        {
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Phone == phone);

            if (existingClient != null)
            {
                return existingClient.ClientId;
            }

            var newClient = new Client
            {
                Name = name,
                Phone = phone
            };

            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();

            return newClient.ClientId;
        }
        public int GetClientID()
        {
            int lastId = 0;
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PrintlyDB;Integrated Security=True;TrustServerCertificate=True";

            string sql = "SELECT MAX(ClientId) FROM Clients"; 

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        lastId = Convert.ToInt32(result);
                    }
                }
            }
            return lastId;
        }
        
    }
}
