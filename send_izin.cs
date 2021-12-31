using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M4.Service.Web.PasaliOglu
{
  public  class send_izin
    {
        public void Izin()
        {

            string BasTarih = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd HH:mm:ss");
            string BitTarih = DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd HH:mm:ss");

            StringBuilder ExecString = new StringBuilder();
            ExecString.AppendFormat("exec sp_AllIzin '{0}','{1}'", BasTarih, BitTarih);


            DataTable result = new SYS_SERVIS().SQLFill(ExecString.ToString());
            //Gönderilecek objeyi tanımladık
            List<IzinObje> sendData = new List<IzinObje>();

            Result p = new Result()
            {
                wcfusername = "*************",
                wcfuserpass = "*******************************",
                persons = new List<IzinObje>()

            };


            new M4.Common.DataAccessLayer.SYS_SERVIS().SQLExec("truncate table SendIzinRequest");
            for (int i = 0; i < result.Rows.Count; i++)
            {

                var client = new RestClient("url");
                client.Timeout = -1;

                var request = new RestRequest(Method.POST);
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Cookie", "ASP.NET_SessionId=4bwfh0yt4o0kgdlc0ccpgsl0");

                bool ucretlimi = true;

                if (result.Rows[i]["Ucretli"].ToString() == "1")
                {
                    ucretlimi = true;
                }
                else if (result.Rows[i]["Ucretli"].ToString() == "0")
                {
                    ucretlimi = false;
                }
                IzinObje izin = new IzinObje()
                {



                    SicilTckNo = result.Rows[i]["TCKimlikNo"].ToString(),
                    IzinTipi = result.Rows[i]["IzinTip"].ToString(),
                    TalepTarihi = result.Rows[i]["Tarih"].ToString(),

                    BaslangicZamani = result.Rows[i]["BasTarih"].ToString(),
                    BitisZamani = result.Rows[i]["BitTarih"].ToString(),

                    Ucretlimi = ucretlimi,
                    TalepGun = result.Rows[i]["Gun"].ToString(),
                    TalepSaat = result.Rows[i]["Saat"].ToString(),
                    Aciklama = result.Rows[i]["Aciklama"].ToString(),
                    IzinID = (int)result.Rows[i]["ID"]
                };
                p.persons.Clear();
         
                p.persons.Add(izin);
                //Göderilecek objeyi doldurarak Serialize json hale getirdik
                string body = JsonConvert.SerializeObject(p);
            
             
                //Datayı Gönderdik
                request.AddParameter("application/json", body, ParameterType.RequestBody);


                var response = client.Execute(request);
                var dataResult = JsonConvert.DeserializeObject<IzinAktarResult>(response.Content);

                string data = @"{ ""IzinAktarimResult"":" + dataResult.IzinAktarimResult + " }";
                var dataResult2 = JsonConvert.DeserializeObject<IzinAktarim>(data);

               
                foreach (var m in dataResult2.IzinAktarimResult)
                {
                    if (m.ReturnStatus)
                    {
                        StringBuilder pers = new StringBuilder();
                        pers.AppendFormat("UPDATE  Izinler set   ResponseCheck ='1'  WHERE ID='{0}'", m.IzinID);


                        new M4.Common.DataAccessLayer.SYS_SERVIS().SQLExec(pers.ToString());
                    }

                }





                response = null;
                p.persons.Clear();
                dataResult2.IzinAktarimResult.Clear();

            }






        }
    }
}
