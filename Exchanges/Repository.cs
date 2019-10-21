using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml;

namespace Exchanges
{
    public class Repository
    {
        private SqlCommand cmd;

        public Repository()
        {
            cmd = new SqlCommand(ConfigurationManager.ConnectionStrings["Valutes"].ToString());
        }

        public bool CheckLastDate(string valuteName)
        {
            using (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("valuteName", valuteName);
                cmd.Parameters.AddWithValue("date", DateTime.Now.ToShortDateString());
                cmd.CommandText = "select * from Курс where Валюта = @valuteName and Дата = @date";

                using(var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        return true;
                }
            }

            return false;
        }

        public double GerRate(string valuteName, DateTime date)
        {
            using (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("valuteName", valuteName);
                cmd.Parameters.AddWithValue("date", DateTime.Now.ToShortDateString());
                cmd.CommandText = "select Rate.Курс rate from Курс Rate join Валюта Valute on Rate.Валюта = Valute.Валюта where Rate.Валюта = @valuteName and Rate.Дата = @date";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        return double.Parse(reader["rate"].ToString());
                }
            }

            return -1;
        }

        public void WriteNewRate(string valuteCode)
        {
            if (CheckLastDate(valuteCode))
                return;

            var xDoc = new XmlDocument();

            xDoc.Load($"http://www.cbr.ru/scripts/XML_daily.asp?date_req=" + DateTime.Today.ToShortDateString());
            var b = xDoc.GetElementsByTagName("Valute");
            var xRoot = xDoc.DocumentElement;

            var valute = new Valute();

            foreach (XmlElement xnode in xRoot)
            {                
                if(xnode.Attributes.GetNamedItem("ID").Value != valuteCode)
                    continue;

                valute.Code = valuteCode;
                valute.Date = DateTime.Today;

                foreach (XmlNode childnode in xnode.ChildNodes)
                {
                    if (childnode.Name == "Name")
                        valute.Name = childnode.InnerText;

                    if (childnode.Name == "Value")
                        valute.Rate = double.Parse(childnode.InnerText);
                }

                break;
            }

            using (cmd)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("valute", valute.Name);
                cmd.Parameters.AddWithValue("date", valute.Date.ToShortDateString());
                cmd.Parameters.AddWithValue("rate", valute.Rate);
                cmd.CommandText = "inset into Курс (Дата, Курс, Валюта) values (@date, @rate, @valute)";
                cmd.ExecuteNonQuery();
            }
        }


    }
}
