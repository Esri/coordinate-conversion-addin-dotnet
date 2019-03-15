using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;
using CoordinateConversionLibrary.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProAppCoordConversionModule.ViewModels
{
    public class ProOutputCoordinateViewModel : OutputCoordinateViewModel
    {
        string headers = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}"
                        , "CType", "DVisibility", "Format", "Name", "OutputCoordinate", "SRFactoryCode", "SRName");

        #region overrides

        public override void OnAddNewOutputCoordinate(object obj)
        {
            var outputCoordItem = obj as OutputCoordinateModel;
            if (outputCoordItem == null)
                return;

            var dlg = new ProEditOutputCoordinateView(CoordinateConversionLibraryConfig.AddInConfig.DefaultFormatList, this.GetInUseNames(), new OutputCoordinateModel() { CType = outputCoordItem.CType, Format = outputCoordItem.Format, Name = outputCoordItem.Name, SRName = outputCoordItem.SRName, SRFactoryCode = outputCoordItem.SRFactoryCode });

            var vm = dlg.DataContext as EditOutputCoordinateViewModel;
            if (vm == null)
                return;

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

        public override void OnImportButtonCommand(object obj)
        {
            try
            {
                var openDialog = new OpenFileDialog();
                openDialog.Title = "Open File";
                openDialog.CheckFileExists = true;
                openDialog.CheckPathExists = true;
                openDialog.Filter = "csv files|*.csv";
                if (openDialog.ShowDialog() == true)
                {
                    var filePath = openDialog.FileName;
                    var s = File.ReadAllText(filePath);
                    var dt = new DataTable();
                    string[] tableData = s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var col = from cl in tableData[0].Split(",".ToCharArray())
                              select new DataColumn(cl);
                    dt.Columns.AddRange(col.ToArray());
                    (from st in tableData.Skip(1)
                     select dt.Rows.Add(st.Split(",".ToCharArray()))).ToList();
                    var temp = dt;
                    foreach (DataRow item in dt.Rows)
                    {
                        string itemName = Convert.ToString(item["Name"]);
                        var coordFormats = CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Where(x => x.Name == itemName).ToList();
                        if (coordFormats.Count > 0)
                        {
                            CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Where(x => x.Name == itemName).Select(x =>
                            {
                                x.CType = (CoordinateType)Enum.Parse(typeof(CoordinateType), Convert.ToString(item["Ctype"]));
                                x.DVisibility = (Visibility)Enum.Parse(typeof(Visibility), Convert.ToString(item["DVisibility"]));
                                x.Format = Convert.ToString(item["Format"]);
                                x.OutputCoordinate = Convert.ToString(item["OutputCoordinate"]);
                                x.SRFactoryCode = Convert.ToInt32(item["SRFactoryCode"]);
                                x.SRName = Convert.ToString(item["SRName"]);
                                return x;
                            }).ToList();
                        }
                        else
                        {
                            CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Add(
                            new OutputCoordinateModel()
                            {
                                CType = (CoordinateType)Enum.Parse(typeof(CoordinateType), Convert.ToString(item["Ctype"])),
                                DVisibility = (Visibility)Enum.Parse(typeof(Visibility), Convert.ToString(item["DVisibility"])),
                                Format = Convert.ToString(item["Format"]),
                                Name = itemName,
                                OutputCoordinate = Convert.ToString(item["OutputCoordinate"]),
                                SRFactoryCode = Convert.ToInt32(item["SRFactoryCode"]),
                                SRName = Convert.ToString(item["SRName"])
                            });
                        }
                    }
                }
            }
            catch (Exception)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Something went wrong.");
            }
        }

        public override void OnExportButtonCommand(object obj)
        {
            try
            {
                if (CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList.Count == 0)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No data available");
                    return;
                }
                var saveDialog = new SaveFileDialog();
                saveDialog.Title = "Save File";
                saveDialog.Filter = "csv files|*.csv";
                saveDialog.ShowDialog();
                var filePath = saveDialog.FileName;
                using (var file = File.CreateText(filePath))
                {
                    var list = CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList
                        .Select(x => new
                        {
                            CType = x.CType,
                            DVisibility = x.DVisibility,
                            Format = x.Format,
                            Name = x.Name,
                            OutputCoordinate = x.OutputCoordinate,
                            SRFactoryCode = x.SRFactoryCode,
                            SRName = x.SRName
                        });
                    file.Write(headers);
                    file.WriteLine();
                    foreach (var arr in list)
                    {
                        if (arr == null) continue;
                        var str = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6}"
                            , arr.CType, arr.DVisibility, arr.Format, arr.Name, arr.OutputCoordinate, arr.SRFactoryCode, arr.SRName);
                        file.Write(str);
                        file.WriteLine();
                    }
                }
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("File Exported to " + filePath);
            }
            catch (Exception)
            {
            }
        }

        public override void OnResetButtonCommand(object obj)
        {
            CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList = new System.Collections.ObjectModel.ObservableCollection<OutputCoordinateModel>();
            CoordinateConversionLibraryConfig.AddInConfig.SaveConfiguration();
            RaisePropertyChanged(() => CoordinateConversionLibraryConfig.AddInConfig.OutputCoordinateList);
        }
        #endregion
    }
}
