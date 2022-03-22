using System.Net;
using Microsoft.AspNetCore.Mvc;
// using Grpc.Net.Client;
using ProtoBuf;
using TransitRealtime;

namespace DillonFletcher;

[Route("api/[controller]")]
[ApiController]
public class Transit : Controller
{
    // GET
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello");
    }
    
    [HttpGet("WeGo")]
    public IActionResult WeGo(string stop)
    {
        var req = WebRequest.Create("http://transitdata.nashvillemta.org/TMGTFSRealTimeWebService/tripupdate/tripupdates.pb");
        var feed = Serializer.Deserialize<FeedMessage>(req.GetResponse().GetResponseStream());

        // var cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        var busesAndStops = feed.Entities.Select(bus => 
                bus.TripUpdate.StopTimeUpdates
                    .Where(stu =>stu.StopId == stop)
                    .Select(stopTimeUpdate => new { bus.TripUpdate, stopTimeUpdate })
            )
            .SelectMany(b => b)
            .Select(b => new
            {
                BusId = b.TripUpdate?.Vehicle?.Id,
                b.TripUpdate?.Trip?.RouteId,
                b.stopTimeUpdate?.StopId,
                ArrivalTime = b.stopTimeUpdate?.Arrival != null ?
                    UnixTimeConverter(b.stopTimeUpdate.Arrival.Time) :
                    (DateTime?)null,
                ArrivalDelay = b.stopTimeUpdate?.Arrival?.Delay,
                DepartureTime = b.stopTimeUpdate?.Departure != null ?
                    UnixTimeConverter(b.stopTimeUpdate.Departure.Time) :
                    (DateTime?)null,
                DepartureDelay = b.stopTimeUpdate?.Departure?.Delay
            })
            .OrderBy(busAndTrip => busAndTrip.ArrivalTime)
            .ThenBy(busAndTrip => busAndTrip.DepartureTime)
            .ToList();
        
        return Ok(busesAndStops);
    }

    private static DateTime UnixTimeConverter(long unixTicks) =>
        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTicks);
}