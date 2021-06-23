var tname="Fecha";

var ToTable="foo";
var ToColumn="date";


var CrearFiscal="si";
var CrearSemana="si";
var CrearSemanaFiscal="si";
var CrearIso="si";
var MostrarFechasRelativas="si";
/* Fin de variables de comportamiento */ 

var addexpresion="si";
foreach(var t in Model.Expressions)
{
    if (t.Name=="_StartYear")
        addexpresion="no";
}

if(addexpresion=="si"){
var exp=Model.AddExpression("_StartYear","2019");
exp.Expression="2016  meta [IsParameterQuery=true, Type=\"Number\", IsParameterQueryRequired=true]";
exp.Kind=ExpressionKind.M;
exp=Model.AddExpression("_EndYear","2025");
exp.Expression="2023 meta [IsParameterQuery=true, Type=\"Number\", IsParameterQueryRequired=true]";
exp.Kind=ExpressionKind.M;
exp=Model.AddExpression("_FiscalStartMonth","6");
exp.Expression="6 meta [IsParameterQuery=true, Type=\"Number\", IsParameterQueryRequired=true]";
exp.Kind=ExpressionKind.M;

}

var tab2= Model.AddTable();

var tab=Model.Tables[0];

foreach(var t in Model.Tables)
{
    if (t.Name==tname)
    { tab=t;
    }
}

if (tab.Name!=tname)
    tab=Model.AddTable(tname);

