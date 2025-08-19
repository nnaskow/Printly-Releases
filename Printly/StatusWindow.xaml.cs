using Printly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Printly
{
    /// <summary>
    /// Interaction logic for StatusWindow.xaml
    /// </summary>
    public partial class StatusWindow : Window
    {
        private Order _order;

        public StatusWindow(Order order)
        {
            InitializeComponent();
            statusTxtBox.Focus();

            using (var context = new PrintlyDBContext())
            {
                _order = context.Orders.FirstOrDefault(o => o.OrderId == order.OrderId);
            }

            if (!string.IsNullOrEmpty(_order.Status))
            {
                currentStatus.Content = _order.Status;
                currentStatus.Foreground = Brushes.Green;
                dot.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/png/Basic_green_dot.png"));
            }
            else
            {
                currentStatus.Content = "Няма статус";
                currentStatus.Foreground = Brushes.Red;
                dot.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/png/Basic_red_dot.png"));
            }
        }

        private void changeStatus_Click(object sender, RoutedEventArgs e)
        {
            string newStatus = statusTxtBox.Text.Trim();

            if (!string.IsNullOrEmpty(newStatus))
            {
                using (var context = new PrintlyDBContext())
                {
                    var orderFromDb = context.Orders.FirstOrDefault(o => o.OrderId == _order.OrderId);
                    if (orderFromDb != null)
                    {
                        orderFromDb.Status = newStatus;
                        context.SaveChanges();
                    }
                }

                currentStatus.Content = newStatus;
                currentStatus.Foreground = Brushes.Green;
                dot.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/png/Basic_green_dot.png"));

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Моля въведете нов статус!", "Грешка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void statusTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                changeStatus_Click(sender, e);
            }
        }
    }
}
