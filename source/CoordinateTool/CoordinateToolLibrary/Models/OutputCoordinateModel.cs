using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CoordinateToolLibrary.Helpers;

namespace CoordinateToolLibrary.Models
{
    public class OutputCoordinateModel : NotificationObject
    {
        public OutputCoordinateModel()
        {
            DVisibility = Visibility.Collapsed;
        }

        public Visibility DVisibility { get; set; }

        #region Name

        private string name = String.Empty;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        #endregion

        public Dictionary<string, string> Props { get; set; }

        #region OutputCoordinate

        private string outputCoordinate = String.Empty;


        public string OutputCoordinate
        {
            get { return outputCoordinate; }
            set
            {
                outputCoordinate = value;
                RaisePropertyChanged(() => OutputCoordinate);
            }
        }

        #endregion

        public void ToggleVisibility()
        {
            if (this.DVisibility == Visibility.Collapsed)
                this.DVisibility = Visibility.Visible;
            else
                this.DVisibility = Visibility.Collapsed;

            RaisePropertyChanged(() => DVisibility);
        }
    }
}
