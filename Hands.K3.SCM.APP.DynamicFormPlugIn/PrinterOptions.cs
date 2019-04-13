using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS;
using System.ComponentModel;
using Kingdee.BOS.Util;
using Hands.K3.SCM.APP.Utils.Utils;
using Newtonsoft.Json;
using Kingdee.BOS.Core.DynamicForm;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    [Description("打印机选项动态表单插件")]
    public class PrinterOptions : AbstractDynamicFormPlugIn
    {
        public List<string> numbers = null;
        public string GetSelectedPrinterName()
        {
            string printerName = Convert.ToString(this.View.Model.GetValue("F_HS_SelectPrinter"));

            return printerName;
        }
        public override void OnInitialize(InitializeEventArgs e)
        {
            base.OnInitialize(e);
            numbers = JsonConvert.DeserializeObject<List<string>>(e.Paramter.GetCustomParameter("trackingNumber").ToString());

        }

        public override void CreateNewData(BizDataEventArgs e)
        {
            base.CreateNewData(e);

            ComboFieldEditor comboEidtor = this.View.GetControl<ComboFieldEditor>("F_HS_SelectPrinter");
            List<EnumItem> comboOptions = new List<EnumItem>();
            List<string> printers = PrintUtil.GetPrinterList();

            comboOptions.Add(new EnumItem()
            { EnumId = "", Value = "", Caption = new LocaleValue("") }); // 空选项

            for (int i = 0; i < printers.Count; i++) 
            {
                string ProjectName = printers[i];
                comboOptions.Add(
                new EnumItem()
                { EnumId = "" + i + 1, Value = ProjectName, Caption = new LocaleValue(ProjectName) });
            }

            comboEidtor.SetComboItems(comboOptions);
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            base.ButtonClick(e);

            if (e.Key.EqualsIgnoreCase("F_HS_Confirm"))
            {
                try
                {
                    string printerName = GetSelectedPrinterName();

                    if (numbers != null && numbers.Count > 0)
                    {
                        foreach (var num in numbers)
                        {
                            if (num != null)
                            {
                                PrintUtil.Print("C:\\Fedex\\" + num + ".pdf", printerName);
                            }
                        }
                    }

                    this.View.Close();
                }
                catch (Exception ex)
                {
                    this.View.ShowErrMessage(ex.Message+Environment.NewLine+ex.StackTrace,"",MessageBoxType.Error);
                }
                
            }
            else if (e.Key.EqualsIgnoreCase("F_HS_Cancel"))
            {
                this.View.Close();
            }
        }
    }
}
