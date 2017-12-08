# xmldataimport
XmlDataImport kjører test data inn i en SQL Server database for datadreven automatiske tester. Verktøyet kan brukes enten fra kommandolinje eller inn i en automatiserte test. 

Verktøyet støtter: 
- innsetting og sletting
- forskjellige datatyper
- funksjoner
- variable 
- fremmende nøkler

XmlDataImport forenkler oppretting og vedlikehold av automatiserte data-drevet tester. Det en spesielt nyttig for testdekning når en del av løsningens logikk ligger i lagrete prosedyrer.  
Fra SQL Server Management Studio er det lett å ekstraher XML test data for bruk med verktøyet  
## Enhetstest 
### Eksempel bruk i en enhetstest  

	[TestClass]
	public class UserTest
	{
		private static XmlInsert _xmlDataImport;
		
		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext)
		{
			_xmlDataImport = new XmlInsert(false, TestConstants.TestDataDir + "\\xmld\\Application\\status.xmld");
		}
	
		[ClassCleanup]
		public static void ClassCleanup()
		{
			_xmlDataImport.Dispose();
		}
		
		[TestMethod]
		public void must_block_status_change_when_excluded_subject_code()
		{
	            var referenceDateTime = DateTime.Parse("2016-01-07T09:00:00");	           

### Forklaring av anbefalt bruk i en enhetstest 
1. Kjør data inn bare en gang per testkjøring i ```ClassInitialize```.
1. Slett data bare en gang i ```ClassCleanup```.  

## Kommandolinje  
### Bruk fra kommandolinje
Bruk med en enkel xmld fil:  
```UDir.XmlDataImport.Console.exe .\examplexmld\Persons.xmld```  
Bruk med flere xmld filer:  
```UDir.XmlDataImport.Console.exe .\examplexmld\Persons.xmld .\examplexmld\under\othertable.xmld```  
Bruk med en mappe (kjør inn data fra alle .xmld filer i mappen):  
```UDir.XmlDataImport.Console.exe .\examplexmld\```  
Bruk med flere mapper:  
```UDir.XmlDataImport.Console.exe .\examplexmld\ .\examplexmld\under\```  

## Konfigurasjon  
### App.config  
Når du kjører XmlDataImport i en test eller fra kommando linjen, må du hat litt konfigurasjon i din App.config fil. Du må legge til en ```connectionString``` element i ```connectionStrings```. Du må også legg til en referanse med navn til connectionString'en i en setting med nøkkel ```DBInstanceName``` element blant ````appSettings````:   

	<configuration>
	  	<connectionStrings>
	    	<add name="MYDB" connectionString="Data Source=(local);Initial Catalog=MYDB;Integrated Security=True;User Instance=False" providerName="System.Data.SqlClient" />
	  	</connectionStrings>
	  	<appSettings>
    		<add key="DBInstanceName" value="MYDB" />
    		<add key="ignoreNoChange" value="false" />
	  	</appSettings>
	</configuration>

Parameteren ```ignoreNoChange``` styrer oppførsel når det blir ingen endring i databasen etter du har lanserer XmlDataImport. Dersom parameter er satt til true, så vil du få en varsel når kjøring resulterer i ingen endring:

	No records inserted. There were 0 statements issued against the DB. Paths used: Persons.xmld


## XMLD fil
### XMLD fil eksempel

	<?xml version="1.0" encoding="utf-8" ?>
	<Root>
	  <![CDATA[ Eventuelle kommentarer kan legges inn sånn. Se hvordan vi håndterer 'større enn' tegn med &gt;]]>
	<Setup>
	  DECLARE @Group uniqueidentifier = 'EB26BB94-51D6-4507-9057-4F3CC52D0821';
	  DELETE FROM Answers WHERE Group = @Group;
	  DELETE FROM Answers WHERE DeliveryTime &gt; '2007-10-04T12:10:00.000';
	</Setup>
	<Answers>
		<AnswerId>1a2c8a68-c1de-4c9d-ad52-353c59f52f7b</AnswerId>
		<AnswerDokument>(select cast(N'' as xml).value('xs:base64Binary("JVBERigoyNTcyMgolJUVPRgo=")', 'varbinary(max)'))</AnswerDokument>
		<Group>EB26BB94-51D6-4507-9057-4F3CC52D0821</Group>
		<GroupDesc>Gruppe med veeeeldig langt navn;EV;NOR1033;Norsk tegnspråk</GroupDesc>
		<DeliveryTime>2007-10-05T12:10:00.000</DeliveryTime>
		<StudentId>123</StudentId>
		<Status>6</Status>
	</Answers>
	<Answers>
		<AnswerId>1a2c8a68-c1de-4c9d-ad52-353c59f5zzz</AnswerId>
		<AnswerDokument>(select cast(N'' as xml).value('xs:base64Binary("JVBERigoyNTcyMgolJUVPRgo=")', 'varbinary(max)'))</AnswerDokument>
	 	<Group>EB26BB94-51D6-4507-9057-4F3CC52D0821</Group>
		<GroupDesc>Gruppe med veeeeldig langt navn;EV;NOR1033;Norsk tegnspråk</GroupDesc>
		<DeliveryTime>GETDATE()</DeliveryTime>
		<StudentId>123</StudentId>		
		<Status>1</Status>
	</Answers>
	</Root>

### Forklaringer rundt XMLD eksempel fil struktur
En XLMD fil innholder:

1. En ````<Root>```` tag på begynnelse og slut
1. En valgfrit ````<Startup>```` tag med SQL som innhold. Applikasjonen kjører denne SQL i en egen transaksjon **før** innsetting av tabell data.
1. En valgfrit ````<Setup>```` tag med opprydding SQL som innhold. Applikasjonen kjører denne SQL i en egen transaksjon **før** og **etter** innsetting av tabell data.
1. En valgfrit ````<Teardown>```` tag med opprydding SQL som innhold. Applikasjonen kjører denne SQL i en egen transaksjon **på siste**.
1. Data fra en eller flere tabeller. Bruk ````<TabellNavn>```` som tag (f.eks ````<Answers>````).
1. Kolonne data innenfor hver ````<TabellNavn>```` tag. Hvis du vil ikke innsette data for en gitt kolonne bare ikke legg inn taggen for kolonnen blant kolonner for tabellen.  

### Kolonner
#### Kolonne data type
- Vanligvis er kolonne innehold tolket som string.
- Unntak er kolonner: 
  -  som inneholder funksjoner (f.eks. ```GETDATE()```) 
  -  som inneholder ekspresjoner mellom parenteser (f.eks. ```(select cast(N'' as xml).value('xs:base64Binary("JVBERigoyNTcyMgolJUVPRgo=")```)  

Bruk funksjoner eller ekspresjoner når du ikke vil innsette kolonneverdien som en string: (```Convert(uniqueidentifier, '12345678-1234-1234-1234-123456789123')```)   

#### Håndtering av kolonner med fremmede nøkler
Bruk en ```SELECT``` mellom ```(...)```:  
  
	<Answers>
	    <AnswerId>1a2c8a68-c1de-4c9d-ad52-353c59f52f7b</AnswerId>
	    <AnswerDokument>(select cast(N'' as xml).value('xs:base64Binary("JVBERigoyNTcyMgolJUVPRgo=")', 'varbinary(max)'))</AnswerDokument>
	    <Group>EB26BB94-51D6-4507-9057-4F3CC52D0821</Group>
	    <GroupDesc>Gruppe med veeeeldig langt navn;EV;NOR1033;Norsk tegnspråk</GroupDesc>
	</Answers>
	<ConvertedAnswer>
	     <AnswerId>(SELECT a.AnswerId FROM Answers a WHERE b.AnswerId = '1a2c8a68-c1de-4c9d-ad52-353c59f52f7b')</AnswerId>
	    <ConvertedDocument>Convert(varbinary, 'PD94bWwgdmVyc2lvbj0iMS4wIj8+DQo8T3ucz4=')</ConvertedDocument>
	    <DeliveryTime>1974-12-31T12:10:00.000</DeliveryTime>
	    <Status>2</Status>
	</ConvertedAnswer>

### Variabler  
Du kan, om du vil, definere string variabler i en dictionary argument sendt inn i konstruktøren til XmlInsert:  

	_xmlDataImport = new XmlInsert(true,
			new Dictionary<string, string> { { "StudentId", _studentId } }, //Variable i XMLD fil
			TestConstants.TestDataDir + "\\xmld\\Application\\status.xmld");

Variabler skal benyttes i ```variable()``` ekspresjoner inn i XMLD filen:  

	<Setup>
	  DELETE FROM Answers WHERE StudentId = variable('StudentId');
	</Setup>
	<Answers>
		<AnswerId>1a2c8a68-c1de-4c9d-ad52-353c59f52f7b</AnswerId>
		<AnswerDokument>(select cast(N'' as xml).value('xs:base64Binary("JVBERigoyNTcyMgolJUVPRgo=")', 'varbinary(max)'))</AnswerDokument>
		<Group>EB26BB94-51D6-4507-9057-4F3CC52D0821</Group>
		<GroupDesc>Gruppe med veeeeldig langt navn;EV;NOR1033;Norsk tegnspråk</GroupDesc>
		<EksamenStarttid>2016-01-08T09:00:00</EksamenStarttid>
		<StudentId>variable('StudentId')</StudentId>


Om du vil ha en annen datatype enn string, da kan du benytte SQL ```Convert()``` funksjonen:  

	<Status>Convert(int, variable('Number1'))</Status>

Variabler er nyttige for å kunne bruke den samme XMLD fil i forskjellige test scenarier med forskjellige data. I dette tilfelle bruk XmlInsert i en ```using(...) ``` statement inn i din test metode:

		public void GetAnswer_must_retrieve_expected_answer()
		{
			var firstCandidateNumber = "123";
			var firstCandidateStatus = "8";
			var answer = new Answer(new User(), null);
			var group = new Guid("9BA98065-0D07-4360-B2D4-B6198B8D1082");

			using (new XmlInsert(false,
				new Dictionary<string, string>
				{
					{"FirstCandidateNumber", firstCandidateNumber},
					{"FirstCandidateStatus", firstCandidateStatus}
				},
				TestConstants.TestDataDir + "\\xmld\\web\\Application\\answer.xmld"))
			{
				var answerList = answer.GetAnswer(candidategroupCode, firstCandidateNumber;
								
				Assert.IsTrue(answerList.Count >= 2,
					"Failed to retreive expected answers.");
			} 
		}

### Enkelt oppretting av XML data filer fra SQS Management Studio
Du kan hente opp nesten automatisk riktig formatert data fra SQL Server som XML med bruk av en sånn forespørsel:

	SELECT TOP 1 * FROM Schools
	    FOR XML PATH('Schools'), ROOT('Root')

Resultat:

	<Root>
	  <Schools>
	    <SchoolID>9876</SchoolID>
	    <SchoolName>Blabla School</SchoolName>
	  </Schools>
	</Root>
