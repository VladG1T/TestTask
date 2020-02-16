using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server {
    public class Helpers {
        // пересылка изображения в формате base64
        public static string ImageToBase64String(string filePath) {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(filePath));
        }
    }
}