tab.DataCategory= "Time";
var partition=tab.Partitions.First();
partition.Query=@"let
    Source =List.Dates(#date(_StartYear,1,1),Duration.Days(#date(_EndYear+1,1,1) -#date(_StartYear,1,1)) ,#duration(1,0,0,0)),
    #""Converted to Table"" = Table.FromList(Source, Splitter.SplitByNothing(), null, null, ExtraValues.Error),
    #""Changed Type"" = Table.TransformColumnTypes(#""Converted to Table"",{{""Column1"", type datetime}}),
    #""Sorted Rows"" = Table.Sort(#""Changed Type"",{{""Column1"", Order.Ascending}}),
    #""Renamed Columns"" = Table.RenameColumns(#""Sorted Rows"",{{""Column1"", ""Date""}}),
    #""Inserted Year"" = Table.AddColumn(#""Renamed Columns"", ""Year"", each Date.Year([Date]), Int64.Type),
    #""Inserted Month"" = Table.AddColumn(#""Inserted Year"", ""Month"", each Date.Month([Date]), Int64.Type),
    #""Inserted Month Name"" = Table.AddColumn(#""Inserted Month"", ""Month Name"", each Date.MonthName([Date]), type text),
    #""Inserted Month Name Short"" = Table.AddColumn(#""Inserted Month Name"", ""Month Name Short"", each Text.Start([Month Name],3)),
    #""Inserted Mont Name Complete"" = Table.AddColumn(#""Inserted Month Name Short"", ""Month Name Complete"", each [Month Name] & "" "" & Text.From([Year])),
    #""Inserted Month Name Short Complete"" = Table.AddColumn(#""Inserted Mont Name Complete"", ""Month Name Short Complete"", each [Month Name Short] & "" "" & Text.End(Text.From([Year]),2)),
    #""Changed Type6"" = Table.TransformColumnTypes(#""Inserted Month Name Short Complete"",{{""Month Name Short"", type text},{""Month Name Complete"", type text},{""Month Name Short Complete"", type text}}),
    #""Inserted Days in Month"" = Table.AddColumn(#""Changed Type6"", ""Days in Month"", each Date.DaysInMonth([Date]), Int64.Type),
    #""Removed Columns"" = Table.RemoveColumns(#""Inserted Days in Month"",{""Days in Month""}),
    #""Inserted Quarter"" = Table.AddColumn(#""Removed Columns"", ""Quarter"", each Date.QuarterOfYear([Date]), Int64.Type),
    #""Inserted Week of Year"" = Table.AddColumn(#""Inserted Quarter"", ""Week of Year"", each Date.WeekOfYear([Date]), Int64.Type),
    #""Inserted Day"" = Table.AddColumn(#""Inserted Week of Year"", ""Day"", each Date.Day([Date]), Int64.Type),
    #""Inserted Day of Week"" = Table.AddColumn(#""Inserted Day"", ""Day of Week"", each Date.DayOfWeek([Date]), Int64.Type),
    #""Fecha key"" = Table.AddColumn(#""Inserted Day of Week"", ""idFecha"", each [Year]*10000+[Month]*100+[Day]),
    #""Changed Type3"" = Table.TransformColumnTypes(#""Fecha key"",{{""idFecha"", Int64.Type}}),
    #""Added Custom4"" = Table.AddColumn(#""Changed Type3"", ""MonthKey"", each [Year]*100+[Month]),
    #""Changed Type4"" = Table.TransformColumnTypes(#""Added Custom4"",{{""MonthKey"", Int64.Type}}),
    #""Added Custom5"" = Table.AddColumn(#""Changed Type4"", ""QuarterCode"", each [Year]*100 + (if [Month] <=3 then 1 else if [Month]<=6 then 2 else if [Month]<=9 then 3 else 4)),
    #""Changed Type5"" = Table.TransformColumnTypes(#""Added Custom5"",{{""QuarterCode"", Int64.Type}}),
    #""Inserted Day of Year"" = Table.AddColumn(#""Changed Type5"", ""Day of Year"", each Date.DayOfYear([Date]), Int64.Type),
    #""Inserted Day Name"" = Table.AddColumn(#""Inserted Day of Year"", ""Day Name"", each Date.DayOfWeekName([Date]), type text),
    #""Added Custom"" = Table.AddColumn(#""Inserted Day Name"", ""FiscalDate"", each Date.AddMonths([Date],-1*_FiscalStartMonth)),
    #""Changed Type1"" = Table.TransformColumnTypes(#""Added Custom"",{{""FiscalDate"", type datetime}}),
    #""Fiscal week of Year"" = Table.AddColumn(#""Changed Type1"", ""Fiscal Week of Year"", each Date.WeekOfYear([FiscalDate]), Int64.Type),
    #""FiscalYear"" = Table.AddColumn(#""Fiscal week of Year"", ""FiscalYear"", each Date.Year([FiscalDate]), Int64.Type),
    #""CurrentYear"" = Table.AddColumn(FiscalYear, ""Current Year"", each if  [Year]=Date.Year(DateTime.LocalNow()) then ""Current"" else if [Year]>Date.Year(DateTime.LocalNow()) then ""Future Year"" else ""Past Year""),
    #""CurrentMonth"" = Table.AddColumn(CurrentYear, ""Current Month"", each if [Year]*100+[Month] = Date.Year(DateTime.LocalNow()) *100+ Date.Month(DateTime.LocalNow())   then ""Current"" else if  [Year]*100+[Month] > Date.Year(DateTime.LocalNow()) *100+ Date.Month(DateTime.LocalNow())  then ""Future Month"" else ""Past Month""),
    #""CurrentWeek"" = Table.AddColumn(CurrentMonth, ""Current Week"", each if [Year]*100+[Month] = Date.Year(DateTime.LocalNow()) *100+ Date.WeekOfYear(DateTime.LocalNow())   then ""Current"" else if  [Year]*100+[Month] > Date.Year(DateTime.LocalNow()) *100+ Date.WeekOfYear(DateTime.LocalNow())  then ""Future Week"" else ""Past Week""),
    #""CurrentDay"" = Table.AddColumn(CurrentWeek, ""Current Day"", each if   [Date]=Date.From(DateTime.LocalNow()) then ""Current"" else if Date.From( [Date])>=Date.From(DateTime.LocalNow()) then ""Future Day"" else ""Past Day""),
    #""CurrentFiscalYear"" = Table.AddColumn(CurrentDay, ""Current Fiscal Year"", each if  [FiscalYear]=Date.Year(Date.AddMonths(DateTime.LocalNow(),-1*_FiscalStartMonth)) then ""Current"" else if   [FiscalYear]>=Date.Year(Date.AddMonths(DateTime.LocalNow(),-1*_FiscalStartMonth)) then ""Future Year"" else ""Past Year""),
    #""CurrentFiscalMonth"" = Table.AddColumn(CurrentFiscalYear, ""Current Fiscal Month"", each [Current Month]),
    #""CurrentFiscalWeek"" = Table.AddColumn(CurrentFiscalMonth, ""Current Fiscal Week"", each [Current Week]), 
       ObtenIsoWeek= (Fecha as date) => let
                    #""Removed Other Columns"" = Table.FromRows({{Fecha}},{""FullDateAlternateKey""}),
                    #""Added Custom"" = Table.AddColumn(#""Removed Other Columns"", ""PrimerDiaAño"", each Date.AddDays( #date(Date.Year([FullDateAlternateKey]),1,4) ,-1* Date.DayOfWeek(  #date(Date.Year([FullDateAlternateKey]),1,4),Day.Monday))),
                    #""Added Custom1"" = Table.AddColumn(#""Added Custom"", ""PrimerDiaAnioAA"", each Date.AddDays( #date(Date.Year([FullDateAlternateKey])-1,1,4) ,-1* Date.DayOfWeek(  #date(Date.Year([FullDateAlternateKey])-1,1,4),Day.Monday))),
                    #""Added Custom2"" = Table.AddColumn(#""Added Custom1"", ""PrimerDiaAnioSig"", each Date.AddDays( #date(1+Date.Year([FullDateAlternateKey]),1,4) ,-1* Date.DayOfWeek(  #date(1+Date.Year([FullDateAlternateKey]),1,4),Day.Monday))),
                    #""Changed Type"" = Table.TransformColumnTypes(#""Added Custom2"",{{""PrimerDiaAño"", type date}, {""PrimerDiaAnioAA"", type date}, {""PrimerDiaAnioSig"", type date}}),
                    #""Added Custom3"" = Table.AddColumn(#""Changed Type"", ""AnioNatural"", each Date.Year([FullDateAlternateKey])),
                    #""Added Conditional Column"" = Table.AddColumn(#""Added Custom3"", ""Custom"", each if [FullDateAlternateKey] >= [PrimerDiaAnioSig] then [AnioNatural] +1 else if [FullDateAlternateKey] >= [PrimerDiaAño] then [AnioNatural]  else [AnioNatural] -1 ),
                    #""Sorted Rows"" = Table.Sort(#""Added Conditional Column"",{{""FullDateAlternateKey"", Order.Descending}}),
                    #""Renamed Columns"" = Table.RenameColumns(#""Sorted Rows"",{{""Custom"", ""IsoYear""}}),
                    #""Changed Type1"" = Table.TransformColumnTypes(#""Renamed Columns"",{{""IsoYear"", Int64.Type}}),
                    #""Added Custom4"" = Table.AddColumn(#""Changed Type1"", ""IsoWeek"", each if [FullDateAlternateKey] >= [PrimerDiaAnioSig] then 1 else if [FullDateAlternateKey] >= [PrimerDiaAño] then 1+Number.RoundDown ( Duration.Days ([FullDateAlternateKey]-[PrimerDiaAño])/7 )  else 1+ Number.RoundDown ( Duration.Days ([FullDateAlternateKey]-Date.AddDays( #date(Date.Year([FullDateAlternateKey])-1,1,4) ,-1* Date.DayOfWeek(  #date(Date.Year([FullDateAlternateKey])-1,1,4),Day.Monday)))/7 )),
                        #""Removed Other Columns1"" = Table.SelectColumns(#""Added Custom4"",{""FullDateAlternateKey"", ""IsoYear"", ""IsoWeek""})
                        
                    in
                        #""Removed Other Columns1"" ,
    #""AniadirIsoWeek""=Table.AddColumn( #""CurrentFiscalWeek"",""DatosIso"",each ObtenIsoWeek( Date.From( [Date]))),
    #""Expanded DatosIso"" = Table.ExpandTableColumn(AniadirIsoWeek, ""DatosIso"", {""IsoYear"", ""IsoWeek""}, {""IsoYear"", ""IsoWeek""}),
   #""Added Custom1"" = Table.AddColumn(#""Expanded DatosIso"", ""DayOffset"", each Duration.Days([Date]-DateTime.LocalNow())),
    #""Added Custom2"" = Table.AddColumn(#""Added Custom1"", ""YearOffset"", each [Year]- Date.Year(DateTime.LocalNow())),
    #""Added Custom3"" = Table.AddColumn(#""Added Custom2"", ""MonthOffset"", each -1* (  (12 - [Month]) + (-[YearOffset]-1)*12 + Date.Month(DateTime.LocalNow()) )),
    #""Added Custom6"" = Table.AddColumn(#""Added Custom3"", ""QuarterOffset"", each -1* (  (4 - [Quarter]) + (-[YearOffset]-1)*4 + Date.QuarterOfYear(DateTime.LocalNow()) )),
    #""Changed Type2"" = Table.TransformColumnTypes(#""Added Custom6"",{{""DayOffset"", Int64.Type}, {""IsoWeek"", Int64.Type}, {""IsoYear"", Int64.Type}, {""YearOffset"", Int64.Type},{""QuarterOffset"", Int64.Type},  {""MonthOffset"", Int64.Type}, {""Current Year"", type text}, {""Current Month"", type text}, {""Current Week"", type text}, {""Current Day"", type text}, {""Current Fiscal Year"", type text}, {""Current Fiscal Month"", type text}, {""Current Fiscal Week"", type text}})
  
in
   #""Changed Type2""
";
partition.Mode=ModeType.Import ;
tab.Partitions.ConvertToPowerQuery();


if (tab.Columns.Count==0)
{
var displayFolder="Rest of";
var col=tab.AddDataColumn("Date","Date");
col.DisplayFolder=displayFolder;
 col=tab.AddDataColumn("FiscalDate","FiscalDate");
col.DisplayFolder=displayFolder;

col=tab.AddDataColumn("Year","Year");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Month","Month");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Month Name","Month Name");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("MonthKey","MonthKey");
col.DisplayFolder=displayFolder+@"\keys";
col=tab.AddDataColumn("Month Name Short","Month Name Short");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Month Name Complete","Month Name Complete");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Month Name Short Complete","Month Name Short Complete");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Quarter","Quarter");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("QuarterCode","QuarterCode");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Week of Year","Week of Year");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Day","Day");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Day Of Week","Day of Week");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Day of Year","Day of Year");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Day Name","Day Name");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Fiscal Week of Year","Fiscal Week of Year");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("FiscalYear","FiscalYear");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("Current Year","Current Year");
col=tab.AddDataColumn("Current Month","Current Month");
col=tab.AddDataColumn("Current Week","Current Week");
col=tab.AddDataColumn("Current Day","Current Day");
col=tab.AddDataColumn("Current Fiscal Year","Current Fiscal Year");
col=tab.AddDataColumn("Current Fiscal Month","Current Fiscal Month");
col=tab.AddDataColumn("Current Fiscal Week","Current Fiscal Week");
col=tab.AddDataColumn("ISOYear","IsoYear");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("ISOWeek","IsoWeek");
col.DisplayFolder=displayFolder;
col=tab.AddDataColumn("DayOffset","DayOffset");
col=tab.AddDataColumn("MonthOffset","MonthOffset");
col=tab.AddDataColumn("YearOffset","YearOffset");
col=tab.AddDataColumn("QuarterOffset","QuarterOffset");
col=tab.AddDataColumn("idFecha","idFecha");
col.DisplayFolder=displayFolder+@"\keys";
}
foreach(var col in tab.Columns)
{
    col.IsHidden=false;
    col.SummarizeBy = AggregateFunction.None;
    
}

