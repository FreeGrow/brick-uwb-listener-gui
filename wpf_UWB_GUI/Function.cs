using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_UWB_GUI
{
    class Function
    {
        public static Anchor anchor_search_from_list(String label_id, List<Device_Reference> device_Reference)
        {
            try
            {
                Anchor my_ancohr = new Anchor();
                for (int i = 0; i < device_Reference.Count; i++)
                {
                    if (device_Reference[i].type.Equals("Anchor"))
                    {
                        //Console.WriteLine("Label : " + label_id + ", " + device_Reference[i].serial_num);
                        if (label_id.Equals(device_Reference[i].serial_num))
                        {
                            my_ancohr.label = device_Reference[i].serial_num;
                            my_ancohr.x = device_Reference[i].anchor_install_x;
                            my_ancohr.y = device_Reference[i].anchor_install_y;
                            my_ancohr.z = device_Reference[i].anchor_install_z;

                            return my_ancohr;
                            //Console.WriteLine("Anchor : " + my_ancohr.label + ", X : " + my_ancohr.x + ", Y : " + my_ancohr.y + ", Z : " + my_ancohr.z);
                        }
                    }
                }
            }
            catch { }


            return null;

        }

        public Tag tag_search_from_lit(String tag_id, List<Tag> tags)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tag_id.Equals(tags[i]))
                {
                    return tags[i];
                }
            }
            return null;
        }
    }
}
