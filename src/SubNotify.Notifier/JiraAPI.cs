

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Amazon.SecurityToken.Model.Internal.MarshallTransformations;
using SubNotify.Core;

namespace SubNotify.Notifier
{
    public class JiraAPI
    {
        // https://learn.microsoft.com/en-ca/dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use
        private HttpClient _sharedHTTPClient;
        private string _username;
        private string _APIKey;
        private string _JiraDomain;
        private const string _user_agent = "LSKYSD SubNotify 1.0";
        private string _jira_project_id = "10025"; // https://lssd202.atlassian.net/rest/api/2/issue/createmeta
        private string _jira_issue_type_id = "10027"; // https://lssd202.atlassian.net/rest/api/3/issuetype


        public JiraAPI(string Username, string APIKey, string JiraDomain, string DestinationProjectID, string DestinationIssueTypeID)
        {
            this._username = Username;
            this._APIKey = APIKey;
            this._JiraDomain = JiraDomain;
            this._jira_issue_type_id = DestinationIssueTypeID;
            this._jira_project_id = DestinationProjectID;

            this._sharedHTTPClient = new()
            {
                BaseAddress = new Uri($"https://{JiraDomain}.atlassian.net/rest/api/3/")
            };

            _sharedHTTPClient.DefaultRequestHeaders.Add("User-Agent", _user_agent);
            string _authHeaderContent = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{this._username}:{this._APIKey}"));
            _sharedHTTPClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authHeaderContent);

        }

        private static string JSONJiraTable(List<KeyValuePair<string, string>> TableData)
        {
            StringBuilder returnMe = new StringBuilder();

            returnMe.Append(
                """
                {
                    "type": "table",
                    "attrs": {
                        "isNumberColumnEnabled": false,
                        "layout": "default",
                        "localId": "328adb07-8f4c-4ccd-bf3e-c39a23402ab6"
                    },
                    "content": [
                """);

                    foreach(KeyValuePair<string, string> kvp in TableData)
                    {
                        returnMe.Append(" { \"type\": \"tableRow\", \"content\": [ ");
                        returnMe.Append($" {{ \"type\": \"tableCell\", \"content\": [ {{ \"type\": \"paragraph\", \"content\": [ {{ \"type\": \"text\", \"text\": \"{SanitizeJSONValue(kvp.Key)}\", \"marks\": [ {{ \"type\": \"strong\" }} ] }} ] }} ] }},\n");
                        returnMe.Append($" {{ \"type\": \"tableCell\", \"content\": [ {{ \"type\": \"paragraph\", \"content\": [ {{ \"type\": \"text\", \"text\": \"{SanitizeJSONValue(kvp.Value)}\" }} ] }} ] }}\n");
                        returnMe.Append(" ] },");
                    }

                    // Delete the last character (a comma that won't be needed)
                    returnMe.Remove(returnMe.Length - 1, 1);


            returnMe.Append("""
                ]
            }
            """);

            return returnMe.ToString();
        }

        private static string SanitizeJSONValue(string input)
        {
            return input.Replace("\"", "\\\"");
        }

        private static List<KeyValuePair<string, string>> ConvertSubEventToKeyValuePair(SubEvent SubEvent)
        {
            List<KeyValuePair<string, string>> returnMe = new List<KeyValuePair<string, string>>();

            returnMe.Add(new KeyValuePair<string, string>(@"Submitted Timestamp", SubEvent.RequestedTimestampUTC.ToLongDateString() + " " + SubEvent.RequestedTimestampUTC.ToLongTimeString()));
            returnMe.Add(new KeyValuePair<string, string>("School", SubEvent.SchoolName));
            returnMe.Add(new KeyValuePair<string, string>("Submitted By", $"{SubEvent.RequestorName} ({SubEvent.RequestorEmail})"));
            returnMe.Add(new KeyValuePair<string, string>("Start Date", SubEvent.StartDate.ToLongDateString()));
            returnMe.Add(new KeyValuePair<string, string>("End Date", SubEvent.EndDate.ToLongDateString()));
            returnMe.Add(new KeyValuePair<string, string>("Substitute", SubEvent.SubName));
            returnMe.Add(new KeyValuePair<string, string>("Attach to email?", SubEvent.SubNeedsAccessToEmail ? "✅ YES" : "⛔ No"));
            returnMe.Add(new KeyValuePair<string, string>("Substituting for", SubEvent.SubstituteFor));
            returnMe.Add(new KeyValuePair<string, string>("Notes", SubEvent.Notes));
            returnMe.Add(new KeyValuePair<string, string>("Request ID", SubEvent.Id.ToString()));

            return returnMe;
        }

        
        private string CreateNotificationText_Onboard(SubEvent SubEvent)
        {
            StringBuilder rawJSON = new StringBuilder();

            rawJSON.Append("{ \"fields\": { ");
            rawJSON.Append($" \"project\": {{ \"id\": \"{_jira_project_id}\" }},");
            rawJSON.Append($" \"issuetype\": {{ \"id\": \"{_jira_issue_type_id}\" }},");
            rawJSON.Append($" \"duedate\": \"{SubEvent.StartDate.ToString("yyyy-MM-d")}\", ");
            rawJSON.Append($" \"summary\": \"SubSecretary ONBOARD - {SubEvent.StartDate.ToShortDateString()} to {SubEvent.EndDate.ToShortDateString()} - {SubEvent.SchoolName}\",");
                    
            rawJSON.Append("""            
                    "description": {
                    "version": 1,
                    "type": "doc",
                    "content": [
                        {
                        "type": "heading",
                        "attrs": {
                            "level": 1
                        },
                        "content": [
                            {
                            "type": "text",
                            "text": "Substitute Secretary Onboard"
                            }
                        ]
                        },
                        {
                        "type": "paragraph",
                        "content": [
                            {
                            "type": "text",
                            "text": "A substitute secretary notification has been submitted - see details below."
                            }
                        ]
                        },
            """);

            rawJSON.Append(JSONJiraTable(ConvertSubEventToKeyValuePair(SubEvent)));

            rawJSON.Append("""
                          ,
                            {
                                "type": "paragraph",
                                "content": [
                                    {
                                    "type": "text",
                                    "text": "This ticket was created using the automated Living Sky \"SubNotify\" system. If you need to reach the original requestor for clarification, please contact them directly as they will not see responses to this ticket. "
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
            """);

            return rawJSON.ToString();
        }

