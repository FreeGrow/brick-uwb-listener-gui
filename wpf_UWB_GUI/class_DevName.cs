using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_UWB_GUI
{
    public class class_DevName
    {
        String name;
        String dev1;
        String dev2;

        public String getName()
        {
            return name;
        }

        public String getDev1()
        {
            return dev1;
        }

        public String getDev2()
        {
            return dev2;
        }

        public void setName(String strName)
        {
            name = strName;
        }
        public void setDev1(String strDev)
        {
            dev1 = strDev;
        }
        public void setDev2(String strDev)
        {
            dev2 = strDev;
        }

    }
}
