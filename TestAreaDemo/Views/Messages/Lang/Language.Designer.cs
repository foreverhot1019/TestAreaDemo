﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestAreaDemo.Views.Messages.Lang {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Language {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Language() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TestAreaDemo.Views.Messages.Lang.Language", typeof(Language).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 新增时间 的本地化字符串。
        /// </summary>
        public static string AddDate {
            get {
                return ResourceManager.GetString("AddDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 新增人 的本地化字符串。
        /// </summary>
        public static string AddUser {
            get {
                return ResourceManager.GetString("AddUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 内容 的本地化字符串。
        /// </summary>
        public static string Content {
            get {
                return ResourceManager.GetString("Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 信息类型 的本地化字符串。
        /// </summary>
        public static string MsgType {
            get {
                return ResourceManager.GetString("MsgType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标路径 的本地化字符串。
        /// </summary>
        public static string TargetPath {
            get {
                return ResourceManager.GetString("TargetPath", resourceCulture);
            }
        }
    }
}
