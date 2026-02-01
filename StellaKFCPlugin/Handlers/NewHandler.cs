using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CorePlugin.EF;
using StellaKFCPlugin.EF;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StellaKFCPlugin.Handlers
{
    public class NewHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_new", typeof(NewRequest))]
        public async Task<NewResponse> New()
        {
            var request = Request as NewRequest;
            var context = new StellaKFCContext();


            SvProfile? profile = await context.SvProfiles.SingleOrDefaultAsync(x =>
                x.RefId == request.Refid);
            if (profile is not null)
            {
                Logger.LogInformation("profile exists");
                return new NewResponse() { Result = 1 };
            }

            context.SvProfiles.Add(new SvProfile()
                { RefId = request.Refid, Name = request.Name, Code = GetCode(context), KacId = request.Name });
            await context.SaveChangesAsync();

            return new NewResponse(){Result = 0};
        }

        public static string GetCode(StellaKFCContext context)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            gen:
            string code = r.Next(1, 9999).ToString("D4") + "-" + r.Next(1, 9999).ToString("D4");

            if (context.SvProfiles.Any(x => x.Code == code)) goto gen;

            return code;

        }
    }
}
