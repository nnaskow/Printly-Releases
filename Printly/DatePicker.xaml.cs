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
    /// Interaction logic for DatePicker.xaml
    /// </summary>
    public partial class DatePicker : Window
    {
        public DateTime? SelectedDate { get; private set; }
        private Order _order;

        public DatePicker(Order order)
        {
            InitializeComponent();
            _order = order;
            MarkDT.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.TryParse(MarkDT.Text, out var date))
            {
                SelectedDate = date.Date
                    .AddHours(DateTime.Now.Hour)
                    .AddMinutes(DateTime.Now.Minute)
                    .AddSeconds(DateTime.Now.Second);

                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Моля, въведете валидна дата.");
            }
        }
    }
}
