using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

public class EmployeeController : Controller
{
    public async Task<ActionResult> Index()
    {
        // Get data from API
        var employees = GetEmployees();

        //sorting descending according to total working hours
        employees.Sort((a, b) => b.WorkingTime.CompareTo(a.WorkingTime));

        return View("Employee", employees);
    }

    private List<NamesTime> GetEmployees()
    {

        var client = new HttpClient();
        var response = client.GetAsync("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==").Result;
        var data = response.Content.ReadAsStringAsync().Result;
        var employees = JsonConvert.DeserializeObject<List<Employee>>(data);
        long hour = 1000 * 60 * 60;
        List<NamesTime> namesTime = new List<NamesTime>();
        Boolean found = false;
        for (int i = 0; i < employees.Count; i++)
        {
            found = false;
            if (employees[i].EmployeeName != null)
            {
                for (int j = 0; j < namesTime.Count; j++)
                {
                    if (namesTime[j].EmployeeName == employees[i].EmployeeName) found = true;
                }
                if(found == false)
                {
                    namesTime.Add(new NamesTime { EmployeeName = employees[i].EmployeeName, WorkingTime = 0 });
                }
            }
            
        }

        for (int j = 0; j < namesTime.Count; j++)
        {
            for (int i = 0; i < employees.Count; i++)
            {
                if (namesTime[j].EmployeeName == employees[i].EmployeeName)
                {
                    DateTimeOffset starTime = new DateTimeOffset(employees[i].StarTimeUtc);
                    long starTimeMs = starTime.ToUnixTimeMilliseconds();

                    DateTimeOffset endTime = new DateTimeOffset(employees[i].EndTimeUtc);
                    long endTimeMs = endTime.ToUnixTimeMilliseconds();

                    namesTime[j].WorkingTime += endTimeMs - starTimeMs; //milisec
                }
            }
            namesTime[j].WorkingTime = (int)Math.Round(namesTime[j].WorkingTime / hour);
        }

        return namesTime;
    }
}

public class Employee
{
    public string EmployeeName { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public DateTime StarTimeUtc { get; set; }
}

public class NamesTime
{
    public string EmployeeName { get; set; }
    public double WorkingTime { get; set; }
}