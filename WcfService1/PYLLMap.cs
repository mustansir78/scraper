using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfService1
{
    public class PYLLMap : CsvClassMap<PYLLRecord>
    {
        public PYLLMap()
        {
            Map(m => m.Reporting_Period).Name("Reporting period");
            Map(m => m.Breakdown).Name("Breakdown");
            Map(m => m.Level).Name("Level");
            Map(m => m.Level_Description).Name("Level description");
            Map(m => m.Condition).Name("Condition");
            Map(m => m.Gender).Name("Gender");
            Map(m => m.DSR).Name("DSR");
            Map(m => m.CI_Lower).Name("CI lower");
            Map(m => m.CI_Upper).Name("CI upper");
            Map(m => m.Registered_Patients).Name("Registered patients");
            Map(m => m.Years_of_life_lost).Name("Years of life lost");
            Map(m => m.Observed_deaths).Name("Observed deaths");

        }
    }
}