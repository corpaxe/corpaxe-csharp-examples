namespace Examples.Console
{
    using System;
    using System.Dynamic;
    using System.IO;
    using System.Net;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Program
    {
        private const string BaseApiUrl = "https://sandboxcorpaxeapi.azurewebsites.net/";
        private const string ApiVersion = "1";

        private const int UnprocessibleEntityStatusCode = 422;

        public static void Main(string[] args)
        {
            var consumerKey = "<FILL IN CONSUMER KEY HERE>";
            var consumerSecret = "<FILL IN CONSUMER SECRET HERE>";
            var username = "<FILL IN USERNAME HERE>";
            var password = "<FILL IN PASSWORD HERE>";

            var accessToken = GetAccessToken(consumerKey, consumerSecret, username, password);

            var eventAsJsonString = @"{
                ""name"": ""John Smith (TMT Analyst)"",
                ""description"": ""John Smith (TMT Analyst)"",
                ""announced"": ""2014-04-21T04:00:00Z"",
                ""start"": ""2014-05-20T04:00:00Z"",
                ""end"": ""2014-05-23T04:00:00Z"",
                ""eventType"": ""AnalystMarketing"",
                ""requestDeadline"": ""0001-01-01T00:00:00Z"",
                ""registrationDeadline"": ""0001-01-01T00:00:00Z"",
                ""link"": null,
                ""saveTheDate"": false,
                ""uniqueId"": ""AnalystMarketing-Example"",
                ""locations"": [
                  {
                    ""name"": ""Dallas"",
                    ""meetingTypesOffered"": [],
                    ""start"": ""2014-05-20T13:00:00Z"",
                    ""end"": ""2014-05-20T21:00:00Z"",
                    ""address"": {
                      ""address1"": ""123 state"",
                      ""address2"": ""Address Line 2"",
                      ""address3"": ""Address Line 3"",
                      ""city"": ""Dallas"",
                      ""state"": ""TX"",
                      ""province"": ""Province"",
                      ""country"": ""USA"",
                      ""zipCode"": ""12345""
                    },
                    ""corporates"": [],
                    ""id"": 7742
                  }
                ],
                ""regions"": [],
                ""corporates"": [],
                ""coordinators"": [
                  {
                    ""type"": 1,
                    ""name"": ""John Doe"",
                    ""firstName"": null,
                    ""lastName"": null,
                    ""email"": null,
                    ""phone"": null,
                    ""title"": null,
                    ""roles"": [
                      ""SellsideCoordinator""
                    ],
                    ""otherEmail"": null
                  }
                ],
                ""analysts"": [
                  {
                    ""type"": 3,
                    ""name"": ""John Smith"",
                    ""firstName"": null,
                    ""lastName"": null,
                    ""email"": null,
                    ""phone"": null,
                    ""title"": null,
                    ""roles"": [
                      ""SellsideAnalyst""
                    ],
                    ""otherEmail"": null
                  },
                  {
                    ""type"": 3,
                    ""name"": ""Jane Smith"",
                    ""firstName"": null,
                    ""lastName"": null,
                    ""email"": null,
                    ""phone"": null,
                    ""title"": null,
                    ""roles"": [
                      ""SellsideAnalyst""
                    ],
                    ""otherEmail"": null
                  }
                ],
                ""experts"": [],
                ""gicsCodes"": [
                  ""45""
                ],
                ""tags"": [],
                ""accessList"": {
                  ""companies"": [
                    ""ALL""
                  ]
                },
                ""status"": ""NoChange"",
            }";

            var eventId = CreateEventAndReturnId(accessToken, eventAsJsonString);
            if (eventId > 0)
            {
                Console.WriteLine("Id created: {0}", eventId);
                var dynamicEvent = GetEventById(accessToken, eventId);
                Console.WriteLine("Id retrieved: {0}", dynamicEvent.id.ToString());
            }
            
            Console.ReadLine();
        }

        public static string GetAccessToken(string consumerKey, string consumerSecret, string username, string password)
        {
            var request = (HttpWebRequest)WebRequest.Create(BaseApiUrl + "api/token");
            var authorizationHeader = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(consumerKey + ":" + consumerSecret));
            request.Headers.Add("Authorization", "Basic " + authorizationHeader);
            request.Accept = string.Format("application/json; version={0}", ApiVersion);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.Proxy = null;

            var postBody = Encoding.UTF8.GetBytes(string.Format("grant_type=password&username={0}&password={1}", username, password));
            request.ContentLength = postBody.Length;
            request.GetRequestStream().Write(postBody, 0, postBody.Length);

            var obj = GetResponseAsDynamicObject(request);
            return (null != obj) ? obj.access_token : null;
        }

        public static long CreateEventAndReturnId(string accessToken, string eventAsJsonString)
        {
            var request = (HttpWebRequest)WebRequest.Create(BaseApiUrl + "api/events");
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.Accept = string.Format("application/json; version={0}", ApiVersion);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Proxy = null;

            var postBody = Encoding.UTF8.GetBytes(eventAsJsonString);
            request.ContentLength = postBody.Length;
            request.GetRequestStream().Write(postBody, 0, postBody.Length);

            var obj = GetResponseAsDynamicObject(request);
            return (null != obj) ? long.Parse(obj.id.ToString()) : -1;
        }

        public static dynamic GetEventById(string accessToken, long eventId)
        {
            var request = (HttpWebRequest)WebRequest.Create(BaseApiUrl + "api/events/" + eventId);
            request.Headers.Add("Authorization", "Bearer " + accessToken);
            request.Accept = string.Format("application/json; version={0}", ApiVersion);
            request.ContentType = "application/json";
            request.Method = "GET";
            request.Proxy = null;

            return GetResponseAsDynamicObject(request);
        }

        private static dynamic GetResponseAsDynamicObject(HttpWebRequest request)
        {
            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                return GetDynamicObjectFromStream(stream);
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response as HttpWebResponse)
                {
                    if (response != null)
                    {
                        if (response.StatusCode == (HttpStatusCode)UnprocessibleEntityStatusCode)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                dynamic validationErrors = GetDynamicObjectFromStream(stream);
                                if (validationErrors != null)
                                {
                                    Console.WriteLine("Event failed validation due to the following issues:");
                                    foreach (var validationError in validationErrors.Errors)
                                    {
                                        Console.WriteLine(validationError.RawErrorMessage);
                                    }
                                    return null;
                                }
                            }
                        }
                    }
                }

                throw;
            }
            return null;
        }

        private static dynamic GetDynamicObjectFromStream(Stream stream)
        {
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
                    var converter = new ExpandoObjectConverter();
                    return JsonConvert.DeserializeObject<ExpandoObject>(reader.ReadToEnd(), converter);
                }
            }

            return null;
        }
    }
}
