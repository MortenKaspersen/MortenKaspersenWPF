using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DesktopApp.Models;
using System.Windows.Threading;
using DesktopApp.Utilities;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Controls;
using DesktopApp.Views;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using DesktopApp.Data;
using System.Windows.Media;
using System.Runtime.CompilerServices;

//Der er i denne opgave taget udgangspunkt i AgentAssignment fra undervisningen! 

namespace DesktopApp.ViewModels
{
    class MainWindowViewModelLocations : BindableBase
    {
        private ObservableCollection<Location> locations;
        private string filePath = "";
        private DispatcherTimer timer = new DispatcherTimer();
        private readonly string AppTitle = "Tree Location Desktop App";

        public MainWindowViewModelLocations()
        {
            locations = new ObservableCollection<Location>
            {
                #if DEBUG
                new Location("001", "Red Wood", "Cali Street 12", "12 Red Woods, 1 Pine"),
                new Location("002", "Boxwood", "Paradise Falls", "12 Boxwoods")
                #endif
            };
            CurrentLocation = null;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            clock.Update();
        }

        #region Properties

        public ObservableCollection<Location> Locations
        {
            get { return locations; }
            set { SetProperty(ref locations, value); }
        }

        

        public Location CurrentLocation
        {
            get { return currentLocation; }
            set
            {
                SetProperty(ref currentLocation, value);
            }
        }

