/******************************************************************************* 
  * Copyright 2016 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/ 
using System;
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using System.Windows.Data;
using System.Windows;
using CoordinateConversionLibrary.Helpers;

namespace ProAppCoordConversionModule.UI
{
    /// <summary>
    /// ViewModel for the embeddable control.
    /// </summary>
    internal class FlashEmbeddedControlViewModel : EmbeddableControl
    {
        public FlashEmbeddedControlViewModel(XElement options)
            : base(options)
        {
            FlashAnimationCompletedCommand = new RelayCommand(OnFlashAnimationCompletedCommand);
        }

        public RelayCommand FlashAnimationCompletedCommand { get; set; }

        private double _mapWidth = 1920.0;
        public double MapWidth
        {
            get
            {
                return _mapWidth;
            }

            set
            {
                SetProperty(ref _mapWidth, value, () => MapWidth);
            }
        }
        private double _mapHeight = 1080.0;
        public double MapHeight
        {
            get
            {
                return _mapHeight;
            }

            set
            {
                SetProperty(ref _mapHeight, value, () => MapHeight);
            }
        }
        
        private bool _flash = false;
        public bool Flash
        {
            get
            {
                return _flash;
            }
            set
            {
                SetProperty(ref _flash, value, () => Flash);
            }
        }
        private System.Windows.Point _clientPoint = new System.Windows.Point(0, 0);
        public System.Windows.Point ClientPoint
        {
            get { return _clientPoint; }
            set
            {
                SetProperty(ref _clientPoint, value, () => ClientPoint);
            }
        }
        private System.Windows.Point _screenPoint = new System.Windows.Point(0, 0);
        public System.Windows.Point ScreenPoint
        {
            get { return _screenPoint; }
            set
            {
                SetProperty(ref _screenPoint, value, () => ScreenPoint);
            }
        }

        private void OnFlashAnimationCompletedCommand(object obj)
        {
            Mediator.NotifyColleagues("FLASH_COMPLETED", null);
        }

        public void RunFlashAnimation()
        {
            Flash = true;
        }
    }

    internal class ScreenToClientPointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var screenPoint = (System.Windows.Point)values[0];
            var c = values[1] as System.Windows.Controls.Canvas;
            if (c == null)
                return -999;

            var source = PresentationSource.FromVisual(c);

            if (source == null)
                return 0;

            c.Width = (double)values[2] * 2.0;
            c.Height = (double)values[3] * 2.0;

            var point = c.PointFromScreen(screenPoint);
            var ps = parameter.ToString();
            if (ps == "X")
                return point.X;
            else if (ps == "Y")
                return point.Y;
            else if (ps == "NEGHALFWIDTH")
                return -1 * (c.ActualWidth / 2.0);
            else if (ps == "NEGHALFHEIGHT")
                return -1 * (c.ActualHeight / 2.0);

            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    internal class ScreenToClientPointMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var screenPoint = (System.Windows.Point)values[0];
            var c = values[1] as System.Windows.Controls.Canvas;
            if (c == null)
                return -999;

            var source = PresentationSource.FromVisual(c);

            if (source == null)
                return 0;

            c.Width = (double)values[2] * 2.0;
            c.Height = (double)values[3] * 2.0;

            var point = c.PointFromScreen(screenPoint);
            var halfWidth = c.ActualWidth / 2.0;
            var halfHeight = c.ActualHeight / 2.0;

            double x = point.X * 2.0;
            double y = point.Y * 2.0;

            if (point.Y > halfHeight)
                y -= (point.Y - halfHeight) + 6;

            if (point.X > halfWidth)
                x -= (point.X - halfWidth) + 6;

            return new Thickness(x, y, 0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
