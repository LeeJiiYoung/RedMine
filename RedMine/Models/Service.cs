using BizScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace RedMine.Models
{
    public class Service
    {
        public static Scraper scraper = new Scraper();
        public static int page = 0;
        public static bool flag = true;
        public Service()
        {
            using (var db = new MainDbContext())
            {
                page = int.Parse(db.Database.SqlQuery<int>("Select id from master_seq").ToList().First().ToString());
            }
        }
        public void GetSellerCollect()
        {
            Scraper ShopScraper = new Scraper();
            HtmlDocument sellerInfo = new HtmlDocument();
            List<string> urlList = new List<string>();
            try
            {
                string text = File.ReadAllText(@"D:\Source\판매자정보수집.txt");
                if (!File.Exists(@"D:\Source\판매자정보수집.txt"))
                {
                    // Create a file to write to. 
                    text = "[]";
                    File.WriteAllText(@"D:\Source\판매자정보수집.txt", text);
                }
                text = File.ReadAllText(@"D:\Source\판매자정보수집.txt");
                text = (text == "") ? "[]" : text;
                JArray jarr = JsonConvert.DeserializeObject<JArray>(text);
                if (flag)
                {
                  
                    for (int i = page; i < 10000; i++)
                    {
                        if (flag)
                        {
                            Thread.Sleep(3000);
                            scraper.Go("http://browse.gmarket.co.kr/list?category=200000505&k=24&p=" + page);
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(scraper.Html);
                            HtmlNodeCollection hnc = null;
                            //클래스 탐색
                            hnc = doc.DocumentNode.SelectNodes("//a[@class='link__shop']");
                            foreach (HtmlNode hn in hnc)
                            {
                                if (flag)
                                {
                                    if (urlList.AsEnumerable().Where(r => r == hn.Attributes["href"].Value).Count() > 0)
                                        continue;
                                    urlList.Add(hn.Attributes["href"].Value);
                                    Thread.Sleep(2000);
                                    string url = hn.Attributes["href"].Value;
                                    ShopScraper.ContentType = "application/x-www-form-urlencoded; charset=utf-8";
                                    ShopScraper.Go(url);
                                    sellerInfo.LoadHtml(ShopScraper.Html);
                                    HtmlNodeCollection SellerInfo = sellerInfo.DocumentNode.SelectNodes("//div[@class='seller_info_box']/dl/dd");
                                    HtmlNodeCollection SellerInfo2 = sellerInfo.DocumentNode.SelectNodes("//div[@class='seller_info_box']/dl/dt");

                                    string compnm = "";
                                    string nm = "";
                                    string tel = "";
                                    string email = "";
                                    string compNo = "";
                                    string adress = "";
                                    string sopmalurl = url;
                                    if (SellerInfo2 == null) continue;
                                    for (int j=0; j<SellerInfo2.Count; j++)
                                    {
                                        switch (SellerInfo2[j].InnerText)
                                        {
                                            case "상호":
                                                compnm = SellerInfo[j].InnerText;
                                                break;
                                            case "대표자":
                                                nm = (string)SellerInfo[j].InnerText;
                                                break;
                                            case "전화번호":
                                                tel = (string)SellerInfo[j].InnerText;
                                                break;
                                            case "이메일":
                                                email = (string)SellerInfo[j].InnerText;
                                                break;
                                            case "사업자번호":
                                                compNo = (string)SellerInfo[j].InnerText;
                                                break;
                                            case "영업소재지":
                                                adress = (string)SellerInfo[j].InnerText;
                                                break;
                                        }
                                    }

                                    JObject jobj = new JObject();
                                    jobj["bizrno"] = compNo;
                                    jobj["cmpny_nm"] = compnm;
                                    jobj["rprsntv_nm"] = nm;
                                    jobj["cmpny_adres"] = adress;
                                    jobj["telno"] = tel;
                                    jobj["email_adres"] = email;
                                    jobj["bsnm_regist_telno"] = compNo;
                                    jobj["sopmal_url"] = url;
                                    jobj["sopmal_grade"] = "ZZZZZZ";
                                    jobj["sopmal_ctgry"] = "홈>>여성의류>>원피스";
                                    jarr.Add(jobj);
                                    string postData = "";
                                    postData = JsonConvert.SerializeObject(jobj);
                                    url = "http://dev.ebizway.co.kr:8082/api/BizesmApi/BatchTB_SELLER";
                                    ShopScraper.ContentType = "application/json";
                                    ShopScraper.Go(url, postData);
                                    //File.WriteAllText(@"D:\Source\판매자정보수집.txt", jarr.ToString());

                                }
                                else { return; }
                            }
                            using (var db = new MainDbContext())
                            {
                                page = int.Parse(db.Database.SqlQuery<int>("Select get_seq('sellerCollect')").ToList().First().ToString());
                            }
                            if (page == 2)
                                return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        public void StopCollect()
        {
            flag = false;
        }
    }
}