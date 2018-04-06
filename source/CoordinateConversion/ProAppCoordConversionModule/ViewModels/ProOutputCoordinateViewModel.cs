using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using CoordinateConversionLibrary.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProAppCoordConversionModule.ViewModels
{
    public class ProOutputCoordinateViewModel : OutputCoordinateViewModel
    {
        #region overrides

        public override void OnAddNewOutputCoordinate(object obj)
        {
            var outputCoordItem = obj as OutputCoordinateModel;

            if (outputCoordItem == null)
                return;

            var dlg = new ProEditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, this.GetInUseNames(), new OutputCoordinateModel() { CType = outputCoordItem.CType, Format = outputCoordItem.Format, Name = outputCoordItem.Name, SRName = outputCoordItem.SRName, SRFactoryCode = outputCoordItem.SRFactoryCode });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            vm.WindowTitle = CoordinateConversionLibrary.Properties.Resources.TitleAddNewOutputCoordinate;

            if (dlg.ShowDialog() == true)
            {
                outputCoordItem.Format = vm.Format;

                CoordinateType type;
                if (Enum.TryParse<CoordinateType>(vm.CategorySelection, out type))
                {
                    outputCoordItem.CType = type;
                }

                outputCoordItem.Name = vm.OutputCoordItem.Name;
                outputCoordItem.SRFactoryCode = vm.OutputCoordItem.SRFactoryCode;
                outputCoordItem.SRName = vm.OutputCoordItem.SRName;

                CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Add(outputCoordItem);
                this.UpdateOutputs();
                CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
            }
        }

        public override void OnConfigCommand(object obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj as string))
                return;

            var outputCoordItem = GetOCMByName(obj as string);
            var InUseNames = GetInUseNames();
            InUseNames.Remove(outputCoordItem.Name);
            var dlg = new ProEditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, InUseNames,
                new OutputCoordinateModel()
                {
                    CType = outputCoordItem.CType,
                    Format = outputCoordItem.Format,
                    Name = outputCoordItem.Name,
                    SRName = outputCoordItem.SRName,
                    SRFactoryCode = outputCoordItem.SRFactoryCode
                });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            vm.WindowTitle = CoordinateConversionLibrary.Properties.Resources.TitleEditOutputCoordinate;

            if (dlg.ShowDialog() == true)
            {
                outputCoordItem.Name = vm.OutputCoordItem.Name;
                outputCoordItem.Format = vm.Format;
                outputCoordItem.SRFactoryCode = vm.OutputCoordItem.SRFactoryCode;
                outputCoordItem.SRName = vm.OutputCoordItem.SRName;

                CoordinateType type;
                if (Enum.TryParse<CoordinateType>(vm.CategorySelection, out type))
                {
                    outputCoordItem.CType = type;
                }

                UpdateOutputs();
            }

            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
        }

        #endregion
    }
}
