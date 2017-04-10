﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MyLauncher;

using System.Net;
using System.IO;

namespace SeLiap
{
    public partial class Form1 : Form
    {
        public class ListSortStyle
        {
            public string text;
            public enum Style
            {
                Unique,      // 重複する名前を削除
                EqConect,    // 重複する名前は、続けてリストアップする
                EqCount,     // 重複する名前は、x[数字] で表記する
                Normal,      // 重複削除せず(未加工)
                OldUniue,    // 逆順、重複する名前を削除
                OldEqConect, // 逆順、重複する名前は、続けてリストアップする
                OldEqCount,  // 逆順、重複する名前は、x[数字] で表記する
            }
            public Style style;
            public ListSortStyle(string text, Style style)
            {
                this.text = text;
                this.style = style;
            }

            public override string ToString()
            {
                return text;
            }
        }

        public class Sendensha
        {
            public string name;
            public int counter;
            public Sendensha( string name)
            {
                this.name = name;
                counter = 1;
            }
        }

        ListSortStyle list_sort_style = null;
        List<ListSortStyle> list_sort_styles = new List<ListSortStyle>();
        List<Sendensha> sendenshas = new List<Sendensha>();

        string config_file = @"config.txt";
        List<Common.ConfigConectUI> config_conect_ui = new List<Common.ConfigConectUI>(); // 設定とUIの接続と保存・読み込みの汎用化

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            list_sort_styles.Add(new ListSortStyle("重複する名前を削除", ListSortStyle.Style.Unique));
            list_sort_styles.Add(new ListSortStyle("重複する名前は、続けてリストアップする", ListSortStyle.Style.EqConect));
            list_sort_styles.Add(new ListSortStyle("重複する名前は、x[数字] で表記する", ListSortStyle.Style.EqCount));
            //
            list_sort_styles.Add(new ListSortStyle("重複削除せず(未加工)",                         ListSortStyle.Style.Normal));
            list_sort_styles.Add(new ListSortStyle("逆順、重複する名前を削除",                     ListSortStyle.Style.OldUniue));
            list_sort_styles.Add(new ListSortStyle("逆順、重複する名前は、続けてリストアップする", ListSortStyle.Style.OldEqConect));
            list_sort_styles.Add(new ListSortStyle("逆順、重複する名前は、x[数字] で表記する",     ListSortStyle.Style.OldEqCount));

            foreach (var s in list_sort_styles)
            {
                comboBox1.Items.Add(s.ToString());
            }


            // コンフィグファイルとUIの関連付けと読み込み
            config_conect_ui.Add(new Common.ConfigConectUI("Main.URL", textBox1));
            config_conect_ui.Add(new Common.ConfigConectUI("Main.列挙スタイル", comboBox1));
            config_conect_ui.Add(new Common.ConfigConectUI("Main.敬称.CheckBox", checkBox1));
            config_conect_ui.Add(new Common.ConfigConectUI("Main.敬称.Text",     textBox2));
            try
            {   // ファイルがなくロードできないことがあるので
                Common.LoadConfig(config_file, config_conect_ui);
            }
            catch
            {
                list_sort_style = list_sort_styles[0];
                comboBox1.Text = list_sort_style.ToString();
            }

            if (checkBox1.Checked)
            {
                textBox2.Enabled = true;
            }
            else
            {
                textBox2.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string sm_id = textBox1.Text;
            var sm_id_start = sm_id.IndexOf("sm");
            if (sm_id_start >= 0 )
            {
                sm_id = sm_id.Substring(sm_id_start + 2);
            }

            //}
            var string_list = new List<string>();
            var offset = 0;
            var page_limit = 100;
            var num = 0;
            richTextBox1.Text = "";

            if (!CheckNumOnley(sm_id))
            {
                AddLog("入力エラー");
            }
            else
            {
                while (true)
                {
                    var add_num = AddSendenshaStringList(ref string_list, sm_id, offset, page_limit);
                    if (add_num == 0) break;
                    num += add_num;
                    offset += page_limit;
                }

                // リストを反転させるかどうか
                if (IsReversList())
                {
                    string_list.Reverse();
                }

                sendenshas.Clear();
                foreach (var str in string_list)
                {
                    var is_add = true;
                    foreach (var s in sendenshas)
                    {
                        if (s.name == str)
                        {
                            s.counter++;
                            is_add = false;
                            break;
                        }
                    }
                    if (is_add)
                    {
                        sendenshas.Add(new Sendensha(str));
                    }
                }


                // 敬称設定の反映
                var foot_text = "";
                if ( checkBox1.Checked )
                {
                    foot_text = textBox2.Text;
                }

                // 出力方式に応じた出力をする
                // Old(Revers)と取得順で多重で記述しているところが微妙...
                switch (GetOutputStyle())
                {
                    case ListSortStyle.Style.Unique:
                        foreach (var s in sendenshas)
                        {
                            richTextBox1.Text += s.name + foot_text + "\n";
                        }
                        break;
                    case ListSortStyle.Style.EqConect:
                        foreach (var s in sendenshas)
                        {
                            for (var i = 0; i < s.counter; i++)
                            {
                                richTextBox1.Text += s.name + foot_text + "\n";
                            }
                        }
                        break;
                    case ListSortStyle.Style.EqCount:
                        foreach (var s in sendenshas)
                        {
                            if (s.counter > 1)
                            {
                                richTextBox1.Text += s.name + foot_text + " x" + s.counter.ToString() + "\n";
                            }
                            else
                            {
                                richTextBox1.Text += s.name + foot_text + "\n";
                            }
                        }
                        break;
                    case ListSortStyle.Style.Normal:
                        foreach (var s in string_list)
                        {
                            richTextBox1.Text += s + foot_text + "\n";
                        }
                        break;
                }

                toolStripStatusLabel1.Text = string.Format("宣伝者数 : {0}", sendenshas.Count);
                AddLog("sm" + sm_id + " 宣伝者の解析完了");
            }
        }

