using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace TeslaMed.Services
{
    public class SmsService
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public SmsService() 
        {
            Login = "teslamedbishkek";
            Password = "BKyi9Sdh";
            Sender = "TESLAMED.KG";
            Text1 = "Здравствуйте! Результаты вашего исследования готовы, Вы можете получить их на сайте result.teslamed.kg, введя персональный код ";
            Text2 = ". С уважением, МДЦ “TESLAMED”, лицензия МЗ КР НГМУ № 3591.";
        }

        public async Task<string> SmsSend(string code, string? firstNumber, string? secondNumber)
        {
            var httpClient = new HttpClient();

            var xmlRequest = new XDocument();
            var messageElement = new XElement("message");
            var phonesElement = new XElement("phones");

            //!!!Для экономии во время тестирования использовать максимально короткий текст сообщения, на релизе раскомментировать на требуемый

            //var text = $"{code}";

            var text = Text1 + $"{code}" + Text2;

            var XmlResponse = new XmlDocument();

            messageElement.Add(new XElement("login", Login));
            messageElement.Add(new XElement("pwd", Password));
            messageElement.Add(new XElement("id", code));
            messageElement.Add(new XElement("sender", Sender));
            messageElement.Add(new XElement("text", text));
            if (firstNumber != null)
                phonesElement.Add(new XElement("phone", NumberFormat(firstNumber)));
            if (secondNumber != null)
                phonesElement.Add(new XElement("phone", NumberFormat(secondNumber)));
            messageElement.Add(new XElement(phonesElement));

            //!!!Для проверки статусов отправки без траты стредств раскомментировать 

            //messageElement.Add(new XElement("test", "1"));

            xmlRequest.Add(messageElement);

            var stringContent = new StringContent(xmlRequest.ToString(), Encoding.UTF8, "application/xml");
            var response = await httpClient.PostAsync("http://smspro.nikita.kg/api/message", stringContent);
            var XmlStringResponse = await response.Content.ReadAsStringAsync();
            XmlResponse.LoadXml(XmlStringResponse);
            string status = XmlResponse.GetElementsByTagName("status")[0].InnerText;

            if (status == "0" || status == "11" || (status == "7" && firstNumber != null && secondNumber != null) || status == "10")
                return SmsReport(code, (firstNumber != null && secondNumber != null) ? true : false).Result;
            else
                return "SendStatus" + status;
        }

        public async Task<string> SmsReport(string code, bool isTwoNumbers)
        {
            var httpClient = new HttpClient();

            var xdocument = new XDocument();
            var messageElement = new XElement("dr");

            messageElement.Add(new XElement("login", Login));
            messageElement.Add(new XElement("pwd", Password));
            messageElement.Add(new XElement("id", code));
            xdocument.Add(messageElement);

            var stringContent = new StringContent(xdocument.ToString(), Encoding.UTF8, "application/xml");
            var response = await httpClient.PostAsync("https://smspro.nikita.kg/api/dr", stringContent);
            var myXmlString = await response.Content.ReadAsStringAsync();

            var document = new XmlDocument();
            document.LoadXml(myXmlString);
            string status = document.GetElementsByTagName("status")[0].InnerText;
            if (status == "0")
            {
                status = document.GetElementsByTagName("report")[0].InnerText;
                if (status != "0" && status != "1" && status != "3" && document.GetElementsByTagName("report").Count > 1)
                {
                    string secondNumberReportStatus = document.GetElementsByTagName("report")[1].InnerText;
                    if (secondNumberReportStatus == "0" || secondNumberReportStatus == "1" || secondNumberReportStatus == "3")
                        status = secondNumberReportStatus;
                }
                return "Report" + status;
            }
            else if (status == "4" && isTwoNumbers == true)
                return "SendStatus7";
            else
                return "ReportStatus" + status;
        }
        private string NumberFormat(string number)
        {
            if (number.ToCharArray()[0] == '0')
                return "996" + number.Remove(0, 1);
            else if (number[..3] != "996" && number[..4] != "+996")
                return "996" + number;
            else
                return number;
        }
    }
}
