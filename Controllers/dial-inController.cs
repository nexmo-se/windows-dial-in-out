using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vonage.Voice.Nccos;

using Microsoft.AspNetCore.Mvc.RazorPages;

///////////////////////
//Controllers for Views
///////////////////////
namespace Opentok_Dial_in_out.Pages
{
    /*
       * This creates a Vonage session where this Opentok session can connect and users can dial in
       * When the dial_in)room/:roomId request is made, a template is rendered is served with the 
       * sessionid, token, pinCode, roomId, and apiKey.
    */
    public class dial_in_roomModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Roomid { get; set; }
        public string? pinCode { get; set; }
        public string? sessionId { get; set; }
        public string? token { get; set; }
        public string API_KEY = CommonHelpers.API_KEY.ToString();
        public string API_SECRET = CommonHelpers.API_SECRET;
        public string conferenceNumber = CommonHelpers.conferenceNumber;
        public IActionResult OnGet()
        {
            Console.WriteLine("Room ID {0}", Roomid);
            if (CommonHelpers.store.ContainsKey(Roomid))
            {
                sessionId = CommonHelpers.store[Roomid];
                token = CommonHelpers.generateToken(sessionId);
                pinCode = CommonHelpers.store[sessionId];
                return Page();
            }
            else
            {
                pinCode = CommonHelpers.generatePin();
                try
                {
                    OpenTokSDK.Session session = CommonHelpers.OT.CreateSession("", OpenTokSDK.MediaMode.ROUTED);
                    sessionId = session.Id;
                    token = CommonHelpers.generateToken(sessionId);
                    CommonHelpers.store[Roomid] = sessionId;
                    CommonHelpers.store[pinCode] = sessionId;
                    CommonHelpers.store[sessionId] = pinCode;
                }
                catch (OpenTokSDK.Exception.OpenTokException e)
                {
                    ContentResult res = Content("There was an error");
                    res.StatusCode = 500;
                    return res;
                }

            }
            return Page();
        }
    }

    /*
       * This connects an Opentok Session to any SIP (sip:phone@sip.nexmo.com) endpoint 
       * When the dial_out_room/:roomId request is made, a template is rendered is served with the 
       * sessionid, token, pinCode, roomId, and apiKey.
    */
    public class dial_out_roomModel : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public string? roomId { get; set; }
        public string? sessionId { get; set; }
        public string? token { get; set; }
        public string API_KEY = CommonHelpers.API_KEY.ToString();
        public string API_SECRET = CommonHelpers.API_SECRET;
        public IActionResult OnGet()
        {
            if (CommonHelpers.store.ContainsKey(roomId))
            {
                sessionId = CommonHelpers.store[roomId];
                token = CommonHelpers.generateToken(sessionId);
                return Page();
            }
            else
            {
                try
                {
                    OpenTokSDK.Session session = CommonHelpers.OT.CreateSession("", OpenTokSDK.MediaMode.ROUTED);
                    sessionId = session.Id;
                    token = CommonHelpers.generateToken(sessionId);
                    CommonHelpers.store[roomId] = sessionId;
                }
                catch (OpenTokSDK.Exception.OpenTokException e)
                {
                    ContentResult res = Content("There was an error");
                    res.StatusCode = 500;
                    return res;
                }

            }
            return Page();
        }
    }
}

