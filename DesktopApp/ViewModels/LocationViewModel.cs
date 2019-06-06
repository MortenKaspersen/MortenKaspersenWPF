using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopApp.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DesktopApp.ViewModels
{
    class LocationViewModel : BindableBase
    {
        public LocationViewModel(string title, Location location)
        {
            Title = title;
            CurrentLocation = location;
        }

        #region Properties
        string title;

        public string Title
        {
            get { return title; }
            set
            {
                SetProperty(ref title, value);
            }
        }

        Location currentLocation;

        public Location CurrentLocation
        {
            get { return currentLocation; }
            set
            {
                SetProperty(ref currentLocation, value);
            }
        }

        //bool isValid;

        public bool IsValid
        {
            get
            {
                bool isValid = true;
                if (string.IsNullOrWhiteSpace(CurrentLocation.ID))
                    isValid = false;
                if (string.IsNullOrWhiteSpace(CurrentLocation.Name))
                    isValid = false;
                return isValid;
            }
            //set
            //{
            //    SetProperty(ref isValid, value);
            //}
        }

        
        #endregion

        

    }
}
