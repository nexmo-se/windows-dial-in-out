namespace Opentok_Dial_in_out
{
    public class CommonHelpers
    {

        //Settings
        public static int API_KEY = ; //Opentok API Key
        public static string API_SECRET = ""; //Opentok Secret
        public static string sip_username = ""; //Vonage API Key
        public static string sip_password = ""; //Vonage API Secret
        public static string sip_endpoint = "@sip-ap1.nexmo.com"; //this is Asia Pacific endpoint, use sip.nexmo.com if US
        public static string conferenceNumber = "";
        public static string serverUrl = ""; //this servers url
        public static Dictionary<string, string> store = new Dictionary<string, string>();
        public static Random _random = new();

        public static  OpenTokSDK.OpenTok OT = new OpenTokSDK.OpenTok(API_KEY, API_SECRET);

        public static string generateToken(string sessionId, string sipTokenData = "")
        {
            return OT.GenerateToken(sessionId, OpenTokSDK.Role.PUBLISHER, 0, sipTokenData);

        }

        public static string generatePin()
        {
            string pin = _random.Next(1000, 10000).ToString("D4");
            if (store.ContainsKey(pin))
            {
                return generatePin();
            }
            return pin;
        }


        public static OpenTokSDK.DialOptions setSipOptions()
        {
            OpenTokSDK.DialOptions dialOpt = new OpenTokSDK.DialOptions();
            OpenTokSDK.DialAuth auth = new OpenTokSDK.DialAuth();

            auth.Username = sip_username;
            auth.Password = sip_password;
            dialOpt.Auth = auth;
            
            dialOpt.Secure = false;
            return dialOpt;
        }

        public static string test_func()
        {
            return "TestFunc";
        }
    }
}