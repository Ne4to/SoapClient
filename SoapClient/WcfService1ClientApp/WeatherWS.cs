//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyNs4
{
    
    
    public interface WeatherSoap
    {
        
        /// <summary>
        /// Gets Information for each WeatherID
        /// </summary>
        System.Threading.Tasks.Task<GetWeatherInformationResponse> GetWeatherInformationAsync(GetWeatherInformationRequest request);
        
        /// <summary>
        /// Allows you to get your City Forecast Over the Next 7 Days, which is updated hourly. U.S. Only
        /// </summary>
        System.Threading.Tasks.Task<GetCityForecastByZIPResponse> GetCityForecastByZIPAsync(GetCityForecastByZIPRequest request);
        
        /// <summary>
        /// Allows you to get your City's Weather, which is updated hourly. U.S. Only
        /// </summary>
        System.Threading.Tasks.Task<GetCityWeatherByZIPResponse> GetCityWeatherByZIPAsync(GetCityWeatherByZIPRequest request);
    }
    
    public partial class WeatherSoapClient : SoapServices.SoapClientBase, WeatherSoap
    {
        
        /// <summary>
        /// Gets Information for each WeatherID
        /// </summary>
        public virtual System.Threading.Tasks.Task<GetWeatherInformationResponse> GetWeatherInformationAsync(GetWeatherInformationRequest request)
        {
            return this.CallAsync<GetWeatherInformationRequest, GetWeatherInformationResponse>("http://ws.cdyne.com/WeatherWS/GetWeatherInformation", request);
        }
        
        /// <summary>
        /// Allows you to get your City Forecast Over the Next 7 Days, which is updated hourly. U.S. Only
        /// </summary>
        public virtual System.Threading.Tasks.Task<GetCityForecastByZIPResponse> GetCityForecastByZIPAsync(GetCityForecastByZIPRequest request)
        {
            return this.CallAsync<GetCityForecastByZIPRequest, GetCityForecastByZIPResponse>("http://ws.cdyne.com/WeatherWS/GetCityForecastByZIP", request);
        }
        
        /// <summary>
        /// Allows you to get your City's Weather, which is updated hourly. U.S. Only
        /// </summary>
        public virtual System.Threading.Tasks.Task<GetCityWeatherByZIPResponse> GetCityWeatherByZIPAsync(GetCityWeatherByZIPRequest request)
        {
            return this.CallAsync<GetCityWeatherByZIPRequest, GetCityWeatherByZIPResponse>("http://ws.cdyne.com/WeatherWS/GetCityWeatherByZIP", request);
        }
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetWeatherInformationRequest
    {
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetWeatherInformationResponse
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public ArrayOfWeatherDescription GetWeatherInformationResult;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class ArrayOfWeatherDescription
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public WeatherDescription[] WeatherDescription;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class WeatherDescription
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public short WeatherID;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Description;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string PictureURL;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityForecastByZIPRequest
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string ZIP;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityForecastByZIPResponse
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public ForecastReturn GetCityForecastByZIPResult;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class ForecastReturn
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public bool Success;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string ResponseText;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string State;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string City;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public string WeatherStationCity;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public ArrayOfForecast ForecastResult;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class ArrayOfForecast
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public Forecast[] Forecast;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class Forecast
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public System.DateTime Date;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public short WeatherID;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string Desciption;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public temp Temperatures;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public POP ProbabilityOfPrecipiation;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class temp
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string MorningLow;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string DaytimeHigh;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class POP
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string Nighttime;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string Daytime;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityWeatherByZIPRequest
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string ZIP;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityWeatherByZIPResponse
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public WeatherReturn GetCityWeatherByZIPResult;
    }
    
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class WeatherReturn
    {
        
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public bool Success;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string ResponseText;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string State;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string City;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public string WeatherStationCity;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public short WeatherID;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=6)]
        public string Description;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=7)]
        public string Temperature;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=8)]
        public string RelativeHumidity;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=9)]
        public string Wind;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=10)]
        public string Pressure;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=11)]
        public string Visibility;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=12)]
        public string WindChill;
        
        [System.Xml.Serialization.XmlElementAttribute(Order=13)]
        public string Remarks;
    }
}