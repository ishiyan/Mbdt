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
using System.Reflection;

namespace EuronextInstrumentIndexConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string dotXml = ".xml";
        private string initialDirectory = ".";
        private Brush initialInputForeground = null;
        private Brush initialOutputForeground = null;
        private Parser parser = new Parser();
        public static Parser MasterParser = new Parser();
        private int currentInstrument = -1;

        private void UpdateButtons()
        {
            if (1 > parser.Count || -1 == currentInstrument)
            {
                buttonFirst.IsEnabled = false;
                buttonPrev.IsEnabled = false;
                buttonNext.IsEnabled = false;
                buttonLast.IsEnabled = false;
                currentInstrumentTextBlock.Text = string.Empty;
                parseTextBox.Text = string.Empty;
                indexExpander.IsExpanded = false;
                stockExpander.IsExpanded = false;
                inavExpander.IsExpanded = false;
                etfExpander.IsExpanded = false;
                etvExpander.IsExpanded = false;
                fundExpander.IsExpanded = false;
            }
            else
            {
                buttonFirst.IsEnabled = currentInstrument > 0;
                buttonPrev.IsEnabled = currentInstrument > 0;
                buttonNext.IsEnabled = currentInstrument < parser.Count - 1;
                buttonLast.IsEnabled = currentInstrument < parser.Count - 1;
                currentInstrumentTextBlock.Text = string.Format("{0} of {1}", currentInstrument + 1, parser.Count);
                parseTextBox.Text = string.Empty;
                parser.OriginalInstrumentList[currentInstrument].List.ForEach(s => { parseTextBox.AppendText(s + Environment.NewLine); });
                ConvertedInstrument convertedInstrument = parser.ConvertedInstrumentList[currentInstrument];
                convertedBinding.DataContext = convertedInstrument;
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

        private void buttonSearchMaster_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count && -1 != currentInstrument && 0 == MasterParser.ProblemCount)
            {
                ConvertedInstrument convertedInstrument = parser.ConvertedInstrumentList[currentInstrument];
                masterTextBox.Clear();
                StringWriter stringWriter = new StringWriter();
                string file = convertedInstrument.File;
                FileInfo fileInfo = null == file ? null : new FileInfo(file);
                MasterParser.ConvertedInstrumentList.FindAll(t =>
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Web browser looks ugly...
            //this.ExtendGlassFrame();
            MasterParser.Parse("master.xml", false);
            if (0 < MasterParser.ProblemCount)
            {
                masterTextBlock.Foreground = Brushes.Red;
                masterTextBox.Foreground = Brushes.Red;
                MasterParser.ProblemList.ForEach(t => masterTextBox.AppendText(t + Environment.NewLine));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            buttonParse.IsEnabled = false;
            buttonSaveThis.IsEnabled = false;
            buttonSaveAll.IsEnabled = false;
            buttonFirst.IsEnabled = false;
            buttonPrev.IsEnabled = false;
            buttonNext.IsEnabled = false;
            buttonLast.IsEnabled = false;
            if (null != App.CommandArgs && !string.IsNullOrEmpty(App.CommandArgs[0]))
                inputFileTextBox.Text = App.CommandArgs[0];
            initialInputForeground = inputFileTextBox.Foreground;
            initialOutputForeground = outputFileTextBox.Foreground;
        }

        private void buttonSelectParse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.InitialDirectory = initialDirectory;
            dlg.Multiselect = false;
            dlg.ValidateNames = true;
            dlg.Title = "Select an instrument index to open";
            dlg.FileName = inputFileTextBox.Text;
            dlg.DefaultExt = dotXml;
            dlg.Filter = "Xml documents (.xml)|*.xml";
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                inputFileTextBox.Text = dlg.FileName;
            }
        }

        private void buttonParse_Click(object sender, RoutedEventArgs e)
        {
            buttonFirst.IsEnabled = false;
            buttonPrev.IsEnabled = false;
            buttonNext.IsEnabled = false;
            buttonLast.IsEnabled = false;
            currentInstrumentTextBlock.Text = string.Empty;
            string file = inputFileTextBox.Text;
            if (!string.IsNullOrEmpty(file))
            {
                if (!file.EndsWith(dotXml))
                    file += dotXml;
                if (File.Exists(file))
                {
                    parser.Parse(file, false);
                    logListBox.ItemsSource = 0 == parser.ProblemCount ? null : parser.ProblemList;
                    currentInstrument = 0 == parser.Count ? -1 : 0;
                    UpdateButtons();
                }
                else
                    logListBox.ItemsSource = null;
            }
            else
                logListBox.ItemsSource = null;
        }

        private void goindexButton_Click(object sender, RoutedEventArgs e)
        {
            int index;
            if (0 < parser.Count && int.TryParse(goindexTextBox.Text, out index))
            {
                if (index > parser.Count)
                    index = parser.Count;
                index--;
                if (0 > index)
                    index = 0;
                currentInstrument = index;
                UpdateButtons();
            }
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            logListBox.ItemsSource = null;
        }

        private void buttonFirst_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count)
            {
                currentInstrument = 0;
                UpdateButtons();
            }
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count && 0 < currentInstrument)
            {
                currentInstrument--;
                UpdateButtons();
            }
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count && parser.Count > currentInstrument + 1)
            {
                currentInstrument++;
                UpdateButtons();
            }
        }

        private void buttonLast_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count)
            {
                currentInstrument = parser.Count - 1;
                UpdateButtons();
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
            string file = outputFileTextBox.Text;
            if (!string.IsNullOrEmpty(file))
            {
                if (!file.EndsWith(dotXml))
                    file += dotXml;
                parser.Save(file, currentInstrument);
            }
        }

        private void buttonSaveAll_Click(object sender, RoutedEventArgs e)
        {
            string file = outputFileTextBox.Text;
            if (!string.IsNullOrEmpty(file))
            {
                if (!file.EndsWith(dotXml))
                    file += dotXml;
                parser.Save(file);
            }
        }

        private void inputFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string file = inputFileTextBox.Text;
            if (!string.IsNullOrEmpty(file))
            {
                if (!file.EndsWith(dotXml))
                    file += dotXml;
                if (File.Exists(file))
                {
                    buttonParse.IsEnabled = true;
                    inputFileTextBox.Foreground = initialInputForeground;
                }
                else
                {
                    buttonParse.IsEnabled = false;
                    inputFileTextBox.Foreground = Brushes.Red;
                }
            }
            else
                buttonParse.IsEnabled = false;
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
                    buttonSaveAll.IsEnabled = true;
                    outputFileTextBox.Foreground = initialOutputForeground;
                }
                else
                {
                    buttonSaveThis.IsEnabled = false;
                    buttonSaveAll.IsEnabled = false;
                    outputFileTextBox.Foreground = Brushes.Red;
                }
            }
            else
            {
                buttonSaveThis.IsEnabled = false;
                buttonSaveAll.IsEnabled = false;
            }
        }

        private void isinTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (parser.ConvertedInstrumentList.Count > currentInstrument)
                webBrowser.Navigate(new Uri(parser.ConvertedInstrumentList[currentInstrument].EuronextIsinSearch));
        }

        private void buttonClone_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count && -1 != currentInstrument)
            {
                NewWindow newWindow = new NewWindow();
                newWindow.ConvertedInstrument = parser.ConvertedInstrumentList[currentInstrument];
                newWindow.Show();
            }
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            NewWindow newWindow = new NewWindow();
            newWindow.ConvertedInstrument = new ConvertedInstrument();
            newWindow.Show();
        }

        private void buttonCreateCurrentFiles_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count && -1 != currentInstrument)
                parser.ConvertedInstrumentList[currentInstrument].CreateFiles();
        }

        private void buttonCreateAllFiles_Click(object sender, RoutedEventArgs e)
        {
            if (0 < parser.Count)
                parser.ConvertedInstrumentList.ForEach(t => t.CreateFiles());
        }

    }
}
