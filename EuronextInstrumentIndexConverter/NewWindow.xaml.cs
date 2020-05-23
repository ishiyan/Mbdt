using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace EuronextInstrumentIndexConverter
{
    /// <summary>
    /// Interaction logic for NewWindow.xaml
    /// </summary>
    public partial class NewWindow : Window
    {
        private const string dotXml = ".xml";
        private string initialDirectory = ".";
        private Brush initialOutputForeground = null;
        private ConvertedInstrument convertedInstrument = null;
        private Parser masterParser = MainWindow.MasterParser;

        public ConvertedInstrument ConvertedInstrument
        {
           get { return convertedInstrument; }
           set { convertedInstrument = value; convertedBinding.DataContext = convertedInstrument; UpdateButtons(); }
        }

        private void UpdateButtons()
        {
            if (null == convertedInstrument)
            {
                indexExpander.IsExpanded = false;
                stockExpander.IsExpanded = false;
                inavExpander.IsExpanded = false;
                etfExpander.IsExpanded = false;
                etvExpander.IsExpanded = false;
                fundExpander.IsExpanded = false;
            }
            else
            {
                string type = convertedInstrument.Type;
                indexExpander.IsExpanded = "index" == type;
                stockExpander.IsExpanded = "stock" == type;
                inavExpander.IsExpanded = "inav" == type;
                etfExpander.IsExpanded = "etf" == type;
                etvExpander.IsExpanded = "etv" == type;
                fundExpander.IsExpanded = "fund" == type;
                buttonSearchMaster_Click(null, null);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Web browser looks ugly...
            //this.ExtendGlassFrame();
        }

        public NewWindow()
        {
            InitializeComponent();
            buttonSaveThis.IsEnabled = false;
            initialOutputForeground = outputFileTextBox.Foreground;
            convertedBinding.DataContext = convertedInstrument;
            if (0 < masterParser.ProblemCount)
            {
                masterTextBlock.Foreground = Brushes.Red;
                masterTextBox.Foreground = Brushes.Red;
                masterParser.ProblemList.ForEach(t => masterTextBox.AppendText(t + Environment.NewLine));
            }
        }

        private void buttonSelectSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.CheckFileExists = true;
            dlg.InitialDirectory = initialDirectory;
            dlg.ValidateNames = true;
            dlg.Title = "Select an instrument index to save";
            dlg.FileName = outputFileTextBox.Text;
            dlg.DefaultExt = dotXml;
            dlg.Filter = "Xml documents (.xml)|*.xml";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                this.outputFileTextBox.Text = dlg.FileName;
            }
        }

        private void buttonSaveThis_Click(object sender, RoutedEventArgs e)
        {
            string fileName = outputFileTextBox.Text;
            if (!string.IsNullOrEmpty(fileName) && null != convertedInstrument)
            {
                if (!fileName.EndsWith(dotXml))
                    fileName += dotXml;
                using (StreamWriter file = new StreamWriter(fileName))
                {
                    file.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    file.WriteLine("<instruments>");
                    convertedInstrument.Save(file);
                    file.WriteLine();
                    file.WriteLine("</instruments>");
                }
            }
        }

        private void outputFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string file = outputFileTextBox.Text;
            if (!string.IsNullOrEmpty(file))
            {
                if (!file.EndsWith(dotXml))
                    file += dotXml;
                if (!File.Exists(file))
                {
                    buttonSaveThis.IsEnabled = true;
                    outputFileTextBox.Foreground = initialOutputForeground;
                }
                else
                {
                    buttonSaveThis.IsEnabled = false;
                    outputFileTextBox.Foreground = Brushes.Red;
                }
            }
            else
            {
                buttonSaveThis.IsEnabled = false;
            }
        }

        private void buttonSearchMaster_Click(object sender, RoutedEventArgs e)
        {
            if (null != convertedInstrument && 0 == masterParser.ProblemCount)
            {
                masterTextBox.Clear();
                StringWriter stringWriter = new StringWriter();
                string file = convertedInstrument.File;
                FileInfo fileInfo = null == file ? null : new FileInfo(file);
                masterParser.ConvertedInstrumentList.FindAll(t =>
                    (!string.IsNullOrEmpty(convertedInstrument.Isin) && convertedInstrument.Isin.Equals(t.Isin)) ||
                    (!string.IsNullOrEmpty(convertedInstrument.Symbol) && convertedInstrument.Symbol.Equals(t.Symbol)) ||
                    (!string.IsNullOrEmpty(convertedInstrument.Name) && convertedInstrument.Name.Equals(t.Name)) ||
                    (!string.IsNullOrEmpty(convertedInstrument.Symbol) && t.File.EndsWith(convertedInstrument.Symbol + dotXml)) ||
                    (null != fileInfo && t.File.EndsWith(fileInfo.Name))
                ).ForEach(s =>
                {
                    masterTextBox.AppendText(s.FinderHeadline + Environment.NewLine);
                    s.Save(stringWriter);
                });
                masterTextBox.AppendText(stringWriter.ToString());
            }
        }

        private void buttonCreateFiles_Click(object sender, RoutedEventArgs e)
        {
            if (null != convertedInstrument)
                convertedInstrument.CreateFiles();
        }

        private void isinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null != convertedInstrument)
            {
                webBrowser.Navigate(new Uri(convertedInstrument.EuronextIsinSearch));
                UpdateButtons();
            }
        }

        private void genericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null != convertedInstrument)
                UpdateButtons();
        }

    }
}
