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

using System;
using System.Windows.Data;
using System.Windows.Controls;
using CoordinateConversionLibrary;
using CoordinateConversionLibrary.Helpers;
using CoordinateConversionLibrary.Views;
using CoordinateConversionLibrary.ViewModels;

namespace ArcMapAddinCoordinateConversion.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            ConvertTabView = new CCConvertTabView();
            ConvertTabView.DataContext = new ConvertTabViewModel();

            CollectTabView = new CCCollectTabView();
            CollectTabView.DataContext = new CollectTabViewModel();

            //HasInputError = false;
            //IsToolGenerated = false;
        }

        //public bool IsHistoryUpdate { get; set; }
        //public bool IsToolGenerated { get; set; }

        public CCConvertTabView ConvertTabView { get; set; }
        public CCCollectTabView CollectTabView { get; set; }

        object selectedTab = null;
        public object SelectedTab
        {
            get { return selectedTab; }
            set
            {
                if (selectedTab == value)
                    return;

                selectedTab = value;
                var tabItem = selectedTab as TabItem;
                Mediator.NotifyColleagues(Constants.TAB_ITEM_SELECTED, ((tabItem.Content as UserControl).Content as UserControl).DataContext);
                //TODO let the other viewmodels determine what to do when tab selection changes
                if (tabItem.Header.ToString() == CoordinateConversionLibrary.Properties.Resources.HeaderCollect)
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Collect);
                else
                    Mediator.NotifyColleagues(CoordinateConversionLibrary.Constants.SetToolMode, MapPointToolMode.Convert);
            }
        }
    }
}
