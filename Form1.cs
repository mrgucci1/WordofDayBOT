using System;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordofDayBOT
{
    public partial class WordofDayBOT : Form
    {
        dataTableObjects dtobj = new dataTableObjects();
        public WordofDayBOT()
        {
            InitializeComponent();
            InitializeAsync();
        }
        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            webView.CoreWebView2.ScriptDialogOpening += CoreWebView2_ScriptDialogOpening;
        }
        private void CoreWebView2_ScriptDialogOpening(object sender, Microsoft.Web.WebView2.Core.CoreWebView2ScriptDialogOpeningEventArgs e)
        {
            e.Accept();
        }
        private void WordofDayBOT_Load(object sender, EventArgs e)
        {
            driverFunction();
        }
        public async Task driverFunction()
        {
            //boolean variable to check if errors occur in supporting functions
            bool errorCheck = true;
            printToTextbox("Waiting for browser initilization");
            await PutTaskDelay(10);
            if (webView == null || webView.CoreWebView2 == null)
            {
                Application.Restart();
                Environment.Exit(0);
            }
            //navigate to word of day website
            printToTextbox("navigating to word of day website");
            webView.CoreWebView2.Navigate("https://www.merriam-webster.com/word-of-the-day");
            await PutTaskDelay(5);
            errorCheck = await checkWebsiteLoad("document.getElementsByClassName('word-and-pronunciation')[0].getElementsByTagName('h1')[0].innerText");
            if (errorCheck == false) { printToTextbox("website failed to load, exiting"); await PutTaskDelay(5); Environment.Exit(0); }
            //scrape data
            DataTable dt = new DataTable();
            dt = dtobj.getWordOfDayTable(dt);
            DataRow dr = dt.NewRow();
            printToTextbox("scraping values...");
            dr["insertdate"] = DateTime.Now.ToShortDateString();
            dr["wordofday"] = await scrapeStringClean("document.getElementsByClassName('word-and-pronunciation')[0].getElementsByTagName('h1')[0].innerText");
            dr["definition"] = await scrapeStringClean("document.getElementsByClassName('wod-definition-container')[0].getElementsByTagName('p')[0].innerText");
            dr["example"] = await scrapeStringClean("document.getElementsByClassName('wod-definition-container')[0].getElementsByTagName('p')[1].innerText");
            dr["longexample"] = await scrapeStringClean("document.getElementsByClassName('wotd-examples')[0].getElementsByClassName('left-content-box')[0].innerText");
            dr["funfact"] = await scrapeStringClean("document.getElementsByClassName('did-you-know-wrapper')[0].getElementsByTagName('p')[0].innerText");
            printToTextbox("done,inserting into SQL database");
            dt.Rows.Add(dr);
            dtobj.tableInsert(dt, "onestone-EXPERIMENTS", "insights_wordofday");
            WordOfDayObject wordofdayOb = new WordOfDayObject(dr["insertdate"].ToString(), dr["wordofday"].ToString(), dr["definition"].ToString(), dr["example"].ToString(), dr["longexample"].ToString(), dr["funfact"].ToString());
            await postToSlack(wordofdayOb);
        }
        public async Task postToSlack(WordOfDayObject wordofdayOb)
        {
            var data = new NameValueCollection();
            data["token"] = "xoxb-1446060487383-2261014106708-9xiOPGIejUYEPqLDZ4fZarr3";
            data["channel"] = "insights-bot";
            data["as_user"] = "true";           // to send this message as the user who owns the token, false by default
            data["text"] = $"Todays date: {wordofdayOb.insertdate} :aliensmall:\n\nWord of the day is: *{wordofdayOb.wordofday}*\nDefinition: {wordofdayOb.definition}\nUsed in sentence: {wordofdayOb.example}\n\nExamples: {wordofdayOb.longexample}\n\nFun fact: {wordofdayOb.funfact}";
            var client = new WebClient();
            var response = client.UploadValues("https://slack.com/api/chat.postMessage", "POST", data);
            string responseInString = Encoding.UTF8.GetString(response);
        }
        //scrape value from website and clean string
        public async Task<string> scrapeStringClean(string javaScript)
        {
            string returnVar = await webView.CoreWebView2.ExecuteScriptAsync(javaScript);
            returnVar = returnVar.ToString().Replace(@"\", "");
            return returnVar;
        }
        public async Task<bool> checkWebsiteLoad(string functionString)
        {
            var returnVar = "undefined";
            Stopwatch whileTimer = new Stopwatch();
            whileTimer.Start();
            while (returnVar.ToString().Contains("undefined") || returnVar.ToString().Contains("null"))
            {
                returnVar = await webView.CoreWebView2.ExecuteScriptAsync(functionString);
                returnVar = returnVar.ToString().Replace("\"", "");
                printToTextbox("Not Loaded");
                await PutTaskDelay(2);
                if (whileTimer.ElapsedMilliseconds > 300000)
                {
                    printToTextbox("Timeout in checkTableLoad");
                    return false;
                }
            }
            await PutTaskDelay(5);
            printToTextbox("Loaded!");
            return true;
        }
        public void printToTextbox(string printme)
        {
            richTextBox1.AppendText(printme);
            richTextBox1.AppendText(Environment.NewLine);
        }
        public async Task evalScrip(string functionString, int delay, string action)
        {
            await webView.CoreWebView2.ExecuteScriptAsync(functionString);
            printToTextbox($"{action} - Delay={delay} Seconds");
            await PutTaskDelay(delay);
        }
        public async Task PutTaskDelay(int delay)
        {
            //Multiply int by 1000 to get in miliseconds
            delay = delay * 1000;
            await Task.Delay(delay);
        }


        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {
            // set the current caret position to the end
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            // scroll it automatically
            richTextBox1.ScrollToCaret();
            try { using (StreamWriter newTask = new StreamWriter(Directory.GetCurrentDirectory() + $@"\temp_log_{DateTime.Now.ToString("yyyyMMdd")}.txt", false)) { newTask.WriteLine(richTextBox1.Text); } }
            catch { printToTextbox("Used by another process error"); }
        }
    }
}
