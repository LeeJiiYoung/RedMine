using BizScraper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using System.Threading;

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
            try
            {
                if (flag)
                {
                    for (int i = page; i < 2; i++)
                    {
                        if (flag)
                        {
                            scraper.Go("http://browse.gmarket.co.kr/list?category=200000507&k=24&p=" + page);
                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(scraper.Html);
                            HtmlNodeCollection hnc = null;
                            //클래스 탐색
                            hnc = doc.DocumentNode.SelectNodes("//a[@class='link__shop']");
                            foreach (HtmlNode hn in hnc)
                            {
                                if (flag)
                                {
                                    Thread.Sleep(2000);
                                    string url = hn.Attributes["href"].Value;
                                    ShopScraper.Go(url);
                                    sellerInfo.LoadHtml(ShopScraper.Html);
                                    HtmlNodeCollection SellerInfo = sellerInfo.DocumentNode.SelectNodes("//div[@class='seller_info_box']/dl/dd");
                                    //상호
                                    string compnm = SellerInfo[0].InnerText;

                                    //대표자
                                    string nm = (string)SellerInfo[1].InnerText;

                                    // 전화번호
                                    string tel = (string)SellerInfo[2].InnerText;

                                    // 팩스
                                    string fax = (string)SellerInfo[3].InnerText;

                                    // 메일
                                    string email = (string)SellerInfo[4].InnerText;

                                    // 사업자번호
                                    string compNo = (string)SellerInfo[5].InnerText;

                                    // 주소
                                    string adress = (string)SellerInfo[6].InnerText;
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