using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV
{
    public class CSVAnalyze
    {
        public class Line
        {
            public List<string> items = new List<string>();
        }

        public List<Line> lines = new List<Line>();

        public CSVAnalyze( string data )
        {
            var split_lines = data.Split('\n');
            for( var i=0; i< split_lines.Length; i++)
            {
                split_lines[i] = split_lines[i].Replace("\r", "");
            }


            foreach (var line_source in split_lines)
            {
                var line = new Line();
                var tmp_string = "";
                for ( var i=0; i<line_source.Length; i++ )
                {
                    var char_ = line_source[i];
                    if (char_=='"')
                    {

                    }
                    else if (char_ == ',')
                    {
                        line.items.Add(tmp_string); tmp_string = "";
                    }
                    else
                    {
                        tmp_string += char_;
                    }
                }
                //if (tmp_string!="")
                //{
                line.items.Add(tmp_string); tmp_string = "";
                //}
                lines.Add(line);
            }
        }

    }
}
