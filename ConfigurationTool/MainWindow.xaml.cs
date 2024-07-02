using ConfigurationTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ConfigurationTool
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            DataContext = mainWindowViewModel;
        }
    }
}
