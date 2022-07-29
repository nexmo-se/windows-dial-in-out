# Opentok Dial In/Out sample

  This app shows how to connect to an OpenTok session, publish a stream, subscribe to **multiple streams**, and use OpenTok SIP Interconnect with Vonage to dial in to a conference or dial out to a Vonage number.

## Configuring the application

Before running the application, you need to configure the demo first. Open CommonHelpers and populate theese variables:
  
    public static int API_KEY = ; //Opentok API Key
    public static string API_SECRET = ""; //Opentok Secret
    public static string sip_username = ""; //Vonage API Key
    public static string sip_password = ""; //Vonage API Secret
    public static string sip_endpoint = "@sip-ap1.nexmo.com"; //this is Asia Pacific endpoint, use sip.nexmo.com if US
    public static string conferenceNumber = "";
    public static string serverUrl = ""; //this servers url
    
You have to add these nuget packages as well
  * OpenTok
  * Vonage
    
## Setting up OpenTok & Nexmo projects
  For OpenTok:
  * Create an API Project to get the API Key and Secret.

  For Vonage:
  * Sign up for a [Nexmo](https://developer.vonage.com/) account to get the API Key and Secret. 
  
## Running
  * Run it on Visual Studio
  * Forward the port (5265 in this app) using portforwarding ot ngrok
  * Configure the serverUrl to whatever URL this app ends with
  * confiure your Vonage Answer URL to serverUrl/answer_nexmo
  * Go to serverUrl/dial_in_room/:roomid to test conference dialin sample (create a vonage session and connect to that via sip)
  * Go to serverUrl/dial_put_room/:roomid to test dialing out a nexmo phone number (connect to a sip endpoint)