tab.DataCategory= "Time";
var fecha=tab.Columns["Date"];
fecha.IsHidden=true;
fecha.IsKey=true;

var colyear= tab.Columns["Year"];
var colMonth = tab.Columns["Month Name"];
if (colMonth.SortByColumn == null )
{ 
    colMonth.SortByColumn=tab.Columns["Month"];
}
 colMonth = tab.Columns["Month Name short"];
if (colMonth.SortByColumn == null )
{ 
    colMonth.SortByColumn=tab.Columns["Month"];
}
colMonth = tab.Columns["Month Name Complete"];
if (colMonth.SortByColumn == null )
{ 
    colMonth.SortByColumn=tab.Columns["MonthKey"];
}
colMonth = tab.Columns["Month Name Short Complete"];
if (colMonth.SortByColumn == null )
{ 
    colMonth.SortByColumn=tab.Columns["MonthKey"];
}

var colDate = tab.Columns["Date"];
var crearjerarquiaincial="no";
if (tab.Hierarchies.Count()==0 )
{    tab.AddHierarchy("Calendar");
    crearjerarquiaincial="si";
}

var Hier =tab.Hierarchies[0];

if (crearjerarquiaincial == "si")
 {
    Hier.AddLevel(colyear,"Year");
    Hier.AddLevel(colMonth,"Month");
    Hier.AddLevel(colDate,"Date");
}
var ExistH=tab.Hierarchies[0];
ExistH=null;
foreach(var h in tab.Hierarchies)
{
    if (h.Name=="Fiscal")
          CrearFiscal="no";
}




