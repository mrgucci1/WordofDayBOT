using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordofDayBOT
{
    public class WordOfDayObject
    {
        public string insertdate {get; set;}
        public string wordofday { get; set; }
        public string definition { get; set; }
        public string example { get; set; }
        public string longexample { get; set; }
        public string funfact { get; set; }
        public WordOfDayObject(string InsertDate, string Wordofday, string Definition, string Example, string Longexample, string Funfact)
        {
            insertdate = InsertDate;
            wordofday = Wordofday;
            definition = Definition;
            example = Example;
            longexample = Longexample;
            funfact = Funfact;
        }
    }
}