///////////////////////
//Controllers for APIs
///////////////////////
namespace Opentok_Dial_in_out.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class testController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
            ContentResult res = Content("There was an error dialing out");
            res.StatusCode = 500;
            return res;
        }
    }

    /*
       * When the dial-in get request is made, the dial method of the OpenTok Dial API is invoked
    */
    [Route("[controller]")]
    [ApiController]
    public class dial_outController : ControllerBase
    {
        public string? roomId { get; set; }
        public string? pinCode { get; set; }
        public string? sessionId { get; set; }
        public string? token { get; set; }
        public string? sipTokenData { get; set; }
        public string? sipUri { get; set; }
        public Task<OpenTokSDK.Sip>? sipCall { get; set; }
        public string? json_res { get; set; }
        public string API_KEY = CommonHelpers.API_KEY.ToString();
        public string API_SECRET = CommonHelpers.API_SECRET;
        public string? phoneNumber;
        public string sip_endpoint = CommonHelpers.sip_endpoint;
        [HttpGet]
        public IActionResult OnGet()
        {
            roomId = Request.Query["roomId"];
            if (Request.Query.ContainsKey("phoneNumber")) //if a phone number is provided, use that, if not use the conference number
            {
                phoneNumber = Request.Query["phoneNumber"];
            }
            else phoneNumber = CommonHelpers.conferenceNumber;
            sipTokenData = $"{{\"sip\":true, \"role\":\"client\", \"name\":\"{phoneNumber}\"}}";
            sessionId = CommonHelpers.store[roomId];
            token = CommonHelpers.generateToken(sessionId, sipTokenData);
            OpenTokSDK.DialOptions dialOptions = CommonHelpers.setSipOptions();
            sipUri = $"sip:{phoneNumber}{sip_endpoint};transport=tls";
            try
            {
                Console.WriteLine("RoomID: {0}", roomId);
                Console.WriteLine("SessionID: {0}", sessionId);
                Console.WriteLine("Token: {0}", token);
                Console.WriteLine("Sip URI: {0}", sipUri);
                Console.WriteLine("DialOpt Uname: {0}", dialOptions.Auth.Username);
                Console.WriteLine("DialOpt Pass: {0}", dialOptions.Auth.Password);
                Console.WriteLine("DialOpt Secure: {0}", dialOptions.Secure);
                sipCall = CommonHelpers.OT.DialAsync(sessionId, token, sipUri, dialOptions);
                bool completed = sipCall.IsCompleted;
                while(true)
                {
                    sipCall.Wait(1000);       // Wait for 1 second.

                    completed = sipCall.IsCompleted;
                    if (completed)
                    {
                        Console.WriteLine("Task done");
                        CommonHelpers.store[phoneNumber + roomId] = sipCall.Result.ConnectionId.ToString();
                        Console.WriteLine("ConnectionID: {0}", sipCall.Result.ConnectionId.ToString());
                        json_res = JsonSerializer.Serialize(sipCall);
                        return Content(json_res);
                    }
                }
                
                

            }
            catch (OpenTokSDK.Exception.OpenTokArgumentException e)
            {
                ContentResult res = Content("There was an error dialing out " + e.Data.ToString());
                res.StatusCode = 500;
                return res;
            }
        }
    }


    /*
       * When the hang-up get request is made, the forceDisconnect method of the OpenTok API is invoked
    */
    [Route("[controller]")]
    [ApiController]
    public class hang_upController : ControllerBase
    {
        public string? roomId { get; set; }
        public string? sessionId { get; set; }
        public string? connectionId { get; set; }
        public string? phoneNumber;
        
        [HttpGet]
        public IActionResult OnGet()
        {
            roomId = Request.Query["roomId"];
            if (Request.Query.ContainsKey("phoneNumber")) //if a phone number is provided, use that, if not use the conference number
            {
                phoneNumber = Request.Query["phoneNumber"];
            }else phoneNumber = CommonHelpers.conferenceNumber;
            if (CommonHelpers.store.ContainsKey(roomId) &&
                CommonHelpers.store.ContainsKey(phoneNumber + roomId))
            {
                sessionId = CommonHelpers.store[roomId];
                connectionId = CommonHelpers.store[phoneNumber + roomId];
                Console.WriteLine("HU_sessionId: {0}", sessionId);
                Console.WriteLine("HU_connectionId: {0}", connectionId);
                try
                {
                    CommonHelpers.OT.ForceDisconnect(sessionId, connectionId);
                    ContentResult res = Content("Ok");
                    res.StatusCode = 200;
                    return res;
                }
                catch (OpenTokSDK.Exception.OpenTokWebException e)
                {
                    Console.WriteLine("Hangup_Exception: {0}", e.GetMessage());
                    ContentResult res = Content("There was an error hanging up");
                    res.StatusCode = 500;
                    return res;
                }

            }
            else
            {
                ContentResult res = Content("There was an error hanging up");
                res.StatusCode = 400;
                return res;
            }
        }
    }

    [Route("[controller]")]
    [ApiController]
    public class nexmo_answerController : ControllerBase
    {

        public string serverUrl = CommonHelpers.serverUrl;

        [HttpGet]
        public IActionResult OnGet()
        {
            Console.WriteLine("CALL");
            if (Request.Query.ContainsKey("SipHeader_X-OpenTok-SessionId"))
            {
                var conversationAction = new ConversationAction() { Name = Request.Query["SipHeader_X-OpenTok-SessionId"] };
                Console.WriteLine("SIPSessionID: {0}", Request.Query["SipHeader_X-OpenTok-SessionId"]);
                var talkAction = new TalkAction() { Text = "Connecting" };
                var ncco = new Ncco(talkAction, conversationAction);
                return Content(ncco.ToString());
            }
            else
            {
                
                var talkAction = new TalkAction() { Text = "Please enter a pin code to join the session" };
                var inputAction = new MultiInputAction() { EventUrl = new string[] { $"{serverUrl}/nexmo_dtmf" } };
                var ncco = new Ncco(talkAction, inputAction);
                return Content(ncco.ToString());
            }
            
        }
    }

    [Route("[controller]")]
    [ApiController]
    public class nexmo_dtmfController : ControllerBase
    {

        [HttpPost]
        public IActionResult OnPost([FromBody] dynamic jsonData)
        {
            Console.WriteLine("JSONDATA: {0}", jsonData);
            var data = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(jsonData);
            JsonElement dtmfJson = data["dtmf"];
            Console.WriteLine("dtmf: {0}", dtmfJson);
            string timed_out = dtmfJson.GetProperty("timed_out").ToString();
            string digits = dtmfJson.GetProperty("digits").ToString();
            Console.WriteLine("timeout: {0}", timed_out);
            //var dtmf = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(dtmfJson);
            //Console.WriteLine("dtmf: {0}", dtmf["digits"]);
            //string digits = "0000";

            Console.WriteLine("digits: {0}", digits);
            string sessionId = "";
            if (CommonHelpers.store.ContainsKey(digits))
            {
                sessionId = CommonHelpers.store[digits];
            }
            Console.WriteLine("SessionID: {0}", sessionId);
            var conversationAction = new ConversationAction() { Name = sessionId };
            var ncco = new Ncco(conversationAction);
            return Content(ncco.ToString());
            
        }
    }

    [Route("[controller]")]
    [ApiController]
    public class nexmo_eventsController : ControllerBase
    {

        [HttpPost]
        public IActionResult OnPost([FromBody] string jsonData)
        {
            Console.WriteLine("EVENT: {0}", jsonData);
            return Content(jsonData);

        }
    }

    [Route("[controller]")]
    [ApiController]
    public class postController : ControllerBase
    {
        [HttpPost]
        public IActionResult InitializeAction([FromBody] dynamic Test)
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(Test);
            return Content(data["Test"]);
        }

    }


   
}