/* si lo piden creamos la jerarquía fiscal */
if (CrearFiscal=="si")
{
    Hier= tab.AddHierarchy("Fiscal");
    colyear= tab.Columns["FiscalYear"];
    Hier.AddLevel(colyear,"Year");
    Hier.AddLevel(colMonth,"Month");
    Hier.AddLevel(colDate,"Date");
}
foreach(var h in tab.Hierarchies)
{
    if (h.Name=="Calendar Week")
         CrearSemana="no";
}



/* si lo piden creamos la jerarquía semana */ 
if (CrearSemana=="si")
{
    Hier= tab.AddHierarchy("Calendar Week");
    colyear= tab.Columns["Year"];
    Hier.AddLevel(colyear,"Year");
    var colWeek= tab.Columns["Week of Year"];
    Hier.AddLevel(colWeek,"Week of Year");
    Hier.AddLevel(colDate,"Date");
}

foreach(var h in tab.Hierarchies)
{
    if (h.Name=="Fiscal Week")
         CrearSemanaFiscal="no";
}


/* si lo piden creamos la jerarquía semana */ 
if (CrearSemanaFiscal=="si")
{
    Hier= tab.AddHierarchy("Fiscal Week");
    colyear= tab.Columns["FiscalYear"];
    Hier.AddLevel(colyear,"Fiscal Year");
    var colWeek= tab.Columns["Fiscal Week of Year"];
    Hier.AddLevel(colWeek,"Fiscal Week of Year");
    Hier.AddLevel(colDate,"Date");
}