        int currentIndex = -1;
        public int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                SetProperty(ref currentIndex, value);
            }
        }

        Clock clock = new Clock();
        public Clock Clock { get => clock; set => clock = value; }


        Object _selectedSortOrder = "None";
        public Object SelectedSortOrder
        {
            get { return _selectedSortOrder; }
            set
            {
                SetProperty(ref _selectedSortOrder, value);
                ICollectionView cv = CollectionViewSource.GetDefaultView(Locations);
                var newSortOrder = value as ComboBoxItem;
                var sortDesc = new SortDescription(newSortOrder.Tag.ToString(), ListSortDirection.Ascending);
                if (cv != null)
                {
                    cv.SortDescriptions.Clear();
                    if (newSortOrder.Tag.ToString() != "None")
                        cv.SortDescriptions.Add(sortDesc);
                }
            }
        }


        private string filename = "";
        public string Filename
        {
            get { return filename; }
            set
            {
                SetProperty(ref filename, value);
                RaisePropertyChanged("Title");
            }
        }

        public string Title
        {
            get
            {
                var s = "";
                if (Dirty)
                    s = "*";
                return Filename + s + " - " + AppTitle;
            }
        }

        private bool dirty = false;
        public bool Dirty
        {
            get { return dirty; }
            set
            {
                SetProperty(ref dirty, value);
                RaisePropertyChanged("Title");
            }
        }

        #endregion

        #region Commands

        ICommand _PreviusCommand;
        public ICommand PreviusCommand
        {
            get
            {
                return _PreviusCommand ??
                (_PreviusCommand = new DelegateCommand(
                 PreviusCommandExecute, PreviusCommandCanExecute).ObservesProperty(() => CurrentIndex));
            }
        }

        private void PreviusCommandExecute()
        {
            if (CurrentIndex > 0)
                --CurrentIndex;
        }

        private bool PreviusCommandCanExecute()
        {
            if (CurrentIndex > 0)
                return true;
            else
                return false;
        }

        ICommand _nextCommand;
        public ICommand NextCommand
        {
            get
            {
                return _nextCommand ?? (_nextCommand = new DelegateCommand(
                       () => ++CurrentIndex,
                       () => CurrentIndex < (Locations.Count - 1)).ObservesProperty(() => CurrentIndex));
            }
        }

        ICommand _newCommand;
        public ICommand AddNewLocationCommand
        {
            get
            {
                return _newCommand ?? (_newCommand = new DelegateCommand(() =>
                {
                    var newLocation = new Location();
                    var vm = new LocationViewModel("Add new Location", newLocation);

                    var dlg = new LocationView()
                    {
                        DataContext = vm
                    };
                    if (dlg.ShowDialog() == true)
                    {
                        Locations.Add(newLocation);
                        CurrentLocation = newLocation;
                        Dirty = true;
                    }
                }));
            }
        }

        ICommand _deleteCommand;
        public ICommand DeleteLocationCommand => _deleteCommand ?? (_deleteCommand =
            new DelegateCommand(DeleteLocation, DeleteLocation_CanExecute)
                    .ObservesProperty(() => CurrentIndex));

        private void DeleteLocation()
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to delete agent " + CurrentLocation.Name +
                "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.Yes)
            {
                Locations.Remove(CurrentLocation);
                Dirty = true;
            }
        }

        private bool DeleteLocation_CanExecute()
        {
            if (Locations.Count > 0 && CurrentIndex >= 0)
                return true;
            else
                return false;
        }

        ICommand _editCommand;
        public ICommand EditLocationCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new DelegateCommand(() =>
                {
                    var tempLocation = CurrentLocation.Clone();
                    var vm = new LocationViewModel("Edit Location", tempLocation);

                    var dlg = new LocationView()
                    {
                        DataContext = vm,
                        Owner = App.Current.MainWindow
                    };
                    if (dlg.ShowDialog() == true)
                    {
                        // Copy values back
                        CurrentLocation.ID = tempLocation.ID;
                        CurrentLocation.Name = tempLocation.Name;
                        CurrentLocation.Adresse = tempLocation.Adresse;
                        CurrentLocation.Måletræer = tempLocation.Måletræer;
                        Dirty = true;
                    }
                },
                () =>
                {
                    return CurrentIndex >= 0;
                }
                ).ObservesProperty(() => CurrentIndex));
            }
        }

        ICommand _closeAppCommand;
        public ICommand CloseAppCommand
        {
            get
            {
                return _closeAppCommand ?? (_closeAppCommand = new DelegateCommand(() =>
                {
                    Application.Current.MainWindow.Close();
                }));
            }
        }

        ICommand _SaveAsCommand;
        public ICommand SaveAsCommand
        {
            get { return _SaveAsCommand ?? (_SaveAsCommand = new DelegateCommand<string>(SaveAsCommand_Execute)); }
        }

        private void SaveAsCommand_Execute(string argFilename)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Tree Location Desktop App documents|*.agn|All Files|*.*",
                DefaultExt = "agn"
            };
            if (filePath == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                dialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                filePath = dialog.FileName;
                Filename = Path.GetFileName(filePath);
                SaveFile();
            }
        }

        ICommand _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _SaveCommand ?? (_SaveCommand = new DelegateCommand(SaveFileCommand_Execute, SaveFileCommand_CanExecute)
                  .ObservesProperty(() => Locations.Count));
            }
        }

        private void SaveFileCommand_Execute()
        {
            SaveFile();
        }

        private bool SaveFileCommand_CanExecute()
        {
            return (filename != "") && (Locations.Count > 0);
        }

        private void SaveFile()
        {
            try
            {
                LocationRepository.SaveFile(filePath, Locations);
                Dirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to save file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        ICommand _NewFileCommand;
        public ICommand NewFileCommand
        {
            get { return _NewFileCommand ?? (_NewFileCommand = new DelegateCommand(NewFileCommand_Execute)); }
        }

        private void NewFileCommand_Execute()
        {
            MessageBoxResult res = MessageBox.Show("Any unsaved data will be lost. Are you sure you want to initiate a new file?", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.Yes)
            {
                Locations.Clear();
                Filename = "";
                Dirty = false;
            }
        }


        ICommand _OpenFileCommand;
        public ICommand OpenFileCommand
        {
            get { return _OpenFileCommand ?? (_OpenFileCommand = new DelegateCommand<string>(OpenFileCommand_Execute)); }
        }

        private void OpenFileCommand_Execute(string argFilename)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Tree Location Desktop App documents|*.agn|All Files|*.*",
                DefaultExt = "agn"
            };
            if (filePath == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                dialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                filePath = dialog.FileName;
                Filename = Path.GetFileName(filePath);
                try
                {
                    LocationRepository.ReadFile(filePath, out ObservableCollection<Location> tempLocations);
                    Locations = tempLocations;
                    Dirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to open file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        ICommand _closingCommand;
        public ICommand ClosingCommand
        {
            get
            {
                return _closingCommand ?? (_closingCommand = new
                  DelegateCommand<CancelEventArgs>(ClosingCommand_Execute));
            }
        }

        private void ClosingCommand_Execute(CancelEventArgs arg)
        {
            if (Dirty)
                arg.Cancel = UserRegrets();
        }

        private bool UserRegrets()
        {
            var regret = false;
            MessageBoxResult res = MessageBox.Show("You have unsaved data. Are you sure you want to close the application without saving data first?",
                            "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.No)
            {
                regret = true;
            }
            return regret;
        }

        ICommand _ColorCommand;
        private Location currentLocation;

        public ICommand ColorCommand
        {
            get
            {
                return _ColorCommand ?? (_ColorCommand = new
                  DelegateCommand<String>(ColorCommand_Execute));
            }
        }

        private void ColorCommand_Execute(String colorStr)
        {
            SolidColorBrush newBrush = SystemColors.WindowBrush; // Default color

            try
            {
                if (colorStr != null)
                {
                    if (colorStr != "Default")
                        newBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unknown color name, default color is used", "Program error!");
            }

            Application.Current.Resources["BackgroundBrush"] = newBrush;
        }

        #endregion


        


    }
}
