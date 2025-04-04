using System.Data;
using BenchmarkDotNet.Attributes;
using Dapper;
using DapperBeer.DTO;
using DapperBeer.Model;
using DapperBeer.Tests;

namespace DapperBeer;

public class Assignments3
{
    // 3.1 Question
    // Tip: Kijk in voorbeelden en sheets voor inspiratie.
    // Deze staan in de directory ExampleFromSheets/Relationships.cs. 
    // De sheets kan je vinden op: https://slides.com/jorislops/dapper/
    // Kijk niet te veel naar de voorbeelden van relaties op https://www.learndapper.com/relationships
    // Deze aanpak is niet altijd de manier de gewenst is!
    
    // 1 op 1 relatie (one-to-one relationship)
    // Een brouwmeester heeft altijd 1 adres. Haal alle brouwmeesters op en zorg ervoor dat het address gevuld is.
    // Sorteer op naam.
    // Met andere woorden een brouwmeester heeft altijd een adres (Property Address van type Address), zie de klasse Brewmaster.
    // Je kan dit doen door een JOIN te gebruiken.
    // Je zult de map functie in Query<Brewmaster, Address, Brewmaster>(sql, map: ...) moeten gebruiken om de Address property van Brewmaster te vullen.
    // Kijk in voorbeelden hoe je dit kan doen. Deze staan in de directory ExampleFromSheets/Relationships.cs.
    public static List<Brewmaster> GetAllBrouwmeestersIncludesAddress()
    {
        string sql = """
                        SELECT b.*,
                               '' as AddressSplit,
                               a.*
                        FROM Brewmaster b
                        JOIN address a ON b.addressId = a.addressId
                        ORDER BY b.Name
                     """;

        var addressDict = new Dictionary<int, Address>();

        var connection = DbHelper.GetConnection();
        var addressen = connection.Query<Brewmaster, Address, Brewmaster>(sql, 
                map: (Brewmaster brewmaster, Address address) =>
                {
                    addressDict.TryAdd(address.AddressId, address);
            
                    address = addressDict[address.AddressId];
            
                    brewmaster.Address = address;
                    return brewmaster;
                }, 
                splitOn: "AddressSplit")
            .ToList();
        return addressen;
        //throw new NotImplementedException();
    }

    // 3.2 Question
    // 1 op 1 relatie (one-to-one relationship)
    // Haal alle brouwmeesters op en zorg ervoor dat de brouwer (Brewer) gevuld is.
    // Sorteer op naam.
    public static List<Brewmaster> GetAllBrewmastersWithBrewery()
    {
        string sql = """
                        SELECT bm.*,
                               '' as BrewerSplit,
                               br.*
                        FROM Brewmaster bm
                        JOIN Brewer br ON bm.BrewerId = br.BrewerId
                        ORDER BY bm.Name
                     """;

        var brewerDict = new Dictionary<int, Brewer>();

        var connection = DbHelper.GetConnection();
        var brewerBrewmasters = connection.Query<Brewmaster, Brewer, Brewmaster>(sql, 
                map: (Brewmaster brewmaster, Brewer brewer) =>
                {
                    brewerDict.TryAdd(brewer.BrewerId, brewer);
            
                    brewer = brewerDict[brewer.BrewerId];
            
                    brewmaster.Brewer = brewer;
                    return brewmaster;
                }, 
                splitOn: "BrewerSplit")
            .ToList();
        return brewerBrewmasters;
        throw new NotImplementedException();
    }

