using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeploymentBotWeb.Data;
using DeploymentBotWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeploymentBotWeb.Controllers
{
    public class DeploymentBotController : Controller
    {
        private readonly IAWSLexService awsLexSvc;
        private ISession userHttpSession;
        private Dictionary<string, string> lexSessionData;
        private List<ChatBotMessage> botMessages;
        private string botMsgKey = "ChatBotMessages",
                       botAtrribsKey = "LexSessionData",
                       userSessionID = String.Empty;
        
        public DeploymentBotController(IAWSLexService aWSLexService)
        {
            awsLexSvc = aWSLexService;
        }

        public IActionResult ClearBot()
        {
            userHttpSession = HttpContext.Session;

            //Clear session keys and session information without removing Session ID
            userHttpSession.Clear();

            //New botMessages and lexSessionData objects
            botMessages = new List<ChatBotMessage>();
            lexSessionData = new Dictionary<string, string>();

            userHttpSession.Set<List<ChatBotMessage>>(botMsgKey, botMessages);
            userHttpSession.Set<Dictionary<string, string>>(botAtrribsKey, lexSessionData);

            awsLexSvc.Dispose();
            return View("Chat", botMessages);
        }

        [HttpGet]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShowChat(string userMessage)
        {
            //Get user session and chat info
            userHttpSession = HttpContext.Session;
            userSessionID = userHttpSession.Id;
            botMessages = userHttpSession.Get<List<ChatBotMessage>>(botMsgKey) ?? new List<ChatBotMessage>();
            lexSessionData = userHttpSession.Get<Dictionary<string, string>>(botAtrribsKey) ?? new Dictionary<string, string>();

            //No message was provided, return to current view
            if (String.IsNullOrEmpty(userMessage)) return View("Chat", botMessages);

            //A Valid Message exists, Add to page and allow Lex to process
            botMessages.Add(new ChatBotMessage()
            { MsgType = MessageType.UserMessage, ChatMessage = userMessage });

            await PostUserData(botMessages);

            //Call Amazon Lex with Text, capture response
            var lexResponse = await awsLexSvc.SendTextMsgToLex(userMessage, lexSessionData, userSessionID);

            lexSessionData = lexResponse.SessionAttributes;
            botMessages.Add(new ChatBotMessage()
            { MsgType = MessageType.LexMessage, ChatMessage = lexResponse.Message });

            //Add updated botMessages and lexSessionData object to Session
            userHttpSession.Set<List<ChatBotMessage>>(botMsgKey, botMessages);
            userHttpSession.Set<Dictionary<string, string>>(botAtrribsKey, lexSessionData);

            return View("Chat", botMessages);
        }

        public async Task<IActionResult> PostUserData(List<ChatBotMessage> messages)
        {
            return await Task.Run(() => Chat(messages));
        }
            
        public IActionResult Chat(List<ChatBotMessage> messages)
        {
            return View(messages);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}