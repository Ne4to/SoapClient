//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Aaaa
{
    
    
    public interface WeatherSoap
    {
        
        System.Threading.Tasks.Task<GetWeatherInformationResponse> GetWeatherInformation(GetWeatherInformation request);
        
        System.Threading.Tasks.Task<GetCityForecastByZIPResponse> GetCityForecastByZIP(GetCityForecastByZIP request);
        
        System.Threading.Tasks.Task<GetCityWeatherByZIPResponse> GetCityWeatherByZIP(GetCityWeatherByZIP request);
    }
    
    public partial class WeatherSoapClient : SoapServices.SoapClientBase, WeatherSoap
    {
        
        public virtual System.Threading.Tasks.Task<GetWeatherInformationResponse> GetWeatherInformation(GetWeatherInformation request)
        {
            return this.CallAsync<GetWeatherInformation, GetWeatherInformationResponse>("http://ws.cdyne.com/WeatherWS/GetWeatherInformation", request);
        }
        
        public virtual System.Threading.Tasks.Task<GetCityForecastByZIPResponse> GetCityForecastByZIP(GetCityForecastByZIP request)
        {
            return this.CallAsync<GetCityForecastByZIP, GetCityForecastByZIPResponse>("http://ws.cdyne.com/WeatherWS/GetCityForecastByZIP", request);
        }
        
        public virtual System.Threading.Tasks.Task<GetCityWeatherByZIPResponse> GetCityWeatherByZIP(GetCityWeatherByZIP request)
        {
            return this.CallAsync<GetCityWeatherByZIP, GetCityWeatherByZIPResponse>("http://ws.cdyne.com/WeatherWS/GetCityWeatherByZIP", request);
        }
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="GetWeatherInformation", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetWeatherInformation
    {
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="GetWeatherInformationResponse", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetWeatherInformationResponse
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="GetWeatherInformationResult")]
        public ArrayOfWeatherDescription GetWeatherInformationResult;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="ArrayOfWeatherDescription", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class ArrayOfWeatherDescription
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WeatherDescription")]
        public WeatherDescription[] WeatherDescription;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="WeatherDescription", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class WeatherDescription
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WeatherID")]
        public short WeatherID;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Description")]
        public string Description;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="PictureURL")]
        public string PictureURL;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="GetCityForecastByZIP", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityForecastByZIP
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="ZIP")]
        public string ZIP;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="GetCityForecastByZIPResponse", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityForecastByZIPResponse
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="GetCityForecastByZIPResult")]
        public ForecastReturn GetCityForecastByZIPResult;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="ForecastReturn", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class ForecastReturn
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Success")]
        public bool Success;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="ResponseText")]
        public string ResponseText;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="State")]
        public string State;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="City")]
        public string City;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WeatherStationCity")]
        public string WeatherStationCity;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="ForecastResult")]
        public ArrayOfForecast ForecastResult;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="ArrayOfForecast", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class ArrayOfForecast
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Forecast")]
        public Forecast[] Forecast;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="Forecast", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class Forecast
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Date")]
        public System.DateTime Date;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WeatherID")]
        public short WeatherID;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Desciption")]
        public string Desciption;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Temperatures")]
        public temp Temperatures;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="ProbabilityOfPrecipiation")]
        public POP ProbabilityOfPrecipiation;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="temp", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class temp
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="MorningLow")]
        public string MorningLow;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="DaytimeHigh")]
        public string DaytimeHigh;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="POP", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class POP
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Nighttime")]
        public string Nighttime;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Daytime")]
        public string Daytime;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="GetCityWeatherByZIP", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityWeatherByZIP
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="ZIP")]
        public string ZIP;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="GetCityWeatherByZIPResponse", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class GetCityWeatherByZIPResponse
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="GetCityWeatherByZIPResult")]
        public WeatherReturn GetCityWeatherByZIPResult;
    }
    
    [System.Xml.Serialization.XmlRootAttribute(ElementName="WeatherReturn", Namespace="http://ws.cdyne.com/WeatherWS/")]
    public class WeatherReturn
    {
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Success")]
        public bool Success;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="ResponseText")]
        public string ResponseText;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="State")]
        public string State;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="City")]
        public string City;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WeatherStationCity")]
        public string WeatherStationCity;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WeatherID")]
        public short WeatherID;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Description")]
        public string Description;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Temperature")]
        public string Temperature;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="RelativeHumidity")]
        public string RelativeHumidity;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Wind")]
        public string Wind;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Pressure")]
        public string Pressure;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Visibility")]
        public string Visibility;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="WindChill")]
        public string WindChill;
        
        [System.Xml.Serialization.XmlElementAttribute(ElementName="Remarks")]
        public string Remarks;
    }
}