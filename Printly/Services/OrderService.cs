using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Printly.Models;
using Printly.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Printly.Services
{
    public class OrderService
    {
        private readonly string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PrintlyDB;Integrated Security=True;TrustServerCertificate=True";
        private readonly PrintlyDBContext _context;

        public OrderService(PrintlyDBContext context)
        {
            _context = context;
        }
        public void AddOrder(int clientID,string article,string defect,decimal? price, DateTime date,bool isPrinted)
        {
            var order = new Order
            {
                ClientId=clientID,
                Article=article,
                Defect=defect,
                Price=price,
                DateReceived=date,
                IsPrinted = isPrinted
            };
            using (var _context = new PrintlyDBContext())
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
            }

        }
        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();

            string sql = "SELECT * FROM Orders";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order();                      
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        public void DeleteOrder(int orderID)
        {
            using (var context = new PrintlyDBContext())
            {
                var order = context.Orders.FirstOrDefault(p => p.OrderId == orderID);
                if (order != null)
                {
                    context.Orders.Remove(order);
                    context.SaveChanges();
                }
            }
        }

        public void EditOrder(int orderID,string newArticle, string newDefect, decimal? newPrice,DateTime newDate )
        {
            using (var context = new PrintlyDBContext())
            {
                var order = context.Orders.FirstOrDefault(p => p.OrderId == orderID);

                if (order != null)
                {
                    order.OrderId = orderID;
                    order.Article = newArticle;
                    order.Defect = newDefect;
                    order.Price = newPrice;
                    order.DateReceived = newDate;

                    context.SaveChanges();
                }
            }
        }
    }

}