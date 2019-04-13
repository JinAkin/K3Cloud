using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Entity.EnumType
{
    /// <summary>
    /// 同步数据操作类型
    /// </summary>
    public enum SynOperationType
    {
        /// <summary>
        /// 无
        /// </summary>
        None,
        /// <summary>
        /// 保存
        /// </summary>
        SAVE,
        /// <summary>
        /// 删除
        /// </summary>
        DELETE,
        /// <summary>
        /// 更新
        /// </summary>
        UPDATE,
        /// <summary>
        /// 提交
        /// </summary>
        SUBMIT,
        /// <summary>
        /// 审核
        /// </summary>
        AUDIT,
        /// <summary>
        /// 反审核
        /// </summary>
        UNAUDIT,
        /// <summary>
        /// 分配
        /// </summary>
        ALLOT,
        /// <summary>
        /// 作废
        /// </summary>
        CANCEL,
        /// <summary>
        /// 改单删除后保存
        /// </summary>
        SAVE_AFTER_DELETE,
        /// <summary>
        /// 改单删除后保存(不发送改单请求)
        /// </summary>
        SAVE_AFTER_DELETE_WITHOUTREQ,
        /// <summary>
        /// 合单保存后作废
        /// </summary>
        CANCEL_AFTER_SAVE,
        /// <summary>
        /// 禁用
        /// </summary>
        FORBID,
        /// <summary>
        ///反禁用
        /// </summary>
        ENABLE,
        /// <summary>
        /// 下推
        /// </summary>
        PUSH,
        /// <summary>
        /// 提交审核后分配
        /// </summary>
        ALLOT_AFTER_SUMBIT,
        /// <summary>
        /// 审核后分配
        /// </summary>
        ALLOT_AFTER_AUDIT,
        /// <summary>
        /// 分配后更新
        /// </summary>
        UPDATE_AFTER_ALLOT

    }
}
