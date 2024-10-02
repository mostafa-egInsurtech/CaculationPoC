// See https://aka.ms/new-console-template for more information
using System.Reflection.Emit;
using static System.Runtime.InteropServices.JavaScript.JSType;
// app 
Console.WriteLine("------------ Insurance Premium Calculation --------!");
        //*create a request for 'name '
        Console.WriteLine("Please enter insured name :");
        var _name = Console.ReadLine();
        //     * create a vehicle for request 
        Console.WriteLine("enter vehicle mark :");
        var _mark = Console.ReadLine();
        Console.WriteLine("enter vehicle model :");
        var _model = Console.ReadLine();
        Console.WriteLine("enter vehicle model :");
        var _year = int.Parse( Console.ReadLine() );
        Console.WriteLine("enter vehicle price :");
        var _price = double.Parse(Console.ReadLine());

Context _context = new Context();
Request insuranceReq = new Request(_name, _context.Package.Id, new Vehicle(_mark, _model, _year,_price));
Console.WriteLine("you can select 3 from the additional covers :");
_context.Package.Coverages.ForEach(c => Console.WriteLine(c.key.ToString() +" - " + c.Title));

List<int> listOfCoverages = new List<int>();

for (int i = 0; i < 3; i++)
{
    var x = int.Parse( Console.ReadLine() );
    listOfCoverages.Add(x);
}
foreach (var item in listOfCoverages)
{
    Console.WriteLine("you have selected :" + item);
}

var input = new CalcInput()
{ 
    PackageId = insuranceReq.PackageId,
    Price = insuranceReq.InsuredVehicle.Price,
    Rates = _context.Package.Coverages.Where(c => listOfCoverages.Contains(c.key))
                    .Select(s => new RateInput { Key = s.key, Rate = s.Rate , Editable = s.Editable ,Stage = s.StagetoMultiplied}).ToList(),
};

            Utility utility = new Utility(_context);
            var results =  utility.Calculate(input);
            

Console.WriteLine("your insurance premium is :");
Console.WriteLine(results.PremiumResult.ToString());

Console.WriteLine("and the coverage results  is :");
results.RateResults.ForEach(r => Console.WriteLine(
    _context.Package.Coverages.FirstOrDefault(c => c.key == r.Key)?.Title.ToString() +
    ":" +  r.Result));

Console.WriteLine("-------------- Fees -------------");
results.FeesResults.ForEach(r => Console.WriteLine(
       _context.Package.Fees.FirstOrDefault(c => c.key == r.Key)?.Title.ToString() +
        ":" + r.Result));


Console.ReadLine();

public class RateInput
{
    public int Key { get; set; }
    public double Rate { get; set; }
    public bool Editable { get; set; }
    public StageEnum Stage { get; set; }
    public double? EditableRate { get; set; }

}
public class CalcInput
{
    public List<RateInput> Rates { get; set; }
    public int PackageId { get; set; }
    public double Price { get; set; }
}

public class RateResult
{
    public int Key { get; set; }
    public double Result { get; set; }
}
public class FeesResult
{
    public int Key { get; set; }
    public double Result { get; set; }
}
public class CalcResult
{
    public double PremiumResult { get; set; }
    public List<RateResult> RateResults { get; set; }
    public List<FeesResult> FeesResults { get; set; }


    public CalcResult(double _price)
    {
        this.PremiumResult = _price;
        RateResults = new List<RateResult>();
        FeesResults = new List<FeesResult>();
    }
}

public  class Utility
{
    Context Context =new Context();
    public Utility(Context context)
    {
        Context = context;
    }
    public  CalcResult Calculate(CalcInput input)
    {
            CalcResult calcResult = new CalcResult(input.Price);
            var package = Context.Package;
            foreach (var stage in package.CalculationStages)
            {
                var stageRates = input.Rates.Where(r => r.Stage == stage.key).ToList();
                if (stageRates?.Any() ?? false)
                {
                    var ratesCalculated = stageRates.Select(c => CalcRates(c, calcResult.PremiumResult)).ToList();
                        calcResult.RateResults.AddRange(ratesCalculated);
                        calcResult.PremiumResult = calcResult.PremiumResult + ratesCalculated.Sum(s => s.Result);
                }   
                else if(!calcResult.RateResults?.Any() ?? false)
                {
                    var VehicleRate = package.VehicleRates.FirstOrDefault(c => c.min < input.Price && c.max > input.Price).rate;
                    calcResult.PremiumResult = calcResult.PremiumResult * (VehicleRate / 100);
                }

                    var stageFees = package.Fees.Where(r => r.StagetoMultiplied == stage.key).ToList();
                if (stageFees?.Any() ?? false)
                {
                    var feesResults = stageFees.Select(fee => CalcFees(fee, calcResult.PremiumResult)).ToList();
                    calcResult.FeesResults.AddRange(feesResults);
                    calcResult.PremiumResult = calcResult.PremiumResult + feesResults.Sum(s => s.Result);

                }
        }
        return calcResult;
    }
    public  RateResult CalcRates(RateInput rate ,double Result)
    {
        return new RateResult
        {
            Result = rate.Editable && rate.EditableRate.HasValue ? (rate.EditableRate.Value / 100) * Result : (rate.Rate / 100 ) * Result ,
            Key = rate.Key,
        };
    }
    public  FeesResult CalcFees(Fees fee, double Result)
    {
        return new FeesResult
        {
            Result = fee.Percentage.HasValue ? (fee.Percentage.Value / 100 ) * Result : (fee.value.HasValue ? fee.value.Value : 0),
            Key = fee.key
        };
    }
}

