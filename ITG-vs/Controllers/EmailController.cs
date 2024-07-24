using ITG_vs.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.interfaces;
using System.Xml;

namespace ITG_vs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService emailService;

        public EmailController(IEmailService emailService)
        {
            this.emailService = emailService;
        }
        [HttpPost]
        [Route("send")]
        public IActionResult Send([FromBody] EmailDto emailDto)
        {
            var xml = HttpContext.Session.GetString("xmlTimeline");
            if (xml == null)
            {
                xml = HttpContext.Session.GetString("xmlTimelineRFP");
            }

            if (xml== null)
            {
                return BadRequest("Timeline not found!");
            }
            var xmlTimeline = JsonConvert.DeserializeObject<XmlDocument>(xml);
            if(xmlTimeline == null)
            {
                return BadRequest("Timeline not found!");
            }
            emailService.SendEmail(emailDto.txtEmail, emailDto.txtTitle, xmlTimeline);
            return Ok(new { result = "email sent" });
        }
    }
}
