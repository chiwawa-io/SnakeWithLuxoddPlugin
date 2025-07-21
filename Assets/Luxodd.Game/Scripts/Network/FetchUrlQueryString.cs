using System;
using System.Web;
using Luxodd.Game.Scripts.HelpersAndUtils.Logger;
using UnityEngine;

namespace Luxodd.Game.Scripts.Network
{
    public class FetchUrlQueryString : MonoBehaviour
    {
        private const string TokenNameParameter = "token";
    
        public string Token {get; private set;}
    
        private string _launchQueryString;
    
        private static string GetURLFromQueryStr()
        {
            return Application.absoluteURL;
        }
    
        private void Start()
        {
            _launchQueryString = ReadURLFromQueryString();
            Token = ParseTokenFromURL();
            LoggerHelper.Log("App is running on the url>>>> " + _launchQueryString);
            LoggerHelper.Log($"[{GetType().Name}][{nameof(Start)}] OK, Token: {Token}");
        
        }
    
        private string ReadURLFromQueryString()
        {
            return GetURLFromQueryStr();
        }
    
        private string ParseTokenFromURL()
        {
            var url = _launchQueryString;
            if (string.IsNullOrEmpty(url))
            {
                return "URL is empty";
            }
            var uri = new Uri(url);
            string queryString = uri.Query;
            var parametersCollection = HttpUtility.ParseQueryString(queryString);
            //LoggerHelper.Log($"[{GetType().Name}][{nameof(ParseTokenFromURL)}] OK, URL: {uri}, Query: {queryString}");
            return parametersCollection.Get(TokenNameParameter);
        }
    }
}
