using System.Text;
using System.Text.Json;

namespace Common.Persistance.Extensions;

public static class SendExceptiontoGuga
{

    private static readonly HttpClient _client = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (msg, cert, chain, err) => true
    });

    private static DateTime _lastSent = DateTime.MinValue;

    public static void InformGuga(this Exception exp)
    {
        if ((DateTime.Now - _lastSent).TotalSeconds < 60) return;
        _lastSent = DateTime.Now;

        Task.Run(() => SendMessage(BuildHtmlMessage(exp.Message, exp?.StackTrace ?? "")));
    }

    public static async Task SendMessage(string message)
    {
        try
        {
            var link = $"https://192.168.0.79:2000/api/Temprature/SendEmail?message={Uri.EscapeDataString(message)}";

            var json = JsonSerializer.Serialize(new List<string>
            {
                "aapkhazava22@gmail.com"
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(link, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Email sent successfully.");
            }
            else
            {
                Console.WriteLine($"❌ Failed to send email. Status: {response.StatusCode}");
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }

    public static string BuildHtmlMessage(string message, string stackTrace)
    {
        return $@"
    <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    background-color: #f9f9f9;
                    color: #333;
                    padding: 20px;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background: #fff;
                    border: 1px solid #ddd;
                    border-radius: 8px;
                    padding: 20px;
                    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                }}
                h2 {{
                    color: #e74c3c;
                    border-bottom: 2px solid #e74c3c;
                    padding-bottom: 5px;
                }}
                .problem {{
                    margin: 20px 0;
                    padding: 10px;
                    background-color: #fbe9e7;
                    border-left: 4px solid #e74c3c;
                    color: #e74c3c;
                }}
                .stacktrace {{
                    background-color: #f4f4f4;
                    border: 1px solid #ddd;
                    padding: 10px;
                    border-radius: 5px;
                    white-space: pre-wrap;
                    font-family: Consolas, monospace;
                }}
                .footer {{
                    margin-top: 20px;
                    font-size: 0.9em;
                    color: #666;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <h2>🚨 პრობლემა გვაქვს</h2>
                <p>გუგა,</p>
                <p>საიტი გაითიშა, ნათია ვარ:</p>
                <div class='problem'>
                    <strong>ეს შეცდომა გვაქვს:</strong> {message}
                </div>
                <p><strong>დეტალურად:</strong></p>
                <div class='stacktrace'>{stackTrace}</div>
                <p><strong>დამატებით ინფორმაცია:</strong></p>
                <div class='stacktrace'>გუგა მეც ვცდილობ გამოსწორებას, სერვისების სიცოცხლე გადავამოწმე, გვერდს ვარეფრეშებ, როცა მოიცლი გადაამოწმე</div>
                <p class='footer'>
                    ეს  ესემესი არის გადაუდებელი სიტუაციებისთვის გათვლილი, გთხოვ გადაამოწმო<br>
                    <em>ნათია ჯანდაგიშვილი</em>
                </p>
            </div>
        </body>
    </html>";
    }
}
