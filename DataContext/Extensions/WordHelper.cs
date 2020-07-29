using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Aspose.Words;
using System.Drawing;

namespace DataContext.Extensions
{
    public class WordHelper
    {
        public WordHelper()
        {
            Aspose.Words.License license = new Aspose.Words.License();
            SetAsposeLicense(license);
        }

        private static void SetAsposeLicense(Aspose.Words.License license)
        {
            //            string strLic = @"<License>
            //                                  <Data>
            //                                    <SerialNumber>aed83727-21cc-4a91-bea4-2607bf991c21</SerialNumber>
            //                                    <EditionType>Enterprise</EditionType>
            //                                    <Products>
            //                                      <Product>Aspose.Total</Product>
            //                                    </Products>
            //                                  </Data>
            //                                  <Signature>CxoBmxzcdRLLiQi1kzt5oSbz9GhuyHHOBgjTf5w/wJ1V+lzjBYi8o7PvqRwkdQo4tT4dk3PIJPbH9w5Lszei1SV/smkK8SCjR8kIWgLbOUFBvhD1Fn9KgDAQ8B11psxIWvepKidw8ZmDmbk9kdJbVBOkuAESXDdtDEDZMB/zL7Y=</Signature>
            //                                </License>";

            //            MemoryStream ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(strLic));
            //license.SetLicense(ms);

            string path = HttpContext.Current.Server.MapPath("/Aspose/License.lic");
            license.SetLicense(path);
        }

        /// <summary>
        /// 设置word模板的 书签值，生成新的word文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="wordModelPath">模板名称，会自动找模板</param>
        /// <param name="_SaveFormat">输出文件格式</param>
        /// <returns></returns>
        public static ReturnMsg SetWordModel_BookMarkByT<T>(T ObjT, string wordModelPath = "", SaveFormat _SaveFormat = SaveFormat.Doc)
            where T : class,new()
        {
            ReturnMsg retMsg = new ReturnMsg(true, "");
            try
            {
                wordModelPath = GetWordModelPath<T>(ObjT, wordModelPath);

                string wordModel_Path = wordModelPath;
                if (wordModelPath.IndexOf(":") < 0)
                    wordModel_Path = HttpContext.Current.Server.MapPath(wordModelPath);

                if (!File.Exists(wordModel_Path))
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件不存在";
                    return retMsg;
                }
                string FileClass = Common.GetFileClass(wordModel_Path);
                if (FileClass != "208207" && FileClass != "8075")
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件格式不正确";
                    return retMsg;
                }
                Document doc = new Aspose.Words.Document();

                try
                {
                    doc = new Aspose.Words.Document(wordModel_Path);
                }
                catch (Exception ex)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = Common.GetExceptionMsg(ex);
                    return retMsg;
                }

