using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HSJiDataRipperTool.Models
{
    public class PYLLModel
    {
        public List<PYLLRecord> PYLLList { get; set; }

        public PYLLModel()
        {
            PYLLList = new List<PYLLRecord>();
        }
    }

    public class PYLLRecord
    {
        public string Period { get; set; }
        public string Breakdown { get; set; }
        public string Level { get; set; }
        public string LevelDescription { get; set; }
        public string Gender { get; set; }
        public string DSR { get; set; }
    }
}