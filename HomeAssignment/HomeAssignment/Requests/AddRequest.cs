using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using HomeAssignment.Models;
using Newtonsoft.Json;

namespace HomeAssignment.Requests;

public class AddRequest
{
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Email { get; set; }
    
    public string Telephone { get; set; }
    
    public string Pesel { get; set; }
    
    public int IdTrip { get; set; }

    public string TripName { get; set; }
    
    [JsonProperty(nameof(paymentDate))]
    private string? PaymentDate { get; set; }
    
    [JsonIgnore]
    public DateTime? paymentDate
    {
        get
        {
            if (PaymentDate != null) return DateTime.Parse(PaymentDate);
            return null;
        }
    }
}