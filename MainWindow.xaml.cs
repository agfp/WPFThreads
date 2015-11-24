using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Regex _regex;
        ConcurrentDictionary<string, int> _dictionary = new ConcurrentDictionary<string,int>(); 
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool ValidateForm()
        {
            int wordLength;
            if (!Int32.TryParse(txtWordLength.Text, out wordLength))
            {
                MessageBox.Show("Word length must be a number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (wordLength < 1)
            {
                MessageBox.Show("Word length must be greater than 0", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show("Invalid folder to analyse.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private async Task LoopFiles()
        {
            var wordLength = Int32.Parse(txtWordLength.Text);
            _regex = new Regex(@"[\w'-]{" + wordLength + ",}"); // Regex for English words
            _dictionary.Clear();

            var files = Directory.GetFiles(txtFolder.Text, "*.txt");
            await Task.Run(() => Parallel.ForEach(files, ProcessFile));
        }

        //================================================================================
        // ProcessFile method assumes that the files can be safely read line by line
        //================================================================================
        private void ProcessFile(string filePath)
        {
            using (var stream = new StreamReader(filePath))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    var matches = _regex.Matches(line);
                    foreach (Match match in matches)
                    {
                        var word = match.Value;
                        _dictionary.AddOrUpdate(word, 1, (key, existingValue) => existingValue + 1);
                    }
                }
            }
        }
        
        //================================================================================
        // PrintResults method prints the top 100 most used words 
        //================================================================================
        private void PrintResults()
        {
            var orderedData = _dictionary.OrderByDescending(o => o.Value).Take(100);
            var sb = new StringBuilder();
            foreach (var data in orderedData)
            {
                sb.AppendFormat("{0} - {1}\n", data.Value, data.Key);
            }
            txtResults.Text = sb.ToString();
        }

        private async void btnRunAnalysis_Click(object sender, RoutedEventArgs e)
        {
            btnRunAnalysis.IsEnabled = false;
            if (ValidateForm())
            {
                await LoopFiles();
                PrintResults();
            }
            btnRunAnalysis.IsEnabled = true;
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFolder.Text = dialog.SelectedPath;
            }
        }
    }
}
