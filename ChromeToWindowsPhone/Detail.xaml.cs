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
using Microsoft.Phone.Tasks;
using System.Diagnostics;

namespace SendToWP7
{
    public partial class Detail : PhoneApplicationPage
    {
        public Detail()
        {
            InitializeComponent();

            DataContext = App.ViewModel.SelectedRecord;
            DavuxLibSL.State.Set("DetailView", App.ViewModel.SelectedRecord);
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            EmailComposeTask emc = new EmailComposeTask();
            emc.Subject = "Shared: " + App.ViewModel.SelectedRecord.Title;

            string body = App.ViewModel.SelectedRecord.URL + "\n\n" +
                App.ViewModel.SelectedRecord.Selection + "\n\n" +
                "Sent from Send to WP7 on Windows Phone";
            emc.Body = body;
            emc.Show();
        }

        private void Trash_Click(object sender, EventArgs e)
        {
            App.ViewModel.Items.Remove(App.ViewModel.SelectedRecord);
            App.ViewModel.SelectedRecord = null;
            NavigationService.GoBack();
        }
    }
}