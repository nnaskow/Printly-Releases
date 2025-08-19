using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Printly.Models;
using Printly.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Printly
{
    /// <summary>
    /// Interaction logic for DbManagementWindow.xaml
    /// </summary>

    public partial class DbManagementWindow : Window
    {
        PrintlyDBContext printlyDBContext = new PrintlyDBContext();
        public bool developerMode;

        public DbManagementWindow()
        {
            PrintlyDBContext printlyDBContext = new PrintlyDBContext();
            InitializeComponent();
            LoadOrders(printlyDBContext);
            LoadFinishedOrders(printlyDBContext);
            developerMode = false;
            DeveloperModeOff();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Closed += (s, args) => this.Close();
            mainWindow.Show();
        }

        private void finishedUnfinishedOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            unReadyForSubmissionButton.Visibility = Visibility.Hidden;
            isReadyForSubmissionButton.Visibility = Visibility.Hidden;
            listOfOrdersLabel.Content = "Списък с предадени поръчки";
            unfinishedOrdersButton.Visibility = Visibility.Visible;
            listOrdersDataGrid.Visibility = Visibility.Hidden;
            listFinishedOrdersDG.Visibility = Visibility.Visible;
            finishedOrdersButton.Visibility = Visibility.Hidden;
            markButton.Visibility = Visibility.Hidden;
            unmarkButton.Visibility = Visibility.Visible;
            LoadFinishedOrders(printlyDBContext);
            searchClienntItemTxtBox.Clear();
            searchClientNameTxtBox.Clear();
            searchClientPhoneTxtBox.Clear();
            newNameTxtBox.Clear();
            newDefectTxtBox.Clear();
            newItemTxtBox.Clear();
            newPhoneTxtBox.Clear();
            newPriceTxtBox.Clear();
            descriptionTxtBox.Clear();
            editDateofSubmission.Visibility = Visibility.Visible;
        }

        private void finishedUnfinishedOrdersButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            unReadyForSubmissionButton.Visibility = Visibility.Visible;
            isReadyForSubmissionButton.Visibility = Visibility.Visible;
            unfinishedOrdersButton.Visibility = Visibility.Hidden;
            listOfOrdersLabel.Content = "Списък с незавършени поръчки";
            listOrdersDataGrid.Visibility = Visibility.Visible;
            listFinishedOrdersDG.Visibility = Visibility.Hidden;
            finishedOrdersButton.Visibility = Visibility.Visible;
            markButton.Visibility = Visibility.Visible;
            unmarkButton.Visibility = Visibility.Hidden;
            LoadOrders(printlyDBContext);
            searchClienntItemTxtBox.Clear();
            searchClientNameTxtBox.Clear();
            searchClientPhoneTxtBox.Clear();
            newNameTxtBox.Clear();
            newDefectTxtBox.Clear();
            newItemTxtBox.Clear();
            newPhoneTxtBox.Clear();
            newPriceTxtBox.Clear();
            descriptionTxtBox.Clear();
            editDateofSubmission.Visibility = Visibility.Hidden;

        }

        private void markButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                var dateWindow = new DatePicker(selectedOrder);
                if (dateWindow.ShowDialog() == true && dateWindow.SelectedDate.HasValue)
                {
                    using (var context = new PrintlyDBContext())
                    {
                        var orderToUpdate = context.Orders
                            .FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.IsChecked = true;
                            orderToUpdate.IsSubmissed = dateWindow.SelectedDate.Value;
                            context.SaveChanges();
                            MessageBox.Show("✅ Поръчката е отбелязана като предадена.");

                            LoadOrders(context);
                            LoadFinishedOrders(context);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Моля изберете поръчка от списъка.");
            }
        }
        private void unmarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (listFinishedOrdersDG.SelectedItem is Order selectedOrder)
            {
                using (var context = new PrintlyDBContext())
                {
                    var orderToUpdate = context.Orders.FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.IsChecked = false;
                        orderToUpdate.IsSubmissed = null;
                        context.SaveChanges();
                        MessageBox.Show("✅ Поръчката е отбелязана като непредадена.");

                        LoadOrders(context);
                        LoadFinishedOrders(context);
                    }
                }
            }
            else
            {
                MessageBox.Show("Моля изберете поръчка от списъка.");
            }
        }
        private async void LoadOrders(PrintlyDBContext printlyDBContext)
        {
            try
            {
                using var context = new PrintlyDBContext();
                var activeOrders = context.Orders
                    .Include(o => o.Client)
                    .Where(o => !o.IsChecked)
                    .AsNoTracking()
                    .Select(o => new Order
                    {
                        OrderId = o.OrderId,
                        ClientId = o.ClientId,
                        Article = o.Article,
                        Defect = o.Defect,
                        Price = o.Price,
                        DateReceived = o.DateReceived,
                        Client = o.Client,
                        IsChecked = o.IsChecked,
                        IsPrinted = o.IsPrinted,
                        Accessories = o.Accessories,
                        Description = o.Description,
                        IsReadyForSubmission = o.IsReadyForSubmission,
                        IsSubmissed = o.IsSubmissed
                    })
                    .ToList();

                listOrdersDataGrid.ItemsSource = activeOrders;
                ShowLoading();
                await Task.Delay(800);
                HideLoading();
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Грешка при зареждане на поръчките:\n" + ex.Message);
            }


        }

        public async void LoadFinishedOrders(PrintlyDBContext printlyDBContext)
        {
            try
            {

                using var context = new PrintlyDBContext();
                var completedOrders = context.Orders
                    .Include(o => o.Client)
                    .Where(o => o.IsChecked)
                    .OrderBy(o => o.IsSubmissed)
                    .AsNoTracking()
                    .Select(o => new Order
                    {
                        OrderId = o.OrderId,
                        ClientId = o.ClientId,
                        Article = o.Article,
                        Defect = o.Defect,
                        Price = o.Price,
                        DateReceived = o.DateReceived,
                        Client = o.Client,
                        IsChecked = o.IsChecked,
                        IsPrinted = o.IsPrinted,
                        Accessories = o.Accessories,
                        Description = o.Description,
                        IsReadyForSubmission = o.IsReadyForSubmission,
                        IsSubmissed = o.IsSubmissed
                    })
                    .ToList();

                listFinishedOrdersDG.ItemsSource = completedOrders;
                ShowLoading();
                await Task.Delay(800);
                HideLoading();
            }
            catch (Exception ex)
            {
                MessageBox.Show("⚠️ Грешка при зареждане на поръчките:\n" + ex.Message);
            }

        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult dShow = MessageBox.Show("Сигурни ли сте, че искате да отпечатате този ред?", "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dShow != MessageBoxResult.Yes) return;

                string printerName = "ZDesigner ZD220-203dpi ZPL";
                Order selectedOrder = listOrdersDataGrid.SelectedItem as Order;

                if (selectedOrder == null)
                {
                    MessageBox.Show("Моля изберете поръчка от списъка.");
                    return;
                }

                string clientName = selectedOrder.Client?.Name ?? "";
                string phone = selectedOrder.Client?.Phone ?? "";
                string item = selectedOrder.Article ?? "";
                string defect = selectedOrder.Defect ?? "";
                string price = selectedOrder.Price?.ToString("0.00") ?? "";
                string date = selectedOrder.DateReceived.ToString("dd.MM.yyyy HH:mm");
                string orderId = selectedOrder.OrderId.ToString();

                string zpl;

                bool longClient = clientName.Length > 16;
                bool longDefect = defect.Length > 13;

                if (longClient && longDefect)
                {
                    zpl =
            @"CT~~CD,~CC^~CT~
^XA
~TA000
~JSN
^LT0
^MNW
^MTT
^PON
^PMN
^LH0,0
^JMA
^PR4,4
~SD15
^JUS
^LRN
^CI27
^PA0,1,1,0
^XZ
^XA
^MMT
^PW831
^LL406
^LS0
^FT200,50^A@N,40,40,TT0003M_^FH\^CI28^FDИме:^FS^CI27
^FT200,100^A@N,40,40,TT0003M_^FH\^CI28^FD{ClientName}^FS^CI27
^FT200,150^A@N,40,40,TT0003M_^FH\^CI28^FDТелефон: {Phone}^FS^CI27
^FT200,200^A@N,40,40,TT0003M_^FH\^CI28^FDАртикул: {Item}^FS^CI27
^FT200,250^A@N,40,40,TT0003M_^FH\^CI28^FDДефект:^FS^CI27
^FT200,300^A@N,40,40,TT0003M_^FH\^CI28^FD{Defect}^FS^CI27
^FT200,350^A@N,40,40,TT0003M_^FH\^CI28^FDЦена: {Price}^FS^CI27
^FT200,400^A@N,40,40,TT0003M_^FH\^CI28^FDДата: {Date}^FS^CI27
^FT200,450^A@N,40,40,TT0003M_^FH\^CI28^FDПоръчка №: {OrderId}^FS^CI27
^PQ1,0,1,Y
^XZ";
                }
                else if (longClient)
                {
                    zpl =
            @"CT~~CD,~CC^~CT~
^XA
~TA000
~JSN
^LT0
^MNW
^MTT
^PON
^PMN
^LH0,0
^JMA
^PR4,4
~SD15
^JUS
^LRN
^CI27
^PA0,1,1,0
^XZ
^XA
^MMT
^PW831
^LL406
^LS0
^FT200,100^A@N,40,40,TT0003M_^FH\^CI28^FDИме:^FS^CI27
^FT200,150^A@N,40,40,TT0003M_^FH\^CI28^FD{ClientName}^FS^CI27
^FT200,200^A@N,40,40,TT0003M_^FH\^CI28^FDТелефон: {Phone}^FS^CI27
^FT200,250^A@N,40,40,TT0003M_^FH\^CI28^FDАртикул: {Item}^FS^CI27
^FT200,300^A@N,40,40,TT0003M_^FH\^CI28^FDДефект: {Defect}^FS^CI27
^FT200,350^A@N,40,40,TT0003M_^FH\^CI28^FDЦена: {Price}^FS^CI27
^FT200,400^A@N,40,40,TT0003M_^FH\^CI28^FDДата: {Date}^FS^CI27
^FT200,450^A@N,40,40,TT0003M_^FH\^CI28^FDПоръчка №: {OrderId}^FS^CI27
^PQ1,0,1,Y
^XZ";
                }
                else if (longDefect)
                {
                    zpl =
            @"CT~~CD,~CC^~CT~
^XA
~TA000
~JSN
^LT0
^MNW
^MTT
^PON
^PMN
^LH0,0
^JMA
^PR4,4
~SD15
^JUS
^LRN
^CI27
^PA0,1,1,0
^XZ
^XA
^MMT
^PW831
^LL406
^LS0
^FT200,100^A@N,40,40,TT0003M_^FH\^CI28^FDИме: {ClientName}^FS^CI27
^FT200,150^A@N,40,40,TT0003M_^FH\^CI28^FDТелефон: {Phone}^FS^CI27
^FT200,200^A@N,40,40,TT0003M_^FH\^CI28^FDАртикул: {Item}^FS^CI27
^FT200,250^A@N,40,40,TT0003M_^FH\^CI28^FDДефект:^FS^CI27
^FT200,300^A@N,40,40,TT0003M_^FH\^CI28^FD{Defect}^FS^CI27
^FT200,350^A@N,40,40,TT0003M_^FH\^CI28^FDЦена: {Price}^FS^CI27
^FT200,400^A@N,40,40,TT0003M_^FH\^CI28^FDДата: {Date}^FS^CI27
^FT200,450^A@N,40,40,TT0003M_^FH\^CI28^FDПоръчка №: {OrderId}^FS^CI27
^PQ1,0,1,Y
^XZ";
                }
                else
                {
                    zpl =
            @"CT~~CD,~CC^~CT~
^XA
~TA000
~JSN
^LT0
^MNW
^MTT
^PON
^PMN
^LH0,0
^JMA
^PR4,4
~SD15
^JUS
^LRN
^CI27
^PA0,1,1,0
^XZ
^XA
^MMT
^PW831
^LL406
^LS0
^FT200,100^A@N,40,40,TT0003M_^FH\^CI28^FDИме: {ClientName}^FS^CI27
^FT200,150^A@N,40,40,TT0003M_^FH\^CI28^FDТелефон: {Phone}^FS^CI27
^FT200,200^A@N,40,40,TT0003M_^FH\^CI28^FDАртикул: {Item}^FS^CI27
^FT200,250^A@N,40,40,TT0003M_^FH\^CI28^FDДефект: {Defect}^FS^CI27
^FT200,300^A@N,40,40,TT0003M_^FH\^CI28^FDЦена: {Price}^FS^CI27
^FT200,350^A@N,40,40,TT0003M_^FH\^CI28^FDДата: {Date}^FS^CI27
^FT200,400^A@N,40,40,TT0003M_^FH\^CI28^FDПоръчка №: {OrderId}^FS^CI27
^PQ1,0,1,Y
^XZ";
                }

                string prnToPrint = zpl
                    .Replace("{ClientName}", clientName)
                    .Replace("{Phone}", phone)
                    .Replace("{Item}", item)
                    .Replace("{Defect}", defect)
                    .Replace("{Price}", price)
                    .Replace("{Date}", date)
                    .Replace("{OrderId}", orderId);

                byte[] zplBytes = Encoding.UTF8.GetBytes(prnToPrint);
                bool success = RawPrinterHelper.SendBytesToPrinter(printerName, zplBytes);

                if (success)
                {
                    using (var context = new PrintlyDBContext())
                    {
                        var orderToUpdate = context.Orders.FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.IsPrinted = true;
                            selectedOrder.IsPrinted = true;
                            context.SaveChanges();
                        }
                    }
                    MessageBox.Show("✅ Успешен печат!");
                    listOrdersDataGrid.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("⚠️ Печатът не бе успешен.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                var dShow = MessageBox.Show("Сигурни ли сте, че искате да изтриете тази поръчка?", "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dShow == MessageBoxResult.Yes)
                {
                    DeleteOrderAndReload(selectedOrder, printlyDBContext, LoadOrders);
                }
            }
            else if (listFinishedOrdersDG.SelectedItem is Order selectedOrder1)
            {
                var dShow = MessageBox.Show("Сигурни ли сте, че искате да изтриете тази поръчка?", "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dShow == MessageBoxResult.Yes)
                {
                    DeleteOrderAndReload(selectedOrder1, printlyDBContext, LoadFinishedOrders);
                }
            }
            else
            {
                MessageBox.Show("Моля изберете поръчка за изтриване.");
            }
        }

        private void DeleteOrderAndReload(Order orderToDelete, PrintlyDBContext context, Action<PrintlyDBContext> reloadMethod)
        {
            var order = context.Orders.FirstOrDefault(o => o.OrderId == orderToDelete.OrderId);
            if (order == null)
            {
                MessageBox.Show("Поръчката не беше намерена в базата.");
                return;
            }
            MessageBox.Show("Поръчката беше изтрита!");
            OrderService orderService = new OrderService(context);
            orderService.DeleteOrder(order.OrderId);

            reloadMethod(context);
        }

        private void listOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listOrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                newDefectTxtBox.Text = selectedOrder.Defect;
                newNameTxtBox.Text = selectedOrder.Client.Name;
                newPhoneTxtBox.Text = selectedOrder.Client.Phone;
                newItemTxtBox.Text = selectedOrder.Article;
                descriptionTxtBox.Text = selectedOrder.Description;
                newPriceTxtBox.Text = selectedOrder.Price?.ToString("F2") ?? "";
                if (selectedOrder.IsReadyForSubmission == true)
                {
                    isReadyForSubmissionButton.Visibility = Visibility.Hidden;
                    unReadyForSubmissionButton.Visibility = Visibility.Visible;
                }
                else
                {
                    isReadyForSubmissionButton.Visibility = Visibility.Visible;
                    unReadyForSubmissionButton.Visibility = Visibility.Hidden;
                }

            }


        }
        private void listFinishedOrdersDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listFinishedOrdersDG.SelectedItem is Order selectedOrder)
            {
                newDefectTxtBox.Text = selectedOrder.Defect;
                newNameTxtBox.Text = selectedOrder.Client.Name;
                newPhoneTxtBox.Text = selectedOrder.Client.Phone;
                newItemTxtBox.Text = selectedOrder.Article;
                descriptionTxtBox.Text = selectedOrder.Description;
                newPriceTxtBox.Text = selectedOrder.Price?.ToString("F2") ?? "";
            }
        }
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOrdersDataGrid.SelectedItem is Order)
            {
                if (UpdateOrders(listOrdersDataGrid))
                    LoadOrders(printlyDBContext);
            }
            else if (listFinishedOrdersDG.SelectedItem is Order)
            {
                if (UpdateOrders(listFinishedOrdersDG))
                    LoadFinishedOrders(printlyDBContext);
            }
            else
            {
                MessageBox.Show("Моля, изберете поръчка за редакция.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            newNameTxtBox.Clear();
            newDefectTxtBox.Clear();
            newItemTxtBox.Clear();
            newPhoneTxtBox.Clear();
            newPriceTxtBox.Clear();
            descriptionTxtBox.Clear();
        }

        private bool UpdateOrders(DataGrid dataGrid)
        {
            if (dataGrid.SelectedItem is not Order selectedOrder)
                return false;

            using (var db = new PrintlyDBContext())
            {
                var orderInDb = db.Orders.Include(o => o.Client).FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                if (orderInDb == null)
                {
                    MessageBox.Show("Поръчката не беше намерена в базата.", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }

                // Присвояване директно от текстовите полета
                orderInDb.Defect = string.IsNullOrWhiteSpace(newDefectTxtBox.Text) ? null : newDefectTxtBox.Text;
                orderInDb.Article = newItemTxtBox.Text;
                orderInDb.Description = string.IsNullOrWhiteSpace(descriptionTxtBox.Text) ? null : descriptionTxtBox.Text;

                if (orderInDb.Client != null)
                {
                    orderInDb.Client.Name = newNameTxtBox.Text;
                    orderInDb.Client.Phone = newPhoneTxtBox.Text;
                }

                if (string.IsNullOrWhiteSpace(newPriceTxtBox.Text))
                {
                    orderInDb.Price = null;
                }
                else if (decimal.TryParse(newPriceTxtBox.Text, out decimal parsedPrice))
                {
                    orderInDb.Price = parsedPrice;
                }
                else
                {
                    MessageBox.Show("Невалидна цена.", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                MessageBox.Show("Промените запазени успешно.", "Промени", MessageBoxButton.OK);
                db.SaveChanges();
            }

            return true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) //търсене
        {
            using (var context = new PrintlyDBContext())
            {
                IQueryable<Order> query;

                if (finishedOrdersButton.Visibility == Visibility.Visible)
                {
                    query = context.Orders
                        .Include(o => o.Client)
                        .Where(o => !o.IsChecked);
                }
                else
                {
                    query = context.Orders
                        .Include(o => o.Client)
                        .Where(o => o.IsChecked)
                        .OrderBy(o => o.IsSubmissed);
                }

                SearchMethod(query);
            }
        }

        private void SearchMethod(IQueryable<Order> query)
        {

            string clientName = searchClientNameTxtBox.Text;
            string clientPhone = searchClientPhoneTxtBox.Text;
            string clientItem = searchClienntItemTxtBox.Text;


            if (!string.IsNullOrEmpty(clientName))
            {
                query = query.Where(o => o.Client.Name.Contains(clientName));
            }

            if (!string.IsNullOrEmpty(clientPhone))
            {
                query = query.Where(o => o.Client.Phone.Contains(clientPhone));
            }

            if (!string.IsNullOrEmpty(clientItem))
            {
                query = query.Where(o => o.Article.Contains(clientItem));
            }

            var filteredOrders = query.ToList();

            if (filteredOrders.Count == 0)
            {
                MessageBox.Show("Не са намерени съвпадения.");
                LoadOrders(printlyDBContext);
            }

        }

        private void searchClientNameTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearchFilter();
            }
        }

        private void searchClientPhoneTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearchFilter();
            }
        }

        private void searchClienntItemTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySearchFilter();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            searchClienntItemTxtBox.Clear();
            searchClientNameTxtBox.Clear();
            searchClientPhoneTxtBox.Clear();
            MessageBox.Show("Всички филтри бяха премахнати.");
        }

        private void listOrdersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (listOrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                if (developerMode == true)
                {
                    var dShow = MessageBox.Show($"Reseed на OrderID в БД от номер {selectedOrder.OrderId}", "RESEED CONFIRMATION", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (dShow == MessageBoxResult.Yes)
                    {
                        using (var context = new PrintlyDBContext())
                        {
                            var sql = $"DBCC CHECKIDENT ('Orders', RESEED, {selectedOrder.OrderId})";
                            context.Database.ExecuteSqlRaw(sql);
                            MessageBox.Show("Успешен RESEED!", "ИНФОРМАЦИЯ");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Командата отказана.", "ИНФОРМАЦИЯ");
                    }
                }
                else
                {
                    string message = $"Име: {selectedOrder.Client?.Name}\n" +
                 $"Телефон: {selectedOrder.Client?.Phone}\n" +
                 $"Артикул: {selectedOrder.Article}\n" +
                 $"Дефект: {selectedOrder.Defect}\n" +
                 $"Описание: {selectedOrder.Description}\n" +
                 $"Цена: {selectedOrder.Price?.ToString("F2") ?? "N/A"}\n" +
                 $"Комплектовка: {selectedOrder.Accessories}";

                    MessageBox.Show(message, "Подробни данни на поръчката", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
        }

        private void listFinishedOrdersDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listFinishedOrdersDG.SelectedItem is Order selectedOrder)
            {
                string message = $"Име: {selectedOrder.Client?.Name}\n" +
                                 $"Телефон: {selectedOrder.Client?.Phone}\n" +
                                 $"Артикул: {selectedOrder.Article}\n" +
                                 $"Дефект: {selectedOrder.Defect}\n" +
                                 $"Описание: {selectedOrder.Description}\n" +
                                 $"Цена: {selectedOrder.Price?.ToString("F2") ?? "N/A"}\n" +
                                 $"Комплектовка: {selectedOrder.Accessories}";

                MessageBox.Show(message, "Order Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void isReadyForSubmissionButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                using (var context = new PrintlyDBContext())
                {
                    var orderToUpdate = context.Orders.FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.IsReadyForSubmission = true;
                        context.SaveChanges();
                        MessageBox.Show("✅ Поръчката е отбелязана като завършена.");
                        LoadOrders(context);
                        LoadFinishedOrders(context);
                    }
                }
            }
            else
            {
                MessageBox.Show("Моля изберете поръчка от списъка.");
            }
        }

        private void unReadyForSubmissionButton_Click(object sender, RoutedEventArgs e)
        {
            if (listOrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                using (var context = new PrintlyDBContext())
                {
                    var orderToUpdate = context.Orders.FirstOrDefault(o => o.OrderId == selectedOrder.OrderId);
                    if (orderToUpdate != null)
                    {
                        orderToUpdate.IsReadyForSubmission = false;
                        context.SaveChanges();
                        MessageBox.Show("✅ Поръчката е отбелязана като незавършена.");
                        LoadOrders(context);
                        LoadFinishedOrders(context);
                    }
                }
            }
            else
            {
                MessageBox.Show("Моля изберете поръчка от списъка.");
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (developerMode == true)
            {
                var dialogue2 = MessageBox.Show("Сигурни ли сте, че искате да изключите режим за разработчици?", "РЕЖИМ ЗА РАЗРАБОТЧИЦИ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dialogue2 == MessageBoxResult.Yes) { developerMode = false; MessageBox.Show("Режимът за разработчици бе успешно изключен!", "РЕЖИМ ЗА РАЗРАБОТЧИЦИ"); openSqlDatabase.Visibility = Visibility.Hidden; }
                else
                {
                    MessageBox.Show("Командата бе отменена :)", "Отмяна");
                }
            }
            else
            {
                var dialogue = MessageBox.Show("Сигурни ли сте, че искате да включите режим за разработчици?", "РЕЖИМ ЗА РАЗРАБОТЧИЦИ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dialogue == MessageBoxResult.Yes) { developerMode = true; MessageBox.Show("Режимът за разработчици бе успешно включен!", "РЕЖИМ ЗА РАЗРАБОТЧИЦИ"); openSqlDatabase.Visibility = Visibility.Visible; }
                else
                {
                    MessageBox.Show("Командата бе отменена :)", "Отмяна");
                }

            }
        } //developer mode

        private void newNameTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                editButton_Click(null, null);
                e.Handled = true;
            }
        }

        private void newPhoneTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            OnEnterAction(sender, e);
        }

        private void newItemTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            OnEnterAction(sender, e);
        }

        private void newDefectTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            OnEnterAction(sender, e);
        }

        private void descriptionTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            OnEnterAction(sender, e);
        }

        private void newPriceTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            OnEnterAction(sender, e);
        }

        private void OnEnterAction(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                editButton_Click(null, null);
                e.Handled = true;
            }
        }

        private void openSqlDatabase_Click(object sender, RoutedEventArgs e)
        {
            var dShow = MessageBox.Show("Коя заявка искате да достъпите?\n YES за КЛИЕНТИ\n NO за ПОРЪЧКИ", "ДОСТЪП ДО БАЗА ДАННИ", MessageBoxButton.YesNoCancel);
            string filePath = @"D:\niki\visual studio\projects\printly\sqlQuerySelectAllClients.sql";
            string filePath2 = @"D:\niki\visual studio\projects\printly\sqlQuerySelectAllOrders.sql";
            if (dShow == MessageBoxResult.Yes)
            {
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Файлът не съществува!", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (dShow == MessageBoxResult.No)
            {
                if (File.Exists(filePath2))
                {
                    Process.Start(new ProcessStartInfo(filePath2) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show("Файлът не съществува!", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void searchClientNameTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void searchClientPhoneTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private void searchClienntItemTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
        private async void ApplySearchFilter()
        {
            string clientName = searchClientNameTxtBox.Text.Trim();
            string clientPhone = searchClientPhoneTxtBox.Text.Trim();
            string clientItem = searchClienntItemTxtBox.Text.Trim();

            using (var context = new PrintlyDBContext())
            {
                IQueryable<Order> query;

                if (finishedOrdersButton.Visibility == Visibility.Visible)
                {
                    query = context.Orders
                        .Include(o => o.Client)
                        .Where(o => !o.IsChecked);
                }
                else
                {
                    query = context.Orders
                        .Include(o => o.Client)
                        .Where(o => o.IsChecked)
                        .OrderBy(o => o.IsSubmissed);
                }


                if (!string.IsNullOrWhiteSpace(clientName))
                {
                    query = query.Where(o => EF.Functions.Like(o.Client.Name, $"%{clientName}%"));
                }

                if (!string.IsNullOrWhiteSpace(clientPhone))
                {
                    query = query.Where(o => EF.Functions.Like(o.Client.Phone, $"%{clientPhone}%"));
                }

                if (!string.IsNullOrWhiteSpace(clientItem))
                {
                    query = query.Where(o => EF.Functions.Like(o.Article, $"%{clientItem}%"));
                }

                var filteredOrders = query.ToList();

                if (finishedOrdersButton.Visibility == Visibility.Visible)
                {
                    ShowLoading();
                    await Task.Delay(300);
                    HideLoading();
                    listOrdersDataGrid.ItemsSource = filteredOrders;
                }
                else
                {
                    ShowLoading();
                    await Task.Delay(300);
                    HideLoading();
                    listFinishedOrdersDG.ItemsSource = filteredOrders;
                }

                if (filteredOrders.Count == 0)
                {
                    MessageBox.Show("Няма съвпадения.");
                    searchClienntItemTxtBox.Clear();
                    searchClientNameTxtBox.Clear();
                    searchClientPhoneTxtBox.Clear();
                }
            }
        }

        private async void editAddons_Click(object sender, RoutedEventArgs e)
        {

            var selectedOrder = listOrdersDataGrid.SelectedItem as Order
                          ?? listFinishedOrdersDG.SelectedItem as Order;

            if (selectedOrder != null)
            {
                var editWindow = new EditPackaging(selectedOrder);
                if (editWindow.ShowDialog() == true)
                {
                    listOrdersDataGrid.Items.Refresh();
                    listFinishedOrdersDG.Items.Refresh();
                    ShowLoading();
                    await Task.Delay(800);
                    HideLoading();
                }
            }
            else
            {
                MessageBox.Show("Моля, изберете поръчка.");
            }

        }
        private async void editDateofSubmission_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = listFinishedOrdersDG.SelectedItem as Order;

            if (selectedOrder != null)
            {
                var datePicker = new DatePicker(selectedOrder);
                if (datePicker.ShowDialog() == true && datePicker.SelectedDate.HasValue)
                {
                    selectedOrder.IsSubmissed = datePicker.SelectedDate.Value;

                    // Запазване в базата
                    using (var context = new PrintlyDBContext())
                    {
                        context.Orders.Attach(selectedOrder);
                        context.Entry(selectedOrder).Property(o => o.IsSubmissed).IsModified = true;
                        await context.SaveChangesAsync();
                    }

                    // Обновяване на DataGrid
                    listFinishedOrdersDG.Items.Refresh();

                    ShowLoading();
                    await Task.Delay(400);
                    HideLoading();
                }
            }
            else
            {
                MessageBox.Show("Моля, изберете поръчка.");
            }
        }

        private async void editDateOfAcceptance_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = listOrdersDataGrid.SelectedItem as Order
                       ?? listFinishedOrdersDG.SelectedItem as Order;

            if (selectedOrder != null)
            {
                var datePicker = new DatePicker(selectedOrder);
                if (datePicker.ShowDialog() == true && datePicker.SelectedDate.HasValue)
                {
                    selectedOrder.DateReceived = datePicker.SelectedDate.Value;

                    // Запазване в базата
                    using (var context = new PrintlyDBContext())
                    {
                        context.Orders.Attach(selectedOrder);
                        context.Entry(selectedOrder).Property(o => o.DateReceived).IsModified = true;
                        await context.SaveChangesAsync();
                    }

                    // Обновяване на DataGrid
                    listOrdersDataGrid.Items.Refresh();
                    listFinishedOrdersDG.Items.Refresh();

                    ShowLoading();
                    await Task.Delay(400);
                    HideLoading();
                }
            }
            else
            {
                MessageBox.Show("Моля, изберете поръчка.");
            }
        }

        //Loading
        private void ShowLoading()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            Storyboard sb = (Storyboard)this.Resources["RotateStoryboard"];
            sb.Begin(this, true);
        }

        private void HideLoading()
        {
            Storyboard sb = (Storyboard)this.Resources["RotateStoryboard"];
            sb.Stop(this);
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        private async void statusButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = listFinishedOrdersDG.SelectedItem as Order
                      ?? listOrdersDataGrid.SelectedItem as Order;

            if (selectedOrder != null)
            {
                var statusWindow = new StatusWindow(selectedOrder);
                if (statusWindow.ShowDialog() == true)
                {
                    // Промяната вече е направена в обекта selectedOrder, който се показва в DataGrid-а.
                    // ObservableCollection автоматично ще обнови UI.
                    listFinishedOrdersDG.Items.Refresh();
                    listOrdersDataGrid.Items.Refresh();
                    ShowLoading();
                    await Task.Delay(700);
                    HideLoading();
                }
            }
            else
            {
                MessageBox.Show("Моля изберете поръчка!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void developerModeLogInButton_Click(object sender, RoutedEventArgs e)
        {
            if (!developerMode)
            {
                DeveloperLogin loginWindow = new DeveloperLogin();
                loginWindow.Owner = this;
                loginWindow.ShowDialog();

                if (developerMode)
                {
                    DeveloperModeOn();
                    developerModeLogInButton.Content = "⍈";
                    ShowLoading();
                    await Task.Delay(500);
                    HideLoading();
                }
            }
            else
            {
                MessageBox.Show("Излязохте от Developer Mode.", "Developer Mode", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowLoading();
                await Task.Delay(500);
                HideLoading();
                developerMode = false;
                DeveloperModeOff();
                developerModeLogInButton.Content = "👤";
            }
        }

        private void checkForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }
        private void DeveloperModeOn()
        {
            openSqlDatabase.Visibility = Visibility.Visible;
            openLogsButton.Visibility = Visibility.Visible;
        }
        private void DeveloperModeOff()
        {
            openSqlDatabase.Visibility = Visibility.Hidden;
            openLogsButton.Visibility = Visibility.Hidden;
        }

        private void openLogsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

