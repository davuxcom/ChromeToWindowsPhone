using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using DavuxLibSL;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.Phone.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Reflection;
using Microsoft.Phone.Shell;

namespace SendToWP7
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["StandardAppBar"]; 

            DataContext = App.ViewModel;

            this.Loaded += (_, __) =>
                {
                    // Debug.WriteLine("legacy pair code: " + DavuxLibSL.Settings.Get("PairCode", 0));
                    if (!App.ViewModel.IsDataLoaded)
                    {
                        App.ViewModel.LoadData();
                    }

                    var progressIndicator = SystemTray.ProgressIndicator;

                    if (progressIndicator != null)
                    {
                        return;
                    }

                    progressIndicator = new ProgressIndicator();

                    SystemTray.SetProgressIndicator(this, progressIndicator);

                    Binding binding = new Binding("IsLoadingData") { Source = App.ViewModel };
                    BindingOperations.SetBinding(
                        progressIndicator, ProgressIndicator.IsVisibleProperty, binding);

                    // use same for IsIndeterminate and IsVisible
                    binding = new Binding("IsLoadingData") { Source = App.ViewModel };
                    BindingOperations.SetBinding(
                        progressIndicator, ProgressIndicator.IsIndeterminateProperty, binding);

                    binding = new Binding("LoadingDataMsg") { Source = App.ViewModel };
                    BindingOperations.SetBinding(
                        progressIndicator, ProgressIndicator.TextProperty, binding);
                };

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                var btn = e.AddedItems[0] as ItemViewModel;
                string url = btn.URL;
                try
                {
                    WebBrowserTask wb = new WebBrowserTask();
                    wb.Uri = new Uri(url);
                    wb.Show();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Wb launch: " + ex.Message);
                }
            });
        }

        private void ShowAll_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.SelectedRecord = (sender as MenuItem).Tag as ItemViewModel;
            NavigationService.Navigate(new Uri("/Detail.xaml", UriKind.Relative));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Items.Remove((sender as MenuItem).Tag as ItemViewModel);
            App.ViewModel.Save();
        }

        private void SMS_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;

            SmsComposeTask sct = new SmsComposeTask();
            if (string.IsNullOrEmpty(item.Selection))
            {
                sct.Body = item.URL;
            }
            else
            {
                sct.Body = item.URL + ": " + item.Selection;
            }
            sct.Show();
        }

        private void Email_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;

            EmailComposeTask emc = new EmailComposeTask();
            emc.Subject = "Shared: " + item.Title;
            emc.Body = item.URL + "\n\n" +
                item.Selection + "\n\n" +
                "Sent from 'Send to WP7' on Windows Phone";
            emc.Show();
        }

        private void Map_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;

            if (string.IsNullOrEmpty(item.Selection))
            {
                MessageBox.Show("There is no selected text for this link.  Select the address text before sending the page.");
            }
            else
            {
                WebBrowserTask wbt = new WebBrowserTask();
                wbt.Uri = new Uri("maps:" + Uri.EscapeUriString(item.Selection));
                wbt.Show();
            }
        }

        private void Call_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;
            PhoneCallTask pct = new PhoneCallTask();
            pct.PhoneNumber = string.Join(null, System.Text.RegularExpressions.Regex.Split(item.Selection, "[^\\d]"));

            if (pct.PhoneNumber == "")
            {
                MessageBox.Show("No phone number in the selected text for this link.");
            }
            else
            {
                pct.Show();
            }
        }

        private void SMS_Number_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;
            SmsComposeTask sct = new SmsComposeTask();
            sct.To = string.Join(null, System.Text.RegularExpressions.Regex.Split(item.Selection, "[^\\d]"));

            if (sct.To == "")
            {
                MessageBox.Show("No phone number in the selected text for this link.");
            }
            else
            {
                sct.Show();
            }
        }

        private void CopyLink_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;
            if (string.IsNullOrEmpty(item.URL))
            {
                MessageBox.Show("No link");
            }
            else
            {
                Clipboard.SetText(item.URL);
            }
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;
            if (string.IsNullOrEmpty(item.Selection))
            {
                MessageBox.Show("No selected text for this link.  Highlight some text on the page when sending a link.");
            }
            else
            {
                Clipboard.SetText(item.Selection);
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;
            if (string.IsNullOrEmpty(item.Selection))
            {
                MessageBox.Show("No selected text for this link.  Highlight some text on the page when sending a link.");
            }
            else
            {
                SearchTask st = new SearchTask();
                st.SearchQuery = item.Selection;
                st.Show();
            }
        }

        private void Marketplace_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuItem).Tag as ItemViewModel;

            if (string.IsNullOrEmpty(item.Selection))
            {
                MessageBox.Show("No selected text for this link.  Highlight some text on the page when sending a link.");
            }
            else
            {
                MarketplaceSearchTask mst = new MarketplaceSearchTask();
                mst.SearchTerms = item.Selection;
                mst.Show();
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri("http://www.daveamenta.com/");
            wbt.Show();
        }

        private void lnkSetup_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            App.ViewModel.IsEditMode = !App.ViewModel.IsEditMode;

            if (App.ViewModel.IsEditMode)
            {
                ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["EditAppBar"]; 
            }
            else
            {
                ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["StandardAppBar"]; 
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            App.ViewModel.Update();
        }

        private void Trash_Click(object sender, EventArgs e)
        {
            foreach (var item in App.ViewModel.Items.ToArray())
            {
                if (item.IsChecked) App.ViewModel.Items.Remove(item);
            }
            App.ViewModel.Save();

            App.ViewModel.IsEditMode = false;
            ApplicationBar = (Microsoft.Phone.Shell.ApplicationBar)Resources["StandardAppBar"]; 
        }


        private void About_Click(object sender, EventArgs e)
        {
            DavuxLibSL.App.Assembly = Assembly.GetExecutingAssembly();
            NavigationService.Navigate(new Uri("/DavuxLibSL;component/About.xaml", UriKind.RelativeOrAbsolute));
        }
    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is bool)
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            if (value is bool)
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}