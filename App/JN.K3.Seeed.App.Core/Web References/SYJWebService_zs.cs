﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码由 wsdl 自动生成, Version=4.0.30319.18020。
// 
namespace Hands.K3.SCM.App.Core
{
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="IWebServiceSoapBinding", Namespace="http://webservice.channel.douples.com/")]
    public partial class SYJWebService_zs : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback queryOrderDetailsByQcCodeOperationCompleted;
        
        private System.Threading.SendOrPostCallback holleOperationCompleted;
        
        private System.Threading.SendOrPostCallback queryOrderInfoByIdOperationCompleted;
        
        private System.Threading.SendOrPostCallback createCustomerOrderOperationCompleted;
        
        private System.Threading.SendOrPostCallback queryWipProduceScheduleOperationCompleted;
        
        /// <remarks/>
        public SYJWebService_zs(string ip)
        {
            this.Url = ip;// "http://120.78.88.146/ws/WebService";
        }
        
        /// <remarks/>
        public event queryOrderDetailsByQcCodeCompletedEventHandler queryOrderDetailsByQcCodeCompleted;
        
        /// <remarks/>
        public event holleCompletedEventHandler holleCompleted;
        
        /// <remarks/>
        public event queryOrderInfoByIdCompletedEventHandler queryOrderInfoByIdCompleted;
        
        /// <remarks/>
        public event createCustomerOrderCompletedEventHandler createCustomerOrderCompleted;
        
        /// <remarks/>
        public event queryWipProduceScheduleCompletedEventHandler queryWipProduceScheduleCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://webservice.channel.douples.com/", ResponseNamespace="http://webservice.channel.douples.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string queryOrderDetailsByQcCode([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg0, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg1, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg2, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg3, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg4) {
            object[] results = this.Invoke("queryOrderDetailsByQcCode", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3,
                        arg4});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginqueryOrderDetailsByQcCode(string arg0, string arg1, string arg2, string arg3, string arg4, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("queryOrderDetailsByQcCode", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3,
                        arg4}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndqueryOrderDetailsByQcCode(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void queryOrderDetailsByQcCodeAsync(string arg0, string arg1, string arg2, string arg3, string arg4) {
            this.queryOrderDetailsByQcCodeAsync(arg0, arg1, arg2, arg3, arg4, null);
        }
        
        /// <remarks/>
        public void queryOrderDetailsByQcCodeAsync(string arg0, string arg1, string arg2, string arg3, string arg4, object userState) {
            if ((this.queryOrderDetailsByQcCodeOperationCompleted == null)) {
                this.queryOrderDetailsByQcCodeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnqueryOrderDetailsByQcCodeOperationCompleted);
            }
            this.InvokeAsync("queryOrderDetailsByQcCode", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3,
                        arg4}, this.queryOrderDetailsByQcCodeOperationCompleted, userState);
        }
        
        private void OnqueryOrderDetailsByQcCodeOperationCompleted(object arg) {
            if ((this.queryOrderDetailsByQcCodeCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.queryOrderDetailsByQcCodeCompleted(this, new queryOrderDetailsByQcCodeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://webservice.channel.douples.com/", ResponseNamespace="http://webservice.channel.douples.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void holle() {
            this.Invoke("holle", new object[0]);
        }
        
        /// <remarks/>
        public System.IAsyncResult Beginholle(System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("holle", new object[0], callback, asyncState);
        }
        
        /// <remarks/>
        public void Endholle(System.IAsyncResult asyncResult) {
            this.EndInvoke(asyncResult);
        }
        
        /// <remarks/>
        public void holleAsync() {
            this.holleAsync(null);
        }
        
        /// <remarks/>
        public void holleAsync(object userState) {
            if ((this.holleOperationCompleted == null)) {
                this.holleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnholleOperationCompleted);
            }
            this.InvokeAsync("holle", new object[0], this.holleOperationCompleted, userState);
        }
        
        private void OnholleOperationCompleted(object arg) {
            if ((this.holleCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.holleCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://webservice.channel.douples.com/", ResponseNamespace="http://webservice.channel.douples.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string queryOrderInfoById([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg0, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg1, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg2, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg3) {
            object[] results = this.Invoke("queryOrderInfoById", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginqueryOrderInfoById(string arg0, string arg1, string arg2, string arg3, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("queryOrderInfoById", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndqueryOrderInfoById(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void queryOrderInfoByIdAsync(string arg0, string arg1, string arg2, string arg3) {
            this.queryOrderInfoByIdAsync(arg0, arg1, arg2, arg3, null);
        }
        
        /// <remarks/>
        public void queryOrderInfoByIdAsync(string arg0, string arg1, string arg2, string arg3, object userState) {
            if ((this.queryOrderInfoByIdOperationCompleted == null)) {
                this.queryOrderInfoByIdOperationCompleted = new System.Threading.SendOrPostCallback(this.OnqueryOrderInfoByIdOperationCompleted);
            }
            this.InvokeAsync("queryOrderInfoById", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3}, this.queryOrderInfoByIdOperationCompleted, userState);
        }
        
        private void OnqueryOrderInfoByIdOperationCompleted(object arg) {
            if ((this.queryOrderInfoByIdCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.queryOrderInfoByIdCompleted(this, new queryOrderInfoByIdCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://webservice.channel.douples.com/", ResponseNamespace="http://webservice.channel.douples.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string createCustomerOrder([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg0, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg1, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg2, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg3, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg4) {
            object[] results = this.Invoke("createCustomerOrder", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3,
                        arg4});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BegincreateCustomerOrder(string arg0, string arg1, string arg2, string arg3, string arg4, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("createCustomerOrder", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3,
                        arg4}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndcreateCustomerOrder(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void createCustomerOrderAsync(string arg0, string arg1, string arg2, string arg3, string arg4) {
            this.createCustomerOrderAsync(arg0, arg1, arg2, arg3, arg4, null);
        }
        
        /// <remarks/>
        public void createCustomerOrderAsync(string arg0, string arg1, string arg2, string arg3, string arg4, object userState) {
            if ((this.createCustomerOrderOperationCompleted == null)) {
                this.createCustomerOrderOperationCompleted = new System.Threading.SendOrPostCallback(this.OncreateCustomerOrderOperationCompleted);
            }
            this.InvokeAsync("createCustomerOrder", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3,
                        arg4}, this.createCustomerOrderOperationCompleted, userState);
        }
        
        private void OncreateCustomerOrderOperationCompleted(object arg) {
            if ((this.createCustomerOrderCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.createCustomerOrderCompleted(this, new createCustomerOrderCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://webservice.channel.douples.com/", ResponseNamespace="http://webservice.channel.douples.com/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string queryWipProduceSchedule([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg0, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg1, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg2, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string arg3) {
            object[] results = this.Invoke("queryWipProduceSchedule", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginqueryWipProduceSchedule(string arg0, string arg1, string arg2, string arg3, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("queryWipProduceSchedule", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndqueryWipProduceSchedule(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void queryWipProduceScheduleAsync(string arg0, string arg1, string arg2, string arg3) {
            this.queryWipProduceScheduleAsync(arg0, arg1, arg2, arg3, null);
        }
        
        /// <remarks/>
        public void queryWipProduceScheduleAsync(string arg0, string arg1, string arg2, string arg3, object userState) {
            if ((this.queryWipProduceScheduleOperationCompleted == null)) {
                this.queryWipProduceScheduleOperationCompleted = new System.Threading.SendOrPostCallback(this.OnqueryWipProduceScheduleOperationCompleted);
            }
            this.InvokeAsync("queryWipProduceSchedule", new object[] {
                        arg0,
                        arg1,
                        arg2,
                        arg3}, this.queryWipProduceScheduleOperationCompleted, userState);
        }
        
        private void OnqueryWipProduceScheduleOperationCompleted(object arg) {
            if ((this.queryWipProduceScheduleCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.queryWipProduceScheduleCompleted(this, new queryWipProduceScheduleCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    public delegate void queryOrderDetailsByQcCodeCompletedEventHandler(object sender, queryOrderDetailsByQcCodeCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class queryOrderDetailsByQcCodeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal queryOrderDetailsByQcCodeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    public delegate void holleCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    public delegate void queryOrderInfoByIdCompletedEventHandler(object sender, queryOrderInfoByIdCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class queryOrderInfoByIdCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal queryOrderInfoByIdCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    public delegate void createCustomerOrderCompletedEventHandler(object sender, createCustomerOrderCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class createCustomerOrderCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal createCustomerOrderCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    public delegate void queryWipProduceScheduleCompletedEventHandler(object sender, queryWipProduceScheduleCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.18020")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class queryWipProduceScheduleCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal queryWipProduceScheduleCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}
