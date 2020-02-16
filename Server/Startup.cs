using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Server.Models;

namespace Server {
    public class Startup {
        public static List<Card> CardsList = new List<Card>();
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
            CardsList.Add(new Card { Id = 0, Title = "iPhone 7", Body = Helpers.ImageToBase64String(@"Images/iphone7.jpg") });
            CardsList.Add(new Card { Id = 1, Title = "iPhone 8", Body = Helpers.ImageToBase64String(@"Images/iphone8.jpg") });
            CardsList.Add(new Card { Id = 2, Title = "iPhone X", Body = Helpers.ImageToBase64String(@"Images/iphoneX.jpg") });
            var json = JsonConvert.SerializeObject(CardsList);
            System.IO.File.WriteAllText(@"cards.json", json);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
        }
    }
}