    // 3.3 Question
    // 1 op 1 (0..1) (one-to-one relationship) 
    // Geef alle brouwers op en zorg ervoor dat de brouwmeester gevuld is.
    // Sorteer op brouwernaam.
    //
    // Niet alle brouwers hebben een brouwmeester.
    // Let op: gebruik het correcte type JOIN (JOIN, LEFT JOIN, RIGHT JOIN).
    // Dapper snapt niet dat het om een 1 - 0..1 relatie gaat.
    // De Query methode ziet er als volgt uit (let op het vraagteken optioneel):
    // Query<Brewer, Brewmaster?, Brewer>(sql, map: ...)
    // Wat je kan doen is in de map functie een controle toevoegen, je zou dit verwachten:
    // if (brewmaster is not null) { brewer.Brewmaster = brewmaster; }
    // !!echter dit werkt niet!!!!
    // Plaats eens een breakpoint en kijk wat er in de brewmaster variabele staat,
    // hoe moet dan je if worden?
    public static List<Brewer> GetAllBrewersIncludeBrewmaster()
    {
        string sql = """
                        SELECT br.*,
                               '' as BrewmasterSplit,
                               bm.*
                        FROM Brewer br
                        Left JOIN Brewmaster bm ON br.BrewerId = bm.BrewerId
                        ORDER BY br.Name
                     """;

        var brewmasterDict = new Dictionary<int, Brewmaster>();

        var connection = DbHelper.GetConnection();
        var brewmastersbrewer = connection.Query<Brewer, Brewmaster?, Brewer>(sql, 
                map: (Brewer brewer, Brewmaster? brewmaster) =>
                {
                    if (brewmaster != null && brewmaster.BrewmasterId > 0)
                    {
                        if (!brewmasterDict.ContainsKey(brewmaster.BrewmasterId))
                        {
                            brewmasterDict.Add(brewmaster.BrewmasterId, brewmaster);
                        }

                        brewmaster = brewmasterDict[brewmaster.BrewmasterId];
                    }
                    else
                    {
                        // Als brewmaster NULL of ongeldig is, maak een lege instantie (optioneel)
                        brewmaster = null;
                    }

            
                    brewer.Brewmaster = brewmaster;
                    return brewer;
                }, 
                splitOn: "BrewmasterSplit")
            .ToList();
        return brewmastersbrewer;
        throw new NotImplementedException();
    }
    
    // 3.4 Question
    // 1 op veel relatie (one-to-many relationship)
    // Geef een overzicht van alle bieren. Zorg ervoor dat de property Brewer gevuld is.
    // Sorteer op biernaam en beerId!!!!
    // Zorg ervoor dat bieren van dezelfde brouwerij naar dezelfde instantie van Brouwer verwijzen.
    // Dit kan je doen door een Dictionary<int, Brouwer> te gebruiken.
    // Kijk in voorbeelden hoe je dit kan doen. Deze staan in de directory ExampleFromSheets/Relationships.cs.
    public static List<Beer> GetAllBeersIncludeBrewery()
    {
        string sql = """
                     SELECT b.*,
                            '' as BeerSplit,
                            br.*
                     FROM Beer b
                     LEFT JOIN Brewer br ON b.BrewerId = br.BrewerId
                     ORDER BY b.Name, b.BeerId
                     """;

        var connection = DbHelper.GetConnection();

        
        var brewerDict = new Dictionary<int, Brewer>();

        var beersWithBrewers = connection.Query<Beer, Brewer, Beer>(
            sql,
            map: (Beer beer, Brewer brewer) =>
            {
                
                if (brewer != null && brewer.BrewerId > 0) 
                {
                    if (!brewerDict.ContainsKey(brewer.BrewerId))
                    {
                        brewerDict[brewer.BrewerId] = brewer;
                    }

                    
                    beer.Brewer = brewerDict[brewer.BrewerId];
                }
                else
                {
                    
                    beer.Brewer = null;
                }

                return beer;
            },
            splitOn: "BeerSplit"
        ).ToList(); 

        return beersWithBrewers;
    }
    
    // 3.5 Question
    // N+1 probleem (1-to-many relationship) 
    // Geef een overzicht van alle brouwerijen en hun bieren. Sorteer op brouwerijnaam en daarna op biernaam.
    // Doe dit door eerst een Query<Brewer>(...) te doen die alle brouwerijen ophaalt. (Dit is 1)
    // Loop (foreach) daarna door de brouwerijen en doe voor elke brouwerij een Query<Beer>(...)
    // die de bieren ophaalt voor die brouwerij. (Dit is N)
    // Dit is een N+1 probleem. Hoe los je dit op? Dat zien we in de volgende vragen.
    // Als N groot is (veel brouwerijen) dan kan dit een performance probleem zijn of worden. Probeer dit te voorkomen!
    public static List<Brewer> GetAllBrewersIncludingBeersNPlus1()
    {
        using var connection = DbHelper.GetConnection();
        
        string brewerQuery = "SELECT * FROM Brewer ORDER BY Name;";
        var brewers = connection.Query<Brewer>(brewerQuery).ToList();
        
        foreach (var brewer in brewers)
        {
            string beerQuery = "SELECT * FROM Beer WHERE BrewerId = @BrewerId ORDER BY Name;";
            var beers = connection.Query<Beer>(beerQuery, new { BrewerId = brewer.BrewerId }).ToList();
            
            brewer.Beers = beers;
        }

        return brewers;
    }
    
