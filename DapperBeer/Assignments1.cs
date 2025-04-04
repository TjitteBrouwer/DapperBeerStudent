using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DapperBeer.DTO;
using DapperBeer.Model;
using DapperBeer.Tests;

namespace DapperBeer;

using Dapper;

public class Assignments1 : TestHelper
{

    // Het is ook altijd een goed idee om het resultaat van de Query om te zetten naar een `List<T>`, 
    // Dit kan met de methode ToList() die je aan het einde van de Query() kan toevoegen (connection.Query(sql).ToList()).
    // Onder deze methode staat een test. Zodat je kan controleren of je query en C# code correct werkt.
    // Tip: mocht je een foutmelding krijgen, kijk dan goed naar de foutmelding en de query die je hebt geschreven.
    // Test eerst je SQL-query en kijk dan of je C# code correct werkt met de debugger.
    // Kom je er niet uit na zelf proberen,
    // vraag dan hulp (medestudent, docent). Het kan natuurlijk zijn dat de test niet correct is.
    // Dit is natuurlijk niet de bedoeling (mocht dit zo zijn laat het mij (joris) dan s.v.p. weten). 
    //
    // De test zijn gemaakt met TUnit, dit testframework is relatief nieuw.
    // De tests kan je vinden in de directory Tests.
    // Het kan zijn dat je even een setting moet aanpassen in Rider/Visual Studio. Dit staat beschreven in de README.md.
    // op https://github.com/thomhurst/TUnit
    // LET OP: je kan de testen per stuk runnen, maar ook allemaal tegelijk per Assignment.
    // Maar niet alle assignments (Assigment1.cs, Assignment2.cs, Assignment3.cs) tegelijk!
    // Deze testen zijn niet geschreven om parallel te runnen. Dit zorgt er wel voor dat de testen sneller runnen.
    // 
    // In de directory Model staan de classes die overeenkomen met de database tabellen.
    // In de directory DTO (Data Transfer Object) staan de classes die worden gebruikt als resultaat indien deze
    // niet overeenkomt met een database tabel (Model).
    // 1.1 Question
    // Geef een overzicht van alle brouwers, gesorteerd op naam (alfabetisch).
    // Gebruik hiervoor de class Brewer. En de Query<Brewer>(sql) methode van Dapper.
    public static List<Brewer> GetAllBrewers()
    {
        var sql = "SELECT BrewerId, Name, Country FROM Brewer ORDER BY Name";
        var connection = DbHelper.GetConnection();
        return connection
            .Query<Brewer>(sql)
            .ToList();
    }
    
    // 1.2 Question
    // Geef een overzicht van alle bieren gesorteerd op alcohol percentage (hoog naar laag).
    public static List<Beer> GetAllBeersOrderByAlcohol()
    {
        string sql = "SELECT BeerId, Name, Type, Style, Alcohol, BrewerId FROM Beer ORDER BY Alcohol DESC";
        
        var connection = DbHelper.GetConnection();
        return connection.Query<Beer>(sql).ToList();
        
    }
    
    // 1.3 Question
    // Geef een overzicht van ale bieren voor een bepaald land gesorteerd op bier-naam (alfabetisch).
    // Gebruik hiervoor de class Beer. En in je SQL-query JOIN met de tabel Brewer.
    // Gebruik de Query<Beer>(sql, new {Country = country}) methode van Dapper.
    // In je SQL-query kan je de WHERE-clause gebruiken om te filteren op land.
    // Gebruik hiervoor query parameters placeholders in je SQL-query.
    // Dit voorkomt SQL-injectie (onderwerp van les 2).
    //      WHERE brewer.Country = @Country
    // @Country is een query parameter placeholder.
    public static List<Beer> GetAllBeersSortedByNameForCountry(string country)
    {
        string sql = "Select Beer.*, Brewer.country From Beer Inner Join Brewer ON Beer.BrewerId = Brewer.BrewerId Where Country = @Country Order by Name";
        
        var connection = DbHelper.GetConnection();
        return connection.Query<Beer>(sql, new {Country = country}).ToList();
        throw new NotImplementedException();
    }
    
    // 1.4 Question
    // Tel het aantal brouwerijen. Welke methode van Dapper gebruik je (niet Query<Brewer>)?
    // Een handige website om te kijken is:
    // https://www.learndapper.com/
    // Voor deze vraag kijken specifiek naar deze pagina: https://www.learndapper.com/dapper-query
    public static int CountBrewers()
    {
        String sql = "SELECT COUNT(*) From Brewer";
        var connection = DbHelper.GetConnection();
        return connection.ExecuteScalar<int>(sql);
        throw new NotImplementedException();
    }
    
