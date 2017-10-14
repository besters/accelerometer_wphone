using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.Devices.Sensors;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Popups;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace AccelerometrVSB
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class MainPage : Page
    {

        private Accelerometer _accelerometer;
        
        public MainPage()
        {
            this.InitializeComponent();


            //nastaveni defaultniho textu
            txtData.Text = "DATASET: \n";

            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;
                // Assign an event handler for the reading-changed event
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }

        private async void ClearStorageButton_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new Windows.UI.Popups.MessageDialog(
                "Opravdu si přejete smazat všechna uložená data? " +
                "Budou nenávratně smazána.");

            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ano", new UICommandInvokedHandler(this.clearDataStorage)));
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Ne") { Id = 1 });

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var btn = sender as Button;
            var result = await dialog.ShowAsync();
            
        }

        private async void clearDataStorage(IUICommand command)
        {
            string DataFile = @"Assets\storage.txt";
            Windows.Storage.StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync(DataFile);

            // REMOVE FILE
            await sampleFile.DeleteAsync();
            // CREATE NEW FILE
            await storageFolder.CreateFileAsync(DataFile, Windows.Storage.CreationCollisionOption.ReplaceExisting);

            txtData.Text = "Data byla smazána!";
        }

        bool DataChanged = false;
        string old_x = "", old_y = "", old_z = "";


        StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;


        private async void exportData_Click(object sender, RoutedEventArgs e)
        {
           
            

            Windows.ApplicationModel.Email.EmailMessage emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient("prochazka@unodor.cz"));
            string messageBody = "Exported DATA - accelerometer";
            emailMessage.Body = messageBody;
            string DataFile = @"Assets\storage.txt";
            Windows.Storage.StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            Windows.Storage.StorageFile attachmentFile = await storageFolder.GetFileAsync(DataFile);

            if (attachmentFile != null)
            {
                var stream = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(attachmentFile);
                var attachment = new Windows.ApplicationModel.Email.EmailAttachment(
                         attachmentFile.Name,
                         stream);
                emailMessage.Attachments.Add(attachment);
            }
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);

            /*
            IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();
            //create new file
            using (StreamWriter SW = new StreamWriter(new IsolatedStorageFileStream("Info.txt", FileMode.Create, FileAccess.Write, ISF)))
            {
                string text = "Hi this is the text which will be written to the file and we can retrieve that later";
                SW.WriteLine(text);
                SW.Close();
                MessageBox.Show("text has been saved successfully to the file");
            }

            StorageFolder newFolder = KnownFolders.DocumentsLibrary;
            StorageFile file = await newFolder.CreateFileAsync("export.txt", CreationCollisionOption.ReplaceExisting);

            string ReadedData = await FileIO.ReadTextAsync(file);
            await FileIO.WriteTextAsync(file,ReadedData);
            */

            /*
            FileSavePicker savePicker = new FileSavePicker();

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            savePicker.SuggestedFileName = "export_accelerometer";

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAndContinue();
            if (file != null)
            {
                string DataFile = @"Assets\storage.txt";
                Windows.Storage.StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync(DataFile);
                Windows.Storage.CachedFileManager.DeferUpdates(sampleFile);
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, sampleFile.Name);
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(sampleFile);
            }
            */

        }

        int numero = 0;
        bool changer;
        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            DateTime localDate = DateTime.Now;
              
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

               AccelerometerReading reading = e.Reading;
              
                txtXAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                txtYAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                txtZAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);


                if (numero > 100)
                {
                    txtXoldAxis.Text = "SAVING:" + numero;
                    numero += 1;
                    if (numero > 1000)
                    {
                        changer = true;
                        numero = 0;
                    }
                }
                else
                {
                    numero += 1;
                    txtXoldAxis.Text = "" + numero;
                }
                if (DataChanged == false){
                    old_x = txtXAxis.Text;
                    old_y = txtYAxis.Text;
                    old_z = txtZAxis.Text;
                    txtXoldAxis.Text = old_x;
                    txtYoldAxis.Text = old_y;
                    txtZoldAxis.Text = old_z;

                    DataChanged = true;
                }
                else
                {
                        old_x = txtXAxis.Text;
                        old_y = txtYAxis.Text;
                        old_z = txtZAxis.Text;
                        DataChanged = true;
                        // print and SAVE DATA
                        string new_line = localDate + "|" + old_x + "|" + old_y + "|" + old_z + "\n";
                        
                            // WORK WITH FILE storage.txt
                            string DataFile = @"Assets\storage.txt";
                            Windows.Storage.StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                            Windows.Storage.StorageFile sampleFile = await storageFolder.GetFileAsync(DataFile);
                            //string oldData = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

                            var stream = await sampleFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
                            string new_data;

                            using (var outputStream = stream.GetOutputStreamAt(0))
                            {
                                using (var dataWriter = new Windows.Storage.Streams.DataWriter(outputStream))
                                {
                                    if (changer == true)
                                    {
                                        new_data = new_line;
                                        dataWriter.WriteString(new_data);
                                        await dataWriter.StoreAsync();
                                        await outputStream.FlushAsync();
                                        changer = false;
                                        txtData.Text = new_data;
                                    }
                                    else
                                    {
                                       
                                    }
                                }
                            }

                            stream.Dispose();
                        
                        
                            

                }
            });
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
    }
}