        private string CreateNotificationText_Offboard(SubEvent SubEvent)
        {
            StringBuilder rawJSON = new StringBuilder();

            rawJSON.Append("{ \"fields\": { ");
            rawJSON.Append($" \"project\": {{ \"id\": \"{_jira_project_id}\" }},");
            rawJSON.Append($" \"issuetype\": {{ \"id\": \"{_jira_issue_type_id}\" }},");
            rawJSON.Append($" \"duedate\": \"{SubEvent.EndDate.ToString("yyyy-MM-d")}\", ");
            rawJSON.Append($" \"summary\": \"SubSecretary OFFBOARD - {SubEvent.StartDate.ToShortDateString()} to {SubEvent.EndDate.ToShortDateString()} - {SubEvent.SchoolName}\",");
                    
            rawJSON.Append("""            
                    "description": {
                    "version": 1,
                    "type": "doc",
                    "content": [
                        {
                        "type": "heading",
                        "attrs": {
                            "level": 1
                        },
                        "content": [
                            {
                            "type": "text",
                            "text": "Substitute Secretary Offboard"
                            }
                        ]
                        },
                        {
                        "type": "paragraph",
                        "content": [
                            {
                            "type": "text",
                            "text": "The substitute secretary referenced here has finished and needs to be cleaned up. Remove their access to the school that they were subbing at, and leave their account ready to be set up at the next school the next time they are needed."
                            }
                        ]
                        },
            """);

            rawJSON.Append(JSONJiraTable(ConvertSubEventToKeyValuePair(SubEvent)));           

            rawJSON.Append("""
                          ,
                            {
                                "type": "paragraph",
                                "content": [
                                    {
                                    "type": "text",
                                    "text": "This ticket was created using the automated Living Sky \"SubNotify\" system. If you need to reach the original requestor for clarification, please contact them directly as they will not see responses to this ticket. "
                                    }
                                ]
                            }
                        ]
                    }
                }
            }
            """);

            return rawJSON.ToString();
        }

        private async Task<bool> CreateJiraIssue(string rawJSON)
        {
            string _authHeaderContent = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{this._username}:{this._APIKey}"));
            _sharedHTTPClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authHeaderContent);
            StringContent jsonRequestContent = new StringContent(rawJSON.ToString(), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://{this._JiraDomain}.atlassian.net/rest/api/3/issue/"),
                Content = jsonRequestContent
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", _authHeaderContent);
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(_user_agent);
            request.Headers.Add("Accept", "application/json, text/plain");
            var jsonResponse = await _sharedHTTPClient.SendAsync(request);
            Console.WriteLine(jsonResponse);
            return jsonResponse.IsSuccessStatusCode;
        }

        public async Task<bool> CreateOnboardingTicket(SubEvent SubEvent)
        {
            string rawJSON_Onboard = CreateNotificationText_Onboard(SubEvent);
            return await CreateJiraIssue(rawJSON_Onboard);
        }

        public async Task<bool> CreateOffboardingTicket(SubEvent SubEvent)
        {
            string rawJSON_OffBoard = CreateNotificationText_Offboard(SubEvent);
            return await CreateJiraIssue(rawJSON_OffBoard); 
        }

    }
}

