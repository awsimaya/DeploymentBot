using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DeploymentBotWeb.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Polly;

namespace DeploymentBotWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _appEnvironment;

        public HomeController(IHostingEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Login(IFormFile file)
        {
            CelebrityModel celeb = new CelebrityModel() { IsEmpty = true };

            Directory.Delete(_appEnvironment.WebRootPath + "/resources/", true);
            Directory.CreateDirectory(_appEnvironment.WebRootPath + "/resources/");

            if (null != file && file.Length > 0)
            {
                string speechFileName = "notjeff.mp3";
                string speechText = "";

                // full path to file in temp location
                var filePath = Path.GetTempFileName();
                AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();

                RecognizeCelebritiesRequest recognizeCelebritiesRequest = new RecognizeCelebritiesRequest();
                Amazon.Rekognition.Model.Image img = new Amazon.Rekognition.Model.Image();

                MemoryStream memStream = new MemoryStream();
                file.CopyTo(memStream);
                img.Bytes = memStream;
                recognizeCelebritiesRequest.Image = img;

                RecognizeCelebritiesResponse recognizeCelebritiesResponse = await rekognitionClient.RecognizeCelebritiesAsync(recognizeCelebritiesRequest);

                if (null != recognizeCelebritiesResponse && recognizeCelebritiesResponse.CelebrityFaces.Count > 0)
                {
                    celeb.CelebrityName = recognizeCelebritiesResponse.CelebrityFaces[0].Name;
                    celeb.Confidence = recognizeCelebritiesResponse.CelebrityFaces[0].MatchConfidence;
                    celeb.IsEmpty = false;

                    if (celeb.CelebrityName == "Jeff Bezos")
                    {
                        speechText = "Hello Boss, Welcome to the Deployment Bot. Please continue to start the deployment.";
                        celeb.IsJeff = true;
                        speechFileName = "jeff.mp3";
                    }
                }
                else
                {
                    celeb.CelebrityName = "Sure, you're popular among your friends. But that doesn't make you a celebrity.";
                    celeb.Confidence = 0;
                    celeb.IsEmpty = false;
                    speechText = "Nice try. You're not Jeff, I can't let you in.";
                }

                AmazonPollyClient pollyclient = new AmazonPollyClient();
                Amazon.Polly.Model.SynthesizeSpeechResponse speechResponse =
                    await pollyclient.SynthesizeSpeechAsync(new Amazon.Polly.Model.SynthesizeSpeechRequest()
                    { OutputFormat = OutputFormat.Mp3, Text = speechText, VoiceId = VoiceId.Joanna });

                var stream = new FileStream(_appEnvironment.WebRootPath + "/resources/" + speechFileName, FileMode.Create);
                await speechResponse.AudioStream.CopyToAsync(stream);
                stream.Close();
            }

            // process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return View("Login", celeb);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
