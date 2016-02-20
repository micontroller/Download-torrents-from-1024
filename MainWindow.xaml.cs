using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Net;
using System.IO;

namespace grabDownlaodLinksFrom1024
{
    public partial class MainWindow : Window
    {
        HashSet<string> links = new HashSet<string>();
        List<string> topics = new List<string>();
        List<DateTime> dates = new List<DateTime>();
        string HTML;
        string lastDownloadDate = "";


        public MainWindow()
        {
            InitializeComponent();
            lastDownloadDate = File.ReadLines("./lastDownloadDate.txt").First();
            lblLastDownload.Content = lastDownloadDate;
            button_Copy_Click(null, null);
        }
        
        //find all torrent links from a page
        bool hasLinks()
        {
            string link = getBetween(HTML, "<a href=\"http://www1.newstorrentsspace.info/freeone/file.php/", ".html");
            if (link != "")
            {
                links.Add(link);
                HTML = HTML.Replace("<a href=\"http://www1.newstorrentsspace.info/freeone/file.php/" + link + ".html", "");
                return true;
            }
            else {
                return false;
            }
        }

        //download full html from a page
        string getHTML(string url, int substringIndex)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                return wc.DownloadString(url).Substring(substringIndex);
            }
        }

        //get keyword in a string
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            //grab Topics from first three pages on 1024 fourm
            for (int i = 1; i <= 3; i++)
            {
                HTML = getHTML("http://opp.hegc1024.com/pw/simple/index.php?f3_" + i + ".html", 0);
                while (hasTopic())
                { }
            }

            //get Topics newer than Last Download
            var dt = Convert.ToDateTime(lastDownloadDate).AddDays(-1);
            int flag = 0;
            var topicsCount = topics.Count;
            for (int i = 0; i < topicsCount; i++)
            {
                if (dates[flag] > dt)
                    flag++;
                else {
                    dates.RemoveAt(flag);
                    topics.RemoveAt(flag);
                }
            }

            //grab Links within Topics
            foreach (var url in topics)
            {
                if (!String.IsNullOrEmpty(url))
                {
                    HTML = getHTML(url, 13448);
                    while (hasLinks())
                    { }
                }
            }

            //write last download date to file
            dt = DateTime.Now;
            File.WriteAllText("./lastDownloadDate.txt", dt.ToString("yyyy/MM/dd"));
            lblLastDownload.Content = dt.ToString("yyyy/MM/dd");
            lblLinksCount.Content = links.Count;

            ////build links in html
            //string goodies = "";
            //foreach (var link in links)
            //    goodies += "<a href=\"http://www1.newstorrentsspace.info/freeone/file.php/" + link + ".html\" target=\"_blank\">" + link + "</a><br />";

            ////html save path(desktop)
            //var htmlPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\goodies@" + DateTime.Now.ToString("MM.dd") + ".html";

            ////save html
            //if (links.Count != 0)
            //{
            //    File.WriteAllText(htmlPath, goodies);
            //    MessageBox.Show("Html created:\r\n" + htmlPath);
            //    this.Close();
            //}
            //else {
            //    MessageBox.Show("no updates");
            //    this.Close();
            //}

            if (links.Count != 0)
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var path = desktopPath + "\\goodies@" + DateTime.Now.ToString("MM.dd");
                Directory.CreateDirectory(path);

                using (WebClient wc = new WebClient())
                {
                    foreach (var id in links)
                    {
                        var Uri = "http://www1.newstorrentsspace.info/freeone/down.php?";
                        var parameters = Encoding.UTF8.GetBytes("type=torrent&id=" + id + "&name=" + id);

                        wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        wc.Headers[HttpRequestHeader.Host] = "www1.newstorrentsspace.info";
                        wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0";
                        wc.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                        wc.Headers[HttpRequestHeader.AcceptLanguage] = "zh-TW,zh;q=0.8,en-US;q=0.5,en;q=0.3";
                        wc.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                        wc.Headers[HttpRequestHeader.Referer] = "http://www1.newstorrentsspace.info/freeone/file.php/" + id + ".html";

                        byte[] result = wc.UploadData(Uri, "POST", parameters);
                        File.WriteAllBytes(path + "\\" + id + ".torrent", result);
                    }
                }
                MessageBox.Show("Torrents Downloaded:\r\n" + path);
                this.Close();
            }
            else {
                MessageBox.Show("no updates");
                this.Close();
            }
        }

        //find topics on 1024 fourm
        bool hasTopic()
        {
            var keyWordIndex = HTML.IndexOf(".html\">㊣無碼國產の精彩合集ⓑ");
            if (keyWordIndex != -1)
            {
                var topic = HTML.Substring(keyWordIndex - 7, 7);
                topics.Add("http://opp.hegc1024.com/pw/simple/index.php?" + topic + ".html");
                var date = Convert.ToDateTime("2016/" + HTML.Substring(keyWordIndex + 18, 6).Replace("[", "").Replace("]", "").Replace(".", "/"));
                dates.Add(date);
                HTML = HTML.Remove(keyWordIndex, 5);
                return true;
            }
            else {
                return false;
            }
        }
    }
}
