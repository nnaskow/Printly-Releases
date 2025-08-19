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
    /// Interaction logic for EditPackaging.xaml
    /// </summary>
    public partial class EditPackaging : Window
    {
        private Order _currentOrder;

        public EditPackaging(Order order)
        {
            InitializeComponent();
            _currentOrder = order;
            otherTxtBox.IsEnabled = false;
            otherTxtBox.Opacity = 0.375;

            if (!string.IsNullOrWhiteSpace(order.Accessories))
            {
                var accessories = order.Accessories.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(a => a.Trim());

                List<string> others = new List<string>();

                foreach (var acc in accessories)
                {
                    switch (acc)
                    {
                        case "Калъф":
                            caseCheckBox.IsChecked = true;
                            break;
                        case "Зарядно":
                            chargerCheckBox.IsChecked = true;
                            break;
                        case "Чанта":
                            bagCheckBox.IsChecked = true;
                            break;
                        case "Адаптер":
                            adapterCheckBox.IsChecked = true;
                            break;
                        default:
                            others.Add(acc);
                            break;
                    }
                }

                if (others.Any())
                {
                    othersCheckBox.IsChecked = true;
                    otherTxtBox.Text = string.Join(", ", others);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<string>();

            if (caseCheckBox.IsChecked == true) list.Add("Калъф");
            if (chargerCheckBox.IsChecked == true) list.Add("Зарядно");
            if (bagCheckBox.IsChecked == true) list.Add("Чанта");
            if (adapterCheckBox.IsChecked == true) list.Add("Адаптер");

            if (othersCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(otherTxtBox.Text))
                list.Add(otherTxtBox.Text.Trim());

            _currentOrder.Accessories = string.Join(", ", list);

            using (var db = new PrintlyDBContext())
            {
                db.Orders.Update(_currentOrder);
                db.SaveChanges();
            }

            DialogResult = true;
            Close();
        }

        private void othersCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            otherTxtBox.Clear();
            otherTxtBox.IsEnabled = false;
            otherTxtBox.Opacity = 0.375;
        }

        private void othersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            otherTxtBox.IsEnabled = true;
            otherTxtBox.Opacity = 1;
        }
    }
}
