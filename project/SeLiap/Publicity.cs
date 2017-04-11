using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

namespace SeLiap
{
    // 宣伝者(広告主)についてのモジュール
    
    public class PublicityData
    {
        public string name;
        public string comment;
        public int counter;
        public PublicityData(string name, string comment)
        {
            this.name = name;
            this.comment = comment;
            counter = 1;
        }

        public PublicityData Copy()
        {
            var pd = new PublicityData( name, comment );
            return this;
        }
    }

    public class PublicityLog
    {
        RichTextBox log_output_target;

        public PublicityLog( RichTextBox log_output_target)
        {
            this.log_output_target = log_output_target;
        }

        public void WriteLine( string log )
        {
            log_output_target.Focus();
            log_output_target.AppendText(log + System.Environment.NewLine);

            //log_output_target.Text += log + "\n";
        }
    }

    public class PublicityAnalyze
    {
        public List<PublicityData> none_effect_publicitys = new List<PublicityData>();  // 加工してない宣伝者リスト
        public List<PublicityData> publicitys = new List<PublicityData>();              // 加工済み宣伝者リスト

        PublicityLog log = null;

        public PublicityAnalyze(PublicityLog log )
        {
            this.log = log;
        }

        public void Analyze( string url_source)
        {
            var url = AnalyzeURL(url_source);
            if (!IsURLCheck(url))
            {
                log.WriteLine("URLが不適切です > " + url);
                return;
            }
            log.WriteLine("URLチェッククリア > " + url);

            // ダミーデータを読み込む
            var file = new System.IO.StreamReader("test_data.txt");
            string data = "";
            string line;
            while ((line = file.ReadLine()) != null)
            {
                data += line + "\n";
            }
            file.Close();

            var ja = new JSONPAnalyze();
            var jsonp = ja.Analyze(data);

            
            try
            {
                // meta.status == 200
                // meta.message == "succeed"
                dynamic jsonp_meta = jsonp.value["meta"].value;

                if ( ( jsonp_meta["status"].value == 200 ) && (jsonp_meta["message"].value == "succeed") )
                {
                    log.WriteLine("データ取得成功");

                }
                else
                {
                    log.WriteLine("データ取得失敗 A");
                    return;
                }


            }
            catch
            {
                log.WriteLine("データ取得失敗 B");
                return;
            }
            
            dynamic jsonp_data = jsonp.value["data"].value;
            foreach (var j in jsonp_data)
            {
                var name = j.value["name"].value;
                var comment = j.value["campaignname"].value;
                var item = new PublicityData(name, comment);
                none_effect_publicitys.Add(item);
            }
        }

        // URLを解析しvidの引数として適切な値になるように加工
        protected string AnalyzeURL( string url_source )
        {
            var res = url_source;
            // /の最後でカットする
            {
                var i = res.LastIndexOf(@"/");
                if ( i >= 0 )
                {
                    res = res.Substring(i+1);
                } 
            }

            // 後ろにゴミが入っている場合の削除（?が境界)
            {
                var i = res.LastIndexOf(@"?");
                if (i >= 0)
                {
                    res = res.Substring(0,i);
                }

            }

            return res;
        }

        // URLが適切かどうかチェックする
        protected bool IsURLCheck(string url)
        {
            var rem = url; // 調査の残り

            // 正規表現で言えば /^(sm|so|nm|)([0-9]+)$/ 
            // 先頭が sm| so| nm | 数字である
            if (rem.IndexOf("sm") == 0)
            {
                rem =  rem.Substring(2);
            }
            else if (rem.IndexOf("so") == 0)
            {
                rem = rem.Substring(2);
            }
            else if (rem.IndexOf("nm") == 0)
            {
                rem = rem.Substring(2);
            }

            // 残りが最初から最後まで半角の数字である
            var d = "0123456789";

            foreach( var s1 in rem )
            {
                var is_ok = false;
                foreach( var s2 in d )
                {
                    if ( s1==s2 )
                    {
                        is_ok = true;
                        break;
                    }
                }
                if (!is_ok)
                {
                    // d以外の文字が入っている → false
                    return false;
                }
            }

            return true;
        }

    }
}
