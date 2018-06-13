using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SeLiap
{
    public class JSONPAnalyze
    {
        public class JSONP
        {
            public enum Type{
                None,
                Dict,   // 辞書
                List,   // 配列
                String, // 文字列
                Num,    // 数値
            };

            public string name;
            public Type type = Type.None;
            // 内容は 辞書、配列、変数の場合が有る
            public dynamic value;
            //public Dictionary<string, JSONP> v_dict = new Dictionary<string, JSONP>();
            //public List<string,>

            private JSONP()
            {
            }

            public JSONP( string data, out string rem_data )
            {
                // 変数名の取得
                var i1 = data.IndexOf("\"");
                var i2 = data.IndexOf("\"", i1 + 1);
                name = data.Substring(i1 + 1, i2 - 1);
                var i3 = data.IndexOf(":", i2 + 1); // この先が内容
                data = data.Substring( i3+1 );

                value = GetValue(data, out rem_data);
            }

            private dynamic GetValue( string data, out string rem_data)
            {
                rem_data = "";
                const string nums = "0123456789";
                const string nums2 = "0123456789.";

                var i = 0;
                foreach ( var s in data )
                {
                    if ( s == ' ' )
                    {
                    }
                    else if ( s == '{' )
                    {
                        type = Type.Dict;
                        var dict = new Dictionary<string, JSONP>();
                        rem_data = data.Substring(i + 1);
                        while( true )
                        {
                            var jsonp = new JSONP(rem_data, out rem_data);
                            dict.Add(jsonp.name, jsonp);

                            var i1 = rem_data.IndexOf('}');
                            var i2 = rem_data.IndexOf(',');
                            if ( ( i2 >= 0 ) && ( i1 >= 0 ) && ( i2 < i1 ) )
                            {
                                // ,が先にある
                                rem_data = rem_data.Substring(i2 + 1);
                            }
                            else
                            {
                                if (i1>=0)
                                {
                                    rem_data = rem_data.Substring(i1+1);
                                }
                                return dict;
                            }

                        }
                    }
                    else if (s == '[')
                    {
                        type = Type.List;
                        var list = new List<JSONP>();
                        rem_data = data.Substring(i + 1);
                        while (true)
                        {
                            var jsonp = new JSONP();
                            jsonp.value = GetValue(rem_data, out rem_data);
                            list.Add(jsonp);

                            var i1 = rem_data.IndexOf(']');
                            var i2 = rem_data.IndexOf(',');
                            if ((i2 >= 0) && (i1 >= 0) && (i2 < i1))
                            {
                                // ,が先にある
                                rem_data = rem_data.Substring(i2 + 1);
                            }
                            else
                            {
                                if (i1 >= 0)
                                {
                                    rem_data = rem_data.Substring(i1+1);
                                }
                                return list;
                            }

                        }

                    }
                    else if (s == '\"')
                    {
                        type = Type.String;
                        var i1 = data.IndexOf('\"', i + 1); // todo: 文字列中の"はどうなる？
                        var str = data.Substring(i+1, i1-1);
                        str = EndcodeString(str);
                        rem_data = data.Substring(i1+1);
                        return str;
                    }
                    else
                    {
                        var is_num = false;
                        foreach ( var n in nums)
                        {
                            if ( s==n )
                            {
                                is_num = true;
                                break;
                            }
                        }
                        if ( is_num )
                        {
                            type = Type.Num;
                            // 数字が続くまで
                            for ( var j=i+1; j<data.Count(); j++ )
                            {
                                var is_num2 = false;
                                foreach( var n in nums2 )
                                {
                                    if ( data[j]==n )
                                    {
                                        is_num2 = true;
                                        break;
                                    }
                                }
                                if ( !is_num2 )
                                {
                                    var str = data.Substring(i, j);
                                    rem_data = data.Substring(j);
                                    return int.Parse(str);
                                }
                            }
                            break;
                        }
                    }
                    i++;
                }
                
                // {}
                // []
                // "" 文字列
                // 数値

                return null;
            }

            private string EndcodeString(string src)
            {
                // 文字列のエンコード
                // \uXXXX = [全角文字列]
                // \\     = \
                // \/     = /
                // &amp;  = &
                // &quot; = "
                // &#039; = '
                //
                // 調査済み(変換不要) ;!#$%=~^|@+-* 
                // 未調査 `

                string res = "";

                var size = src.Length;
                for (var i = 0; i < size; i++)
                {
                    var pos = i;
                    if (src.Substring(pos, 1) == "\\")
                    {
                        // SubStringをそのまま使うと文字数が範囲を超えてエラーになるので、長さを確認したクッションの関数が必要
                        if (SubStringEx(src, pos, 2) == "\\u")
                        {
                            var s = SubStringEx(src, pos + 2, 4);
                            int code16 = Convert.ToInt32(s, 16);
                            char c = Convert.ToChar(code16);  // 数値(文字コード) -> 文字
                            string new_char = c.ToString();
                            res += new_char;
                            i += 5;
                        }
                        else if (SubStringEx(src, pos, 2) == "\\/")
                        {
                            res += @"/";
                            i += 1;
                        }
                        else if (SubStringEx(src, pos, 2) == "\\\\")
                        {
                            res += @"\";
                            i += 1;
                        }
                        else
                        {
                            // todo:err
                            res += @"/";
                            i += 1;
                        }
                    }
                    else if (src.Substring(pos, 1) == "&")
                    {
                        // SubStringをそのまま使うと文字数が範囲を超えてエラーになるので、長さを確認したクッションの関数が必要
                        if (SubStringEx(src,pos, 5) == "&amp;")
                        {
                            res += @"&";
                            i += 4;
                        }
                        else if (SubStringEx(src, pos, 6) == "&quot;")
                        {
                            res += "\"";
                            i += 5;
                        }
                        else if (SubStringEx(src, pos, 4) == "&lt;")
                        {
                            res += "<";
                            i += 3;
                        }
                        else if (SubStringEx(src, pos, 4) == "&gt;")
                        {
                            res += ">";
                            i += 3;
                        }
                        else if (SubStringEx( src, pos, 6) == "&#039;")
                        {
                            res += "'";
                            i += 5;
                        }
                        else
                        {
                            // todo:err
                            res += @"&";
                            i += 0;
                        }
                    }
                    else
                    {
                        res += src.Substring(pos, 1);
                    }
                }

                return res;
            }
        }

        static private string SubStringEx( string src, int start, int length )
        {
            //if (src== "実況したい月読ショウタ&鏡音レン")
            //{
            //    var a = 0;
            //}
            if ( start+length > src.Count() )
            {
                length = src.Count() - start;
            }
            var res = src.Substring(start, length);
            return res;
        }

        public string function_name = "";

        public JSONPAnalyze()
        {
        }
        
        public JSONP Analyze( string data)
        {
            // 関数名と、外側の()を削除
            {
                var i = data.IndexOf("(");
                function_name = data.Substring(0, i);
                data = data.Substring(i+1);
                data = data.Substring(0, data.LastIndexOf(")"));
            }

            data = "\"\":" + data; // 無名変数として作成させるために{}の手前に "": を加える（処理を共通化できるので）

            string rem_data;
            var jsonp = new JSONP( data, out rem_data);


            return jsonp;
        }

        public JSONP AnalyzeByJSON(string data)
        {
            data = "\"\":" + data;
            string rem_data;
            var jsonp = new JSONP(data, out rem_data);


            return jsonp;
        }
    }
}
