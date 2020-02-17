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

using System;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

namespace ProAppCoordConversionModule.Common
{
    /// <summary>
    /// Attribute for localization.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
    {
        #region Public methods.
        // ------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizableDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="resourcesType">Type of the resources.</param>
        public LocalizableDescriptionAttribute(string description, Type resourcesType)
            : base(description)
        {
            _resourcesType = resourcesType;
        }

        // ------------------------------------------------------------------
        #endregion

        #region Public properties.
        // ------------------------------------------------------------------

        /// <summary>
        /// Get the string value from the resources.
        /// </summary>
        /// <value></value>
        /// <returns>The description stored in this attribute.</returns>
        public override string Description
        {
            get
            {
                if (!_isLocalized)
                {
                    ResourceManager resMan =
                         _resourcesType.InvokeMember(
                         @"ResourceManager",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as ResourceManager;

                    CultureInfo culture =
                         _resourcesType.InvokeMember(
                         @"Culture",
                         BindingFlags.GetProperty | BindingFlags.Static |
                         BindingFlags.Public | BindingFlags.NonPublic,
                         null,
                         null,
                         new object[] { }) as CultureInfo;

                    _isLocalized = true;

                    if (resMan != null)
                    {
                        DescriptionValue =
                             resMan.GetString(DescriptionValue, culture);
                    }
                }

                return DescriptionValue;
            }
        }

        // ------------------------------------------------------------------
        #endregion

        #region Private variables.
        // ------------------------------------------------------------------

        private readonly Type _resourcesType;
        private bool _isLocalized;

        // ------------------------------------------------------------------
        #endregion
    }
}
