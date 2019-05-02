// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Windows;

namespace CoordinateConversionLibrary.Views
{
    /// <summary>
    /// Interaction logic for SelectCoordinateFieldsDialog.xaml
    /// </summary>
    public partial class AdditionalFieldsView : Window
    {
        public AdditionalFieldsView()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (dgFieldsInfo.Columns.Count > 0)
            {
                if (dgFieldsInfo.Columns.Count > 1)
                {
                    var col1 = dgFieldsInfo.Columns[0];
                    var colLast = dgFieldsInfo.Columns[dgFieldsInfo.Columns.Count - 1];
                    colLast.Width = this.ActualWidth - col1.ActualWidth - 19;
                }
                else
                {
                    var col1 = dgFieldsInfo.Columns[0];
                    col1.Width = this.ActualWidth - 19;
                }
            }
        }
    }
}
