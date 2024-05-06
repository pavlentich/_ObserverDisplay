using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.IO;
using System.Text;
using System.Reflection;
//using Microsoft.Azure.PowerShell.Cmdlets.Fleet.Runtime;

namespace ObserverDisplay
{
    public sealed partial class MainPage : Page
    {
        private StreamSocket socket = new StreamSocket();        
        private StreamWriter streamWriter;
        private DispatcherTimer timer;
        private int testCounter = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {
                await InitializeSocket();
                if (socket == null || socket.Information == null || socket.Information.RemoteAddress == null)
                {
                    ;
                }
            }
            catch (Exception ex)
            {
                string message = " 42 Error initializing socket: " + ClearExeptionMessage(ex.Message);
                adderMethod(CounterRichTextBlock, message);                
                StartServerCheckTimer(); // // if server absent - run check timer 

            }
        }

        private void StartServerCheckTimer()
        {
            ;
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(2); // 
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            testCounter++;
            string message = " 61 Trying to connect to server ";
            adderMethod(CounterRichTextBlock, message);
            try
            {
                if (socket == null || socket.Information == null || socket.Information.RemoteAddress == null)
                {
                    await InitializeSocket(); // if connection was lost or was absent - try to renew it
                }
                ;
                message = " 71 it seems server  connected   ";
                timer.Stop();                
                adderMethod(CounterRichTextBlock, message);
            }
            catch (Exception ex)
            {
                message = " 78 Error checking server status: " + ClearExeptionMessage(ex.Message);
                adderMethod(CounterRichTextBlock, message);

            }
        }
        private async Task InitializeSocket()
        {
            socket = new StreamSocket();
            await socket.ConnectAsync(new Windows.Networking.HostName("localhost"), "12345");

            // Create a StreamWriter instead of DataWriter
            streamWriter = new StreamWriter(socket.OutputStream.AsStreamForWrite(), Encoding.UTF8);

            // create cancel token and initiallize it 
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;                        
            _ = ReadDataFromSocketAsync(cancellationToken);// call  ReadDataFromSocketAsync with cancel token
        }

        private async Task ReadDataFromSocketAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var reader = new StreamReader(socket.InputStream.AsStreamForRead(), Encoding.UTF8))
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Check for cancellation before awaiting
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // Handle cancellation appropriately
                            return;
                        }

                        
                        string data = await reader.ReadLineAsync();// Await the read operation

                        // Process the data as usual
                        if (!string.IsNullOrEmpty(data))
                        {
                            try
                            {
                                var jsonData = JsonConvert.DeserializeObject<dynamic>(data);
                                string message = "Counter: " + jsonData.CounterValue + ", TimeMark: " + jsonData.TimeMark;
                                ((CounterRichTextBlock.Blocks[0] as Paragraph).Inlines[0] as Run).Text = message;
                            }
                            catch (JsonReaderException)
                            {
                                string message = data;
                                adderMethod(CounterRichTextBlock, message);
                  //              timer.Stop();
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                // Handle cancellation appropriately
                string message = " 134 OperationCanceledException: " + ClearExeptionMessage(ex.Message);

                ;
                //adderMethod(CounterRichTextBlock, message);
                adderMethod(CounterRichTextBlock, message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                ;
                string message = " 145 something really wrong: " + ClearExeptionMessage(ex.Message);
                ;
                
                adderMethod(CounterRichTextBlock, message);  // it seems serever died, run the timer to check is alive                 
                ;
                socket = null;// Clear socket/
                StartServerCheckTimer();
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (streamWriter != null)
                {
                    await streamWriter.WriteLineAsync("RESET");
                    string message = " 163 trying Send RESET";
                    adderMethod(CounterRichTextBlock, message);//CounterRichTextBlock.Text = 
                    await streamWriter.FlushAsync();
                    message = "166 Sended"; ; //CounterRichTextBlock.Text = CounterRichTextBlock.Text = 
                    adderMethod(CounterRichTextBlock, message);
                }
                else adderMethod(CounterRichTextBlock, " Server is not ready");
            }
            catch (Exception ex)
            {
                string message = "172 Error sending command to server: " + ClearExeptionMessage(ex.Message); //CounterRichTextBlock.Text = "Error sending command to server: " + ex.Message;CounterRichTextBlock.Text = 
                adderMethod(CounterRichTextBlock, message);

            }
        }
        private async void HelloButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(streamWriter != null)
                { 

                    await streamWriter.WriteLineAsync("HELLO");
                    string message = " 183 trying Send HELLO";
                    adderMethod(CounterRichTextBlock, message);//CounterRichTextBlock.Text = 
                    await streamWriter.FlushAsync();
                    message = "186 Sended"; ; //CounterRichTextBlock.Text = CounterRichTextBlock.Text = 
                    adderMethod(CounterRichTextBlock, message);
                }
                else  adderMethod(CounterRichTextBlock, " Server is not ready");

            }
            catch (Exception ex)
            {
                string message = "191 Error sending command to server: " + ClearExeptionMessage(ex.Message); //CounterRichTextBlock.Text = "Error sending command to server: " + ex.Message;CounterRichTextBlock.Text = 
                adderMethod(CounterRichTextBlock, message);

            }
        }

        private async void StopResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await streamWriter.WriteLineAsync("\"STOP\"");

                string message = " 204 truing Send STOP"; //CounterRichTextBlock.Text = "truing Send STOP";CounterRichTextBlock.Text = "Error sending command to server: " + ex.Message;CounterRichTextBlock.Text = 
                adderMethod(CounterRichTextBlock, message);
                await streamWriter.FlushAsync();
                message = " 207 Sended"; //CounterRichTextBlock.Text = "truing Send STOP";CounterRichTextBlock.Text = "Error sending command to server: " + ex.Message;CounterRichTextBlock.Text = 
                adderMethod(CounterRichTextBlock, message);
            }
            catch (Exception ex)
            {
                string message = " 212 Error sending command to server: " + ClearExeptionMessage(ex.Message); //CounterRichTextBlock.Text = "Error sending command to server: " + ex.Message;CounterRichTextBlock.Text = 
                adderMethod(CounterRichTextBlock, message);
            }
        }

        private string ClearExeptionMessage(string edited_message)
        {            
            int index = edited_message.IndexOf("\r"); 
            if (index != -1) // 
            {
                edited_message = edited_message.Substring(0, index); 
            }
            return edited_message;
        }
        private void adderMethod(RichTextBlock ConsoleField, string Message)
        {
            Run run = new Run();  
            run.Text = Message; 
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(run);
            CounterRichTextBlock.Blocks.Insert(1, paragraph);  
        }
    }
}