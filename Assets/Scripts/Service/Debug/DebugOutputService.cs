using UnityEngine;

namespace Service.Diagnostics
{
    public static class DebugOutputService
    {
        /// <summary>非致命：引用为空但仍可运行</summary>
        public static void RunNullDebug(GameObject gameObject, string componentName)
        {
            string goName = gameObject != null ? gameObject.name : "<null GameObject>";
            Debug.LogWarning(
                $"[Character] 组件引用为空: {componentName} on GameObject: {goName}",
                gameObject);
        }

        /// <summary>致命：引用为空且无法继续运行</summary>
        public static void RunNullFatal(GameObject gameObject, string componentName)
        {
            string goName = gameObject != null ? gameObject.name : "<null GameObject>";
            Debug.LogError(
                $"[Character] 必需组件引用为空: {componentName} on GameObject: {goName}，" +
                $"已禁用该组件。请在 Inspector 中拖入引用。",
                gameObject);
        }

        /// <summary>可选组件缺失：只提示，不禁用</summary>
        public static void RunNullOptional(GameObject gameObject, string componentName, string reason = null)
        {
            string goName = gameObject != null ? gameObject.name : "<null GameObject>";
            string msg = $"[Character] 可选配置 {componentName} 未设置 on {goName}";
            if (!string.IsNullOrEmpty(reason)) msg += $"，{reason}";
            Debug.Log(msg, gameObject);
        }
    }
}