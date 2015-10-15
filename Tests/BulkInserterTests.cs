using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CoreTechs.Common;
using CoreTechs.Common.Database;
using CoreTechs.Common.Text;
using NUnit.Framework;

namespace Tests
{
    internal class BulkInserterTests
    {
        [Test,Explicit]
        public void Test1()
        {
            const string table = "IP2.Location";
            using (var conn = new SqlConnection("server=.;database=afsx;integrated security=true"))
            using(conn.Connect())
            {
                const SqlBulkCopyOptions opts = SqlBulkCopyOptions.KeepNulls;
                using (var bi = new BulkInserter<Record>(conn, table, copyOptions: opts, bufferSize:20000))
                {
                    conn.ExecuteSql($"TRUNCATE TABLE {table}");

                    using (
                        var reader =
                            new StreamReader(
                                @"C:\dev\webbanking\download\IP-COUNTRY-REGION-CITY-LATITUDE-LONGITUDE-ZIPCODE.CSV"))
                    {
                        var records = from rec in reader.ReadCsv()
                            select new Record
                            {
                                IpFrom = Convert.ToInt64(rec[0]),
                                IpTo = Convert.ToInt64(rec[1]),
                                CountryCode = rec[2],
                                CountryName = rec[3],
                                Region = rec[4],
                                City = rec[5],
                                Latitude = rec[6].ConvertTo<decimal?>(),
                                Longitude = rec[7].ConvertTo<decimal?>(),
                                ZipCode = rec[8]
                            };

                        
                        bi.PostBulkInsert += (sender, args) => Console.WriteLine(args.Items.Length);

                        bi.Insert(records);


                    }
                }
            }
        }

        public class Record
        {
            public long IpFrom { get; set; }
            public long IpTo { get; set; }
            public string CountryCode { get; set; }
            public string CountryName { get; set; }
            public string Region { get; set; }
            public string City { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string ZipCode { get; set; }
        }
    }

}