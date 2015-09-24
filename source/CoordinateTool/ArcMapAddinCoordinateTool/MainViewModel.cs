using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Views;

namespace ArcMapAddinCoordinateTool.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            _coordinateToolView = new CoordinateToolView();
        }

        private CoordinateToolView _coordinateToolView;
        public CoordinateToolView CTView
        {
            get
            {
                return _coordinateToolView;
            }
            set
            {
                _coordinateToolView = value;
            }
        }
    }
}
