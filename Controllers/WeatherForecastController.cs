using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private JArray pharmacies;
        private JArray users;
        private Dictionary<string, int> weekTran =new Dictionary<string, int>();

        private readonly ILogger<WeatherForecastController> _logger;        

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            LoadData();
            weekTran.Add("Mon",1);
            weekTran.Add("Tue", 2);
            weekTran.Add("Wed", 3);
            weekTran.Add("Thur", 4);
            weekTran.Add("Fri", 5);
            weekTran.Add("Sat", 6);
            weekTran.Add("Sun", 7);            
        }

        [HttpGet]
        public string Get()
        {                        
            return "DEMO";
        }

        [HttpGet("test")]
        public string Test()
        {                      
            return users.ToString();
        }

        //  List all pharmacies open at a specific time and on a day of the week if requested.
        [HttpGet("ListPharmaciesByTime")]
        public string ListPharmaciesByTime(int day, DateTime time)
        {
            string result = "";
            foreach(var item in pharmacies)
            {
                if (IsOpening(day, time, item) == true)
                {
                    result = result + item["name"] + ", ";
                }
            }
            return result;
        }        
        public void LoadData()
        {
            using (StreamReader r = new StreamReader("data/pharmacies.json"))
            {
                string rawData = r.ReadToEnd();
                pharmacies = JArray.Parse(rawData);
            }

            using (StreamReader r = new StreamReader("data/users.json"))
            {
                string rawData = r.ReadToEnd();
                users = JArray.Parse(rawData);
            }

        }   
        
        // 藥局是否營業
        public bool IsOpening(int day, DateTime time, JToken pharmacy)
        {            
            var openingHours = pharmacy["openingHours"].ToString();
            var period = openingHours.Split(" / ");
            foreach (string item in period)
            {
                int index = 0;
                for(var i = 0; i < item.Length; i++)
                {
                    if(Char.IsNumber(item, i))
                    {
                        index = i;
                        break;
                    }
                }
                var week = item.Substring(0,index-1);
                var openTime = item.Substring(index,item.Length-index);
                var start = Convert.ToDateTime(openTime.Split(" - ")[0]);
                var end = Convert.ToDateTime(openTime.Split(" - ")[1]);
                // case1: Mon - Fri
                if (week.Contains('-'))
                {
                    var subWeek = week.Split(" - ");
                    if (day >= weekTran[subWeek[0]] && day <= weekTran[subWeek[1]])
                    {                        
                        if(time >= start && time <= end)
                        {
                            return true;
                        }
                    }
                }
                // case2: Mon, Wed, Fri
                if (week.Contains(','))
                {
                    foreach(var we in week.Split(", "))
                    {
                        if (weekTran[we] == day)
                        {                          
                            if (time >= start && time <= end)
                            {
                                return true;
                            }
                        }
                    }
                }
                // case3: Fri - Sun 20:00 - 02:00
                if (start > end)
                {
                    if (week.Contains('-'))
                    {
                        var subWeek = week.Split(" - ");
                        if (day >= weekTran[subWeek[0]] && day <= weekTran[subWeek[1]])
                        {
                            if (time >= start && time <= Convert.ToDateTime("23:59:59"))
                            {
                                return true;
                            }
                        }
                        for(var i = weekTran[subWeek[0]]; i <= weekTran[subWeek[1]]; i++)
                        {
                            if(day == i % 7 + 1)
                            {
                                if (time >= Convert.ToDateTime("00:00") && time <= end)
                                {
                                    return true;
                                }
                            }
                        }                       
                    }

                    if(week.Contains(" , "))
                    {
                        foreach (var we in week.Split(", "))
                        {
                            if (weekTran[we] == day)
                            {
                                if (time >= start && time <= Convert.ToDateTime("23:59:59"))
                                {
                                    return true;
                                }
                            }
                            if (day == weekTran[we] % 7 + 1)
                            {
                                if (time >= Convert.ToDateTime("00:00") && time <= end)
                                {
                                    return true;
                                }
                            }
                        }
                    }                    
                }
            }
            return false;
        }

        // List all masks sold by a given pharmacy, sorted by mask name or price.
        [HttpGet("ListMasksByPharmacy")]
        public string ListMasksByPharmacy(string name, string sortBy) 
        {
            foreach(var item in pharmacies)
            {
                if (item["name"].ToString() == name)
                {
                    if (sortBy == "name")
                    {
                        var result = item["masks"].OrderBy(x => x.SelectToken("name")).ToList();
                        return JsonConvert.SerializeObject(result);
                    }  
                    else if (sortBy == "price")
                    {
                        var result = item["masks"].OrderBy(x => x.SelectToken("price")).ToList();
                        return JsonConvert.SerializeObject(result);
                    }
                }
            }
            return "not found";
        }

        // List all pharmacies with more or less than x mask products within a price range.
        [HttpGet("ListPharmacyByPriceRange")]
        public string ListPharmacyByPriceRange(int x, string moreOrLess, string priceRange)
        {
            List<JToken> result = new List<JToken>();
            
            var start = Convert.ToDouble(priceRange.Split('~')[0]);
            var end = Convert.ToDouble(priceRange.Split('~')[1]);
            foreach (var pha in pharmacies)
            {
                int maskCount = 0;
                JArray mask = new JArray();
                foreach (var ma in pha["masks"].Select((value, index) => (value, index)))
                {
                    double price = Convert.ToDouble(ma.value["price"]);                    
                    if (price >= start && price <= end)
                    {
                        mask.Add(ma.value);
                        maskCount++;
                    }
                }
                pha["masks"] = mask;
                if (moreOrLess == "more" && maskCount > x)
                {
                    result.Add(pha);
                }
                if (moreOrLess == "less" && maskCount< x)
                {
                    result.Add(pha);
                }
            }
            return JsonConvert.SerializeObject(result);
        }

        // The top x users by total transaction amount of masks within a date range.
        [HttpGet("ListUserByDateRange")]
        public string ListUserByDateRange(int x, DateTime startTime, DateTime endTime)
        {            
            JArray result = new JArray();
            foreach(var item in users)
            {
                JArray histories = new JArray();
                double amount = 0;
                JToken userResult = JToken.Parse("{ \"name\": \" \", \"amount\": \"0\", \"purchaseHistories\": \"\" }");
                foreach (var history in item["purchaseHistories"])
                {
                    var transactionDate = Convert.ToDateTime(history["transactionDate"]);
                    if(transactionDate >= startTime && transactionDate <= endTime)
                    {
                        histories.Add(history);
                        amount += Convert.ToDouble(history["transactionAmount"]);
                    }                    
                }                 
                userResult["name"] = item["name"];
                userResult["amount"] = amount;
                userResult["purchaseHistories"] = histories;
                result.Add(userResult);
            }
            var resultOrder = result.OrderByDescending(x => x.SelectToken("cashBalance")).Take(x).ToList();
            return JsonConvert.SerializeObject(resultOrder);
        }

        // The total number of masks and dollar value of transactions within a date range.
        [HttpGet("GetMasksAndDollar")]
        public string GetMasksAndDollar(DateTime startTime, DateTime endTime)
        {
            Decimal amount = 0;
            int totalNumber = 0;
            foreach (var item in users)
            {               
                foreach(var history in item["purchaseHistories"])
                {
                    var transactionDate = Convert.ToDateTime(history["transactionDate"]);
                    if (transactionDate >= startTime && transactionDate <= endTime)
                    {
                        amount += Convert.ToDecimal(history["transactionAmount"]);
                        totalNumber += Convert.ToInt32(Regex.Match(history["maskName"].ToString(), @"\d+").Value);
                    }
                }
            }
            return "{\"amount\":"+amount+ ", \"totalNumber\": "+ totalNumber  + "}";
        }

        // Search for pharmacies or masks by name, ranked by relevance to the search term.
        [HttpGet("SearchPharmaciesOrMasks")]
        public string SearchPharmaciesOrMasks(string name)
        {
            JArray result = new JArray();
            foreach (var item in pharmacies)
            {
                if( item["name"].ToString().Contains(name))
                {
                    result.Add(item);
                    break;
                }
                else
                {
                    foreach(var mask in item["masks"])
                    {
                        if (mask["name"].ToString().Contains(name))
                        {
                            result.Add(item);
                            break;
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(result);
        }
     
    }

}