    // 3.6 Question
    // 1 op n relatie (one-to-many relationship)
    // Schrijf een query die een overzicht geeft van alle brouwerijen. Vul per brouwerij de property Beers (List<Beer>) met de bieren van die brouwerij.
    // Sorteer op brouwerijnaam en daarna op biernaam.
    // Gebruik de methode Query<Brewer, Beer, Brewer>(sql, map: ...)
    // Het is belangrijk dat je de map functie gebruikt om de bieren te vullen.
    // De query geeft per brouwerij meerdere bieren terug. Dit is een 1 op veel relatie.
    // Om ervoor te zorgen dat de bieren van dezelfde brouwerij naar dezelfde instantie van Brewer verwijzen,
    // moet je een Dictionary<int, Brewer> gebruiken.
    // Dit is een veel voorkomend patroon in Dapper.
    // Vergeet de Distinct() methode te gebruiken om dubbel brouwerijen (Brewer) te voorkomen.
    //  Query<...>(...).Distinct().ToList().
    
    public static List<Brewer> GetAllBrewersIncludeBeers()
    {
        string sql = """
                     SELECT br.*,
                            '' as BeerSplit,
                            b.*
                     FROM Brewer br
                     LEFT JOIN Beer b ON br.BrewerId = b.BrewerId
                     ORDER BY br.Name, b.Name
                     """;

        using var connection = DbHelper.GetConnection();

        var brewerDict = new Dictionary<int, Brewer>();

        var brewersWithBeers = connection.Query<Brewer, Beer, Brewer>(
            sql,
            map: (Brewer brewer, Beer beer) =>
            {
                if (!brewerDict.TryGetValue(brewer.BrewerId, out var existingBrewer))
                {
                    existingBrewer = brewer;
                    existingBrewer.Beers = new List<Beer>();
                    brewerDict.Add(existingBrewer.BrewerId, existingBrewer);
                }

                if (beer != null && beer.BeerId > 0)
                {
                    existingBrewer.Beers.Add(beer);
                }

                return existingBrewer;
            },
            splitOn: "BeerSplit"
        ).Distinct().ToList();

        return brewersWithBeers;
    }
    
    // 3.7 Question
    // Optioneel:
    // Dezelfde vraag als hiervoor, echter kan je nu ook de Beers property van Brewer vullen met de bieren?
    // Hiervoor moet je wat extra logica in map methode schrijven.
    // Let op dat er geen dubbelingen komen in de Beers property van Beer!
    public static List<Beer> GetAllBeersIncludeBreweryAndIncludeBeersInBrewery()
    {
        throw new NotImplementedException();
    }
    
    // 3.8 Question
    // n op n relatie (many-to-many relationship)
    // Geef een overzicht van alle cafés en welke bieren ze schenken.
    // Let op een café kan meerdere bieren schenken. En een bier wordt vaak in meerdere cafe's geschonken. Dit is een n op n relatie.
    // Sommige cafés schenken geen bier. Dus gebruik LEFT JOINS in je query.
    // Bij n op n relaties is er altijd spraken van een tussen-tabel (JOIN-table, associate-table), in dit geval is dat de tabel Sells.
    // Gebruikt de multi-mapper Query<Cafe, Beer, Cafe>("query", splitOn: "splitCol1, splitCol2").
    // Gebruik de klassen Cafe en Beer.
    // De bieren worden opgeslagen in een de property Beers (List<Beer>) van de klasse Cafe.
    // Sorteer op cafénaam en daarna op biernaam.
    
    // Kan je ook uitleggen wat het verschil is tussen de verschillende JOIN's en wat voor gevolg dit heeft voor het resultaat?
    // Het is belangrijk om te weten wat de verschillen zijn tussen de verschillende JOIN's!!!! Als je dit niet weet, zoek het op!
    // Als je dit namelijk verkeerd doet, kan dit grote gevolgen hebben voor je resultaat (je krijgt dan misschien een verkeerde aantal records).
    public static List<Cafe> OverzichtBierenPerKroegLijstMultiMapper()
    {
        throw new NotImplementedException();
    }



