# WordofDayBOT

The Word of the Day BOT is built using Webview2. It embeds the Microsoft Edge browser in a Winfroms project, evaluating javascript code on the DOM. I use the bot to vist Merrian webster's word of the day webpage, extract all the values, and first store locally as a datatable. The data is then inserted into a SQL database for long term storage, and posted into a slack channel every day.