    // 1.5 Question
    // Geef een overzicht van het aantal brouwerijen per land gesorteerd op aantal brouwerijen (van hoog naar laag).
    // Gebruik hiervoor een aparte class NumberOfBrewersByCountry
    // Voeg hiervoor properties toe aan de class NumberOfBrewersByCountry, namelijk Country en NumberOfBreweries.
    // Gebruik de volgende SELECT-clause zodat de kolomnamen in de resultaten overeenkomen met de properties van de class NumberOfBrewersByCountry.:
    //   SELECT Country, COUNT(1) AS NumberOfBreweries
    // In de directory DTO (Data Transfer Object) staan de classes die worden gebruikt als resultaat
    // voor Queries die net overeenkomen met de database tabellen.
    public static List<NumberOfBrewersByCountry> NumberOfBrewersByCountry() {
        
        string sql = "SELECT Country, COUNT(1) AS NumberOfBreweries from Brewer Group By Country Order by NumberOfBreweries desc";
        
        var connection = DbHelper.GetConnection();
        return connection.Query<NumberOfBrewersByCountry>(sql).ToList();
        throw new NotImplementedException();
    }
    
    // 1.6 Question
    // Geef het bier met het hoogste alcohol percentage terug. Welke methode gebruik je van Dapper (niet Query<Beer>)?
    // Je kan in MySQL de LIMIT 1 gebruiken om 1 record terug te krijgen.
    public static Beer GetBeerWithMostAlcohol()
    {
        string sql = "SELECT * from Beer where alcohol = (Select max(alcohol) from beer)";
        
        var connection = DbHelper.GetConnection();
        return connection.QuerySingle<Beer>(sql);
        throw new NotImplementedException();
    }
    
    // 1.7 Question
    // Gegeven de brewerId geef de brouwer terug. Let op: Wat moet er gebeuren als de brouwcode niet bestaat?
    // Met andere woorden, welke Dapper methode moet je gebruiken? 
    // Brewer? is een nullable type. Dit betekent dat de waarde null kan zijn,
    // indien de brouwerij niet bestaat voor een bepaalde brewerId.
    public static Brewer? GetBreweryByBrewerId(int brewerId)
    {
        string sql = "SELECT * from Brewer where BrewerId = @brewerId";
        
        var connection = DbHelper.GetConnection();
        return connection.QueryFirstOrDefault<Brewer?>(sql, new { brewerId });
        throw new NotImplementedException();
    }
    
    // 1.8 Question
    // Gegeven de BrewerId, geef een overzicht van alle bieren van de brouwerij gesorteerd bij alcohol percentage.
    public static List<Beer> GetAllBeersByBreweryId(int brewerId)
    {
        string sql = "SELECT * from Beer WHERE BrewerId = @brewerId ORDER BY Alcohol desc";
        
        var connection = DbHelper.GetConnection();
        return connection.Query<Beer>(sql, new { brewerId }).ToList();
        throw new NotImplementedException();
    }
    
    // 1.9 Question
    // Geef per cafe (Cafe) aan welke bier (Beer) ze schenken, sorteer op cafe naam en daarna op bier naam.
    // Gebruik hiervoor de class CafeBeer (directory DTO). 
    public static List<CafeBeer> GetCafeBeers()
    {
        string sql =
            "SELECT c.Name AS CafeName, b.Name AS Beers FROM Cafe c INNER JOIN Sells s ON c.CafeId = s.CafeId INNER JOIN Beer b ON s.BeerId = b.BeerId ORDER BY  c.Name ASC,   b.Name ASC; ";

        var connection = DbHelper.GetConnection();
        return connection.Query<CafeBeer>(sql).ToList();
    }

    // De vorige 1.10 Question heb ik verwijderd, deze was nogal lastig
    
    // 1.10 Question
    // Geef de gemiddelde waardering (score in de tabel Review) van een biertje terug gegeven de BeerId.
    public static decimal GetBeerRating(int beerId)
    {
        string sql = "SELECT AVG(Score) From Review Where Beerid = @beerId";
        
        var connection = DbHelper.GetConnection();
        return connection.QuerySingle<decimal>(sql, new { beerId });
        throw new NotImplementedException();
    }
    
    // 1.11 Question
    // Voeg een review toe voor een bier, met andere woorden gebruik een INSERT.
    // De test werkt alleen als de vorige vraag ook correct is gemaakt.
    public static void InsertReview(int beerId, decimal score)
    {
        var connection = DbHelper.GetConnection();
        String sql = "Insert into Review (BeerId, Score) values (@beerId, @score)";
        connection.Execute(sql, new { @beerId, @score });
        
        //throw new NotImplementedException();
    }
    
    // 1.12 Question
    // Voeg een review toe voor bier. Geef de reviewId terug.
    // Deze test werkt alleen decimal GetBeerRating(int beerId) methode correct is (twee vragen hiervoor).
    public static int InsertReviewReturnsReviewId(int beerId, decimal score)
    {
        var connection = DbHelper.GetConnection();
        String sql = "Insert into Review (BeerId, Score) values (@beerId, @score); " + "Select LAST_INSERT_ID();";
        return connection.ExecuteScalar<int>(sql, new { @beerId, @score });
        
    }
    
    // twee methoden verwijderd
}