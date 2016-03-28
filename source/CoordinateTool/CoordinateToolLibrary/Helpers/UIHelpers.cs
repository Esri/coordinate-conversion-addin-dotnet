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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CoordinateConversionLibrary.Helpers
{
  /// <summary>
  /// Common UI related helper methods.
  /// </summary>
  public static class UIHelpers
  {
      public static void UpdateHistory(string input, ObservableCollection<string> list)
      {
            // lets save the last 5 coordinates
            if(!list.Any())
            {
                list.Add(input);
                return;
            }

            if(list.Contains(input))
            {
                list.Remove(input);
                list.Insert(0, input);
            }
            else
            {
                if(input.Length > 1)
                {
                    // check to see if someone is typing the coordinate
                    // only keep the latest
                    var temp = input.Substring(0, input.Length - 1);

                    if(list[0] == temp)
                    {
                        // replace
                        list.Remove(temp);
                        list.Insert(0, input);
                    }
                    else
                    {
                        list.Insert(0, input);

                        while (list.Count > Constants.MAX_HISTORY_COUNT)
                        {
                            list.RemoveAt(Constants.MAX_HISTORY_COUNT);
                        }
                    }
                }
            }
      }

    #region find parent

    /// <summary>
    /// Finds a parent of a given item on the visual tree.
    /// </summary>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="child">A direct or indirect child of the
    /// queried item.</param>
    /// <returns>The first parent item that matches the submitted
    /// type parameter. If not matching item can be found, a null
    /// reference is being returned.</returns>
    public static T TryFindParent<T>(DependencyObject child)
      where T : DependencyObject
    {
      //get parent item
      DependencyObject parentObject = GetParentObject(child);

      //we've reached the end of the tree
      if (parentObject == null) return null;

      //check if the parent matches the type we're looking for
      T parent = parentObject as T;
      if (parent != null)
      {
        return parent;
      }
      else
      {
        //use recursion to proceed with next level
        return TryFindParent<T>(parentObject);
      }
    }


    /// <summary>
    /// This method is an alternative to WPF's
    /// <see cref="VisualTreeHelper.GetParent"/> method, which also
    /// supports content elements. Do note, that for content element,
    /// this method falls back to the logical tree of the element.
    /// </summary>
    /// <param name="child">The item to be processed.</param>
    /// <returns>The submitted item's parent, if available. Otherwise
    /// null.</returns>
    public static DependencyObject GetParentObject(DependencyObject child)
    {
      if (child == null) return null;
      ContentElement contentElement = child as ContentElement;

      if (contentElement != null)
      {
        DependencyObject parent = ContentOperations.GetParent(contentElement);
        if (parent != null) return parent;

        FrameworkContentElement fce = contentElement as FrameworkContentElement;
        return fce != null ? fce.Parent : null;
      }

      //if it's not a ContentElement, rely on VisualTreeHelper
      return VisualTreeHelper.GetParent(child);
    }

    #endregion


    #region update binding sources

    /// <summary>
    /// Recursively processes a given dependency object and all its
    /// children, and updates sources of all objects that use a
    /// binding expression on a given property.
    /// </summary>
    /// <param name="obj">The dependency object that marks a starting
    /// point. This could be a dialog window or a panel control that
    /// hosts bound controls.</param>
    /// <param name="properties">The properties to be updated if
    /// <paramref name="obj"/> or one of its childs provide it along
    /// with a binding expression.</param>
    public static void UpdateBindingSources(DependencyObject obj,
                              params DependencyProperty[] properties)
    {
      foreach (DependencyProperty depProperty in properties)
      {
        //check whether the submitted object provides a bound property
        //that matches the property parameters
        BindingExpression be = BindingOperations.GetBindingExpression(obj, depProperty);
        if (be != null) be.UpdateSource();
      }

      int count = VisualTreeHelper.GetChildrenCount(obj);
      for (int i = 0; i < count; i++)
      {
        //process child items recursively
        DependencyObject childObject = VisualTreeHelper.GetChild(obj, i);
        UpdateBindingSources(childObject, properties);
      }
    }

    #endregion


    /// <summary>
    /// Tries to locate a given item within the visual tree,
    /// starting with the dependency object at a given position. 
    /// </summary>
    /// <typeparam name="T">The type of the element to be found
    /// on the visual tree of the element at the given location.</typeparam>
    /// <param name="reference">The main element which is used to perform
    /// hit testing.</param>
    /// <param name="point">The position to be evaluated on the origin.</param>
    public static T TryFindFromPoint<T>(UIElement reference, Point point)
      where T : DependencyObject
    {
      DependencyObject element = reference.InputHitTest(point)
                                   as DependencyObject;
      if (element == null) return null;
      else if (element is T) return (T)element;
      else return TryFindParent<T>(element);
    }
  }
}
