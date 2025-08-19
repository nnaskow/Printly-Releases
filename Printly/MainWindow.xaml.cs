
using Microsoft.EntityFrameworkCore;
using Printly.Models;
using Printly.Services;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Net;
using System.Net.Mail;
namespace Printly
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ToolTip customerNameToolTip = new ToolTip();
        private DispatcherTimer customerNameTimer = new DispatcherTimer();

        private ToolTip defectTxtBoxToolTip = new ToolTip();
        private DispatcherTimer defectTxtBoxTimer = new DispatcherTimer();
       
        private ToolTip phoneNumberTxtBoxToolTip = new ToolTip();
        private DispatcherTimer phoneNumberTxtBoxTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            phoneNumber.Focus();
            otherTxtBox.Opacity = 0.375;
            otherTxtBox.IsEnabled = false;
            customerNameTimer.Interval = TimeSpan.FromSeconds(2.5);
            customerNameTimer.Tick += (s, e) =>
            {
                customerNameToolTip.IsOpen = false;
                customerNameTimer.Stop();
            };

            defectTxtBoxTimer.Interval = TimeSpan.FromSeconds(2.5);
            defectTxtBoxTimer.Tick += (s, e) =>
            {
                defectTxtBoxToolTip.IsOpen = false;
                defectTxtBoxTimer.Stop();
            };
            phoneNumberTxtBoxTimer.Interval = TimeSpan.FromSeconds(2.5);
            phoneNumberTxtBoxTimer.Tick -= (s, e) =>
            {
                phoneNumberTxtBoxToolTip.IsOpen = false;
                phoneNumberTxtBoxTimer.Stop();
            };
        }
        private void dbManagement_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            DbManagementWindow dbManagementWindow = new DbManagementWindow();
            dbManagementWindow.Closed += (s, args) => this.Close();
            dbManagementWindow.Show();
        }
        private async void addToDBButton_Click(object sender, RoutedEventArgs e)
        {

            var context = new PrintlyDBContext();
            var clientService = new ClientService(context);
            var orderService = new OrderService(context);

                try
                {
                    string clientName = customerName.Text;
                    string phoneNum = phoneNumber.Text;
                    string article = itemTextBox.Text;
                    string defect = defectTxtBox.Text;
                    string price = priceTxtBox.Text;
                    string date = dateDTP.Text;
                    string description = descriptionTxtBox.Text;
                    bool isOnlyDigits = phoneNum.All(char.IsDigit);
                    string printerName = "ZDesigner ZD220-203dpi ZPL";

                    if (string.IsNullOrEmpty(clientName))
                    {
                        MessageBox.Show("Моля въведете име на клиент.");
                        return;
                    }
                    if (string.IsNullOrEmpty(phoneNum) || !isOnlyDigits)
                    {
                        MessageBox.Show("Въведете валиден телефонен номер.");
                        return;
                    }
                    if (string.IsNullOrEmpty(article))
                    {
                        MessageBox.Show("Въведете име на артикул.");
                        return;
                    }              
                    if(string.IsNullOrEmpty(description)) { description= string.Empty; }
                    decimal? parsedPrice = null;
                    if (!string.IsNullOrWhiteSpace(price))
                    {
                        if (!decimal.TryParse(price, out decimal p))
                        {
                            MessageBox.Show("⚠️ Моля въведете валидна цена.");
                            return;
                        }
                        parsedPrice = p;
                    }

                    DateTime parsedDate;
                    if (string.IsNullOrWhiteSpace(date))
                    {
                        parsedDate = DateTime.Now;
                    }
                    else if (!DateTime.TryParse(date, out parsedDate))
                    {
                        MessageBox.Show("⚠️ Моля въведете валидна дата.");
                        return;
                    }

                    List<string> selectedAccessories = new();
                    if (chargerCheckBox.IsChecked == true)
                        selectedAccessories.Add("Зарядно");

                    if (caseCheckBox.IsChecked == true)
                        selectedAccessories.Add("Калъф");

                    if (othersCheckBox.IsChecked == true && !string.IsNullOrWhiteSpace(otherTxtBox.Text))
                        selectedAccessories.Add(otherTxtBox.Text);
                    if (bagCheckBox.IsChecked == true)
                        selectedAccessories.Add("Чанта");
                    if (adapterCheckBox.IsChecked == true)
                        selectedAccessories.Add("Адаптер");

                    string accessories = string.Join(", ", selectedAccessories);
                    bool isPrinted = false;

                    int clientId = await clientService.AddOrGetClientIdAsync(clientName, phoneNum);

                var newOrder = new Order
                    {
                        ClientId = clientId,
                        Article = article,
                        Defect = defect,
                        Price = parsedPrice,
                        DateReceived = parsedDate,
                        IsPrinted = false,
                        Accessories = accessories,
                        Description = description
                    };

                    context.Orders.Add(newOrder);
                    context.SaveChanges();

                    int orderId = newOrder.OrderId;

                string zpl =
               "CT~~CD,~CC^~CT~\n" +
               "^XA\n" +
               "~TA000\n" +
               "~JSN\n" +
               "^LT0\n" +
               "^MNW\n" +
               "^MTT\n" +
               "^PON\n" +
               "^PMN\n" +
               "^LH0,0\n" +
               "^JMA\n" +
               "^PR4,4\n" +
               "~SD15\n" +
               "^JUS\n" +
               "^LRN\n" +
               "^CI27\n" +
               "^PA0,1,1,0\n" +
               "^XZ\n" +
               "^XA\n" +
               "^MMT\n" +
               "^PW831\n" +
               "^LL406\n" +
               "^LS0\n" +
               "^FT200,100^A@N,40,40,TT0003M_^FH\\^CI28^FDИме: {ClientName}^FS^CI27\n" +
               "^FT200,150^A@N,40,40,TT0003M_^FH\\^CI28^FDТелефон: {Phone}^FS^CI27\n" +
               "^FT200,200^A@N,40,40,TT0003M_^FH\\^CI28^FDАртикул: {Item}^FS^CI27\n" +
               "^FT200,250^A@N,40,40,TT0003M_^FH\\^CI28^FDДефект: {Defect}^FS^CI27\n" +
               "^FT200,300^A@N,40,40,TT0003M_^FH\\^CI28^FDЦена: {Price}^FS^CI27\n" +
               "^FT200,350^A@N,40,40,TT0003M_^FH\\^CI28^FDДата: {Date}^FS^CI27\n" +
               "^FT200,400^A@N,40,40,TT0003M_^FH\\^CI28^FDПоръчка №: {OrderId}^FS^CI27\n" +
               "^PQ1,0,1,Y\n" +
               "^XZ";
                if (customerName.Text.Length>16)
                    {                  
                        zpl = "CT~~CD,~CC^~CT~\n" +
         "^XA\n" +
         "~TA000\n" +
         "~JSN\n" +
         "^LT0\n" +
         "^MNW\n" +
         "^MTT\n" +
         "^PON\n" +
         "^PMN\n" +
         "^LH0,0\n" +
         "^JMA\n" +
         "^PR4,4\n" +
         "~SD15\n" +
         "^JUS\n" +
         "^LRN\n" +
         "^CI27\n" +
         "^PA0,1,1,0\n" +
         "^XZ\n" +
         "^XA\n" +
         "^MMT\n" +
         "^PW831\n" +
         "^LL406\n" +
         "^LS0\n" +
         "^FT200,100^A@N,40,40,TT0003M_^FH\\^CI28^FDИме:^FS^CI27\n" +
         "^FT200,150^A@N,40,40,TT0003M_^FH\\^CI28^FD{ClientName}^FS^CI27\n" +
         "^FT200,200^A@N,40,40,TT0003M_^FH\\^CI28^FDТелефон: {Phone}^FS^CI27\n" +
         "^FT200,250^A@N,40,40,TT0003M_^FH\\^CI28^FDАртикул: {Item}^FS^CI27\n" +
         "^FT200,300^A@N,40,40,TT0003M_^FH\\^CI28^FDДефект: {Defect}^FS^CI27\n" +
         "^FT200,350^A@N,40,40,TT0003M_^FH\\^CI28^FDЦена: {Price}^FS^CI27\n" +
         "^FT200,400^A@N,40,40,TT0003M_^FH\\^CI28^FDДата: {Date}^FS^CI27\n" +
         "^FT200,450^A@N,40,40,TT0003M_^FH\\^CI28^FDПоръчка №: {OrderId}^FS^CI27\n" +
         "^PQ1,0,1,Y\n" +
         "^XZ";
                    }
                    if(defectTxtBox.Text.Length>13)
                    {
                        zpl = "CT~~CD,~CC^~CT~\n" +
        "^XA\n" +
        "~TA000\n" +
        "~JSN\n" +
        "^LT0\n" +
        "^MNW\n" +
        "^MTT\n" +
        "^PON\n" +
        "^PMN\n" +
        "^LH0,0\n" +
        "^JMA\n" +
        "^PR4,4\n" +
        "~SD15\n" +
        "^JUS\n" +
        "^LRN\n" +
        "^CI27\n" +
        "^PA0,1,1,0\n" +
        "^XZ\n" +
        "^XA\n" +
        "^MMT\n" +
        "^PW831\n" +
        "^LL406\n" +
        "^LS0\n" +
        "^FT200,100^A@N,40,40,TT0003M_^FH\\^CI28^FDИме: {ClientName}^FS^CI27\n" +
        "^FT200,150^A@N,40,40,TT0003M_^FH\\^CI28^FDТелефон: {Phone}^FS^CI27\n" +
        "^FT200,200^A@N,40,40,TT0003M_^FH\\^CI28^FDАртикул: {Item}^FS^CI27\n" +
        "^FT200,250^A@N,40,40,TT0003M_^FH\\^CI28^FDДефект: ^FS^CI27\n" +
        "^FT200,300^A@N,40,40,TT0003M_^FH\\^CI28^FD{Defect}^FS^CI27\n" +
        "^FT200,350^A@N,40,40,TT0003M_^FH\\^CI28^FDЦена: {Price}^FS^CI27\n" +
        "^FT200,400^A@N,40,40,TT0003M_^FH\\^CI28^FDДата: {Date}^FS^CI27\n" +
        "^FT200,450^A@N,40,40,TT0003M_^FH\\^CI28^FDПоръчка №: {OrderId}^FS^CI27\n" +
        "^PQ1,0,1,Y\n" +
        "^XZ";
                    }
                    if(customerName.Text.Length > 16 && defectTxtBox.Text.Length > 13)
                    {
                    zpl =
"CT~~CD,~CC^~CT~\n" +
"^XA\n" +
"~TA000\n" +
"~JSN\n" +
"^LT0\n" +
"^MNW\n" +
"^MTT\n" +
"^PON\n" +
"^PMN\n" +
"^LH0,0\n" +
"^JMA\n" +
"^PR4,4\n" +
"~SD15\n" +
"^JUS\n" +
"^LRN\n" +
"^CI27\n" +
"^PA0,1,1,0\n" +
"^XZ\n" +
"^XA\n" +
"^MMT\n" +
"^PW831\n" +
"^LL406\n" +
"^LS0\n" +
"^FT200,50^A@N,40,40,TT0003M_^FH\\^CI28^FDИме:^FS^CI27\n" +
"^FT200,100^A@N,40,40,TT0003M_^FH\\^CI28^FD{ClientName}^FS^CI27\n" +
"^FT200,150^A@N,40,40,TT0003M_^FH\\^CI28^FDТелефон: {Phone}^FS^CI27\n" +
"^FT200,200^A@N,40,40,TT0003M_^FH\\^CI28^FDАртикул: {Item}^FS^CI27\n" +
"^FT200,250^A@N,40,40,TT0003M_^FH\\^CI28^FDДефект: ^FS^CI27\n" +
"^FT200,300^A@N,40,40,TT0003M_^FH\\^CI28^FD{Defect}^FS^CI27\n" +
"^FT200,350^A@N,40,40,TT0003M_^FH\\^CI28^FDЦена: {Price}^FS^CI27\n" +
"^FT200,400^A@N,40,40,TT0003M_^FH\\^CI28^FDДата: {Date}^FS^CI27\n" +
"^FT200,450^A@N,40,40,TT0003M_^FH\\^CI28^FDПоръчка №: {OrderId}^FS^CI27\n" +
"^PQ1,0,1,Y\n" +
"^XZ";
                }
                        string zplToPrint = zpl
                            .Replace("{OrderId}", orderId.ToString())
                            .Replace("{ClientName}", clientName)
                            .Replace("{Phone}", phoneNum)
                            .Replace("{Item}", article)
                            .Replace("{Defect}", defect)
                            .Replace("{Price}", parsedPrice?.ToString("0.00") ?? "")
                            .Replace("{Accessories}", accessories)
                            .Replace("{Date}", parsedDate.ToString("dd.MM.yyyy HH:mm"));

                    MessageBoxResult dShow = MessageBox.Show("Желаете ли да отпечатате нововъведените данни?", "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (dShow == MessageBoxResult.Yes)
                    {
                        isPrinted = true;

                        var orderToUpdate = context.Orders.FirstOrDefault(o => o.OrderId == orderId);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.IsPrinted = true;
                            context.SaveChanges();
                        }

                        byte[] zplBytes = Encoding.UTF8.GetBytes(zplToPrint);
                        bool success = RawPrinterHelper.SendBytesToPrinter(printerName, zplBytes);

                        if (success)
                        {
                            MessageBox.Show("✅ Успешен печат!");
                        }
                        else
                        {
                            MessageBox.Show("⚠️ Печатът не бе успешен.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Успешен запис.");
                    }

                    customerName.Clear();
                    phoneNumber.Clear();
                    defectTxtBox.Clear();
                    priceTxtBox.Clear();
                    itemTextBox.Clear();
                    otherTxtBox.Clear();
                    descriptionTxtBox.Clear();
                    chargerCheckBox.IsChecked = false;
                    caseCheckBox.IsChecked = false;
                    othersCheckBox.IsChecked = false;
                    bagCheckBox.IsChecked = false;
                    otherTxtBox.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Грешка при запис: " + ex.Message + "\n\nПодробности: " + ex.InnerException?.Message);
            }
        }     
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            otherTxtBox.IsEnabled =true;
            otherTxtBox.Opacity = 1;

        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            otherTxtBox.IsEnabled = false;
            otherTxtBox.Opacity = 0.375;
            otherTxtBox.Clear();
        }

        private void phoneNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key==Key.Tab) { customerName.Focus(); }
        }

        private void customerName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab) { itemTextBox.Focus(); }
        }

        private void itemTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab) { defectTxtBox.Focus(); }
        }

        private void defectTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab) { descriptionTxtBox.Focus(); }
        }

        private async void searchClientInDB_Click(object sender, RoutedEventArgs e)
        {
            PrintlyDBContext printlyDBContext = new PrintlyDBContext();
            string clientPhone = phoneNumber.Text;
            var results = await printlyDBContext.Clients
           .Where(c => c.Phone == clientPhone).FirstOrDefaultAsync();
            if (results != null)
            {
                customerName.Text = results.Name;
            }
            else if (string.IsNullOrWhiteSpace(phoneNumber.Text))
            {
                MessageBox.Show("Въведете номер в полето.");
            }
            else
            {
                MessageBox.Show("Не беше намерен клиент с такъв номер в базата данни.");
            }
            
        }

        private void customerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = customerName.Text;
            if (text.Length > 16)
            {
                customerNameToolTip.Content = "⚠ Съдържанието е твърде дълго и ще бъде пренесено на нов ред при печат.";
                customerNameToolTip.PlacementTarget = customerName;
                customerNameToolTip.IsOpen = true;

                // Стартираме таймера наново, ако тултипът вече е показан
                customerNameTimer.Stop();
                customerNameTimer.Start();
            }
            else
            {
                customerNameToolTip.IsOpen = false;
                customerNameTimer.Stop();
            }
        }

        private void defectTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = defectTxtBox.Text;
            if (text.Length > 13)
            {
                defectTxtBoxToolTip.Content = "⚠ Съдържанието е твърде дълго и ще бъде пренесено на нов ред при печат.";
                defectTxtBoxToolTip.PlacementTarget = defectTxtBox;
                defectTxtBoxToolTip.IsOpen = true;

                defectTxtBoxTimer.Stop();
                defectTxtBoxTimer.Start();
            }
            else
            {
                defectTxtBoxToolTip.IsOpen = false;
                defectTxtBoxTimer.Stop();
            }
        }

        private void phoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = phoneNumber.Text;
            if (text.Length > 10)
            {
                phoneNumberTxtBoxToolTip.Content = "⚠ ПРЕДУПРЕЖДЕНИЕ: Въведеният номер е повече от 10 символа";
                phoneNumberTxtBoxToolTip.PlacementTarget = defectTxtBox;
                phoneNumberTxtBoxToolTip.IsOpen = true;

                phoneNumberTxtBoxTimer.Stop();
                phoneNumberTxtBoxTimer.Start();
            }
            else if( text.Length<10 && text.Length >= 8)
            {
                phoneNumberTxtBoxToolTip.Content = "⚠ ПРЕДУПРЕЖДЕНИЕ: Въведеният номер е по-малко от 10 символа";
                phoneNumberTxtBoxToolTip.PlacementTarget = defectTxtBox;
                phoneNumberTxtBoxToolTip.IsOpen = true;

                phoneNumberTxtBoxTimer.Stop();
                phoneNumberTxtBoxTimer.Start();
            }
            else
            {
                phoneNumberTxtBoxToolTip.IsOpen = false;
                phoneNumberTxtBoxTimer.Stop();
            }
        }
    } 
}