foreach(var h in tab.Hierarchies)
{
    if (h.Name=="Iso Week")
        CrearIso="no";
}
if( CrearIso=="si")
{
        Hier= tab.AddHierarchy("Iso Week");
        colyear= tab.Columns["IsoYear"];
        Hier.AddLevel(colyear,"Iso Year");
        var colWeek= tab.Columns["IsoWeek"];
        Hier.AddLevel(colWeek,"IsoWeek");
        Hier.AddLevel(colDate,"Date");
}


var ToColumnstr=ToColumn.Split(',');

foreach(var s in ToColumnstr)
{
    
    if (s!="");
    {
        // s.Output();
       try { 
        var FromColumn = Model.Tables[ToTable].Columns[s];


        foreach(var rel in Model.Relationships)
        {
             if (rel.FromColumn.Name==FromColumn.Name && rel.FromColumn.Table ==FromColumn.Table &&
                 rel.ToColumn.Name==colDate.Name && rel.ToColumn.Table==colDate.Table)
                  {
                      ToTable="";
                      ToColumn="";
                  }
        }


        if (ToTable!="" && ToColumn!="")
            
        { 
            /* le llamo from porque generalmente pbi está haciendo las relaciones en la direccion * ---> 1 */
            var rel=Model.AddRelationship();
            rel.FromColumn=FromColumn;
            rel.ToColumn=colDate;
        }
    }
    catch (Exception ex)
    {
        // no hago nada si no existe la tabla con la que enganchar
    }
}

}
if (MostrarFechasRelativas=="si")
{
    foreach(var col in tab.Columns)
    {
        if( col.Name.StartsWith("Current") )
        {
            col.IsHidden=false;
            col.DisplayFolder="Relative Dates";
        }
        
          if( col.Name.EndsWith("Offset") )
        {
            col.IsHidden=false;
            col.DisplayFolder="Offsets";
        }
        
    }
}

tab2.Delete();
var measure= @"
 VAR _min =MIN ( '" + tname + @"'[Date] )
    VAR _max = MAX ( '" + tname + @"'[Date] )
    VAR _todos =SUMMARIZE('" + tname + @"','" + tname + @"'[Date],'" + tname + @"'[DayOffset],""sig"" ,'" + tname + @"'[DayOffset]+1, ""ant"",'" + tname + @"'[DayOffset]-1)
    var _txt1 =CONCATENATEX(_todos,'" + tname + @"'[Date] &  "" | "" & '" + tname + @"'[DayOffset] & "" | "" &[sig] & "" | "" &[ant],"","")
    var _enri= ADDCOLUMNS(_todos, ""tieneAnterior"", COUNTX( var _an=[ant] return filter(_todos,'" + tname + @"'[DayOffset]=_an),1),
                                  ""tienesiguiente"", COUNTX( var _an=[sig] return filter(_todos,'" + tname + @"'[DayOffset]=_an),1) )
    var _enri2=  ADDCOLUMNS(_enri,""concatenar"",
                IF (
                    ISBLANK ( [tieneanterior] ) && NOT ISBLANK ( [tienesiguiente] ),
                    ""-"",
                    IF ( ISBLANK ( [tienesiguiente] ), "","" )
                ))  
    VAR _devolver =
        CONCATENATEX (
            FILTER ( _enri2, NOT ISBLANK ( [concatenar] ) ),
            '" + tname + @"'[Date] & [concatenar],"""",'" + tname + @"'[Date])                                
    var _txt2 =CONCATENATEX(_enri2,'" + tname + @"'[Date] &  "" | "" & [concatenar] ,"","",'" + tname + @"'[Date])                                  
    return _devolver";
    
tab.AddMeasure("Selection",measure);
    
/*
var type = "calculate";
var database = Model.Database.Name;
var table = tab.Name;
var tmsl = "{ \"refresh\": { \"type\": \"%type%\", \"objects\": [ { \"database\": \"%db%\", \"table\": \"%table%\" } ] } }"
    .Replace("%type%", type)
    .Replace("%db%", database)
    .Replace("%table%", table);

ExecuteCommand(tmsl);
*/

