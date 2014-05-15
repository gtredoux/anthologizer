using System.Collections.ObjectModel;
using System.IO;
using com.renoster.AnthologizerIndexerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AnthologizerIndexerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> msgList = new ObservableCollection<string>();
        private Indexer indexer;
        private const int MAX_LIST_COUNT = 2000;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw;
            }
            LogListBox.ItemsSource = msgList;

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }
 
        private void IndexerOnOnFolderIndexing(DirectoryInfo d)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => Record("Starting indexing folder " + d.FullName)));            
        }

        private void Record(string msg)
        {
            msgList.Insert(0,msg);
            if (msgList.Count > MAX_LIST_COUNT)
                msgList.RemoveAt(msgList.Count-1);
            
            //this.LogListBox.Items.Insert(0,msg);
        }

        void indexer_OnFolderIndexed(System.IO.DirectoryInfo d)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => Record("Finished indexing folder " + d.FullName)));
        }

        void indexer_OnErrorIndexing(string name, Exception ex)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => Record("Error indexing " + name + " " + ex.Message)));
        }

        void indexer_OnFileIndexed(System.IO.FileInfo f)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => Record("Finished indexing file " + f.FullName)));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void IndexButton_Click(object sender, RoutedEventArgs e)
        {
            indexer =
                new Indexer(Properties.Settings.Default.root, Properties.Settings.Default.anthologiesDir);
            indexer.OnFileIndexed += indexer_OnFileIndexed;
            indexer.OnErrorIndexing += indexer_OnErrorIndexing;
            indexer.OnFolderIndexing += IndexerOnOnFolderIndexing;
            indexer.OnFolderIndexed += indexer_OnFolderIndexed;

            Task.Run(() => indexer.Index());
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (indexer != null)
            {
                indexer.Stop();
                indexer = null;
            }
        }
    }
}
