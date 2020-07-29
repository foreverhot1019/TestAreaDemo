using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.Extensions
{
    /// <summary>   
    /// 用于取得一个文本文件的编码方式(Encoding)。   
    /// </summary>
    public class FileEncodingHelper
    {
        /// <summary>   
        /// 取得一个文本文件的编码方式。如果无法在文件头部找到有效的前导符，Encoding.Default将被返回。   
        /// </summary>   
        /// <param name="fileName">文件名。</param>   
        /// <returns></returns>
        public static Encoding GetEncoding(string fileName)
        {
            return GetEncoding(fileName, Encoding.Default);
        }

        /// <summary>   
        /// 取得一个文本文件流的编码方式。   
        /// </summary>   
        /// <param name="stream">文本文件流。</param>   
        /// <returns></returns>
        public static Encoding GetEncoding(FileStream stream)
        {
            return GetEncoding(stream, Encoding.Default);
        }

        /// <summary>   
        /// 取得一个文本文件的编码方式。   
        /// </summary>   
        /// <param name="fileName">文件名。</param>   
        /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>   
        /// <returns></returns>
        public static Encoding GetEncoding(string fileName, Encoding defaultEncoding)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            Encoding targetEncoding = GetEncoding(fs, defaultEncoding);
            fs.Close();
            return targetEncoding;
        }

        /// <summary>   
        /// 取得一个文本文件流的编码方式。   
        /// </summary>   
        /// <param name="stream">文本文件流。</param>   
        /// <param name="defaultEncoding">默认编码方式。当该方法无法从文件的头部取得有效的前导符时，将返回该编码方式。</param>   
        /// <returns></returns>
        public static Encoding GetEncoding(FileStream stream, Encoding defaultEncoding)
        {
            Encoding targetEncoding = defaultEncoding;
            if (stream != null && stream.Length >= 2)
            {
                //保存文件流的前4个字节   
                byte byte1 = 0;
                byte byte2 = 0;
                byte byte3 = 0;
                byte byte4 = 0;
                //保存当前Seek位置   
                long origPos = stream.Seek(0, SeekOrigin.Begin);
                stream.Seek(0, SeekOrigin.Begin);

                int nByte = stream.ReadByte();
                byte1 = Convert.ToByte(nByte);
                byte2 = Convert.ToByte(stream.ReadByte());
                if (stream.Length >= 3)
                {
                    byte3 = Convert.ToByte(stream.ReadByte());
                }
                if (stream.Length >= 4)
                {
                    byte4 = Convert.ToByte(stream.ReadByte());
                }
                //根据文件流的前4个字节判断Encoding   
                //Unicode {0xFF, 0xFE};   
                //BE-Unicode {0xFE, 0xFF};   
                //UTF8 = {0xEF, 0xBB, 0xBF};   
                if (byte1 == 0xFE && byte2 == 0xFF)//UnicodeBe   
                {
                    targetEncoding = Encoding.BigEndianUnicode;
                }
                if (byte1 == 0xFF && byte2 == 0xFE && byte3 != 0xFF)//Unicode   
                {
                    targetEncoding = Encoding.Unicode;
                }
                if (byte1 == 0xEF && byte2 == 0xBB && byte3 == 0xBF)//UTF8   
                {
                    targetEncoding = Encoding.UTF8;
                }
                //恢复Seek位置         
                stream.Seek(origPos, SeekOrigin.Begin);
            }
            return targetEncoding;
        }

        // 新增加一个方法，解决了不带BOM的 UTF8 编码问题   
        /// <summary>   
        /// 通过给定的文件流，判断文件的编码类型   
        /// </summary>   
        /// <param name="fs">文件流</param>   
        /// <returns>文件的编码类型</returns>
        public static System.Text.Encoding GetEncoding(Stream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM   
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            byte[] ss = r.ReadBytes(4);
            if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            else
            {
                if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
                {
                    reVal = Encoding.UTF8;
                }
                else
                {
                    int i;
                    int.TryParse(fs.Length.ToString(), out i);
                    ss = r.ReadBytes(i);

                    if (IsUTF8Bytes(ss))
                        reVal = Encoding.UTF8;
                }
            }
            r.Close();
            return reVal;
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(byte[] Top4Byte)
        {
            if (Top4Byte.Length < 4)
                return null;
            // Read the BOM
            var bom = Top4Byte.Take(4).ToArray();
            //using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            //    file.Read(bom, 0, 4);
            //}

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return null;
        }

        /// <summary>   
        /// 判断是否是不带 BOM 的 UTF8 格式   
        /// </summary>   
        /// <param name="data"></param>   
        /// <returns></returns>
        public static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1;  //计算当前正分析的字符应还有的字节数   
            byte curByte; //当前分析的字节.   
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前   
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X　   
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1   
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式!");
            }
            return true;
        }

        // Function to detect the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little
        // & big endian), and local default codepage, and potentially other codepages.
        // 'taster' = number of bytes to check of the file (to save processing). Higher
        // value is slower, but more reliable (especially UTF-8 with special characters
        // later on may appear to be ASCII initially). If taster = 0, then taster
        // becomes the length of the file (for maximum reliability). 'text' is simply
        // the string with the discovered encoding applied to the file.
        public static Encoding detectTextEncoding(byte[] b, out String text, int taster = 1000)
        {
            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) { text = Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4); return Encoding.GetEncoding("utf-32BE"); }  // UTF-32, big-endian 
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) { text = Encoding.UTF32.GetString(b, 4, b.Length - 4); return Encoding.UTF32; }    // UTF-32, little-endian
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) { text = Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2); return Encoding.BigEndianUnicode; }     // UTF-16, big-endian
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) { text = Encoding.Unicode.GetString(b, 2, b.Length - 2); return Encoding.Unicode; }              // UTF-16, little-endian
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) { text = Encoding.UTF8.GetString(b, 3, b.Length - 3); return Encoding.UTF8; } // UTF-8
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76) { text = Encoding.UTF7.GetString(b, 3, b.Length - 3); return Encoding.UTF7; } // UTF-7

            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster value is desired for UTF-8
            if (taster == 0 || taster > b.Length) taster = b.Length;    // Taster size can't be bigger than the filesize obviously.

            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4)
            {
                if (b[i] <= 0x7F) { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0) { i += 2; utf8 = true; continue; }
                if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0) { i += 3; utf8 = true; continue; }
                if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0) { i += 4; utf8 = true; continue; }
                utf8 = false; break;
            }
            if (utf8 == true)
            {
                text = Encoding.UTF8.GetString(b);
                return Encoding.UTF8;
            }

            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.BigEndianUnicode.GetString(b); return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.Unicode.GetString(b); return Encoding.Unicode; } // (little-endian)

            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++)
            {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') && (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') && (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') && (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') && (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') && (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') && (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    )
                {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8; else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    int oldn = n;
                    while (n < taster && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z')))
                    { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try
                    {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        text = Encoding.GetEncoding(internalEnc).GetString(b);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }

            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            text = Encoding.Default.GetString(b);
            return Encoding.Default;
        }
    }

    /// <summary>
    /// 字节文本编码检测
    /// </summary>
    public class TextEncodingDetect
    {
        public Encoding DefaultEncoding = Encoding.GetEncoding("GB2312");

        #region 含有BOM时的，BOM头字节检测 编码

        private readonly byte[] _UTF8Bom =
        {
            0xEF,
            0xBB,
            0xBF
        };

        //utf16le _UnicodeBom
        private readonly byte[] _UTF16LeBom =
        {
            0xFF,
            0xFE
        };

        //utf16be _BigUnicodeBom
        private readonly byte[] _UTF16BeBom =
        {
            0xFE,
            0xFF
        };

        //utf-32le
        private readonly byte[] _UTF32LeBom =
        {
            0xFF,
            0xFE,
            0x00,
            0x00
        };

        //utf-32Be
        private readonly byte[] _UTF32BeBom =
        {
            0x00,
            0x00,
            0xFE,
            0xFF
        };

        #endregion

        /// <summary>
        /// 是否中文
        /// </summary>
        public bool IsChinese = false;

        /// <summary>
        /// 是否拥有Bom头
        /// </summary>
        public bool hasBom = false;

        public enum TextEncode
        {
            None, // Unknown or binary
            Ansi, // 0-255
            Ascii, // 0-127
            Utf8Bom, // UTF8 with BOM
            Utf8Nobom, // UTF8 without BOM
            UnicodeBom, // UTF16 LE with BOM
            UnicodeNoBom, // UTF16 LE without BOM
            BigEndianUnicodeBom, // UTF16-BE with BOM
            BigEndianUnicodeNoBom, // UTF16-BE without BOM

            Utf32Bom,//UTF-32LE with BOM
            Utf32NoBom, //UTF-32 without BOM
            GB2312,//GB2312<GBK<GB18030
            GBK,
            GB18030,
            Big5,
        }

        /// <summary>
        /// 中文编码
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private bool IsChineseEncoding(Encoding encoding)
        {
            return encoding == Encoding.GetEncoding("gb2312") || encoding == Encoding.GetEncoding("gbk") || encoding == Encoding.GetEncoding("big5");
        }

        /// <summary>
        /// 获取文件编码
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public Encoding GetEncoding(byte[] buff)
        {
            return GetEncoding(buff, DefaultEncoding);
        }

        /// <summary>
        /// 获取文件编码
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="defaultEncoding"></param>
        /// <returns></returns>
        public Encoding GetEncoding(byte[] buff, Encoding defaultEncoding)
        {
            hasBom = true;
            //检测Bom
            switch (DetectWithBom(buff))
            {
                case TextEncodingDetect.TextEncode.Utf8Bom:
                    return Encoding.UTF8;
                case TextEncodingDetect.TextEncode.UnicodeBom:
                    return Encoding.Unicode;
                case TextEncodingDetect.TextEncode.BigEndianUnicodeBom:
                    return Encoding.BigEndianUnicode;
                case TextEncodingDetect.TextEncode.Utf32Bom:
                    return Encoding.UTF32;
            }

            hasBom = false;
            if (defaultEncoding != DefaultEncoding && defaultEncoding != Encoding.ASCII)//自定义设置编码，优先处理。
            {
                return defaultEncoding;
            }
            switch (DetectWithoutBom(buff, buff.Length))//自动检测。
            {
                case TextEncodingDetect.TextEncode.Utf8Nobom:
                    return Encoding.UTF8;
                case TextEncodingDetect.TextEncode.UnicodeNoBom:
                    return Encoding.Unicode;
                case TextEncodingDetect.TextEncode.BigEndianUnicodeNoBom:
                    return Encoding.BigEndianUnicode;
                case TextEncodingDetect.TextEncode.Utf32NoBom:
                    return Encoding.UTF32;
                case TextEncodingDetect.TextEncode.Ansi:
                    if (IsChineseEncoding(DefaultEncoding) && !IsChineseEncoding(defaultEncoding))
                    {
                        if (IsChinese)
                        {
                            return Encoding.GetEncoding("GB18030");
                        }
                        else//非中文时，默认选一个。
                        {
                            return Encoding.Unicode;
                        }
                    }
                    else
                    {
                        return defaultEncoding;
                    }
                case TextEncodingDetect.TextEncode.Ascii:
                    return Encoding.ASCII;
                case var a when a == TextEncodingDetect.TextEncode.GB2312 ||
                                a == TextEncodingDetect.TextEncode.GBK ||
                                a == TextEncodingDetect.TextEncode.GB18030:
                    return Encoding.GetEncoding("GB18030");
                case TextEncodingDetect.TextEncode.Big5:
                    return Encoding.GetEncoding("Big5");
                default:
                    return null;
            }
        }

        /// <summary>
        /// 检测BOM头
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public TextEncode DetectWithBom(byte[] buffer)
        {
            if (buffer != null)
            {
                int size = buffer.Length;
                // Check for BOM
                if (size >= 2 && buffer[0] == _UTF16LeBom[0] && buffer[1] == _UTF16LeBom[1])
                {
                    return TextEncode.UnicodeBom;
                }

                if (size >= 2 && buffer[0] == _UTF16BeBom[0] && buffer[1] == _UTF16BeBom[1])
                {
                    if (size >= 4 && buffer[2] == _UTF32LeBom[2] && buffer[3] == _UTF32LeBom[3])
                    {
                        return TextEncode.Utf32Bom;
                    }
                    return TextEncode.BigEndianUnicodeBom;
                }

                if (size >= 3 && buffer[0] == _UTF8Bom[0] && buffer[1] == _UTF8Bom[1] && buffer[2] == _UTF8Bom[2])
                {
                    return TextEncode.Utf8Bom;
                }
            }
            return TextEncode.None;
        }

        /// <summary>
        ///     Automatically detects the Encoding type of a given byte buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>The Encoding type or Encoding.None if unknown.</returns>
        public TextEncode DetectWithoutBom(byte[] buffer, int size)
        {
            // Now check for valid UTF8
            TextEncode encoding = CheckUtf8(buffer, size);
            if (encoding == TextEncode.Utf8Nobom)
            {
                return encoding;
            }

            // ANSI or None (binary) then 一个零都没有情况。
            if (!ContainsZero(buffer, size))
            {
                CheckChinese(buffer, size, out var txtEncod);
                if (IsChinese)
                {
                    if (txtEncod != TextEncode.None)
                        return txtEncod;
                    else
                        return TextEncode.Ansi;
                }
            }

            // Now try UTF16  按寻找换行字符先进行判断
            encoding = CheckByNewLineChar(buffer, size);
            if (encoding != TextEncode.None)
            {
                return encoding;
            }

            // 没办法了，只能按0出现的次数比率，做大体的预判
            encoding = CheckByZeroNumPercent(buffer, size);
            if (encoding != TextEncode.None)
            {
                return encoding;
            }

            // Found a null, return based on the preference in null_suggests_binary_
            return TextEncode.None;
        }

        /// <summary>
        ///     Checks if a buffer contains text that looks like utf16 by scanning for
        ///     newline chars that would be present even in non-english text.
        ///     以检测换行符标识来判断。
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>Encoding.none, Encoding.Utf16LeNoBom or Encoding.Utf16BeNoBom.</returns>
        private static TextEncode CheckByNewLineChar(byte[] buffer, int size)
        {
            if (size < 2)
            {
                return TextEncode.None;
            }

            // Reduce size by 1 so we don't need to worry about bounds checking for pairs of bytes
            size--;

            int le16 = 0;
            int be16 = 0;
            int le32 = 0;//检测是否utf32le。
            int zeroCount = 0;//utf32le 每4位后面多数是0
            uint pos = 0;
            while (pos < size)
            {
                byte ch1 = buffer[pos++];
                byte ch2 = buffer[pos++];

                if (ch1 == 0)
                {
                    if (ch2 == 0x0a || ch2 == 0x0d)//\r \t 换行检测。
                    {
                        ++be16;
                    }
                }
                if (ch2 == 0)
                {
                    zeroCount++;
                    if (ch1 == 0x0a || ch1 == 0x0d)
                    {
                        ++le16;
                        if (pos + 1 <= size && buffer[pos] == 0 && buffer[pos + 1] == 0)
                        {
                            ++le32;
                        }

                    }
                }

                // If we are getting both LE and BE control chars then this file is not utf16
                if (le16 > 0 && be16 > 0)
                {
                    return TextEncode.None;
                }
            }

            if (le16 > 0)
            {
                if (le16 == le32 && buffer.Length % 4 == 0)
                {
                    return TextEncode.Utf32NoBom;
                }
                return TextEncode.UnicodeNoBom;
            }
            else if (be16 > 0)
            {
                return TextEncode.BigEndianUnicodeNoBom;
            }
            else if (buffer.Length % 4 == 0 && zeroCount >= buffer.Length / 4)
            {
                return TextEncode.Utf32NoBom;
            }
            return TextEncode.None;
        }

        /// <summary>
        /// Checks if a buffer contains any nulls. Used to check for binary vs text data.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        private static bool ContainsZero(byte[] buffer, int size)
        {
            uint pos = 0;
            while (pos < size)
            {
                if (buffer[pos++] == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Checks if a buffer contains text that looks like utf16. This is done based
        ///     on the use of nulls which in ASCII/script like text can be useful to identify.
        ///     按照一定的空0数的概率来预测。
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>Encoding.none, Encoding.Utf16LeNoBom or Encoding.Utf16BeNoBom.</returns>
        private TextEncode CheckByZeroNumPercent(byte[] buffer, int size)
        {
            //单数
            int oddZeroCount = 0;
            //双数
            int evenZeroCount = 0;

            // Get even nulls
            uint pos = 0;
            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    evenZeroCount++;
                }

                pos += 2;
            }

            // Get odd nulls
            pos = 1;
            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    oddZeroCount++;
                }

                pos += 2;
            }

            double evenZeroPercent = evenZeroCount * 2.0 / size;
            double oddZeroPercent = oddZeroCount * 2.0 / size;

            // Lots of odd nulls, low number of even nulls 这里的条件做了修改
            if (evenZeroPercent < 0.1 && oddZeroPercent > 0)
            {
                return TextEncode.UnicodeNoBom;
            }

            // Lots of even nulls, low number of odd nulls 这里的条件也做了修改
            if (oddZeroPercent < 0.1 && evenZeroPercent > 0)
            {
                return TextEncode.BigEndianUnicodeNoBom;
            }

            // Don't know
            return TextEncode.None;
        }

        /// <summary>
        ///     Checks if a buffer contains valid utf8.
        ///     以UTF8 的字节范围来检测。
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>
        ///     Encoding type of Encoding.None (invalid UTF8), Encoding.Utf8NoBom (valid utf8 multibyte strings) or
        ///     Encoding.ASCII (data in 0.127 range).
        /// </returns>
        /// <returns>2</returns>
        private TextEncode CheckUtf8(byte[] buffer, int size)
        {
            // UTF8 Valid sequences
            // 0xxxxxxx  ASCII
            // 110xxxxx 10xxxxxx  2-byte
            // 1110xxxx 10xxxxxx 10xxxxxx  3-byte
            // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx  4-byte
            //
            // Width in UTF8
            // Decimal      Width
            // 0-127        1 byte
            // 194-223      2 bytes
            // 224-239      3 bytes
            // 240-244      4 bytes
            //
            // Subsequent chars are in the range 128-191
            bool onlySawAsciiRange = true;
            uint pos = 0;

            while (pos < size)
            {
                byte ch = buffer[pos++];

                if (ch == 0)
                {
                    return TextEncode.None;
                }

                int moreChars;
                if (ch <= 127)
                {
                    // 1 byte
                    moreChars = 0;
                }
                else if (ch >= 194 && ch <= 223)
                {
                    // 2 Byte
                    moreChars = 1;
                }
                else if (ch >= 224 && ch <= 239)
                {
                    // 3 Byte
                    moreChars = 2;
                }
                else if (ch >= 240 && ch <= 244)
                {
                    // 4 Byte
                    moreChars = 3;
                }
                else
                {
                    return TextEncode.None; // Not utf8
                }

                // Check secondary chars are in range if we are expecting any
                while (moreChars > 0 && pos < size)
                {
                    onlySawAsciiRange = false; // Seen non-ascii chars now

                    ch = buffer[pos++];
                    if (ch < 128 || ch > 191)
                    {
                        return TextEncode.None; // Not utf8
                    }

                    --moreChars;
                }
            }

            // If we get to here then only valid UTF-8 sequences have been processed

            // If we only saw chars in the range 0-127 then we can't assume UTF8 (the caller will need to decide)
            return onlySawAsciiRange ? TextEncode.Ascii : TextEncode.Utf8Nobom;
        }

        /// <summary>
        /// 是否中文编码（GB2312、GBK、Big5）
        /// </summary>
        private void CheckChinese(byte[] buffer, int size, out TextEncode txtEncode)
        {
            txtEncode = TextEncode.None;
            IsChinese = false;
            if (size < 2)
            {
                return;
            }

            // Reduce size by 1 so we don't need to worry about bounds checking for pairs of bytes
            size--;
            uint pos = 0;
            bool isCN = false;
            while (pos < size)
            {
                //GB2312
                //0xB0-0xF7(176-247)
                //0xA0-0xFE（160-254）

                //GBK
                //0x81-0xFE（129-254）
                //0x40-0xFE（64-254）

                //Big5
                //0x81-0xFE（129-255）
                //0x40-0x7E（64-126）  OR 0xA1－0xFE（161-254）
                byte ch_1 = buffer[pos++];
                byte ch_2 = buffer[pos++];
                var tuple = Tuple.Create(ch_1, ch_2);
                switch (tuple)
                {
                    case var a when (a.Item1 >= 176 && a.Item1 <= 247 && a.Item2 >= 160 && a.Item2 <= 254):
                        txtEncode = TextEncode.GB2312;
                        txtEncode = TextEncode.GB18030;//直接赋值-GB18030>GBK>GB2312
                        IsChinese = true;
                        break;
                    case var a when (a.Item1 >= 129 && a.Item1 <= 254 && a.Item2 >= 64 && a.Item2 <= 254):
                        txtEncode = TextEncode.GBK;
                        txtEncode = TextEncode.GB18030;//直接赋值-GB18030>GBK>GB2312
                        IsChinese = true;
                        break;
                    case var a when (a.Item1 >= 129 && ((a.Item2 >= 64 && a.Item2 <= 126) || (a.Item2 >= 161 && a.Item2 <= 254))):
                        txtEncode = TextEncode.Big5;
                        IsChinese = true;
                        return;
                    default:
                        IsChinese = false;
                        break;
                }
                //isCN = (ch1 >= 176 && ch1 <= 247 && ch2 >= 160 && ch2 <= 254)
                //    || (ch1 >= 129 && ch1 <= 254 && ch2 >= 64 && ch2 <= 254)
                //    || (ch1 >= 129 && ((ch2 >= 64 && ch2 <= 126) || (ch2 >= 161 && ch2 <= 254)));
                //if (isCN)
                //{
                //    IsChinese = true;
                //    return;
                //}
            }
        }
    }

    /// <summary>
    /// 文字编码检测。
    /// 用于检测一篇文章使用什么编码方式进行编码。
    /// </summary>
    public class StreamBianMaJianCe
    {
        /// <summary>
        /// BigEndianUnicode编码高频汉字编码
        /// </summary>
        private List<byte[]> _BigEndianUnicodeGaoPinZiFuBianMaLsit = new List<byte[]>()
        {
            new byte[2]{118,132},new byte[2]{78,0},new byte[2]{86,253},new byte[2]{87,40},new byte[2]{78,186},new byte[2]{78,134},new byte[2]{103,9},new byte[2]{78,45},
            new byte[2]{102,47},new byte[2]{94,116},new byte[2]{84,140},new byte[2]{89,39},new byte[2]{78,26},new byte[2]{78,13},new byte[2]{78,58},new byte[2]{83,209},
            new byte[2]{79,26},new byte[2]{93,229},new byte[2]{126,207},new byte[2]{78,10},new byte[2]{87,48},new byte[2]{94,2},new byte[2]{137,129},new byte[2]{78,42},
            new byte[2]{78,167},new byte[2]{143,217},new byte[2]{81,250},new byte[2]{136,76},new byte[2]{79,92},new byte[2]{117,31},new byte[2]{91,182},new byte[2]{78,229},
            new byte[2]{98,16},new byte[2]{82,48},new byte[2]{101,229},new byte[2]{108,17},new byte[2]{103,101},new byte[2]{98,17},new byte[2]{144,232},new byte[2]{91,249},
            new byte[2]{143,219},new byte[2]{89,26},new byte[2]{81,104},new byte[2]{94,250},new byte[2]{78,214},new byte[2]{81,108},new byte[2]{95,0},new byte[2]{78,236},
            new byte[2]{87,58},new byte[2]{92,85},new byte[2]{101,246},new byte[2]{116,6},new byte[2]{101,176},new byte[2]{101,185},new byte[2]{78,59},new byte[2]{79,1},
            new byte[2]{141,68},new byte[2]{91,158},new byte[2]{91,102},new byte[2]{98,165},new byte[2]{82,54},new byte[2]{101,63},new byte[2]{109,78},new byte[2]{117,40},
            new byte[2]{84,12},new byte[2]{78,142},new byte[2]{108,213},new byte[2]{154,216},new byte[2]{149,127},new byte[2]{115,176},new byte[2]{103,44},new byte[2]{103,8},
            new byte[2]{91,154},new byte[2]{83,22},new byte[2]{82,160},new byte[2]{82,168},new byte[2]{84,8},new byte[2]{84,193},new byte[2]{145,205},new byte[2]{81,115},
            new byte[2]{103,58},new byte[2]{82,6},new byte[2]{82,155},new byte[2]{129,234},new byte[2]{89,22},new byte[2]{128,5},new byte[2]{83,58},new byte[2]{128,253},
            new byte[2]{139,190},new byte[2]{84,14},new byte[2]{92,49},new byte[2]{123,73},new byte[2]{79,83},new byte[2]{78,11},new byte[2]{78,7},new byte[2]{81,67},
            new byte[2]{121,62},new byte[2]{143,199},new byte[2]{82,77},new byte[2]{151,98},new byte[2]{48,2},new byte[2]{255,12},new byte[2]{255,31},new byte[2]{255,1}
        };
        /// <summary>
        /// UTF8编码高频汉字编码
        /// </summary>
        private List<byte[]> _UTF8GaoPinZiFuBianMaLsit = new List<byte[]>()
        {
            new byte[3]{231,154,132},new byte[3]{228,184,128},new byte[3]{229,155,189},new byte[3]{229,156,168},new byte[3]{228,186,186},new byte[3]{228,186,134},
            new byte[3]{230,156,137},new byte[3]{228,184,173},new byte[3]{230,152,175},new byte[3]{229,185,180},new byte[3]{229,146,140},new byte[3]{229,164,167},
            new byte[3]{228,184,154},new byte[3]{228,184,141},new byte[3]{228,184,186},new byte[3]{229,143,145},new byte[3]{228,188,154},new byte[3]{229,183,165},
            new byte[3]{231,187,143},new byte[3]{228,184,138},new byte[3]{229,156,176},new byte[3]{229,184,130},new byte[3]{232,166,129},new byte[3]{228,184,170},
            new byte[3]{228,186,167},new byte[3]{232,191,153},new byte[3]{229,135,186},new byte[3]{232,161,140},new byte[3]{228,189,156},new byte[3]{231,148,159},
            new byte[3]{229,174,182},new byte[3]{228,187,165},new byte[3]{230,136,144},new byte[3]{229,136,176},new byte[3]{230,151,165},new byte[3]{230,176,145},
            new byte[3]{230,157,165},new byte[3]{230,136,145},new byte[3]{233,131,168},new byte[3]{229,175,185},new byte[3]{232,191,155},new byte[3]{229,164,154},
            new byte[3]{229,133,168},new byte[3]{229,187,186},new byte[3]{228,187,150},new byte[3]{229,133,172},new byte[3]{229,188,128},new byte[3]{228,187,172},
            new byte[3]{229,156,186},new byte[3]{229,177,149},new byte[3]{230,151,182},new byte[3]{231,144,134},new byte[3]{230,150,176},new byte[3]{230,150,185},
            new byte[3]{228,184,187},new byte[3]{228,188,129},new byte[3]{232,181,132},new byte[3]{229,174,158},new byte[3]{229,173,166},new byte[3]{230,138,165},
            new byte[3]{229,136,182},new byte[3]{230,148,191},new byte[3]{230,181,142},new byte[3]{231,148,168},new byte[3]{229,144,140},new byte[3]{228,186,142},
            new byte[3]{230,179,149},new byte[3]{233,171,152},new byte[3]{233,149,191},new byte[3]{231,142,176},new byte[3]{230,156,172},new byte[3]{230,156,136},
            new byte[3]{229,174,154},new byte[3]{229,140,150},new byte[3]{229,138,160},new byte[3]{229,138,168},new byte[3]{229,144,136},new byte[3]{229,147,129},
            new byte[3]{233,135,141},new byte[3]{229,133,179},new byte[3]{230,156,186},new byte[3]{229,136,134},new byte[3]{229,138,155},new byte[3]{232,135,170},
            new byte[3]{229,164,150},new byte[3]{232,128,133},new byte[3]{229,140,186},new byte[3]{232,131,189},new byte[3]{232,174,190},new byte[3]{229,144,142},
            new byte[3]{229,176,177},new byte[3]{231,173,137},new byte[3]{228,189,147},new byte[3]{228,184,139},new byte[3]{228,184,135},new byte[3]{229,133,131},
            new byte[3]{231,164,190},new byte[3]{232,191,135},new byte[3]{229,137,141},new byte[3]{233,157,162},new byte[3]{227,128,130},new byte[3]{239,188,140},
            new byte[3]{239,188,159},new byte[3]{239,188,129},
        };
        /// <summary>
        /// Unicode编码高频汉字编码
        /// </summary>
        private List<byte[]> _UnicodeGaoPinZiFuBianMaLsit = new List<byte[]>()
        {
            new byte[2]{132,118},new byte[2]{0,78},new byte[2]{253,86},new byte[2]{40,87},new byte[2]{186,78},new byte[2]{134,78},new byte[2]{9,103},new byte[2]{45,78},
            new byte[2]{47,102},new byte[2]{116,94},new byte[2]{140,84},new byte[2]{39,89},new byte[2]{26,78},new byte[2]{13,78},new byte[2]{58,78},new byte[2]{209,83},
            new byte[2]{26,79},new byte[2]{229,93},new byte[2]{207,126},new byte[2]{10,78},new byte[2]{48,87},new byte[2]{2,94},new byte[2]{129,137},new byte[2]{42,78},
            new byte[2]{167,78},new byte[2]{217,143},new byte[2]{250,81},new byte[2]{76,136},new byte[2]{92,79},new byte[2]{31,117},new byte[2]{182,91},new byte[2]{229,78},
            new byte[2]{16,98},new byte[2]{48,82},new byte[2]{229,101},new byte[2]{17,108},new byte[2]{101,103},new byte[2]{17,98},new byte[2]{232,144},new byte[2]{249,91},
            new byte[2]{219,143},new byte[2]{26,89},new byte[2]{104,81},new byte[2]{250,94},new byte[2]{214,78},new byte[2]{108,81},new byte[2]{0,95},new byte[2]{236,78},
            new byte[2]{58,87},new byte[2]{85,92},new byte[2]{246,101},new byte[2]{6,116},new byte[2]{176,101},new byte[2]{185,101},new byte[2]{59,78},new byte[2]{1,79},
            new byte[2]{68,141},new byte[2]{158,91},new byte[2]{102,91},new byte[2]{165,98},new byte[2]{54,82},new byte[2]{63,101},new byte[2]{78,109},new byte[2]{40,117},
            new byte[2]{12,84},new byte[2]{142,78},new byte[2]{213,108},new byte[2]{216,154},new byte[2]{127,149},new byte[2]{176,115},new byte[2]{44,103},new byte[2]{8,103},
            new byte[2]{154,91},new byte[2]{22,83},new byte[2]{160,82},new byte[2]{168,82},new byte[2]{8,84},new byte[2]{193,84},new byte[2]{205,145},new byte[2]{115,81},
            new byte[2]{58,103},new byte[2]{6,82},new byte[2]{155,82},new byte[2]{234,129},new byte[2]{22,89},new byte[2]{5,128},new byte[2]{58,83},new byte[2]{253,128},
            new byte[2]{190,139},new byte[2]{14,84},new byte[2]{49,92},new byte[2]{73,123},new byte[2]{83,79},new byte[2]{11,78},new byte[2]{7,78},new byte[2]{67,81},
            new byte[2]{62,121},new byte[2]{199,143},new byte[2]{77,82},new byte[2]{98,151},new byte[2]{2,48},new byte[2]{12,255},new byte[2]{31,255},new byte[2]{1,255}
        };
        /// <summary>
        /// UTF32编码高频汉字编码
        /// </summary>
        private List<byte[]> _UTF32GaoPinZiFuBianMaLsit = new List<byte[]>()
        {
            new byte[4]{132,118,0,0},new byte[4]{0,78,0,0},new byte[4]{253,86,0,0},new byte[4]{40,87,0,0},new byte[4]{186,78,0,0},new byte[4]{134,78,0,0},new byte[4]{9,103,0,0},
            new byte[4]{45,78,0,0},new byte[4]{47,102,0,0},new byte[4]{116,94,0,0},new byte[4]{140,84,0,0},new byte[4]{39,89,0,0},new byte[4]{26,78,0,0},new byte[4]{13,78,0,0},
            new byte[4]{58,78,0,0},new byte[4]{209,83,0,0},new byte[4]{26,79,0,0},new byte[4]{229,93,0,0},new byte[4]{207,126,0,0},new byte[4]{10,78,0,0},new byte[4]{48,87,0,0},
            new byte[4]{2,94,0,0},new byte[4]{129,137,0,0},new byte[4]{42,78,0,0},new byte[4]{167,78,0,0},new byte[4]{217,143,0,0},new byte[4]{250,81,0,0},new byte[4]{76,136,0,0},
            new byte[4]{92,79,0,0},new byte[4]{31,117,0,0},new byte[4]{182,91,0,0},new byte[4]{229,78,0,0},new byte[4]{16,98,0,0},new byte[4]{48,82,0,0},new byte[4]{229,101,0,0},
            new byte[4]{17,108,0,0},new byte[4]{101,103,0,0},new byte[4]{17,98,0,0},new byte[4]{232,144,0,0},new byte[4]{249,91,0,0},new byte[4]{219,143,0,0},new byte[4]{26,89,0,0},
            new byte[4]{104,81,0,0},new byte[4]{250,94,0,0},new byte[4]{214,78,0,0},new byte[4]{108,81,0,0},new byte[4]{0,95,0,0},new byte[4]{236,78,0,0},new byte[4]{58,87,0,0},
            new byte[4]{85,92,0,0},new byte[4]{246,101,0,0},new byte[4]{6,116,0,0},new byte[4]{176,101,0,0},new byte[4]{185,101,0,0},new byte[4]{59,78,0,0},new byte[4]{1,79,0,0},
            new byte[4]{68,141,0,0},new byte[4]{158,91,0,0},new byte[4]{102,91,0,0},new byte[4]{165,98,0,0},new byte[4]{54,82,0,0},new byte[4]{63,101,0,0},new byte[4]{78,109,0,0},
            new byte[4]{40,117,0,0},new byte[4]{12,84,0,0},new byte[4]{142,78,0,0},new byte[4]{213,108,0,0},new byte[4]{216,154,0,0},new byte[4]{127,149,0,0},new byte[4]{176,115,0,0},
            new byte[4]{44,103,0,0},new byte[4]{8,103,0,0},new byte[4]{154,91,0,0},new byte[4]{22,83,0,0},new byte[4]{160,82,0,0},new byte[4]{168,82,0,0},new byte[4]{8,84,0,0},
            new byte[4]{193,84,0,0},new byte[4]{205,145,0,0},new byte[4]{115,81,0,0},new byte[4]{58,103,0,0},new byte[4]{6,82,0,0},new byte[4]{155,82,0,0},new byte[4]{234,129,0,0},
            new byte[4]{22,89,0,0},new byte[4]{5,128,0,0},new byte[4]{58,83,0,0},new byte[4]{253,128,0,0},new byte[4]{190,139,0,0},new byte[4]{14,84,0,0},new byte[4]{49,92,0,0},
            new byte[4]{73,123,0,0},new byte[4]{83,79,0,0},new byte[4]{11,78,0,0},new byte[4]{7,78,0,0},new byte[4]{67,81,0,0},new byte[4]{62,121,0,0},new byte[4]{199,143,0,0},
            new byte[4]{77,82,0,0},new byte[4]{98,151,0,0},new byte[4]{2,48,0,0},new byte[4]{12,255,0,0},new byte[4]{31,255,0,0},new byte[4]{1,255,0,0}
        };
        /// <summary>
        /// UTF7编码高频汉字编码
        /// </summary>
        private List<byte[]> _UTF7GaoPinZiFuBianMaLsit = new List<byte[]>()
        {
            new byte[5]{43,100,111,81,45},new byte[5]{43,84,103,65,45},new byte[5]{43,86,118,48,45},new byte[5]{43,86,121,103,45},new byte[5]{43,84,114,111,45},new byte[5]{43,84,111,89,45},
            new byte[5]{43,90,119,107,45},new byte[5]{43,84,105,48,45},new byte[5]{43,90,105,56,45},new byte[5]{43,88,110,81,45},new byte[5]{43,86,73,119,45},new byte[5]{43,87,83,99,45},
            new byte[5]{43,84,104,111,45},new byte[5]{43,84,103,48,45},new byte[5]{43,84,106,111,45},new byte[5]{43,85,57,69,45},new byte[5]{43,84,120,111,45},new byte[5]{43,88,101,85,45},
            new byte[5]{43,102,115,56,45},new byte[5]{43,84,103,111,45},new byte[5]{43,86,122,65,45},new byte[5]{43,88,103,73,45},new byte[5]{43,105,89,69,45},new byte[5]{43,84,105,111,45},
            new byte[5]{43,84,113,99,45},new byte[5]{43,106,57,107,45},new byte[5]{43,85,102,111,45},new byte[5]{43,105,69,119,45},new byte[5]{43,84,49,119,45},new byte[5]{43,100,82,56,45},
            new byte[5]{43,87,55,89,45},new byte[5]{43,84,117,85,45},new byte[5]{43,89,104,65,45},new byte[5]{43,85,106,65,45},new byte[5]{43,90,101,85,45},new byte[5]{43,98,66,69,45},
            new byte[5]{43,90,50,85,45},new byte[5]{43,89,104,69,45},new byte[5]{43,107,79,103,45},new byte[5]{43,87,47,107,45},new byte[5]{43,106,57,115,45},new byte[5]{43,87,82,111,45},
            new byte[5]{43,85,87,103,45},new byte[5]{43,88,118,111,45},new byte[5]{43,84,116,89,45},new byte[5]{43,85,87,119,45},new byte[5]{43,88,119,65,45},new byte[5]{43,84,117,119,45},
            new byte[5]{43,86,122,111,45},new byte[5]{43,88,70,85,45},new byte[5]{43,90,102,89,45},new byte[5]{43,100,65,89,45},new byte[5]{43,90,98,65,45},new byte[5]{43,90,98,107,45},
            new byte[5]{43,84,106,115,45},new byte[5]{43,84,119,69,45},new byte[5]{43,106,85,81,45},new byte[5]{43,87,53,52,45},new byte[5]{43,87,50,89,45},new byte[5]{43,89,113,85,45},
            new byte[5]{43,85,106,89,45},new byte[5]{43,90,84,56,45},new byte[5]{43,98,85,52,45},new byte[5]{43,100,83,103,45},new byte[5]{43,86,65,119,45},new byte[5]{43,84,111,52,45},
            new byte[5]{43,98,78,85,45},new byte[5]{43,109,116,103,45},new byte[5]{43,108,88,56,45},new byte[5]{43,99,55,65,45},new byte[5]{43,90,121,119,45},new byte[5]{43,90,119,103,45},
            new byte[5]{43,87,53,111,45},new byte[5]{43,85,120,89,45},new byte[5]{43,85,113,65,45},new byte[5]{43,85,113,103,45},new byte[5]{43,86,65,103,45},new byte[5]{43,86,77,69,45},
            new byte[5]{43,107,99,48,45},new byte[5]{43,85,88,77,45},new byte[5]{43,90,122,111,45},new byte[5]{43,85,103,89,45},new byte[5]{43,85,112,115,45},new byte[5]{43,103,101,111,45},
            new byte[5]{43,87,82,89,45},new byte[5]{43,103,65,85,45},new byte[5]{43,85,122,111,45},new byte[5]{43,103,80,48,45},new byte[5]{43,105,55,52,45},new byte[5]{43,86,65,52,45},
            new byte[5]{43,88,68,69,45},new byte[5]{43,101,48,107,45},new byte[5]{43,84,49,77,45},new byte[5]{43,84,103,115,45},new byte[5]{43,84,103,99,45},new byte[5]{43,85,85,77,45},
            new byte[5]{43,101,84,52,45},new byte[5]{43,106,56,99,45},new byte[5]{43,85,107,48,45},new byte[5]{43,108,50,73,45},new byte[5]{43,77,65,73,45},new byte[5]{43,47,119,119,45},
            new byte[5]{43,47,120,56,45},new byte[5]{43,47,119,69,45}
        };
        /// <summary>
        /// GB18030编码高频汉字编码
        /// </summary>
        private List<byte[]> _GB18030GaoPinZiFuBianMaLsit = new List<byte[]>()
        {
            new byte[2]{181,196},new byte[2]{210,187},new byte[2]{185,250},new byte[2]{212,218},new byte[2]{200,203},new byte[2]{193,203},new byte[2]{211,208},new byte[2]{214,208},new byte[2]{202,199},
            new byte[2]{196,234},new byte[2]{186,205},new byte[2]{180,243},new byte[2]{210,181},new byte[2]{178,187},new byte[2]{206,170},new byte[2]{183,162},new byte[2]{187,225},new byte[2]{185,164},
            new byte[2]{190,173},new byte[2]{201,207},new byte[2]{181,216},new byte[2]{202,208},new byte[2]{210,170},new byte[2]{184,246},new byte[2]{178,250},new byte[2]{213,226},new byte[2]{179,246},
            new byte[2]{208,208},new byte[2]{215,247},new byte[2]{201,250},new byte[2]{188,210},new byte[2]{210,212},new byte[2]{179,201},new byte[2]{181,189},new byte[2]{200,213},new byte[2]{195,241},
            new byte[2]{192,180},new byte[2]{206,210},new byte[2]{178,191},new byte[2]{182,212},new byte[2]{189,248},new byte[2]{182,224},new byte[2]{200,171},new byte[2]{189,168},new byte[2]{203,251},
            new byte[2]{185,171},new byte[2]{191,170},new byte[2]{195,199},new byte[2]{179,161},new byte[2]{213,185},new byte[2]{202,177},new byte[2]{192,237},new byte[2]{208,194},new byte[2]{183,189},
            new byte[2]{214,247},new byte[2]{198,243},new byte[2]{215,202},new byte[2]{202,181},new byte[2]{209,167},new byte[2]{177,168},new byte[2]{214,198},new byte[2]{213,254},new byte[2]{188,195},
            new byte[2]{211,195},new byte[2]{205,172},new byte[2]{211,218},new byte[2]{183,168},new byte[2]{184,223},new byte[2]{179,164},new byte[2]{207,214},new byte[2]{177,190},new byte[2]{212,194},
            new byte[2]{182,168},new byte[2]{187,175},new byte[2]{188,211},new byte[2]{182,175},new byte[2]{186,207},new byte[2]{198,183},new byte[2]{214,216},new byte[2]{185,216},new byte[2]{187,250},
            new byte[2]{183,214},new byte[2]{193,166},new byte[2]{215,212},new byte[2]{205,226},new byte[2]{213,223},new byte[2]{199,248},new byte[2]{196,220},new byte[2]{201,232},new byte[2]{186,243},
            new byte[2]{190,205},new byte[2]{181,200},new byte[2]{204,229},new byte[2]{207,194},new byte[2]{205,242},new byte[2]{212,170},new byte[2]{201,231},new byte[2]{185,253},new byte[2]{199,176},
            new byte[2]{195,230},new byte[2]{161,163},new byte[2]{163,172},new byte[2]{163,191},new byte[2]{163,161}
        };
        /// <summary>
        /// 表示需要猜测编码的流。
        /// </summary>
        private Stream _thisStream;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_canShuStream">参数流</param>
        public StreamBianMaJianCe(Stream _canShuStream)
        {
            if (_canShuStream == null || _canShuStream.Length < 100)
            {
                throw new System.NullReferenceException("WenZiBianMaJianCe构造函数参数_canShuStream不能为空且长度不能小于100");
            }
            _thisStream = _canShuStream;
        }
        /// <summary>
        /// 检测。
        /// </summary>
        /// <returns>流的编码</returns>
        public Encoding JianCe()
        {
            byte[] _streamByte = DuQuWeiZiJie();
            Encoding _returnEncoding = Encoding.UTF8;
            //通过BOM头来判断编码,有BOM头也就没必要去猜了。
            if (GenJuBomCaiBianMa(_streamByte, out _returnEncoding) == false)
            {
                _returnEncoding = CaiJieWenZiBianMa(_streamByte);
            }
            return _returnEncoding;
        }
        /// <summary>
        /// 读取数据为字节。
        /// </summary>
        /// <returns></returns>
        private byte[] DuQuWeiZiJie()
        {
            BinaryReader _BinaryReader = new BinaryReader(_thisStream);
            byte[] _returnByte = new byte[_thisStream.Length];
            //判断流的长度。
            if (_thisStream.Length < (long)int.MaxValue)//小于整型值的情况
            {
                _BinaryReader.Read(_returnByte, 0, (int)_thisStream.Length);
            }
            else//大于整型值的情况
            {
                long _index = 0;
                while (_index < _returnByte.Length)
                {
                    _returnByte.CopyTo(_BinaryReader.ReadBytes(1024), _index);
                    _index = _index + 1024L;
                }
            }
            return _returnByte;
        }
        /// <summary>
        /// 根据BOM头返回编码格式。
        /// </summary>
        /// <param name="_streamByte">流的字节组。</param>
        /// <returns></returns>
        private bool GenJuBomCaiBianMa(byte[] _streamByte, out Encoding _ruCanEncoding)
        {
            //132 49 149 51 GB-18030
            if (_streamByte[0] == 132 && _streamByte[1] == 49 && _streamByte[2] == 149 && _streamByte[3] == 51)
            {
                _ruCanEncoding = Encoding.GetEncoding("GB-18030");
                return true;
            }

            //239 187 191 UTF-8
            if (_streamByte[0] == 239 && _streamByte[1] == 187 && _streamByte[2] == 191)
            {
                _ruCanEncoding = Encoding.UTF8;
                return true;
            }

            //254 255 Unicode
            if (_streamByte[0] == 254 && _streamByte[1] == 255)
            {
                _ruCanEncoding = Encoding.Unicode;
                return true;
            }

            if (_streamByte[0] == 255 && _streamByte[1] == 254)
            {
                //255 254 0 0 UTF-32
                if (_streamByte[2] == 0 && _streamByte[3] == 0)
                {
                    _ruCanEncoding = Encoding.UTF32;
                }

                //255 254 BigEndianUnicode
                _ruCanEncoding = Encoding.BigEndianUnicode;
                return true;
            }

            //43 47 118 UTF-7
            if (_streamByte[0] == 43 && _streamByte[1] == 47 && _streamByte[2] == 118)
            {
                //[ 56 | 57 | 43 | 47 ]
                if (_streamByte[3] == 56 || _streamByte[3] == 57 || _streamByte[3] == 43 || _streamByte[3] == 47)
                {
                    _ruCanEncoding = Encoding.UTF7;
                    return true;
                }
            }

            _ruCanEncoding = null;
            return false;
        }
        /// <summary>
        /// 猜解文字编码，适用于没有BOM头的情况。
        /// </summary>
        /// <param name="_streamByte">流的字节组。</param>
        /// <returns></returns>
        private Encoding CaiJieWenZiBianMa(byte[] _streamByte)
        {
            //按照中文世界编码使用概率确定猜解顺序。         
            //UTF8
            if (GaoPinZiJianCeUFT8(_streamByte) == true)
            {
                return Encoding.UTF8;
            }
            //gb18030
            if (GaoPinZiJianCeGB18030(_streamByte) == true)
            {
                return Encoding.GetEncoding("gb18030");
            }
            //UTF7
            if (GaoPinZiJianCeUTF7(_streamByte) == true)
            {
                return Encoding.UTF7;
            }
            //UTF-32任何字符都以四个字节编码，必然可以被四整除
            if (_streamByte.Length % 4 == 0)
            {
                if (GaoPinZiJianCeUTF32(_streamByte) == true)//UTF32
                {
                    return Encoding.UTF32;
                }

            }
            //BigEndianUnicode和Unicode任何字符都以二个字节编码，必然可以被二整除
            if (_streamByte.Length % 2 == 0)
            {
                //Unicode
                if (GaoPinZiJianCeUnicode(_streamByte) == true)
                {
                    return Encoding.Unicode;
                }
                //BigEndianUnicod
                if (GaoPinZiJianCeBigEndianUnicode(_streamByte) == true)
                {
                    return Encoding.BigEndianUnicode;
                }
            }
            //如果上述猜解都失败了，则返回UTF-8。
            return Encoding.UTF8;
        }
        /// <summary>
        /// 高频字检查法，UFT8编码方法
        /// </summary>
        /// <param name="_streamByte">流字节组</param>
        /// <returns></returns>
        private bool GaoPinZiJianCeUFT8(byte[] _streamByte)
        {
            //命中次数。
            int _mingZhongCiShu = 0;

            foreach (byte[] bShuZu in _UTF8GaoPinZiFuBianMaLsit)
            {
                for (int i = 0; i < _streamByte.Length; i++)
                {
                    //判断首编码是否相等
                    if (bShuZu[0] == _streamByte[i])
                    {
                        //如果首字节相等，则检查后面二个字节是否也相等
                        if ((i + 1 < _streamByte.Length && _streamByte[i + 1] == bShuZu[1])
                            && (i + 2 < _streamByte.Length && _streamByte[i + 2] == bShuZu[2]))
                        {
                            _mingZhongCiShu++;
                        }
                    }
                }

                //UTF-8有一定几率与其他编码冲突，故而增加命中次数，减少误差。
                if (_mingZhongCiShu > 2)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 高频字检查法，Unicode编码方法
        /// </summary>
        /// <param name="_streamByte">流字节组</param>
        /// <returns></returns>
        private bool GaoPinZiJianCeUnicode(byte[] _streamByte)
        {
            foreach (byte[] bShuZu in _UnicodeGaoPinZiFuBianMaLsit)
            {
                for (int i = 0; i < _streamByte.Length; i++)
                {
                    //判断首编码是否相等
                    if (bShuZu[0] == _streamByte[i]
                        && (i + 1 < _streamByte.Length && _streamByte[i + 1] == bShuZu[1]))
                    {
                        //如果首字节相等，则检查后面二个字节是否也相等
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 高频字检查法，UTF32编码方法
        /// </summary>
        /// <param name="_streamByte">流字节组</param>
        /// <returns></returns>
        private bool GaoPinZiJianCeUTF32(byte[] _streamByte)
        {
            foreach (byte[] bShuZu in _UTF32GaoPinZiFuBianMaLsit)
            {
                for (int i = 0; i < _streamByte.Length; i++)
                {
                    //判断首编码是否相等
                    if (bShuZu[0] == _streamByte[i])
                    {
                        //如果首字节相等，则检查后面二个字节是否也相等
                        if ((i + 1 < _streamByte.Length && _streamByte[i + 1] == bShuZu[1])
                            && (i + 2 < _streamByte.Length && _streamByte[i + 2] == bShuZu[2])
                            && (i + 3 < _streamByte.Length && _streamByte[i + 3] == bShuZu[3]))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 高频字检查法，UTF7编码方法
        /// </summary>
        /// <param name="_streamByte">流字节组</param>
        /// <returns></returns>
        private bool GaoPinZiJianCeUTF7(byte[] _streamByte)
        {
            foreach (byte[] bShuZu in _UTF7GaoPinZiFuBianMaLsit)
            {
                for (int i = 0; i < _streamByte.Length; i++)
                {
                    //判断首编码是否相等
                    if (bShuZu[0] == _streamByte[i])
                    {
                        //如果首字节相等，则检查后面二个字节是否也相等
                        if ((i + 1 < _streamByte.Length && _streamByte[i + 1] == bShuZu[1])
                            && (i + 2 < _streamByte.Length && _streamByte[i + 2] == bShuZu[2])
                            && (i + 3 < _streamByte.Length && _streamByte[i + 3] == bShuZu[3])
                            && (i + 4 < _streamByte.Length && _streamByte[i + 4] == bShuZu[4]))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 高频字检查法，GB18030编码方法
        /// </summary>
        /// <param name="_streamByte">流字节组</param>
        /// <returns></returns>
        private bool GaoPinZiJianCeGB18030(byte[] _streamByte)
        {
            foreach (byte[] bShuZu in _GB18030GaoPinZiFuBianMaLsit)
            {
                for (int i = 0; i < _streamByte.Length; i++)
                {
                    //判断首编码是否相等
                    if (bShuZu[0] == _streamByte[i])
                    {
                        //如果首字节相等，则检查后面二个字节是否也相等
                        if ((i + 1 < _streamByte.Length && _streamByte[i + 1] == bShuZu[0]))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 高频字检查法，Unicode编码方法
        /// </summary>
        /// <param name="_streamByte">流字节组</param>
        /// <returns></returns>
        private bool GaoPinZiJianCeBigEndianUnicode(byte[] _streamByte)
        {
            foreach (byte[] bShuZu in _BigEndianUnicodeGaoPinZiFuBianMaLsit)
            {
                for (int i = 0; i < _streamByte.Length; i++)
                {

                    //判断首编码是否相等
                    if (bShuZu[0] == _streamByte[i])
                    {
                        //如果首字节相等，则检查后面二个字节是否也相等
                        if ((i + 1 < _streamByte.Length && _streamByte[i + 1] == bShuZu[1]))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Nchardet ICharsetDetectionObserver-接口实现
    /// 当NChardet引擎认为自己已经探测出正确的编码时，它就会调用这个Notify方法，
    /// 用户程序可以从这个Nodify方法中得到通知（重写ICharsetDetectionObserver接口的Notify实现）。
    /// </summary>
    public class MyCharsetDetectionObserver : NChardet.ICharsetDetectionObserver
    {
        public Encoding retEncode { get; set; }

        public string EncodeName { get; set; }

        /// <summary>
        /// 通知编码格式
        /// </summary>
        /// <param name="charset"></param>
        public void Notify(string charset)
        {
            EncodeName = charset;
            retEncode = Encoding.GetEncoding(charset);
        }
    }
}
