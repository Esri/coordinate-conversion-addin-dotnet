/******************************************************************************* 
  * Copyright 2015 Esri 
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using CoordinateConversionLibrary.Helpers;

namespace CoordinateConversionLibrary.Views
{
    /// <summary>
    /// Interaction logic for OutputCoordinateView.xaml
    /// </summary>
    public partial class OutputCoordinateView : UserControl
    {
        public OutputCoordinateView()
        {
            InitializeComponent();
        }

        #region DraggedItem

        /// <summary>
        /// DraggedItem Dependency Property
        /// </summary>
        public static readonly DependencyProperty DraggedItemProperty =
            DependencyProperty.Register("DraggedItem", typeof(OutputCoordinateModel), typeof(OutputCoordinateView));

        /// <summary>
        /// Gets or sets the DraggedItem property.  This dependency property 
        /// indicates ....
        /// </summary>
        public OutputCoordinateModel DraggedItem
        {
            get { return (OutputCoordinateModel)GetValue(DraggedItemProperty); }
            set { SetValue(DraggedItemProperty, value); }
        }

        #endregion

        #region Drag and Drop Rows

        /// <summary>
        /// Keeps in mind whether
        /// </summary>
        public bool IsDragging { get; set; }
        /// <summary>
        /// Initiates a drag action if the grid is not in edit mode.
        /// </summary>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is DataGridRowHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep is DataGridRowHeader)
            {

                while (dep != null && !(dep is System.Windows.Controls.DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep is System.Windows.Controls.DataGridRow)
                {
                    IsDragging = true;
                    DraggedItem = (dep as System.Windows.Controls.DataGridRow).Item as OutputCoordinateModel;
                }
            }
        }

        /// <summary>
        /// Completes a drag/drop operation.
        /// </summary>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsDragging)
            {
                return;
            }

            //get the target item
            OutputCoordinateModel targetItem = (OutputCoordinateModel)ocGrid.SelectedItem;

            if (targetItem == null || !ReferenceEquals(DraggedItem, targetItem))
            {
                var list = CoordinateConversionViewModel.AddInConfig.OutputCoordinateList;
                
                //remove the source from the list
                list.Remove(DraggedItem);

                //get target index
                var targetIndex = list.IndexOf(targetItem);

                //move source at the target's location
                list.Insert(targetIndex, DraggedItem);

                //select the dropped item
                ocGrid.SelectedItem = DraggedItem;
                
                var vm = DataContext as OutputCoordinateViewModel;
                if (vm != null)
                {
                    // save the config file
                    vm.SaveOutputConfiguration();
                }
            }

            //reset
            ResetDragDrop();
        }


        /// <summary>
        /// Closes the popup and resets the
        /// grid to read-enabled mode.
        /// </summary>
        private void ResetDragDrop()
        {
            IsDragging = false;
            popup1.IsOpen = false;
            ocGrid.IsReadOnly = false;
        }


        /// <summary>
        /// Updates the popup's position in case of a drag/drop operation.
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging || e.LeftButton != MouseButtonState.Pressed)
            {
                //reset
                //ResetDragDrop();
                return;
            }

            //display the popup if it hasn't been opened yet
            if (!popup1.IsOpen)
            {
                //switch to read-only mode
                ocGrid.IsReadOnly = true;

                //make sure the popup is visible
                popup1.IsOpen = true;
            }


            Size popupSize = new Size(popup1.ActualWidth, popup1.ActualHeight);
            popup1.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);

            //make sure the row under the grid is being selected
            Point position = e.GetPosition(ocGrid);
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(ocGrid, position);
            if (row != null) ocGrid.SelectedItem = row.Item;
        }

        #endregion

        private void ocView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as OutputCoordinateViewModel;
            if (vm == null)
                return;

            // load the config file
            //vm.LoadOutputConfiguration();
            CoordinateConversionViewModel.AddInConfig.LoadConfiguration();
        }

        private void ocView_Unloaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as OutputCoordinateViewModel;
            if (vm == null)
                return;

            // save the config file
            vm.SaveOutputConfiguration();
        }

    }
}
