using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OutputBridges
{
    public class HueSender : PacketSender
    {
        private const string apiVarFormattingString = @"api/k1gK3p0BkT2NAGVYYzCtQkZHTCEiuSTEUnHtmT08/lights/{0}/state";
        private const string url = @"http://192.168.0.175/";
        private const string commandFormattingString = "{{\"bri\": {0}, \"transitiontime\": 0, \"ct\": {1}}}";
        private RestClient client = new RestClient(url);

        public override void SetTarget(string target) { }
        public override bool SendValues(ChannelValues values)
        {
            //var client = new RestClient(url);
            var request = new RestRequest(String.Format(apiVarFormattingString, values.isRightChannel?2:6), Method.PUT);
            request.RequestFormat = DataFormat.Json;
            //string jsonString = String.Format(commandFormattingString, ((values.Y + values.B)/2) - 1, getCT(values));
            string jsonString = String.Format(commandFormattingString, Math.Max(values.Y, values.B), getCT(values));
            request.AddParameter("text/json", jsonString, ParameterType.RequestBody);
            var result = client.Execute(request);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Debug.WriteLine(jsonString);
                return true;
            }
            else
            {
                Debug.WriteLine(result.ToString());
                return false;
            }
        }

        private double getCT(ChannelValues values)
        {
            //return 326 + 173 * (values.Y - values.B) / 255; /*210*/  /*60*/ /*mid=135*/
            return Convert.ToUInt16(153 + 347 * (Math.Atan2(values.Y, values.B) * (180 / Math.PI)) / 90); /*210*/  /*60*/ /*mid=135*/

        }
    }
}