public  class Context
{
    public Package Package { get; set; }

    public Context()
    {
        //create a package 
        this.Package = new Package("platinuem package", 1, new List<(double, double, double)>()
            {
                new (0 ,499000,3.2),
                new (550000 ,800000,2.9),
                new (800001,1300000,2.4),
                new (1300001,1300000,2.1),
            });
        // create a packageCovergaes
        this.Package.Coverages = new List<Coverage>()
        {
            new Coverage{ key = 1 ,Title ="Coverage 1", Editable = false , Rate = 25 , StagetoMultiplied = StageEnum.ManualRate  },
            new Coverage{ key = 2 ,Title ="Coverage 2", Editable = false , Rate = 25 , StagetoMultiplied = StageEnum.NetPremium  },
            new Coverage{ key = 3 ,Title ="Coverage 3", Editable = false , Rate = 25 , StagetoMultiplied = StageEnum.NetPremium  },
            new Coverage{ key = 4 ,Title ="Coverage 4", Editable = false , Rate = 25 , StagetoMultiplied = StageEnum.SumInsured  },
            new Coverage{ key = 5 ,Title ="Coverage 5", Editable = false , Rate = 25 , StagetoMultiplied = StageEnum.TotalNetPremium  },
            new Coverage{ key = 6 ,Title ="Coverage 6", Editable = false , Rate = 25 , StagetoMultiplied = StageEnum.TotalNetPremium  },

        };
        // create a packageFees 
        this.Package.Fees = new List<Fees>()
        {
            new Fees{ key = 1 ,Title ="Annual fees",Percentage = 3.5 , StagetoMultiplied = StageEnum.FinalPremium , value = null  },
            new Fees{ key = 2 ,Title ="Dimensional tax",Percentage = 1.2 , StagetoMultiplied = StageEnum.FinalPremium , value = null  },
            new Fees{ key = 3 ,Title ="stamps 1",Percentage = 0.2 , StagetoMultiplied = StageEnum.FinalPremium , value = null  },
            new Fees{ key = 4 ,Title ="stamps 2",Percentage = 0.5 , StagetoMultiplied = StageEnum.FinalPremium , value = null  },
            new Fees{ key = 5 ,Title ="issuance fees",Percentage = null , StagetoMultiplied = StageEnum.FinalPremium , value = 90.25  },
            new Fees{ key = 6 ,Title ="governmental fees",Percentage = null , StagetoMultiplied = StageEnum.FinalPremium , value = 10.15  },

        };

        // create calculation stages 
        this.Package.CalculationStages = new List<Stage>()
        {
            new Stage{ key = StageEnum.ManualRate},
            new Stage{ key = StageEnum.NetPremium},
            new Stage{ key = StageEnum.TotalNetPremium},
            new Stage{ key = StageEnum.FinalPremium},
        };

    }


}

public class Request
{
    public string InsuredPerson { get; set; }
    public int   PackageId { get; set; }
    public Vehicle InsuredVehicle { get; set; }

    public Request(string _personName , int _package ,Vehicle _vehicle )
    {
        this.PackageId = _package;
        this.InsuredPerson = _personName;
        this.InsuredVehicle = _vehicle;
    }
}
public class Vehicle
{
    public string Mark { get; set; }
    public string Model { get; set; }
    public double Price { get; set; }
    public int Year { get; set; }

    public Vehicle(string _mark,string _model,int _year , double _price )
    {
        this.Mark = _mark;
        this.Model = _model;
        this.Price = _price;
        this.Year = _year;
    }
}
public class Package
{
    public int Id { get; set; }
    public string PackageName { get; set; }
    public List<(double min, double max, double rate)> VehicleRates { get; set; }
    public Package(string _packageName , int _id , List<(double, double, double)> _rates)
    {
        this.Id = _id;
        this.PackageName = _packageName;
        this.VehicleRates = _rates;
    }
    public List<Coverage> Coverages { get; set; }
    public List<Fees> Fees { get; set; }
    public List<Stage> CalculationStages { get; set; }

}
public class Coverage { 

    public int key { get; set; }
    public string Title { get; set; }
    public StageEnum StagetoMultiplied { get; set; }
    public double Rate { get; set; }
    public bool Editable { get; set; }
    public int RoundDigits { get; set; }
}
public class Fees {
    public int key { get; set; }
    public string Title { get; set; }
    public StageEnum StagetoMultiplied { get; set; }
    public double? Percentage { get; set; }
    public double? value { get; set; }
}

public class Stage {
    public StageEnum key { get; set; }
}

public enum StageEnum
{
    ManualRate = 1 ,
    NetPremium = 2 ,
    TotalNetPremium= 3 ,
    SumInsured = 4 ,
    FinalPremium = 5 ,
}