using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            txtResults.Text = await GetDataAsync();
        }
        private async Task<string> GetDataAsync()
        {
            var x = new HttpClient();
            var url = new Uri("http://www.cnn.com");
            return await x.GetStringAsync(url);
            //  At the point where the await shows up above...
            //  The UI thread that called this method and 
            //  created this task is released to do other things

            //  In a couple of seconds. data will return
            //  And will attempt to signal to UI thread
            //  "I'm ready with my data now"
            //  It does this using the normal API message 
            //  pump (which the UI must be able to receive). 
            //  However, in the button click above, 
            //  we told our UI thread SIT at this line 
            //  of code and do not process ANYTHING until 
            //  this task completes.
            //  We are now deadlocked, because we also won't
            //  be processing any WINAPI messages arriving on 
            //  the UI thread
        }
    }
}
