using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackSparrus
{
    public class WebManager
    {
        IWebDriver driver;

        public WebManager()
        {
            FirefoxOptions option = new FirefoxOptions();
            //option.AddArgument("-headless");

            var ffds = FirefoxDriverService.CreateDefaultService();
            ffds.HideCommandPromptWindow = true;

            this.driver = new FirefoxDriver(ffds, option);

            driver.Navigate().GoToUrl("https://www.dofuspourlesnoobs.com/resolution-de-chasse-aux-tresors.html");
            //driver.Navigate().GoToUrl("https://dofus-map.com/fr/hunt");
            //((IJavaScriptExecutor)driver).ExecuteScript("Array.from(document.getElementsByTagName('button')).filter(item => item.getAttribute('mode') == 'primary')[0].click();");
        }

        public void CloseDriver()
        {
            driver.Close();
            driver.Quit();
        }

        public int GetHintDistance(Point startPoint, Direction direction, string hint, out string mostAccurateHint)
        {           
            Console.WriteLine(String.Join("\t", new string[] { startPoint.X.ToString(), startPoint.Y.ToString(), TreasureHub.GetDirectionString(direction) }));

            // Prevent world prompt
            IJavaScriptExecutor jsExecuter = (IJavaScriptExecutor)driver;

            // Set pos
            jsExecuter.ExecuteScript(String.Format("document.getElementsByClassName('huntposx')[0].value = {0};", startPoint.X.ToString()));
            jsExecuter.ExecuteScript(String.Format("document.getElementsByClassName('huntposy')[0].value = {0};", startPoint.Y.ToString()));

            // Set direction
            jsExecuter.ExecuteScript(String.Format("document.querySelector('#hunt{0}').click();", TreasureHub.GetDirectionString(direction)));

            // Select hint
            IWebElement hintsElement = driver.FindElement(By.Id("clue-choice-select"));
            IEnumerable<IWebElement> hints = hintsElement.FindElements(By.TagName("option"));

            Dictionary<string, string> hintDict = hints.ToDictionary(x => x.Text, x => x.GetAttribute("value"));
            List<string> hintTexts = hints.Where(pElem => pElem.GetAttribute("value") != "null" && pElem.GetAttribute("disabled") == null).Select(pElem => pElem.Text).ToList();

            if (hintTexts.Count == 0)
                throw new Exception("Erreur dans la récupération de l'indice en ligne");

            mostAccurateHint = ReturnMostAccurateString(hint, hintTexts);

            if (hintTexts.Contains(hint) && mostAccurateHint != hint)
            {
                Console.WriteLine();
            }

            // Select hint in the list
            jsExecuter.ExecuteScript(String.Format("document.getElementById('clue-choice-select').value = {0};", hintDict[mostAccurateHint]));

            // Submit
            jsExecuter.ExecuteScript("document.getElementsByClassName('clue-search')[0].click();");

            object nb = jsExecuter.ExecuteScript("return document.getElementsByClassName('hunt-result-direction')[0].innerText;");

            // Return amount of maps to travel
            return int.Parse(jsExecuter.ExecuteScript("return document.getElementsByClassName('hunt-result-direction')[0].innerText;").ToString());
        }

        //private List<string> ParseHtml(string html)
        //{
        //    HtmlDocument htmlDoc = new HtmlDocument();
        //    htmlDoc.LoadHtml(html);
        //    IEnumerable<HtmlNode> optionNodes = htmlDoc.GetElementbyId("hintName").Elements("option");

        //    foreach(HtmlNode hintNode in optionNodes)
        //    {

        //    }
        //}

        public static string ReturnMostAccurateString(string text, List<string> strings)
        {
            List<int> matchList = new List<int>();
            foreach (string Track in strings)
            {
                matchList.Add(LevenshteinDistance(Track, text));
            }
            return strings.ElementAt(matchList.IndexOf(matchList.Min()));
        }

        public static int LevenshteinDistance(string source, string target)
        {
            if (String.IsNullOrEmpty(source))
            {
                if (String.IsNullOrEmpty(target)) return 0;
                return target.Length;
            }
            if (String.IsNullOrEmpty(target)) return source.Length;

            if (source.Length > target.Length)
            {
                var temp = target;
                target = source;
                source = temp;
            }

            var m = target.Length;
            var n = source.Length;
            var distance = new int[2, m + 1];
            // Initialize the distance 'matrix'
            for (var j = 1; j <= m; j++) distance[0, j] = j;

            var currentRow = 0;
            for (var i = 1; i <= n; ++i)
            {
                currentRow = i & 1;
                distance[currentRow, 0] = i;
                var previousRow = currentRow ^ 1;
                for (var j = 1; j <= m; j++)
                {
                    var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                    distance[currentRow, j] = Math.Min(Math.Min(
                        distance[previousRow, j] + 1,
                        distance[currentRow, j - 1] + 1),
                        distance[previousRow, j - 1] + cost);
                }
            }
            return distance[currentRow, m];
        }
    }

}