        // リストを反転させるかどうか
        private bool IsReversList()
        {
            switch(list_sort_style.style)
            {
                default:
                    return false;
                case ListSortStyle.Style.OldUniue:
                    return true;
                case ListSortStyle.Style.OldEqConect:
                    return true;
                case ListSortStyle.Style.OldEqCount:
                    return true;
            }
        }

        // 出力スタイル
        private ListSortStyle.Style GetOutputStyle()
        {
            switch (list_sort_style.style)
            {
                default:
                    return list_sort_style.style;
                case ListSortStyle.Style.OldUniue:
                    return ListSortStyle.Style.Unique;
                case ListSortStyle.Style.OldEqConect:
                    return ListSortStyle.Style.EqConect;
                case ListSortStyle.Style.OldEqCount:
                    return ListSortStyle.Style.EqCount;
            }
        }

        private int AddSendenshaStringList( ref List<string> string_list, string sm_id, int offset, int page_limit )
        {

            var url = string.Format(@"http://uad-api.nicovideo.jp/UadsCampaignService/getAdHistoryJsonp?vid=sm{0}&offset={1}&limit={2}",sm_id,offset,page_limit);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream s = res.GetResponseStream();
            StreamReader sr = new StreamReader(s);
            string content = sr.ReadToEnd();

            var num = 0;
            {
                var i = 0;
                while (true)
                {
                    var str_start = content.IndexOf("\"name\"", i);
                    if (str_start < 0) break;
                    var name_offset = 9;
                    var str_end = content.IndexOf("\"", str_start + name_offset);
                    var str = content.Substring(str_start + name_offset - 1, str_end - str_start - name_offset + 1);
                    string_list.Add(GetEndcode(str));
                    //Console.WriteLine("* " + str);
                    i = str_start + 1;
                    num++;
                    WaitSleep.Do(10);
                }
            }

            return num;
        }

        private string GetEndcode( string src )
        {
            string res = "";

            var size = src.Length;
            for (var i = 0; i < size; i++)
            {
                var pos = i;
                if (src.Substring(pos, 1) == "\\")
                {
                    if (src.Substring(pos, 2) == "\\u")
                    {

                        var s = src.Substring(pos + 2, 4);
                        int code16 = Convert.ToInt32(s, 16);
                        char c = Convert.ToChar(code16);  // 数値(文字コード) -> 文字
                        string new_char = c.ToString();
                        res += new_char;
                        i += 5;
                    }
                    else if (src.Substring(pos, 2) == "\\/")
                    {
                        res += @"/";
                        i += 1;
                    }
                    else if (src.Substring(pos, 2) == "\\\\")
                    {
                        res += @"\";
                        i += 1;
                    }
                    else
                    {
                        //Console.WriteLine("err " + src);
                        //richTextBox2.Text += "解析できない宣伝者名がありました " + src;
                        AddLog("解析できない宣伝者名がありました " + src);
                        res += @"/";
                        i += 1;
                    }
                }
                else
                {
                    res += src.Substring(pos, 1);
                }
            }

            return res;
        }

        private bool CheckNumOnley(string str)
        {
            char[] ok_list = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if (str.Length == 0) return false;

            foreach ( var s1 in str)
            {
                bool is_ok = false;
                foreach ( var s2 in ok_list )
                {
                    if (s1 == s2)
                    {
                        is_ok = true;
                        break;
                    }
                }
                if(is_ok==false) return false;
            }
            return true;
        }

        private void AddLog( string text )
        {
            var rtb = richTextBox2;

            rtb.Text += text + "\n";

            rtb.SelectionStart = rtb.Text.Length; //カレット位置を末尾に移動
            rtb.Focus(); //テキストボックスにフォーカスを移動
            rtb.ScrollToCaret();//カレット位置までスクロール

        }

        object target_control = null;
        private void textBox1_Click(object sender, EventArgs e)
        {
            if (target_control==null)
            {
                target_control = sender;

                var tb = sender as TextBox;
                tb.SelectAll();
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            target_control = null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var s in list_sort_styles)
            {
                if ( comboBox1.Text==s.text )
                {
                    list_sort_style = s;
                    break;
                }
            }
            Common.SaveConfig(config_file, config_conect_ui);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Common.SaveConfig(config_file, config_conect_ui);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if ( checkBox1.Checked )
            {
                textBox2.Enabled = true;
            }
            else
            {
                textBox2.Enabled = false;
            }

            Common.SaveConfig(config_file, config_conect_ui);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Common.SaveConfig(config_file, config_conect_ui);
        }
    }

    

}