                if (doc == null)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件解析出错";
                    return retMsg;
                }
                if (ObjT == null)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "设置模板文件的数据不能为空";
                    return retMsg;
                }
                BookmarkCollection bookMarks = doc.Range.Bookmarks;
                if (bookMarks.Count <= 0)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件，不存在任何书签";
                    return retMsg;
                }
                else
                {
                    //使用DocumentBuilder对象插入一些文档对象，如插入书签，插入文本框，插入复选框，插入一个段落，插入空白页，追加或另一个word文件的内容等。
                    var builder = new DocumentBuilder(doc);

                    foreach (var bookmark in bookMarks)
                    {
                        var book_mark = bookmark as Aspose.Words.Bookmark;
                        book_mark.Text = "";// 清掉标示
                        //定位到指定位置进行插入操作
                        builder.MoveToBookmark(book_mark.Name);
                        PropertyInfo OPropertyInfo = Common.GetProtityInfoByFieldName(ObjT, book_mark.Name);
                        if (OPropertyInfo != null)
                        {
                            var objval = OPropertyInfo.GetValue(ObjT);
                            string DataType = OPropertyInfo.PropertyType.Name;
                            var DataTypeInfo = OPropertyInfo.PropertyType.GetTypeInfo();
                            if (objval != null)
                            {
                                //判断是否是泛型
                                if (OPropertyInfo.PropertyType.IsGenericType && OPropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    var Arguments = OPropertyInfo.PropertyType.GetGenericArguments();
                                    DataType = Arguments[0].Name;

                                    #region  创建List<T> 实例 并赋值

                                    Type ListType = typeof(List<>);
                                    ListType = ListType.MakeGenericType(OPropertyInfo.PropertyType);
                                    var ObjListT = Activator.CreateInstance(ListType);
                                    MethodInfo OListMethodInfo = ListType.GetMethod("Add");
                                    List<object> arrRepParam = new List<object>();
                                    arrRepParam.Add(objval);
                                    OListMethodInfo.Invoke(ObjListT, arrRepParam.ToArray());

                                    #endregion

                                    #region  创建WordHelper 实例 并调用 WriteTableByList 方法

                                    Type WordHelperType = typeof(WordHelper);
                                    var ObjWordHelper = Activator.CreateInstance(WordHelperType);
                                    MethodInfo OMethodInfo = WordHelperType.GetMethod("WriteTableByList").MakeGenericMethod(OPropertyInfo.PropertyType);
                                    List<object> arrWriteTableByListParam = new List<object>();
                                    arrWriteTableByListParam.Add(bookmark);
                                    arrWriteTableByListParam.Add(doc);
                                    arrWriteTableByListParam.Add(ObjListT);
                                    OMethodInfo.Invoke(ObjWordHelper, arrWriteTableByListParam.ToArray());

                                    #endregion
                                }
                                //判断是否派生自IEnumerable
                                else if (OPropertyInfo.PropertyType.GetInterface("IEnumerable", false) != null && DataType.ToLower().IndexOf("string")<0)
                                {
                                    var  Arrobjval = objval as System.Collections.IEnumerable;
                                    #region  创建List<T> 实例 并赋值

                                    Type ListTType = null;//泛型类
                                    var IEnumerableTypes = OPropertyInfo.PropertyType.GetGenericArguments();
                                    if (IEnumerableTypes.Any())
                                        ListTType = IEnumerableTypes[0];

                                    Type ListType = typeof(List<>);
                                    ListType = ListType.MakeGenericType(ListTType);
                                    var ObjListT = Activator.CreateInstance(ListType);
                                    MethodInfo AddMethodInfo = ListType.GetMethod("Add");
                                    foreach (var item in Arrobjval)
                                    {
                                        AddMethodInfo.Invoke(ObjListT, new object[] { item });
                                    }

                                    //var Methods = objval.GetType().GetMethods();
                                    //MethodInfo OListMethodInfo = objval.GetType().GetMethod("Count");
                                    //if (OListMethodInfo != null) 
                                    //{
                                    //    //OListMethodInfo = OListMethodInfo.MakeGenericMethod(ListTType);
                                    //    MethodInfo AddMethodInfo = ListType.GetMethod("Add");
                                    //    MethodInfo ToListMethodInfo = ListType.GetMethod("Add");
                                    //    if (ToListMethodInfo != null)
                                    //    {
                                    //        ObjListT = ToListMethodInfo.Invoke(retobj, null);
                                    //    }
                                    //}

                                    //MethodInfo OListMethodInfo = objval.GetType().GetMethod("GetEnumerator");
                                    //if (OListMethodInfo != null)
                                    //{
                                    //    var Enumeratorobj = OListMethodInfo.Invoke(objval, null) as System.Collections.IDictionaryEnumerator;
                                    //    Enumeratorobj.
                                    //}

                                    //List<object> arrRepParam = new List<object>();
                                    //foreach (var item in (objval as System.Collections.IList))
                                    //{
                                    //    arrRepParam = new List<object>();
                                    //    arrRepParam.Add(item);
                                    //    OListMethodInfo.Invoke(ObjListT, arrRepParam.ToArray());
                                    //}

                                    #endregion

                                    #region  创建WordHelper 实例 并调用 WordHelper 方法

                                    Type WordHelperType = typeof(WordHelper);
                                    var ObjWordHelper = Activator.CreateInstance(WordHelperType);
                                    MethodInfo[] MethodInfos = WordHelperType.GetMethods();
                                    MethodInfo OMethodInfo = MethodInfos.Where(x => x.Name.StartsWith("WriteTableByList") && x.IsGenericMethod).FirstOrDefault().MakeGenericMethod(ListTType);
                                    List<object> arrWriteTableByListParam = new List<object>();
                                    arrWriteTableByListParam.Add(bookmark);
                                    arrWriteTableByListParam.Add(doc);
                                    arrWriteTableByListParam.Add(ObjListT);
                                    
                                    OMethodInfo.Invoke(ObjWordHelper, arrWriteTableByListParam.ToArray());

                                    #endregion
                                }
                                else
                                {
                                    builder.Write(objval.ToString());
                                }
                            }
                        }
                    }
                    string NewFilePath = SaveDoc(doc, _SaveFormat);
                    retMsg.retResult = true;
                    retMsg.MsgStr = NewFilePath;
                }
            }
            catch (Exception ex)
            {
                retMsg.retResult = false;
                retMsg.MsgStr = Common.GetExceptionMsg(ex);
            }
            return retMsg;
        }

        /// <summary>
        /// 获取word模板 路径
        /// </summary>
        /// <param name="wordModelPath">模板名称，会自动找模板</param>
        /// <returns></returns>
        private static string GetWordModelPath<T>(T ObjT, string wordModelPath = "") where T : class,new()
        {
            if (string.IsNullOrEmpty(wordModelPath))
            {
                var TypeName = ObjT.GetType().ToString();
                if (TypeName.LastIndexOf('.') >= 0)
                    TypeName = TypeName.Substring(TypeName.LastIndexOf('.') + 1);
                wordModelPath = "/FileModel/WordModel/" + TypeName + ".docx";
                if (!File.Exists(HttpContext.Current.Server.MapPath(wordModelPath)))
                {
                    wordModelPath = "/FileModel/WordModel/" + TypeName + ".doc";
                    if (!File.Exists(HttpContext.Current.Server.MapPath(wordModelPath)))
                    {
                        wordModelPath = "/FileModel/" + TypeName + ".doc";
                        if (!File.Exists(HttpContext.Current.Server.MapPath(wordModelPath)))
                        {
                            wordModelPath = "/FileModel/" + TypeName + ".docx";
                        }
                    }
                }
            }
            else
            {
                string FileModelPath = System.Configuration.ConfigurationManager.AppSettings["FileModelPath"] == null ? "/FileModel/" : System.Configuration.ConfigurationManager.AppSettings["FileModelPath"].ToString();
                if (wordModelPath.IndexOf(FileModelPath) < 0)
                {
                    wordModelPath = "/" + FileModelPath + "/WordModel/" + wordModelPath;
                    if (!File.Exists(HttpContext.Current.Server.MapPath(wordModelPath)))
                    {
                        wordModelPath = "/" + FileModelPath + "/" + wordModelPath;
                    }
                }
            }

            return wordModelPath;
        }

        /// <summary>
        /// 保存word文档
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="_SaveFormat"></param>
        /// <param name="NewFilePath"></param>
        /// <returns>返回新文件路径</returns>
        private static string SaveDoc(Document doc, SaveFormat _SaveFormat = SaveFormat.Doc, string _NewFilePath = "")
        {
            string NewFilePath = _NewFilePath;
            if (string.IsNullOrEmpty(NewFilePath))
            {
                NewFilePath = System.Configuration.ConfigurationManager.AppSettings["FileDownLoadPath"] == null ? "/FileDownLoad/" : System.Configuration.ConfigurationManager.AppSettings["FileDownLoadPath"].ToString();
                NewFilePath = NewFilePath + "/WordModelOutPut/" + DateTime.Now.ToString("yyyy-MM/dd/");
                if (NewFilePath.IndexOf(":") < 0)
                    NewFilePath = HttpContext.Current.Server.MapPath(NewFilePath);
                //文件夹不存在的话，创建 文件夹路径
                if (!Directory.Exists(NewFilePath))
                    Directory.CreateDirectory(NewFilePath);

                NewFilePath += "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + (new Random()).Next(1, 999).ToString("000");
            }
            NewFilePath += "." + _SaveFormat.ToString().ToLower();
            doc.Save(NewFilePath, _SaveFormat);
            return NewFilePath;
        }

        /// <summary>
        /// 设置word模板的 书签值，生成多张图片
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjT"></param>
        /// <param name="wordModelPath"></param>
        /// <returns></returns>
        public static ReturnMsg SetWordModel_BookMarkImgByT<T>(T ObjT, string wordModelPath = "")
            where T : class,new()
        {
            ReturnMsg retMsg = new ReturnMsg(true, "");
            List<Image> ArrWordImg = new List<Image>();
            try
            {
                wordModelPath = GetWordModelPath<T>(ObjT, wordModelPath);
                string wordModel_Path = wordModelPath;
                if (wordModelPath.IndexOf(":") < 0)
                    wordModel_Path = HttpContext.Current.Server.MapPath(wordModelPath);

                if (!File.Exists(wordModel_Path))
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件不存在";
                    return retMsg;
                }
                string FileClass = Common.GetFileClass(wordModel_Path);
                if (FileClass != "208207" && FileClass != "8075")
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件格式不正确";
                    return retMsg;
                }
                Document doc = new Aspose.Words.Document();

                try
                {
                    doc = new Aspose.Words.Document(wordModel_Path);
                }
                catch (Exception ex)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = Common.GetExceptionMsg(ex);
                    return retMsg;
                }

                if (doc == null)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件解析出错";
                    return retMsg;
                }
                if (ObjT == null)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "设置模板文件的数据不能为空";
                    return retMsg;
                }
                BookmarkCollection bookMarks = doc.Range.Bookmarks;
                if (bookMarks.Count <= 0)
                {
                    retMsg.retResult = false;
                    retMsg.MsgStr = "模板文件，不存在任何书签";
                    return retMsg;
                }
                else
                {
                    //使用DocumentBuilder对象插入一些文档对象，如插入书签，插入文本框，插入复选框，插入一个段落，插入空白页，追加或另一个word文件的内容等。
                    var builder = new DocumentBuilder(doc);

                    foreach (var bookmark in bookMarks)
                    {
                        var book_mark = bookmark as Aspose.Words.Bookmark;
                        book_mark.Text = "";// 清掉标示
                        //定位到指定位置进行插入操作
                        builder.MoveToBookmark(book_mark.Name);
                        object objval = Common.GetProtityValue(ObjT, book_mark.Name);
                        if (objval != null)
                        {
                            builder.Write(objval.ToString());
                        }
                    }
                    Stream imgStream = new MemoryStream();
                    Aspose.Words.Rendering.ImageOptions ImageOpts = new Aspose.Words.Rendering.ImageOptions();
                    ImageOpts.JpegQuality = 100;
                    if (doc.BuiltInDocumentProperties.Pages > 1)
                    {
                        doc.SaveToImage(0, doc.BuiltInDocumentProperties.Pages, HttpContext.Current.Server.MapPath("/DownLoad") + "/word.tiff", ImageOpts);
                        doc.SaveToImage(0, doc.BuiltInDocumentProperties.Pages, imgStream, System.Drawing.Imaging.ImageFormat.Tiff, ImageOpts);
                    }
                    else
                    {
                        doc.SaveToImage(0, doc.BuiltInDocumentProperties.Pages, HttpContext.Current.Server.MapPath("/DownLoad") + "/word.tiff", ImageOpts);
                        doc.SaveToImage(0, doc.BuiltInDocumentProperties.Pages, imgStream, System.Drawing.Imaging.ImageFormat.Jpeg, ImageOpts);
                    }

                    if (imgStream.Length > 0)
                    {
                        Image img = Image.FromStream(imgStream);
                        ArrWordImg.Add(img);
                    }

                    //for (int pageindex = 0; pageindex < doc.BuiltInDocumentProperties.Pages; pageindex++)
                    //{
                    //    imgStream = null;
                    //    doc.SaveToImage(pageindex, doc.BuiltInDocumentProperties.Pages, imgStream, System.Drawing.Imaging.ImageFormat.Tiff, ImageOpts);
                    //    if (imgStream != null)
                    //    {
                    //        if (imgStream.Length > 0)
                    //        {
                    //            Image img = Image.FromStream(imgStream);
                    //            ArrWordImg.Add(img);
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                var ErrMsg = Common.GetExceptionMsg(ex);
                retMsg.ArrWordImg = null;
                retMsg.retResult = false;
                retMsg.MsgStr = ErrMsg;
                return retMsg;
            }
            retMsg.ArrWordImg = ArrWordImg;
            return retMsg;
        }

        /// <summary>
        /// 标签插入Table
        /// </summary>
        /// <typeparam name="T">Model类</typeparam>
        /// <param name="bookMark">书签</param>
        /// <param name="doc">word文档</param>
        /// <param name="ArrValue">要插入的 List数据</param>
        public static void WriteTableByList<T>(Bookmark bookMark, Document doc, List<T> ArrValue)
            where T : class,new()
        {
            bookMark.Text = "";
            var builder = new DocumentBuilder(doc);
            builder.MoveToBookmark(bookMark.Name);

            int rowCount = ArrValue.Count();
            var ArrFields = Common.GetAllFieldNameByModel<T>();
            int columnCount = ArrFields.Count;

            builder.MoveToBookmark(bookMark.Name);
            builder.StartTable();//开始画Table              
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center; // RowAlignment.Center;                  

            string str = string.Empty;
            double MaxWidth = 50;//列最大宽度

            //列宽
            Dictionary<string, float> dictHeaherWidth = new Dictionary<string, float>();
            //用于测量文字宽度
            Image img = Image.FromFile(HttpContext.Current.Server.MapPath("/Images/file.png"));
            Graphics g = Graphics.FromImage(img);

            System.Drawing.Font font = new System.Drawing.Font(new FontFamily("宋体"), 8, FontStyle.Regular);

            //builder.RowFormat.Height = 20;
            //添加列头  
            foreach (var item in ArrFields)
            {
                dynamic dmic = item as dynamic;
                builder.InsertCell();
                //Table单元格边框线样式  
                builder.CellFormat.Borders.LineStyle = LineStyle.Single;
                SizeF TextSize = g.MeasureString(dmic.MetaDataTypeName, font, (int)MaxWidth / 8);
                
                //Table此单元格宽度  
                builder.CellFormat.Width = TextSize.Width / 8;
                //此单元格中内容垂直对齐方式  
                builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;
                builder.CellFormat.VerticalMerge = Aspose.Words.Tables.CellMerge.None;
                //字体大小  
                builder.Font.Size = font.Size;
                //是否加粗  
                builder.Bold = font.Style.Equals(FontStyle.Bold);
                //向此单元格中添加内容  
                builder.Write(dmic.MetaDataTypeName);
                dictHeaherWidth.Add(dmic.Name, TextSize.Width / 8);
            }
            builder.EndRow();

            //添加每行数据  
            for (int i = 0; i < rowCount; i++)
            {
                var j = 0;
                foreach (var dmicitem in ArrFields)
                {
                    if (dmicitem == null)
                        continue;
                    dynamic dmic = dmicitem as dynamic;
                    var valStr = Common.GetProtityValue(ArrValue[i], dmic.Name);
                    str = valStr == null ? "" : valStr.ToString();

                    //插入Table单元格  
                    builder.InsertCell();
                    //Table单元格边框线样式  
                    builder.CellFormat.Borders.LineStyle = LineStyle.Single;

                    //SizeF TextSize = g.MeasureString(dmic.Name, font, (int)MaxWidth);
                    float textwidth = 10;
                    var Where = dictHeaherWidth.Where(x => x.Key == dmic.Name );
                    if (Where.Any())
                    {
                        textwidth = Where.FirstOrDefault().Value;
                    }
                    //Table此单元格宽度 跟随列头宽度  
                    builder.CellFormat.Width = textwidth;  
                    //此单元格中内容垂直对齐方式  
                    builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                    builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;
                    builder.CellFormat.VerticalMerge = Aspose.Words.Tables.CellMerge.None;
                    //字体大小  
                    builder.Font.Size = font.Size;
                    //是否加粗  
                    builder.Bold = font.Style.Equals(FontStyle.Bold);
                    //向此单元格中添加内容  
                    builder.Write(str);
                    j++;
                }
                //Table行结束  
                builder.EndRow();
            }
            builder.EndTable();
        }

        /// <summary>
        /// 标签插入Table
        /// </summary>
        /// <typeparam name="T">Model类</typeparam>
        /// <param name="bookMark">书签</param>
        /// <param name="doc">word文档</param>
        /// <param name="ArrValue">要插入的 List数据</param>
        public static void WriteTableByList(Bookmark bookMark, Document doc, DataTable dt)
        {
            bookMark.Text = "";
            var builder = new DocumentBuilder(doc);
            builder.MoveToBookmark(bookMark.Name);

            int rowCount = dt.Rows.Count;
            int columnCount = dt.Columns.Count;

            builder.MoveToBookmark(bookMark.Name);
            builder.StartTable();//开始画Table              
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center; // RowAlignment.Center;                  

            string str = string.Empty;

            builder.RowFormat.Height = 20;

            //添加列头  
            for (var i = 0; i < dt.Columns.Count;i++ )
            {
                builder.InsertCell();
                //Table单元格边框线样式  
                builder.CellFormat.Borders.LineStyle = LineStyle.Single;
                //Table此单元格宽度  
                builder.CellFormat.Width = 600;
                //此单元格中内容垂直对齐方式  
                builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;
                builder.CellFormat.VerticalMerge = Aspose.Words.Tables.CellMerge.None;
                //字体大小  
                builder.Font.Size = 10;
                //是否加粗  
                builder.Bold = true;
                //向此单元格中添加内容  
                builder.Write(dt.Columns[i].ColumnName);
            }
            builder.EndRow();

            //添加每行数据  
            for (int i = 0; i < rowCount; i++)
            {
                for ( int j = 0; j < dt.Columns.Count; j++)
                {
                    str = dt.Rows[i][j].ToString();  

                    //插入Table单元格  
                    builder.InsertCell();

                    //Table单元格边框线样式  
                    builder.CellFormat.Borders.LineStyle = LineStyle.Single;

                    //Table此单元格宽度 跟随列头宽度  
                    //builder.CellFormat.Width = 500;  

                    //此单元格中内容垂直对齐方式  
                    builder.CellFormat.VerticalAlignment = Aspose.Words.Tables.CellVerticalAlignment.Center;
                    builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;
                    builder.CellFormat.VerticalMerge = Aspose.Words.Tables.CellMerge.None;

                    //字体大小  
                    builder.Font.Size = 10;

                    //是否加粗  
                    builder.Bold = false;

                    //向此单元格中添加内容  
                    builder.Write(str);

                    j++;
                }
                //Table行结束  
                builder.EndRow();
            }
            builder.EndTable();
        }
    }

    public class ReturnMsg
    {
        public ReturnMsg()
        {
        }

        public ReturnMsg(bool _retResult, string _ErrMsg)
        {
            retResult = _retResult;
            MsgStr = _ErrMsg;
        }

        /// <summary>
        /// 返回 成功/失败
        /// </summary>
        public bool retResult { get; set; }

        /// <summary>
        /// 错误/成功信息，成功时是 路径
        /// </summary>
        public string MsgStr { get; set; }

        /// <summary>
        /// 输出图片时，使用
        /// </summary>
        public List<Image> ArrWordImg { get; set; }
    }

}