    // 3.9 Question
    // We gaan nu nog een niveau dieper. Geef een overzicht van alle brouwerijen, met daarin de bieren die ze verkopen,
    // met daarin in welke cafés ze verkocht worden.
    // Sorteer op brouwerijnaam, biernaam en cafenaam. 
    // Gebruik (vul) de class Brewer, Beer en Cafe.
    // Gebruik de methode Query<Brewer, Beer, Cafe, Brewer>(...) met daarin de juiste JOIN's in de query en splitOn parameter.
    // Je zult twee dictionaries moeten gebruiken. Een voor de brouwerijen en een voor de bieren.
    public static List<Brewer> GetAllBrewersIncludeBeersThenIncludeCafes()
{
    string sql = """
                 SELECT 
                     br.BrewerId,
                     br.Name,
                     br.Country,
                     '' AS BrewerSplit,
                     b.BeerId,
                     b.Name,
                     b.Type,
                     b.Style,
                     b.Alcohol AS AlcoholPercentage,
                     b.BrewerId AS BeerBrewerId,
                     '' AS BeerSplit,
                     c.CafeId,
                     c.Name,
                     c.Address,
                     c.City
                 FROM Brewer br
                     LEFT JOIN Beer b ON br.BrewerId = b.BrewerId
                     LEFT JOIN Sells s ON b.BeerId = s.BeerId
                     LEFT JOIN Cafe c ON s.CafeId = c.CafeId
                 ORDER BY br.Name, b.Name, c.Name
                 """;

    using var connection = DbHelper.GetConnection();

    // Dictionaries voor duplicaatcontrole
    var brewerDict = new Dictionary<int, Brewer>();
    var beerDict = new Dictionary<int, Beer>();

    return connection.Query<Brewer, Beer, Cafe, Brewer>(
        sql,
        map: (brewer, beer, cafe) =>
        {
            // Controleer of Brewer al bestaat, anders aanmaken
            if (!brewerDict.TryGetValue(brewer.BrewerId, out var existingBrewer))
            {
                existingBrewer = new Brewer
                {
                    BrewerId = brewer.BrewerId,
                    Name = brewer.Name,
                    Country = brewer.Country,
                    Beers = new List<Beer>()
                };
                brewerDict.Add(brewer.BrewerId, existingBrewer);
            }

            // Controleer of Beer bestaat, anders aanmaken
            if (beer != null && !beerDict.TryGetValue(beer.BeerId, out var existingBeer))
            {
                existingBeer = new Beer
                {
                    BeerId = beer.BeerId,
                    Name = beer.Name,
                    Type = beer.Type,
                    Style = beer.Style,
                    Alcohol = beer.Alcohol,
                    BrewerId = beer.BrewerId,
                    Cafes = new List<Cafe>()
                };
                beerDict.Add(beer.BeerId, existingBeer);
                existingBrewer.Beers.Add(existingBeer);
            }

            // Voeg Café toe aan bestaande bier (indien café beschikbaar is en er een bier bestaat)
            if (beer != null && cafe != null)
            {
                var beerToUpdate = beerDict[beer.BeerId];
                if (!beerToUpdate.Cafes.Any(c => c.CafeId == cafe.CafeId)) // Vermijd dubbele cafés
                {
                    beerToUpdate.Cafes.Add(new Cafe
                    {
                        CafeId = cafe.CafeId,
                        Name = cafe.Name,
                        Address = cafe.Address,
                        City = cafe.City
                    });
                }
            }

            return existingBrewer;
        },
        splitOn: "BrewerSplit,BeerSplit"
    ).Distinct().ToList();
}


    
    // 3.10 Question - Er is geen test voor deze vraag
    // Optioneel: Geef een overzicht van alle bieren en hun de bijbehorende brouwerij.
    // Sorteer op brouwerijnaam, biernaam.
    // Gebruik hiervoor een View BeerAndBrewer (maak deze zelf). Deze view bevat alle informatie die je nodig hebt gebruikt join om de tabellen Beer, Brewer.
    // Let op de kolomnamen in de view moeten uniek zijn. Dit moet je dan herstellen in de query waarin je view gebruik zodat Dapper het snap
    // (SELECT BeerId, BeerName as Name, Type, ...). Zie BeerName als voorbeeld hiervan.
    public static List<Beer> GetBeerAndBrewersByView()
    {
        throw new NotImplementedException();
    }
}