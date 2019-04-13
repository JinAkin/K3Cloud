using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.DynamicFormPlugIn
{
    public class S160425ShowInteractionEdit : AbstractDynamicFormPlugIn
    {
        private dynamic _customMessageModel;
        /// <summary>
        /// 接收操作给出的自定义消息对象
        /// </summary>
        /// <param name="e"></param>
        public override void OnInitialize(InitializeEventArgs e)
        {
            // 取到操作插件传出的自定义消息对象：后面需要把消息对象中的内容，显示到界面上
            this._customMessageModel = e.Paramter.GetCustomParameter("CustomMessageModel");
        }
        /// <summary>
        /// 在界面上显示消息内容
        /// </summary>
        /// <param name="e"></param>
        public override void AfterBindData(EventArgs e)
        {
            if (this._customMessageModel != null)
            {
                this.View.GetControl("F_JD_MessageTitle").Text = this._customMessageModel.MessageTitle;
                this.View.GetControl("F_JD_MessageContent").Text = this._customMessageModel.MessageContent;
            }
        }
        /// <summary>
        /// 用户选择完毕，系统继续
        /// </summary>
        /// <param name="e"></param>
        public override void ButtonClick(ButtonClickEventArgs e)
        {
            if (e.Key.EqualsIgnoreCase("F_JD_Yes"))
            {// 用户选择了继续

                // 是否继续
                bool redo = true;
                // 在此执行操作时，需要传递给服务端插件的内容，如用户选择的选项，录入的数值等：
                // 服务端插件，需要据此进行后续处理
                dynamic redoParameter = new System.Dynamic.ExpandoObject();
                redoParameter.Field1 = "用户录入的值（仅供演示）";
                // 把需要传递给服务操作插件的数据，包在InteractionFormResult对象中，返回给父界面
                this.View.ReturnToParentWindow(new InteractionFormResult(redo, redoParameter));
                // 关闭消息显示界面
                this.View.Close();
            }
            else if (e.Key.EqualsIgnoreCase("F_JD_No"))
            {
                // 不需继续，直接关闭界面即可
                this.View.Close();
            }
        }
    }
}
