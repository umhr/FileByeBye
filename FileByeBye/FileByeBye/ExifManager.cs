using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileByeBye
{
    class ExifManager
    {

        static public string getExif(string path)
        {
            //Console.WriteLine("ExifManager:" + path);

            string result = "";

            using (var origin = new Bitmap(path))
            {
                result += "Size:" + origin.Size;

                foreach (var item in origin.PropertyItems)
                {
                    
                    if (item.Id == 0x0112)
                    {
                        result += ", Orientation:" + item.Value[0];
                    }
                    else if (item.Id == 0x0132)
                    {
                        result += ", PropertyTagDateTime:" + item.Value[0];
                    }
                    else if (item.Id == 0x829A)
                    {
                        // 露出時間
                        result += ", PropertyTagExifExposureTime:" + item.Value[0];
                    }
                    else if (item.Id == 0x8827)
                    {
                        // ISO 速度
                        result += ", PropertyTagExifISOSpeed:" + item.Value[0];
                    }
                    else if (item.Id == 0x9209)
                    {
                        result += ", PropertyTagExifFlash:" + item.Value[0];
                    }
                    else if (item.Id == 0x920A)
                    {
                        // 焦点距離
                        result += ", PropertyTagExifFocalLength:" + item.Value[0];
                    }
                    else if (item.Id == 0x9290)
                    {
                        result += ", PropertyTagExifDTSubsec:" + item.Value[0];
                    }

                    //result += ", "+item.Id.ToString("x4") + ":" + item.Value[0];
                    //Console.WriteLine(item.Value.Length);
                }
            }

            //Console.WriteLine(result);
            return result;
        }
    }
}
