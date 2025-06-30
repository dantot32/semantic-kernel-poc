using Microsoft.SemanticKernel;
using NAudio.SoundFont;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;

namespace Gpt4oMini;
internal class TestPlugin
{
    //[KernelFunction("get_person_age")]
    //[Description("Gets the age of person by providing name as parameter")]
    //[return: Description("age in number")]
    //public int GetPersonAge(string name)
    //{   
    //    if (name == "Daniele")
    //        return 34;
    //    if (name == "Vittoria")
    //        return 34;
    //    if (name == "Giulio")
    //        return 24;
    //    if (name == "Biagio")
    //        return 56;
    //    return -1;
    //}

    [KernelFunction("book_a_reservation")]
    [Description("by passing name, surname, start date, end date and hotel name it place a reservation")]
    public bool BookAReservationAction(string name, string surname, DateTime stratDate, DateTime endDate, string hotelName)
    {
       Console.WriteLine($"Prenotazione effettuata per {name} {surname} dal {stratDate} al {endDate} presso {hotelName}");
        return true;
    }

    //[KernelFunction("make_a_wish_to_a_person")]
    //[Description("Make a wish to a person by passing wish name")]
    //public bool MakeAWish(string wishName) 
    //{ 
    //    if (wishName == "Secchio")
    //    {
    //        Console.WriteLine("Il secchio te lo compri solo!");
    //        return false;
    //    }

    //    Console.WriteLine($"Il regalo per te è {wishName}!!!");
    //    return true;
    //}
}
